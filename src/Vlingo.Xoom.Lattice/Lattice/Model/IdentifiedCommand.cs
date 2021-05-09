// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Xoom.Lattice.Model
{
    /// <summary>
    /// Provides the means to request the identity of the <see cref="Command"/>.
    /// </summary>
    public abstract class IdentifiedCommand : Command
    {
        /// <summary>
        /// Construct my default state with a type version of 1.
        /// </summary>
        protected IdentifiedCommand()
        {
        }
        
        /// <summary>
        /// Construct my default state with a <paramref name="commandTypeVersion"/> greater than 1.
        /// </summary>
        /// <param name="commandTypeVersion">The int version of this command type</param>
        protected IdentifiedCommand(int commandTypeVersion) : base(commandTypeVersion)
        {
        }

        /// <summary>
        /// Gets the <code>string</code> identity of this <see cref="Command"/>.
        /// </summary>
        public abstract string Identity { get; }
    }
}