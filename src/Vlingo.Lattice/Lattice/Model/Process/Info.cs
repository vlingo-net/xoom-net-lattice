// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Lattice.Exchange;

namespace Vlingo.Lattice.Model.Process
{
    /// <summary>
    /// Holder of registration information.
    /// </summary>
    /// <typeparam name="T">The type of Process of the registration</typeparam>
    public abstract class Info<T>
    {
        public IExchange Exchange { get; }
        public string ProcessName { get; }

        public Type ProcessType => typeof(T);

        /// <summary>
        /// Construct my default state.
        /// </summary>
        /// <param name="processName">The string name of the Process</param>
        /// <param name="exchange">The exchange</param>
        public Info(string processName, IExchange exchange)
        {
            ProcessName = processName;
            Exchange = exchange;
        }

        /// <summary>
        /// Construct my default state.
        /// </summary>
        /// <param name="processName">The string name of the Process</param>
        public Info(string processName) : this(processName, NullExchange.Instance)
        {
        }
    }
}