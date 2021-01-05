// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Symbio.Store.Object;

namespace Vlingo.Lattice.Lattice.Model.Object
{
    /// <summary>
    /// Holder of registration information.
    /// </summary>
    public class Info
    {
        /// <summary>
        /// Construct my default state.
        /// </summary>
        /// <param name="store">The instance of <see cref="IObjectStore"/></param>
        /// <param name="storeName">The name of the object store</param>
        /// <param name="queryObjectExpression">The <see cref="QueryExpression"/> used to retrieve a single instance</param>
        /// <param name="mapper">The persistent <see cref="StateObjectMapper"/> between Object type and persistent type</param>
        public Info(IObjectStore store, string storeName, QueryExpression queryObjectExpression, StateObjectMapper mapper)
        {
            Store = store;
            StoreName = storeName;
            QueryObjectExpression = queryObjectExpression;
            Mapper = mapper;
        }
        
        public StateObjectMapper Mapper { get; }

        public QueryExpression QueryObjectExpression { get; }

        public IObjectStore Store { get; }

        public string StoreName { get; }
    }
}