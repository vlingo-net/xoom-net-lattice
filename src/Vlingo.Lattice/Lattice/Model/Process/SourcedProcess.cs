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
using Vlingo.Lattice.Exchange;
using Vlingo.Lattice.Model.Sourcing;
using Vlingo.Symbio;
using Vlingo.Symbio.Store.Journal;
using Vlingo.Symbio.Store.Object;

namespace Vlingo.Lattice.Model.Process
{
    /// <summary>
    /// Abstract base definition for all concrete sourced process types. My extenders
    /// <code>Emit()</code> <see cref="Command"/> and/or <see cref="DomainEvent"/> instances
    /// that cause reactions by my process collaborators. The underlying support
    /// comes from the <see cref="Sourced{T}"/> base, with <see cref="ProcessMessage"/> serving
    /// as the <typeparamref name="T"/>. Thus, every emitted <see cref="Command"/> and <see cref="DomainEvent"/>
    /// is wrapped by a <see cref="ProcessMessage"/>. Due to the fact that I am effectively
    /// dual sourced, my state is comprised of a stream of all emitted instances of
    /// <see cref="Source{T}"/> types. In case you do not desire a given <see cref="Source{T}"/>
    /// to contribute to my state, use the <see cref="M:Send()"/> behaviors for those rather
    /// than the <code>Emit()</code> behaviors. Note, however, that <see cref="M:Send()"/> is
    /// subject to failures of the underlying <see cref="IExchange"/> mechanism.
    /// </summary>
    /// <typeparam name="T">The type of the process state and used by the <see cref="Chronicle"/></typeparam>
    public abstract class SourcedProcess<T> : Sourced<T>, IProcess<T> where T : StateObject
    {
        private readonly Info<SourcedProcess<T>> _info;
        private readonly List<Source> _applied;

        public Chronicle<T> Chronicle => Snapshot<Chronicle<T>>();
        
        public abstract string ProcessId { get; }
        
        /// <summary>
        /// Construct my default state using my <code>address</code> as my <code>streamName</code>.
        /// </summary>
        public SourcedProcess() : this(null)
        {
        }
        
        protected SourcedProcess(string? streamName) : base(streamName)
        {
            _info = Stage.World.ResolveDynamic<ProcessTypeRegistry<T>>(typeof(ProcessTypeRegistry<T>).Name).Info<SourcedProcess<T>>();
            _applied = new List<Source>(2);
        }
        
        /// <summary>
        /// Uses the underlying <see cref="Journal{T}"/> for Event Sourcing semantics.
        /// </summary>
        /// <param name="command">The command to source</param>
        public void Process(Command command)
        {
            _applied.Add(command);
            Apply(new ProcessMessage(command));
        }

        public ICompletes<T1> Process<T1>(Command command, Func<T1> andThen)
        {
            _applied.Add(command);
            return Apply(new ProcessMessage(command), andThen);
        }

        public void Process(DomainEvent @event)
        {
            _applied.Add(@event);
            Apply(new ProcessMessage(@event));
        }

        public ICompletes<T1> Process<T1>(DomainEvent @event, Func<T1> andThen)
        {
            _applied.Add(@event);
            return Apply(new ProcessMessage(@event), andThen);
        }

        public void ProcessAll<TSource>(IEnumerable<Source<TSource>> sources)
        {
            var listSources = sources.ToList();
            _applied.AddRange(listSources);
            Apply(ProcessMessage.Wrap(listSources));
        }

        public ICompletes<T1> ProcessAll<T1, TSource>(IEnumerable<Source<TSource>> sources, Func<T1> andThen)
        {
            var listSources = sources.ToList();
            _applied.AddRange(listSources);
            return Apply(ProcessMessage.Wrap(listSources), andThen);
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