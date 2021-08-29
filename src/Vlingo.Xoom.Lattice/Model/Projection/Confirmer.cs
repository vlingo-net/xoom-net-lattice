// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Xoom.Lattice.Model.Projection
{
    /// <summary>
    /// Defines the functional interface used to confirm the completion of projections operations.
    /// </summary>
    public sealed class Confirmer
    {
        private readonly Action _toConfirm;

        public Confirmer(Action toConfirm) => _toConfirm = toConfirm;

        /// <summary>
        /// Confirms the completion of projections operations.
        /// </summary>
        public void Confirm() => _toConfirm();
    }
}