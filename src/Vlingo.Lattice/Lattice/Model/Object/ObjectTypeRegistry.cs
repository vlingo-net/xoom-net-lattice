// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Vlingo.Symbio.Store;
using Vlingo.Symbio.Store.Object;
using Vlingo.Xoom.Actors;

namespace Vlingo.Lattice.Model.Object
{
    /// <summary>
    /// Registry for <code>object</code> types are stored in an <see cref="IObjectStore"/>, using
    /// persistent <see cref="StateObjectMapper"/> for round trip mapping and <see cref="QueryExpression"/>
    /// single instance retrieval.
    /// </summary>
    public class ObjectTypeRegistry
    {
        internal static readonly string InternalName = Guid.NewGuid().ToString();
        private readonly Dictionary<Type, object> _stores = new Dictionary<Type, object>();

        /// <summary>
        /// Construct my default state and register me with the <paramref name="world"/>.
        /// </summary>
        /// <param name="world">The World to which I am registered</param>
        public ObjectTypeRegistry(World world) => world.RegisterDynamic(InternalName, this);

        /// <summary>
        /// Answer the <see cref="Info{TState}"/>.
        /// </summary>
        /// <returns><see cref="Info{TState}"/></returns>
        public Info<T> Info<T>() => (Info<T>)_stores[typeof(T)];
        
        /// <summary>
        /// Gets the same instance of registry after registering the <see cref="T:Info{TState}"/>.
        /// </summary>
        /// <param name="info"><see cref="T:Info{TState}"/> to register</param>
        /// <typeparam name="T">The type of the registration info</typeparam>
        /// <returns>The same instance of <see cref="ObjectTypeRegistry"/></returns>
        public ObjectTypeRegistry Register<T>(Info<T> info)
        {
            if (!_stores.ContainsKey(typeof(T)))
            {
                _stores.Add(typeof(T), info);   
            }
            
            return this;
        }
    }
}