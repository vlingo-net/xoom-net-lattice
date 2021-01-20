// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Concurrent;
using Vlingo.Actors;

namespace Vlingo.Lattice.Model.Process
{
    /// <summary>
    /// Registry for <see cref="IProcess{TState}"/> types.
    /// </summary>
    public class ProcessTypeRegistry<T>
    {
        internal static readonly string InternalName = Guid.NewGuid().ToString();
        private readonly ConcurrentDictionary<Type, object> _stores = new ConcurrentDictionary<Type, object>();

        /// <summary>
        /// Construct my default state and register me with the <see cref="World"/>.
        /// </summary>
        /// <param name="world">The World to which I am registered</param>
        public ProcessTypeRegistry(World world) => world.RegisterDynamic(InternalName, this);
        
        /// <summary>
        /// Answer the <see cref="Info{T}"/> of the <typeparamref name="T"/> type.
        /// </summary>
        /// <returns><see cref="Info{T}"/></returns>
        public Info<T> Info()
        {
            if (_stores.TryGetValue(typeof(T), out var value))
            {
                return (Info<T>) value;
            }

            throw new ArgumentOutOfRangeException($"No info registered fro {typeof(T).Name}");
        }

        /// <summary>
        /// Answer myself after registering the <see cref="Info{T}"/>.
        /// </summary>
        /// <param name="info"><see cref="Info{T}"/> to register</param>
        /// <returns>The registry</returns>
        public ProcessTypeRegistry<T> Register(Info<T> info)
        {
            _stores.AddOrUpdate(info.ProcessType, info, (type, o) => info);
            return this;
        }
    }
}