// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Lattice.Model.Projection
{
    /// <summary>
    /// Defines the control management for a given <see cref="IProjection"/>.
    /// </summary>
    public interface IProjectionControl
    {
        /// <summary>
        /// Gets the <see cref="Confirmer"/> for the given <see cref="IProjectable"/> through
        /// which confirmation can be performed as a single operation.
        /// </summary>
        /// <param name="projectable">The <see cref="IProjectable"/> requiring confirmation of completed projections operations</param>
        /// <param name="control">The <see cref="IProjectionControl"/></param>
        /// <returns>The <see cref="Confirmer"/></returns>
        Action ConfirmerFor(IProjectable projectable, IProjectionControl control);
        
        /// <summary>
        /// Confirms that all projection operations have been completed.
        /// </summary>
        /// <param name="projectionId">the string unique identity of the projection operation.</param>
        void ConfirmProjected(string projectionId);
    }

    public static class ProjectionControlExtensions
    {
        public static Confirmer ConfirmerFor(this IProjectionControl projectionControl, IProjectable projectable, IProjectionControl control) 
            => new Confirmer(() => control.ConfirmProjected(projectable.ProjectionId));
    }
}