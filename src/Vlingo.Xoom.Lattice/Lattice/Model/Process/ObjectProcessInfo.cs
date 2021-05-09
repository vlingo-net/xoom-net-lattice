// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Lattice.Exchange;
using Vlingo.Xoom.Lattice.Model.Object;

namespace Vlingo.Xoom.Lattice.Model.Process
{
    /// <summary>
    /// Holder of registration information for <see cref="ObjectProcess{T}"/> types.
    /// </summary>
    /// <typeparam name="TProcess">The type of the process</typeparam>
    /// <typeparam name="TState">The Type of the state for the process</typeparam>
    public class ObjectProcessInfo<TProcess, TState> : Info where TProcess : IProcess<TState>
    {
        public ObjectTypeRegistry Registry { get; }
        
        /// <summary>
        /// Construct my default state.
        /// </summary>
        /// <param name="processName">The name of the current process</param>
        /// <param name="exchange">The <see cref="IExchange"/></param>
        /// <param name="registry">The <see cref="ObjectTypeRegistry"/> used by the <see cref="ObjectProcess{T}"/></param>
        public ObjectProcessInfo(string processName, IExchange exchange, ObjectTypeRegistry registry) : base(typeof(TProcess), processName, exchange)
            => Registry = registry;

        /// <summary>
        /// Construct my default state.
        /// </summary>
        /// <param name="processName">The name of the current process</param>
        /// <param name="registry">The <see cref="ObjectTypeRegistry"/> used by the <see cref="ObjectProcess{T}"/></param>
        public ObjectProcessInfo(string processName, ObjectTypeRegistry registry) : base(typeof(TProcess), processName) => Registry = registry;
    }
}