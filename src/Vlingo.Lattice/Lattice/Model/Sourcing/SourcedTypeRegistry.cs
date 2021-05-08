// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Concurrent;
using Vlingo.Xoom.Symbio;
using Vlingo.Xoom.Symbio.Store.Journal;
using Vlingo.Xoom.Actors;
using IDispatcher = Vlingo.Xoom.Symbio.Store.Dispatch.IDispatcher;

namespace Vlingo.Lattice.Model.Sourcing
{
    /// <summary>
    /// Registry for <see cref="Sourced{T}"/> types that holds the <see cref="Journal{T}"/> type,
    /// <see cref="EntryAdapterProvider"/>, and <see cref="StateAdapterProvider"/>.
    /// </summary>
    public class SourcedTypeRegistry
    {
        private Type? _sourcedType;
        internal static readonly string InternalName = Guid.NewGuid().ToString();
        private readonly ConcurrentDictionary<Type, object> _stores = new ConcurrentDictionary<Type, object>();

        /// <summary>
        /// Answer a new <see cref="SourcedTypeRegistry"/> with registered <paramref name="sourcedTypes"/>, creating
        /// the <see cref="IJournal{TEntry}"/> of type <typeparamref name="TEntry"/>, registering me with the <paramref name="world"/>.
        /// </summary>
        /// <param name="world">The World to which I am registered</param>
        /// <param name="dispatcher"><see cref="IDispatcher"/> of the journal.</param>
        /// <param name="sourcedTypes"><see cref="Sourced{T}"/> types of to register</param>
        /// <returns>The registry</returns>
        public static SourcedTypeRegistry Register<TActor, TEntry, TState>(World world,
            IDispatcher dispatcher, params Type[] sourcedTypes)
            where TActor : Actor
        {
            var journal = world.ActorFor<IJournal<TEntry>>(typeof(TActor), dispatcher);

            return new SourcedTypeRegistry(world, journal, sourcedTypes);
        }

        /// <summary>
        /// Construct my default state with <paramref name="sourcedTypes"/> creating the <see cref="IJournal{T}"/>
        /// and register me with the <paramref name="world"/>.
        /// </summary>
        /// <param name="world">The World to which I am registered</param>
        /// <param name="journal">The journal of this registry</param>
        /// <param name="sourcedTypes"><see cref="Sourced{T}"/> types of to register</param>
        public SourcedTypeRegistry(World world, IJournal journal, params Type[] sourcedTypes) : this (world)
        {
            EntryAdapterProvider.Instance(world);

            foreach (var sourcedType in sourcedTypes)
            {
                Register(Sourcing.Info.RegisterSourced(journal, sourcedType));
            }
        }
        
        /// <summary>
        /// Construct my default state and register me with the <see cref="World"/>.
        /// </summary>
        /// <param name="world">The World to which I am registered</param>
        public SourcedTypeRegistry(World world) => world.RegisterDynamic(InternalName, this);

        /// <summary>
        /// Answer the <see cref="Info"/>.
        /// </summary>
        /// <typeparam name="TSourced">The type of the sourced entity</typeparam>
        /// <returns><see cref="Info"/></returns>
        public Info? Info<TSourced>() => Info(typeof(TSourced));

        /// <summary>
        /// Answer the <see cref="Info"/>.
        /// </summary>
        /// <returns><see cref="Info"/></returns>
        public Info? Info(Type sourcedType)
        {
            _sourcedType = sourcedType;
            
            if (_stores.TryGetValue(sourcedType, out var value))
            {
                return (Info) value;
            }

            return default;
        }

        /// <summary>
        /// Answer myself after registering the <see cref="Info{T}"/>.
        /// </summary>
        /// <param name="info"><see cref="Info{T}"/> to register</param>
        /// <returns>The registry</returns>
        public SourcedTypeRegistry Register(Info info)
        {
            _stores.AddOrUpdate(info.SourcedType, info, (type, o) => info);
            return this;
        }
    }
}