// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Lattice.Exchange;
using Vlingo.Lattice.Model.Sourcing;

namespace Vlingo.Lattice.Model.Process
{
    /// <summary>
    /// Holder of registration information for <see cref="SourcedProcess{T}"/> types.
    /// </summary>
    /// <typeparam name="T">The type of sourced processes</typeparam>
    public class SourcedProcessInfo<T> : Info<T> where T : IProcess<T> 
    {
        public SourcedTypeRegistry Registry { get; }

        /// <summary>
        /// Construct my default state.
        /// </summary>
        /// <param name="processName">The name of the current process</param>
        /// <param name="exchange">The <see cref="IExchange"/></param>
        /// <param name="registry">The <see cref="SourcedTypeRegistry{T}"/> used by the <see cref="SourcedProcess{T}"/></param>
        public SourcedProcessInfo(string processName, IExchange exchange, SourcedTypeRegistry registry) : base(processName, exchange)
            => Registry = registry;

        /// <summary>
        /// Construct my default state.
        /// </summary>
        /// <param name="processName">The name of the current process</param>
        /// <param name="registry">The <see cref="SourcedTypeRegistry{T}"/> used by the <see cref="SourcedProcess{T}"/></param>
        public SourcedProcessInfo(string processName, SourcedTypeRegistry registry) : base(processName) => Registry = registry;
    }
}