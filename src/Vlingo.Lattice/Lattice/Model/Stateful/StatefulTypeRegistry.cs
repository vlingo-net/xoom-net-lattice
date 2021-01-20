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
using Vlingo.Symbio.Store.State;

namespace Vlingo.Lattice.Model.Stateful
{
    /// <summary>
    /// Registry for <see cref="StatefulEntity{T}"/> types that holds the <see cref="IStateStore{TEntry}"/> type
    /// </summary>
    public class StatefulTypeRegistry<T> where T : IEntry
    {
        internal static readonly string InternalName = Guid.NewGuid().ToString();
        private readonly ConcurrentDictionary<Type, object> _stores = new ConcurrentDictionary<Type, object>();

        /// <summary>
        /// Answer a new <see cref="StatefulTypeRegistry{T}"/> after registering all all <paramref name="types"/> with <paramref name="stateStore"/>
        /// using the default store name for each of the <paramref name="types"/>.
        /// </summary>
        /// <param name="world">The World to which I am registered.</param>
        /// <param name="stateStore"><see cref="IStateStore{T}"/>.</param>
        /// <param name="types">The native type of states to be stored.</param>
        /// <returns>The registry</returns>
        public static StatefulTypeRegistry<T> RegisterAll(World world, IStateStore<T> stateStore, params Type[] types)
        {
            var registry = new StatefulTypeRegistry<T>(world);
            
            foreach (var type in types)
            {
                registry.Register(new Info<T>(stateStore, type.Name));
            }

            return registry;
        }

        /// <summary>
        /// Construct my default state and register me with the <see cref="World"/>.
        /// </summary>
        /// <param name="world">The World to which I am registered</param>
        public StatefulTypeRegistry(World world) => world.RegisterDynamic(InternalName, this);
        
        /// <summary>
        /// Answer the <see cref="Info{T}"/> of the <typeparamref name="T"/> type.
        /// </summary>
        /// <returns><see cref="Info{T}"/></returns>
        public Info<T> Info()
        {
            if (_stores.TryGetValue(typeof(IStateStore<T>), out var value))
            {
                return (Info<T>) value;
            }

            throw new ArgumentOutOfRangeException($"No info registered fro {typeof(IStateStore<T>).Name}");
        }

        /// <summary>
        /// Answer myself after registering the <see cref="Info{T}"/>.
        /// </summary>
        /// <param name="info"><see cref="Info{T}"/> to register</param>
        /// <returns>The registry</returns>
        public StatefulTypeRegistry<T> Register(Info<T> info)
        {
            StateTypeStateStoreMap.StateTypeToStoreName(info.StoreName, info.StoreType);
            _stores.AddOrUpdate(info.StoreType, info, (type, o) => info);
            return this;
        }
    }
}