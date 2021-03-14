// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Lattice.Exchange;
using Vlingo.Lattice.Model.Stateful;

namespace Vlingo.Lattice.Model.Process
{
    /// <summary>
    /// Holder of registration information for <see cref="StatefulProcess{T}"/> types.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="StatefulProcess{T}"/> state.</typeparam>
    public class StatefulProcessInfo<T> : Info
    {
        public StatefulTypeRegistry<T> Registry { get; }

        /// <summary>
        /// Construct my default state.
        /// </summary>
        /// <param name="processName">The name of the current process</param>
        /// <param name="exchange">The <see cref="IExchange"/></param>
        /// <param name="registry">The <see cref="StatefulTypeRegistry{T}"/> used by the <see cref="StatefulProcess{T}"/></param>
        public StatefulProcessInfo(string processName, IExchange exchange, StatefulTypeRegistry<T> registry) : base(typeof(T), processName, exchange)
            => Registry = registry;

        /// <summary>
        /// Construct my default state.
        /// </summary>
        /// <param name="processName">The name of the current process</param>
        /// <param name="registry">The <see cref="StatefulTypeRegistry{T}"/> used by the <see cref="StatefulProcess{T}"/></param>
        public StatefulProcessInfo(string processName, StatefulTypeRegistry<T> registry) : base(typeof(T), processName) => Registry = registry;
    }
}