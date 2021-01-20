// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Concurrent;
using Vlingo.Actors;
using Vlingo.Symbio;
using Vlingo.Symbio.Store.Dispatch;
using Vlingo.Symbio.Store.Journal;

namespace Vlingo.Lattice.Model.Sourcing
{
    /// <summary>
    /// Registry for <see cref="Sourced{T}"/> types that holds the <see cref="Journal{T}"/> type,
    /// <see cref="EntryAdapterProvider"/>, and <see cref="StateAdapterProvider"/>.
    /// </summary>
    public class SourcedTypeRegistry<T>
    {
        internal static readonly string InternalName = Guid.NewGuid().ToString();
        private readonly ConcurrentDictionary<Type, object> _stores = new ConcurrentDictionary<Type, object>();

        /// <summary>
        /// Answer a new <see cref="SourcedTypeRegistry{T}"/> with registered <paramref name="sourcedTypes"/>, creating
        /// the <see cref="IJournal{T}"/> of type <typeparamref name="T"/>, registering me with the <paramref name="world"/>.
        /// </summary>
        /// <param name="world">The World to which I am registered</param>
        /// <param name="dispatcher"><see cref="IDispatcher{TDispatchable}"/> of the journal.</param>
        /// <param name="sourcedTypes"><see cref="Sourced{T}"/> types of to register</param>
        /// <returns>The registry</returns>
        public static SourcedTypeRegistry<T> Register<TActor, TEntry, TState>(World world,
            IDispatcher<Dispatchable<IEntry<TEntry>, State<TState>>> dispatcher, params Type[] sourcedTypes)
            where TActor : Actor
        {
            var journal = world.ActorFor<IJournal<T>>(typeof(TActor), dispatcher);

            return new SourcedTypeRegistry<T>(world, journal, sourcedTypes);
        }

        /// <summary>
        /// Construct my default state with <paramref name="sourcedTypes"/> creating the <see cref="IJournal{T}"/>
        /// of type <typeparamref name="T"/>, and register me with the <paramref name="world"/>.
        /// </summary>
        /// <param name="world">The World to which I am registered</param>
        /// <param name="journal">The journal of this registry</param>
        /// <param name="sourcedTypes"><see cref="Sourced{T}"/> types of to register</param>
        public SourcedTypeRegistry(World world, IJournal<T> journal, params Type[] sourcedTypes)
        : this (world)
        {
            EntryAdapterProvider.Instance(world);

            foreach (var sourcedType in sourcedTypes)
            {
                Register(new Info<T>(journal, sourcedType.FullName));
            }
        }
        
        /// <summary>
        /// Construct my default state and register me with the <see cref="World"/>.
        /// </summary>
        /// <param name="world">The World to which I am registered</param>
        public SourcedTypeRegistry(World world) => world.RegisterDynamic(InternalName, this);
        
        /// <summary>
        /// Answer the <see cref="Info{T}"/> of the <typeparamref name="T"/> type.
        /// </summary>
        /// <returns><see cref="Info{T}"/></returns>
        public Info<T>? Info()
        {
            if (_stores.TryGetValue(typeof(Sourced<T>), out var value))
            {
                return (Info<T>) value;
            }

            return default;
        }

        /// <summary>
        /// Answer myself after registering the <see cref="Info{T}"/>.
        /// </summary>
        /// <param name="info"><see cref="Info{T}"/> to register</param>
        /// <returns>The registry</returns>
        public SourcedTypeRegistry<T> Register(Info<T> info)
        {
            _stores.AddOrUpdate(info.SourcedType, info, (type, o) => info);
            return this;
        }
    }
}