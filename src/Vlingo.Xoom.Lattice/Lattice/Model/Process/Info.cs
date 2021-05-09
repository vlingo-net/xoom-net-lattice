// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Lattice.Exchange;

namespace Vlingo.Xoom.Lattice.Model.Process
{
    /// <summary>
    /// Holder of registration information.
    /// </summary>
    public abstract class Info
    {
        public IExchange Exchange { get; }
        public string ProcessName { get; }

        public Type ProcessType { get; }

        /// <summary>
        /// Construct my default state.
        /// </summary>
        /// <param name="processType">The type of the process.</param>
        /// <param name="processName">The string name of the Process</param>
        /// <param name="exchange">The exchange</param>
        public Info(Type processType, string processName, IExchange exchange)
        {
            ProcessType = processType;
            ProcessName = processName;
            Exchange = exchange;
        }

        /// <summary>
        /// Construct my default state.
        /// </summary>
        /// <param name="processType">The type of the process.</param>
        /// <param name="processName">The string name of the Process</param>
        public Info(Type processType, string processName) : this(processType, processName, NullExchange.Instance)
        {
        }
    }
}