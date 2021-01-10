// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Symbio;

namespace Vlingo.Lattice.Model
{
    /// <summary>
    /// An Exception used to indicate the failure of an attempt to <code>Apply()</code>
    /// state and/or <see cref="Source"/> instances.
    /// </summary>
    /// <typeparam name="T">The type of the state</typeparam>
    [Serializable]
    public class ApplyFailedException<T> : Exception
    {
        public ApplyFailedException(Applicable<T> applicable) => Applicable = applicable;

        public ApplyFailedException(Applicable<T> applicable, string? message) : base(message) => Applicable = applicable;

        public ApplyFailedException(Applicable<T> applicable, string? message, Exception? innerException) : base(message, innerException) => Applicable = applicable;

        public Applicable<T> Applicable { get; }
    }
}