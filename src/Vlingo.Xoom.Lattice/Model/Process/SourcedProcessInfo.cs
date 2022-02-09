// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Lattice.Exchange;
using Vlingo.Xoom.Lattice.Model.Sourcing;

namespace Vlingo.Xoom.Lattice.Model.Process
{
    /// <summary>
    /// Holder of registration information for <see cref="SourcedProcess{T}"/> types.
    /// </summary>
    /// <typeparam name="TProcess">The type of sourced process</typeparam>
    /// <typeparam name="TState">The type of the state of the sourced process</typeparam>
    public class SourcedProcessInfo<TProcess, TState> : Info where TProcess : IProcess<TState>
    {
        public SourcedTypeRegistry Registry { get; }

        /// <summary>
        /// Construct my default state.
        /// </summary>
        /// <param name="processName">The name of the current process</param>
        /// <param name="exchange">The <see cref="IExchange"/></param>
        /// <param name="registry">The <see cref="SourcedTypeRegistry"/> used by the <see cref="SourcedProcess{T}"/></param>
        public SourcedProcessInfo(string processName, IExchange exchange, SourcedTypeRegistry registry) : base(typeof(TProcess), processName, exchange)
            => Registry = registry;

        /// <summary>
        /// Construct my default state.
        /// </summary>
        /// <param name="processName">The name of the current process</param>
        /// <param name="registry">The <see cref="SourcedTypeRegistry"/> used by the <see cref="SourcedProcess{T}"/></param>
        public SourcedProcessInfo(string processName, SourcedTypeRegistry registry) : base(typeof(TProcess), processName) => Registry = registry;
    }
}