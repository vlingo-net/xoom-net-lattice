// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Xoom.Lattice.Model.Projection;

/// <summary>
/// Defines the means of dispatching <see cref="IProjectable"/> instances to <see cref="IProjection"/>s
/// based on matching the <see cref="IProjection"/>s that handle descriptive causes.
/// </summary>
public interface IProjectionDispatcher
{
    /// <summary>
    /// Use the <paramref name="projection"/> to project a given <see cref="IProjectable"/> state when <paramref name="becauseOf"/> is matched
    /// with the reasons.
    /// </summary>
    /// <param name="projection">The <see cref="IProjection"/> that may be used.</param>
    /// <param name="becauseOf">the array holding one or more reasons that projection is required.</param>
    void ProjectTo(IProjection projection, string[] becauseOf);
}