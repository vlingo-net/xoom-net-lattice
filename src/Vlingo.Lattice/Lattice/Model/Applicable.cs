// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using Vlingo.Actors;
using Vlingo.Symbio;

namespace Vlingo.Lattice.Model
{
    /// <summary>
    /// The elements used in the attempted <code>Apply()</code>
    /// </summary>
    /// <typeparam name="T">The type of the state</typeparam>
    public class Applicable<T>
    {
        public Applicable(T state, IEnumerable<Source> sources, Metadata metadata, CompletionSupplier<T> completionSupplier)
        {
            State = state;
            Sources = sources;
            Metadata = metadata;
            CompletionSupplier = completionSupplier;
        }
        
        public CompletionSupplier<T> CompletionSupplier { get; }
        
        public Metadata Metadata { get; }
        
        public IEnumerable<Source> Sources { get; }
        
        public T State { get; }
    }
}