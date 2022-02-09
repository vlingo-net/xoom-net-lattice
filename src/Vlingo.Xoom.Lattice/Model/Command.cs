// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Symbio;

namespace Vlingo.Xoom.Lattice.Model
{
    /// <summary>
    /// A abstract base for commands, which are considered a type of <see cref="Source{T}"/>.
    /// </summary>
    public abstract class Command : Source<Command>
    {
        /// <summary>
        /// Construct my default state with a type version of 1.
        /// </summary>
        protected Command()
        {
        }
        
        /// <summary>
        /// Construct my default state with a <paramref name="commandTypeVersion"/> greater than 1.
        /// </summary>
        /// <param name="commandTypeVersion">The int version of this command type</param>
        protected Command(int commandTypeVersion) : base(commandTypeVersion)
        {
        }
    }
}