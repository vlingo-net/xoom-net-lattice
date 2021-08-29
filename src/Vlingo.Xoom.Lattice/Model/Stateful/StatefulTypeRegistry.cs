// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Concurrent;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Symbio;
using Vlingo.Xoom.Symbio.Store.State;

namespace Vlingo.Xoom.Lattice.Model.Stateful
{
    /// <summary>
    /// Registry for <see cref="StatefulEntity{T}"/> types that holds the <see cref="IStateStore"/> type
    /// </summary>
    public class StatefulTypeRegistry
    {
        internal static readonly string InternalName = Guid.NewGuid().ToString();
        private readonly ConcurrentDictionary<Type, object> _stores = new ConcurrentDictionary<Type, object>();

        /// <summary>
        /// Answer a new <see cref="StatefulTypeRegistry"/> after registering all all <paramref name="types"/> with <paramref name="stateStore"/>
        /// using the default store name for each of the <paramref name="types"/>.
        /// </summary>
        /// <param name="world">The World to which I am registered.</param>
        /// <param name="stateStore"><see cref="IStateStore"/>.</param>
        /// <param name="types">The native type of states to be stored.</param>
        /// <returns>The registry</returns>
        public static StatefulTypeRegistry RegisterAll(World world, IStateStore stateStore, params Type[] types)
        {
            var registry = ResolveStatefulTypeRegistry(world);

            registry.RegisterAll(stateStore, types);

            return registry;
        }
        
        /// <summary>
        /// Resolves the <see cref="StatefulTypeRegistry"/> held by the <paramref name="world"/>.
        /// If the registry doesn't exist, a one is instantiated and registered.
        /// </summary>
        /// <param name="world">The <see cref="World"/> where the <see cref="StatefulTypeRegistry"/> is held</param>
        /// <returns><see cref="StatefulTypeRegistry"/></returns>
        public static StatefulTypeRegistry ResolveStatefulTypeRegistry(World world)
        {
            var registry = world.ResolveDynamic<StatefulTypeRegistry>(InternalName);

            if (registry != null)
            {
                return registry;
            }

            return new StatefulTypeRegistry(world);
        }

        /// <summary>
        /// Construct my default state and register me with the <see cref="World"/>.
        /// </summary>
        /// <param name="world">The World to which I am registered</param>
        public StatefulTypeRegistry(World world)
        {
            world.RegisterDynamic(InternalName, this);
            
            StateAdapterProvider.Instance(world);
        }

        /// <summary>
        /// Construct my default state and register it with the <paramref name="world"/>.
        /// </summary>
        /// <param name="world">The World to which I am registered</param>
        /// <param name="stateStore">The state store</param>
        /// <param name="types">Native type of states to be stored</param>
        public StatefulTypeRegistry(World world, IStateStore stateStore, params Type[] types) : this(world) =>
            RegisterAll(stateStore, types);

        public Info Info<T>() => Info(typeof(T));
        
        /// <summary>
        /// Answer the <see cref="Info{T}"/>.
        /// </summary>
        /// <returns><see cref="Info{T}"/></returns>
        public Info Info(Type processType)
        {
            if (_stores.TryGetValue(processType, out var value))
            {
                return (Info) value;
            }

            throw new ArgumentOutOfRangeException($"No info registered for {processType.Name}");
        }

        /// <summary>
        /// Answer myself after registering the <see cref="Info{T}"/>.
        /// </summary>
        /// <param name="info"><see cref="Info{T}"/> to register</param>
        /// <returns>The registry</returns>
        public StatefulTypeRegistry Register(Info info)
        {
            StateTypeStateStoreMap.StateTypeToStoreName(info.StoreName, info.StoreType);
            _stores.AddOrUpdate(info.StoreType, info, (type, o) => info);
            return this;
        }
        
        public void RegisterAll(IStateStore stateStore, Type[] types)
        {
            foreach (var type in types)
            {
                Register(new Info(stateStore, type, type.Name));
            }
        }
    }
}