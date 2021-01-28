// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using Vlingo.Common;
using Vlingo.Lattice.Model.Stateful;
using Vlingo.Symbio;
using Vlingo.Symbio.Store.Object;

namespace Vlingo.Lattice.Model.Process
{
    /// <summary>
    /// Abstract base definition for all concrete stateful process types.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="StatefulEntity{T}"/></typeparam>
    public abstract class StatefulProcess<T> : StatefulEntity<T>, IProcess<T> where T : StateObject, IEntry
    {
        private readonly Info<StatefulProcess<T>> _info;
        private readonly List<Source> _applied;

        public abstract Chronicle<T> Chronicle { get; }
        
        public abstract string ProcessId { get; }
        
        /// <summary>
        /// Construct my default state using my <code>address</code> as my <code>streamName</code>.
        /// </summary>
        public StatefulProcess() : this(null)
        {
        }
        
        protected StatefulProcess(string? streamName) : base(streamName)
        {
            _info = Stage.World.ResolveDynamic<ProcessTypeRegistry<T>>(typeof(ProcessTypeRegistry<T>).Name).Info<StatefulProcess<T>>();
            _applied = new List<Source>(2);
        }

        public void Process(Command command)
        {
            _applied.Add(command);
            Apply(Chronicle.State, new ProcessMessage(command));
        }

        public ICompletes<T1> Process<T1>(Command command, Func<T1> andThen)
        {
            _applied.Add(command);
            return Apply(Chronicle.State, new ProcessMessage(command), andThen);
        }

        public void Process(DomainEvent @event)
        {
            _applied.Add(@event);
            Apply(Chronicle.State, new ProcessMessage(@event));
        }

        public ICompletes<T1> Process<T1>(DomainEvent @event, Func<T1> andThen)
        {
            _applied.Add(@event);
            return Apply(Chronicle.State, new ProcessMessage(@event), andThen);
        }

        public void ProcessAll<TSource>(IEnumerable<Source<TSource>> sources)
        {
            var enumerable = sources.ToList();
            _applied.AddRange(enumerable);
            Apply(Chronicle.State, ProcessMessage.Wrap(enumerable));
        }

        public ICompletes<T1> ProcessAll<T1, TSource>(IEnumerable<Source<TSource>> sources, Func<T1> andThen)
        {
            var enumerable = sources.ToList();
            _applied.AddRange(enumerable);
            return Apply(Chronicle.State, ProcessMessage.Wrap(enumerable), andThen);
        }

        public void Send(Command command) => _info.Exchange.Send(command);

        public void Send(DomainEvent @event) => _info.Exchange.Send(@event);

        protected override void AfterApply()
        {
            foreach (var source in _applied)
            {
                _info.Exchange.Send(source);
            }
            
            _applied.Clear();
        }
    }
}