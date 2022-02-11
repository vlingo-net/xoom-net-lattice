// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Xoom.Lattice.Model.Projection;

/// <summary>
/// Defines the basic protocol for all <code>Projection</code> types.
/// </summary>
public interface IProjection
{
    /// <summary>
    /// Project the given <see cref="IProjectable"/> that is managed by the given <see cref="IProjectionControl"/>.
    /// </summary>
    /// <param name="projectable">The <see cref="IProjectable"/> to project</param>
    /// <param name="control">Control the <see cref="IProjectionControl"/> that manages the results of the <see cref="IProjectable"/></param>
    void ProjectWith(IProjectable projectable, IProjectionControl control);
}