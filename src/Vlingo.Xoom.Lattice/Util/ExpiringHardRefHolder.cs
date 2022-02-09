// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Lattice.Util
{
    /// <summary>
    /// Holds hard references in a queue and provides means to expunge "expired" references based on timeout duration.
    /// </summary>
    public class ExpiringHardRefHolder : Actor, IHardRefHolder, IScheduled<object>
    {
        private readonly Func<DateTime> _now;
        private readonly TimeSpan _timeout;
        private readonly MinHeap<Expiring> _queue;
        
        public ExpiringHardRefHolder() : this(TimeSpan.FromSeconds(20))
        {
        }
        
        public ExpiringHardRefHolder(TimeSpan timeout) : this(() => DateTime.Now, timeout)
        {
        }
        
        private ExpiringHardRefHolder(Func<DateTime> now, TimeSpan timeout)
        {
            _now = now;
            _timeout = timeout;
            _queue = new MinHeap<Expiring>();

            Scheduler.Schedule(SelfAs<IScheduled<object?>>(), null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        }
        
        public void HoldOnTo(object @object)
        {
            var expiry = _now().Add(_timeout);
            Logger.Debug("Holding on to {} until {}", @object, expiry);
            _queue.Add(new Expiring(expiry, @object));
        }

        public void IntervalSignal(IScheduled<object> scheduled, object data)
        {
            Logger.Debug("Starting expired references cleanup at {} ...", _now());
            var count = 0;
            Expiring? next;
            do
            {
                if (_queue.Count == 0)
                {
                    // queue is empty
                    break;
                }
                next = _queue.GetMin();
                
                if (next.BestBefore < _now())
                {
                    // dereference
                    _queue.ExtractDominating();
                    ++count;
                }
                else
                {
                    // stop
                    next = null;
                }
            } while (next != null);
            Logger.Debug("Finished cleanup of expired references at {}. {} removed.", _now(), count);
        }
        
        /// <summary>
        /// Note: this class has a natural ordering that is inconsistent with equals.
        /// </summary>
        private class Expiring : IComparable<Expiring>
        {
            public DateTime BestBefore { get; }
            private readonly object _reference;

            internal Expiring(DateTime bestBefore, object reference)
            {
                if (bestBefore == null)
                {
                    throw new ArgumentNullException(nameof(bestBefore));
                }
                
                if (reference == null)
                {
                    throw new ArgumentNullException(nameof(reference));
                }
                
                BestBefore = bestBefore;
                _reference = reference;
            }
            
            public int CompareTo(Expiring? other)
            {
                if (other == null)
                {
                    return 1;
                }
                
                return BestBefore.CompareTo(other.BestBefore);
            }

            public override bool Equals(object? obj)
            {
                if (this == obj)
                {
                    return true;
                }

                if (obj == null || GetType() != obj.GetType())
                {
                    return false;
                }
                
                var expiring = (Expiring) obj;
                return BestBefore.Equals(expiring.BestBefore) && _reference.Equals(expiring._reference);
            }

            protected bool Equals(Expiring other) => Equals((object) other);

            public override int GetHashCode() => 31 * BestBefore.GetHashCode() + _reference.GetHashCode();

            public override string ToString() => $"Expiring(bestBefore={BestBefore}, reference={_reference})";
        }
    }
}