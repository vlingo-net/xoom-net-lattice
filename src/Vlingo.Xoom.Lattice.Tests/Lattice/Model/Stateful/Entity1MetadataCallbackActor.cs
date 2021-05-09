// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Threading;
using Vlingo.Xoom.Common;
using Vlingo.Xoom.Lattice.Model.Stateful;

namespace Vlingo.Tests.Lattice.Model.Stateful
{
    public class Entity1MetadataCallbackActor : StatefulEntity<Entity1State>, IEntity1
    {
        private readonly AtomicInteger _raceConditions;
        private readonly AtomicBoolean _runsApply = new AtomicBoolean(false);
        private Entity1State _state;

        public Entity1MetadataCallbackActor(AtomicInteger raceConditions, string id) : base(id)
        {
            _raceConditions = raceConditions;
        }

        protected override ICompletes<TResult> Apply<TResult>(Entity1State state, string metadataValue, string operation, Func<TResult> andThen)
        {
            _runsApply.CompareAndSet(false, true);
            return base.Apply(state, metadataValue, operation, andThen);
        }

        protected override ICompletes<TResult> Apply<TSource, TResult>(Entity1State state, IEnumerable<TSource> sources, string metadataValue, string operation, Func<TResult> andThen)
        {
            _runsApply.CompareAndSet(false, true);
            return base.Apply(state, sources, metadataValue, operation, andThen);
        }

        protected override void AfterApply()
        {
            _runsApply.CompareAndSet(true, false);
            base.AfterApply();
        }

        protected override ICompletes Completes()
        {
            if (_runsApply.Get())
            {
                try
                {
                    Thread.Sleep(300);
                }
                catch (Exception e)
                {
                    Logger.Error("Tread sleep aborted", e);
                    throw;
                }
            }

            try
            {
                return base.Completes();
            }
            catch (Exception)
            {
                _raceConditions.IncrementAndGet();
                // Assert.assertNotNull("Race condition has been reproduced!", null);
                throw;
            }
        }

        //===================================
        // Entity1
        //===================================

        public ICompletes<Entity1State> DefineWith(string name, int age)
        {
            if (_state == null)
            {
                return Apply(new Entity1State(Id, name, age), "METADATA", "new", () => _state);
            }

            return Completes().With(_state.Copy());
        }

        public ICompletes<Entity1State> Current() => Completes().With(_state.Copy());

        public void ChangeName(string name) => Apply<Entity1State>(_state.WithName(name), "METADATA", "ChangeName");

        public void IncreaseAge() => Apply<Entity1State>(_state.WithAge(_state.Age + 1), "METADATA", "IncreaseAge");

        //===================================
        // StatefulEntity
        //===================================
        
        protected override void State(Entity1State state) => _state = state;
    }
}