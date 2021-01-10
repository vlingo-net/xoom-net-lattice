// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Vlingo.Actors;
using Vlingo.Symbio.Store;
using Vlingo.Symbio.Store.Object;

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
        private readonly Dictionary<Type, Info> _stores = new Dictionary<Type, Info>();

        /// <summary>
        /// Construct my default state and register me with the <paramref name="world"/>.
        /// </summary>
        /// <param name="world">The World to which I am registered</param>
        public ObjectTypeRegistry(World world) => world.RegisterDynamic(InternalName, this);

        /// <summary>
        /// Answer the <see cref="Info"/>.
        /// </summary>
        /// <returns><see cref="Info"/></returns>
        public Info Info(Type type) => _stores[type];
        
        public ObjectTypeRegistry Register<T>(Info info)
        {
            _stores.Add(typeof(T), info);
            return this;
        }
    }
}