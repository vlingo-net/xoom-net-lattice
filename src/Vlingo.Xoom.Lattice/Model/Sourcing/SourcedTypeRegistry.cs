// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Concurrent;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Symbio;
using Vlingo.Xoom.Symbio.Store.Journal;
using IDispatcher = Vlingo.Xoom.Symbio.Store.Dispatch.IDispatcher;

namespace Vlingo.Xoom.Lattice.Model.Sourcing
{
    /// <summary>
    /// Registry for <see cref="Sourced{T}"/> types that holds the <see cref="Journal{T}"/> type,
    /// <see cref="EntryAdapterProvider"/>, and <see cref="StateAdapterProvider"/>.
    /// </summary>
    public class SourcedTypeRegistry
    {
        private Type? _sourcedType;
        internal static readonly string InternalName = nameof(SourcedTypeRegistry);
        private readonly ConcurrentDictionary<Type, IJournal> _journals = new ConcurrentDictionary<Type, IJournal>();
        private readonly ConcurrentDictionary<Type, object> _stores = new ConcurrentDictionary<Type, object>();

        /// <summary>
        /// Answer a new <see cref="SourcedTypeRegistry"/> with registered <paramref name="sourcedTypes"/>, creating
        /// the <see cref="IJournal{TEntry}"/> of type <typeparamref name="TEntry"/>, registering me with the <paramref name="world"/>.
        /// </summary>
        /// <param name="world">The World to which I am registered</param>
        /// <param name="dispatcher"><see cref="IDispatcher"/> of the journal.</param>
        /// <param name="sourcedTypes"><see cref="Sourced{T}"/> types of to register</param>
        /// <returns>The registry</returns>
        public static SourcedTypeRegistry RegisterAll<TActor, TEntry>(
            World world,
            IDispatcher dispatcher,
            params Type[] sourcedTypes)
            where TActor : Actor
        {
            var registry = ResolveSourcedTypeRegistry(world);

            var journal = registry.JournalOf<IJournal<TEntry>>(typeof(TActor), world, dispatcher);

            registry.RegisterAll(journal, sourcedTypes);

            return registry;
        }
        
        /// <summary>
        /// Answer a new <see cref="SourcedTypeRegistry"/> with registered <paramref name="sourcedTypes"/>, creating
        /// the <see cref="IJournal{TEntry}"/> of type <typeparamref name="TEntry"/>, registering me with the <paramref name="world"/>.
        /// </summary>
        /// <remarks>
        /// Register() is an alias for RegisterAll()
        /// </remarks>
        /// <param name="world">The World to which I am registered</param>
        /// <param name="dispatcher"><see cref="IDispatcher"/> of the journal.</param>
        /// <param name="sourcedTypes"><see cref="Sourced{T}"/> types of to register</param>
        /// <returns>The registry</returns>
        public static SourcedTypeRegistry Register<TActor, TEntry>(
            World world,
            IDispatcher dispatcher,
            params Type[] sourcedTypes)
            where TActor : Actor =>
            RegisterAll<TActor, TEntry>(world, dispatcher, sourcedTypes);

        /// <summary>
        /// Resolves the <see cref="SourcedTypeRegistry"/> held by the <paramref name="world"/>.
        /// </summary>
        /// <param name="world">The <see cref="World"/> where the <see cref="SourcedTypeRegistry"/> is held</param>
        /// <returns>The <see cref="SourcedTypeRegistry"/></returns>
        public static SourcedTypeRegistry ResolveSourcedTypeRegistry(World world)
        {
            var registry = world.ResolveDynamic<SourcedTypeRegistry>(InternalName);

            if (registry != null)
            {
                return registry;
            }

            return new SourcedTypeRegistry(world);
        }

        /// <summary>
        /// Construct my default state with <paramref name="sourcedTypes"/> creating the <see cref="IJournal{T}"/>
        /// and register me with the <paramref name="world"/>.
        /// </summary>
        /// <param name="world">The World to which I am registered</param>
        /// <param name="journal">The journal of this registry</param>
        /// <param name="sourcedTypes"><see cref="Sourced{T}"/> types of to register</param>
        public SourcedTypeRegistry(World world, IJournal journal, params Type[] sourcedTypes) : this (world) =>
            RegisterAll(journal, sourcedTypes);

        /// <summary>
        /// Construct my default state and register me with the <see cref="World"/>.
        /// </summary>
        /// <param name="world">The World to which I am registered</param>
        public SourcedTypeRegistry(World world)
        {
            world.RegisterDynamic(InternalName, this);

            EntryAdapterProvider.Instance(world);
        }

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
        /// Resolves the <see cref="IJournal"/> of the registered <paramref name="journalType"/>
        /// or a new <see cref="IJournal"/> if non-existing.
        /// </summary>
        /// <param name="journalType">The concrete <see cref="Actor"/> type of the Journal to create</param>
        /// <param name="world">The <see cref="World"/> to which journal is registered</param>
        /// <param name="dispatcher">The <see cref="IDispatcher"/> of the <paramref name="journalType"/></param>
        /// <returns><see cref="IJournal"/></returns>
        public IJournal JournalOf<TEntry>(Type journalType, World world, IDispatcher dispatcher)
        {
            foreach (var actorType in _journals.Keys)
            {
                if (actorType == journalType)
                {
                    return _journals[actorType];
                }
            }

            var journal = world.ActorFor<IJournal<TEntry>>(journalType, dispatcher);

            _journals.TryAdd(journalType, journal);

            return journal;
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
        
        public void RegisterAll(IJournal journal, Type[] sourcedTypes)
        {
            foreach (var sourcedType in sourcedTypes)
            {
                Register(Sourcing.Info.RegisterSourced(journal, sourcedType));
            }
        }
    }
}