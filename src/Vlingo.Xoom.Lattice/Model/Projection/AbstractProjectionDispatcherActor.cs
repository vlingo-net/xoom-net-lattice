// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using System.Linq;
using Vlingo.Xoom.Actors;

namespace Vlingo.Xoom.Lattice.Model.Projection
{
    /// <summary>
    /// Abstract base of all <see cref="IProjectionDispatcher"/> types and that
    /// holds the pool of <see cref="IProjection"/> instances that are used to
    /// project <see cref="IProjectable"/> states based on <see cref="MatchableProjections"/>.
    /// </summary>
    public class AbstractProjectionDispatcherActor : Actor, IProjectionDispatcher
    {
        private readonly MatchableProjections _matchableProjections;

        /// <summary>
        /// Construct my default state.
        /// </summary>
        public AbstractProjectionDispatcherActor() => _matchableProjections = new MatchableProjections();

        /// <summary>
        /// Construct my default state with <param name="projectToDescriptions"></param>.
        /// </summary>
        /// <param name="projectToDescriptions">The <see cref="IEnumerable{T}"/> describing my matchable projections</param>
        public AbstractProjectionDispatcherActor(IEnumerable<ProjectToDescription> projectToDescriptions) : this()
        {
            foreach (var projectToDescription in projectToDescriptions)
            {
                var projection = Stage.ActorFor(projectToDescription.ProjectionDefinition);
                ProjectTo(projection, projectToDescription.BecauseOf);
            }
        }
        
        //=====================================
        // ProjectionDispatcher
        //=====================================

        public virtual void ProjectTo(IProjection projection, string[] whenMatchingCause) =>
            _matchableProjections.MayDispatchTo(projection, whenMatchingCause);
        
        //=====================================
        // internal implementation
        //=====================================
        
        /// <summary>
        /// Gets true whether or not I have any <see cref="IProjection"/> that supports the <paramref name="actualCause"/>.
        /// </summary>
        /// <param name="actualCause">The description of the cause that requires <see cref="IProjection"/></param>
        /// <returns>True if has <see cref="IProjection"/> for <paramref name="actualCause"/> otherwise false.</returns>
        protected bool HasProjectionsFor(string actualCause) => ProjectionsFor(actualCause).Any();

        /// <summary>
        /// Gets the <see cref="IProjection"/> that match <paramref name="actualCauses"/>.
        /// </summary>
        /// <param name="actualCauses">Descriptions of the cause that requires <see cref="IProjection"/></param>
        /// <returns>All <see cref="IProjection"/>s matching <paramref name="actualCauses"/></returns>
        protected IEnumerable<IProjection> ProjectionsFor(params string[] actualCauses) => 
            _matchableProjections.MatchProjections(actualCauses);
    }
}