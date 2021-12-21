// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Lattice.Grid.Spaces
{
    public class SpaceActor : Actor, ISpace, IScheduled<IScheduledScanner<IScheduledScannable>>
    {
        private static readonly long _brief = 5;
        private static readonly long _rounding = 100;

        private readonly SortedSet<ExpirableItem> _expirableItems;
        private readonly SortedSet<ExpirableQuery> _expirableQueries;
        private readonly TimeSpan _defaultScanInterval;
        private readonly Dictionary<Type, Dictionary<IKey, ExpirableItem>> _registry;
        private readonly ScheduledQueryRunnerEvictor _scheduledQueryRunnerEvictor;
        private readonly ScheduledSweeper _scheduledSweeper;
        private readonly IScheduled<IScheduledScanner<IScheduledScannable>> _scheduled;
        
        public SpaceActor(TimeSpan defaultScanInterval)
        {
            _defaultScanInterval = defaultScanInterval;
            _expirableItems = new SortedSet<ExpirableItem>();
            _expirableQueries = new SortedSet<ExpirableQuery>();
            _registry = new Dictionary<Type, Dictionary<IKey, ExpirableItem>>();
            _scheduled = SelfAs<IScheduled<IScheduledScanner<IScheduledScannable>>>();
            _scheduledQueryRunnerEvictor = new ScheduledQueryRunnerEvictor(this);
            _scheduledSweeper = new ScheduledSweeper(this);
        }
        
        public ICompletes<T> ItemFor<T>(Type actorType, params object[] parameters)
        {
            // Fail; not implemented. See SpaceItemFactoryRelay#itemFor.
            return Completes().With<T>(default!);
        }

        public ICompletes<KeyItem<T>> Put<T>(IKey key, Item item)
        {
            Manage<T>(key, item);

            return Completes().With(KeyItem<T>.Of(key, (T) item.Object, item.Lease));
        }

        public ICompletes<Optional<KeyItem<T>>> Get<T>(IKey key, Period until)
        {
            var item = Item(key, true);

            if (item == null)
            {
                PeriodicQuery(key, true, until);

                return Completes<Optional<KeyItem<T>>>();
            }

            return Completes().With(Optional.Of(KeyItem<T>.Of(key, (T) item.Object, item.Lease)));
        }

        public ICompletes<Optional<KeyItem<T>>> Take<T>(IKey key, Period until)
        {
            var item = Item(key, false);
            
            if (item == null)
            {
                PeriodicQuery(key, false, until);

                return Completes<Optional<KeyItem<T>>>();
            }

            return Completes().With(Optional.Of(KeyItem<T>.Of(key, (T) item.Object, item.Lease)));
        }

        public void IntervalSignal(IScheduled<IScheduledScanner<IScheduledScannable>> scheduled, IScheduledScanner<IScheduledScannable> data) => 
            data.Scan();

        private ExpirableItem ExpiringItem(IKey key, Item item)
        {
            var expiration = item.Lease.ToFutureDateTime();
            return new ExpirableItem(key, item.Object!, expiration, item.Lease);
        }
        
        private ExpirableQuery ExpiringQuery(IKey key, bool retainItem, Period period)
        {
            var expiration = period.ToFutureDateTime();
            return new ExpirableQuery(key, retainItem, expiration, period, CompletesEventually());
        }
        
        private ExpirableItem? Item(IKey key, bool retain)
        {
            var itemMap = ItemMap(key);

            if (retain)
            {
                return itemMap[key];
            }

            if (itemMap.TryGetValue(key, out var removed))
            {
                itemMap.Remove(key);
                return removed;
            }

            return null;
        }
        
        private Dictionary<IKey, ExpirableItem> ItemMap(IKey key)
        {
            var keyClass = key.GetType();

            var itemMap = GetItemMap(keyClass);

            if (itemMap == null)
            {
                itemMap = new Dictionary<IKey, ExpirableItem>();
                PutItemMap(keyClass, itemMap);
            }

            return itemMap;
        }
        
        private Dictionary<IKey, ExpirableItem>? GetItemMap(Type type)
        {
            if (_registry.TryGetValue(type, out var map))
            {
                return map.ToDictionary(m => m.Key, pair => pair.Value);
            }

            return null;
        }
        
        private void PutItemMap(Type type, Dictionary<IKey, ExpirableItem> itemMap) =>
            _registry.Add(type, itemMap.ToDictionary(p => p.Key, pair => pair.Value));
        
        private void Manage<T>(IKey key, Item item)
        {
            var expiringItem = ExpiringItem(key, item);

            var itemMap = ItemMap(expiringItem.Key);

            itemMap.Add(expiringItem.Key, expiringItem);
            _registry[key.GetType()] = itemMap.ToDictionary(k => k.Key, pair => pair.Value);

            if (!expiringItem.IsMaximumExpiration)
            {
                _expirableItems.Add(expiringItem);

                _scheduledSweeper.ScheduleBy(Spaces.Item.Of(item.Object!, item.Lease));
            }
        }
        
        private void PeriodicQuery(IKey key, bool retain, Period until)
        {
            var query = ExpiringQuery(key, retain, until);

            _expirableQueries.Add(query);

            _scheduledQueryRunnerEvictor.ScheduleBy(query);
        }

        //================================
        // ScheduledQueryRunnerEvictor
        //================================

        private class ScheduledQueryRunnerEvictor : IScheduledScanner<ExpirableQuery>
        {
            private readonly SpaceActor _spaceActor;
            private Optional<ICancellable> _cancellable;
            private TimeSpan _currentDuration;
            
            public ScheduledQueryRunnerEvictor(SpaceActor spaceActor)
            {
                _spaceActor = spaceActor;
                _cancellable = Optional.Empty<ICancellable>();
                _currentDuration = TimeSpan.MaxValue;
            }

            public void Scan()
            {
                var now = DateTime.Now;

                var confirmedExpirables = new List<ExpirableQuery>();

                foreach (var expirableQuery in _spaceActor._expirableQueries)
                {
                    var item = _spaceActor.Item(expirableQuery.Key, expirableQuery.RetainItem);

                    if (item != null)
                    {
                        expirableQuery.Completes.With(Optional.Of(KeyItem<object>.Of(item.Key, item.Object, item.Lease)));
                        confirmedExpirables.Add(expirableQuery);
                    }
                    else if (item == null)
                    {
                        if (now > expirableQuery.ExpiresOn)
                        {
                            confirmedExpirables.Add(expirableQuery);
                            expirableQuery.Completes.With(Optional.Empty<object>());
                        }
                    }
                }

                foreach (var expirableQuery in confirmedExpirables)
                {
                    _spaceActor._expirableQueries.Remove(expirableQuery);
                }

                var iterator = _spaceActor._expirableQueries.GetEnumerator();

                if (iterator.MoveNext())
                {
                    var millis = (iterator.Current.ExpiresOn.GetCurrentSeconds() - DateTime.Now.GetCurrentSeconds()) * 1_000;
                    var minQueryDuration = TimeSpan.FromMilliseconds(millis < 0 ? _rounding : millis);
                    _currentDuration = Min(minQueryDuration, _spaceActor._defaultScanInterval);
                }
                else
                {
                    _currentDuration = _spaceActor._defaultScanInterval;
                }

                Schedule();
                
                iterator.Dispose();
            }

            public void ScheduleBy(IScheduledScannable<ExpirableQuery> scannable)
            {
                var query = scannable.Scannable();
                var rounded = query.Period.ToMilliseconds() + _rounding;

                if (rounded < _currentDuration.TotalMilliseconds)
                {
                    _currentDuration = Min(query.Period.Duration, _spaceActor._defaultScanInterval);
                }

                Schedule();
            }
            
            private TimeSpan Min(TimeSpan duration1, TimeSpan duration2) => duration1.TotalMilliseconds < duration2.TotalMilliseconds ? duration1 : duration2;
            
            private void Schedule()
            {
                _cancellable.IfPresent(canceller => canceller.Cancel());

                _cancellable = Optional.Of(_spaceActor.Scheduler.ScheduleOnce(_spaceActor._scheduled, (IScheduledScanner<IScheduledScannable>) this, TimeSpan.Zero, _currentDuration));
            }
        }
        
        //================================
        // ScheduledSweeper
        //================================
        
        private class ScheduledSweeper : IScheduledScanner<IScheduledScannable>
        {
            private readonly SpaceActor _spaceActor;
            private Optional<ICancellable> _cancellable;
            private TimeSpan _currentDuration;

            public ScheduledSweeper(SpaceActor spaceActor)
            {
                _spaceActor = spaceActor;
                _cancellable = Optional.Empty<ICancellable>();
                _currentDuration = TimeSpan.MaxValue;
            }
            
            public void Scan()
            {
                var now = DateTime.Now;

                var confirmedExpirables = new List<ExpirableItem>();

                foreach (var expirableItem in _spaceActor._expirableItems)
                {
                    if (now > expirableItem.ExpiresOn)
                    {
                        var itemMap = _spaceActor.ItemMap(expirableItem.Key);
                        if (itemMap != null && itemMap.Remove(expirableItem.Key))
                        {
                            _spaceActor._registry[expirableItem.Key.GetType()] = itemMap;
                            confirmedExpirables.Add(expirableItem);
                        }
                    }
                }

                foreach (var expirableItem in confirmedExpirables)
                {
                    _spaceActor._expirableItems.Remove(expirableItem);
                }

                var iterator = _spaceActor._expirableItems.GetEnumerator();

                if (iterator.MoveNext())
                {
                    var millis = iterator.Current.ExpiresOn.GetMillis() - DateTime.Now.GetMillis();
                    _currentDuration = TimeSpan.FromMilliseconds(millis < 0 ? _brief : millis);
                }
                else
                {
                    _currentDuration = _spaceActor._defaultScanInterval;
                }

                Schedule();
                
                iterator.Dispose();
            }

            public void ScheduleBy(IScheduledScannable<IScheduledScannable> scannable)
            {
                var item = scannable.Scannable();
                var rounded = ((Item) item).Lease.Duration.TotalMilliseconds + _rounding;

                if (rounded < _currentDuration.TotalMilliseconds)
                {
                    _currentDuration = ((Item) item).Lease.Duration;

                    Schedule();
                }
            }
            
            private void Schedule()
            {
                _cancellable.IfPresent(canceller => canceller.Cancel());

                _cancellable = Optional.Of(_spaceActor.Scheduler.ScheduleOnce(_spaceActor._scheduled, this, TimeSpan.Zero, _currentDuration));
            }
        }
    }
}