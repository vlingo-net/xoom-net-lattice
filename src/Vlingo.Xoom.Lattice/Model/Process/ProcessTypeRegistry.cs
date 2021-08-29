// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Concurrent;
using Vlingo.Xoom.Actors;

namespace Vlingo.Xoom.Lattice.Model.Process
{
    /// <summary>
    /// Registry for <see cref="IProcess{TState}"/> types.
    /// </summary>
    public class ProcessTypeRegistry
    {
        internal static readonly string InternalName = Guid.NewGuid().ToString();
        private readonly ConcurrentDictionary<Type, Info> _stores = new ConcurrentDictionary<Type, Info>();

        /// <summary>
        /// Gets the <see cref="ProcessTypeRegistry"/> held by the <paramref name="world"/>.
        /// If the registry doesn't exist, a one is instantiated and registered.
        /// </summary>
        /// <param name="world">The <see cref="World"/> where the <see cref="ProcessTypeRegistry"/> is held</param>
        /// <returns><see cref="ProcessTypeRegistry"/></returns>
        public static ProcessTypeRegistry ResolveProcessTypeRegistry(World world)
        {
            var registry = world.ResolveDynamic<ProcessTypeRegistry>(InternalName);

            if (registry != null)
            {
                return registry;
            }

            return new ProcessTypeRegistry(world);
        }
        
        /// <summary>
        /// Construct my default state and register me with the <see cref="World"/>.
        /// </summary>
        /// <param name="world">The World to which I am registered</param>
        public ProcessTypeRegistry(World world) => world.RegisterDynamic(InternalName, this);
        
        /// <summary>
        /// Answer the <see cref="Info"/>.
        /// </summary>
        /// <returns><see cref="Info"/></returns>
        public Info Info(Type processType)
        {
            if (_stores.TryGetValue(processType, out var value))
            {
                return value;
            }

            throw new ArgumentOutOfRangeException($"No info registered for {value?.ProcessType.Name}");
        }

        /// <summary>
        /// Answer myself after registering the <see cref="Info"/>.
        /// </summary>
        /// <param name="info"><see cref="Info"/> to register</param>
        /// <returns>The registry</returns>
        public ProcessTypeRegistry Register(Info info)
        {
            _stores.AddOrUpdate(info.ProcessType, info, (type, o) => info);
            return this;
        }
    }
}