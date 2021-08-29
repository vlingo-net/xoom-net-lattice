// Copyright © 2012-2021 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Lattice.Model.Projection
{
    /// <summary>
    /// Manages the multiple confirmations of <see cref="IProjectable"/>s until the full
    /// count are virtually confirmed, after which the actual confirmation is performed.
    /// </summary>
    public interface IMultiConfirming
    {
        /// <summary>
        /// Include the <see cref="IProjectable"/> to manage its confirmations to <paramref name="count"></paramref> times
        /// and then perform the actual confirmation.
        /// </summary>
        /// <param name="projectable">the <see cref="IProjectable"/> to manage</param>
        /// <param name="count">The number of times that confirmation must occur for final confirmation</param>
        void ManageConfirmationsFor(IProjectable projectable, int count);
        
        /// <summary>
        /// Gets a list of <see cref="IProjectable"/> of managed <see cref="IProjectable"/>s within a <see cref="ICompletes"/>.
        /// </summary>
        /// <returns></returns>
        ICompletes<IEnumerable<IProjectable>> ManagedConfirmations();
    }

    public static class MultiConfirming
    {
        public static long DefaultExpirationLimit = 3000;
    }
}