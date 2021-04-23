// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Vlingo.Symbio;
using Vlingo.Xoom.Actors;

namespace Vlingo.Lattice.Model.Projection
{
    public abstract class ProjectToDescription
    {
        private readonly List<string> _becauseOf;
        private readonly List<Type> _becauseOfType;
        public string[] BecauseOf => _becauseOf.ToArray();
        public IEnumerable<Type> BecauseOfType => _becauseOfType;
        public Expression<Func<IProjection>> ProjectionDefinition { get; }

        public ProjectToDescription(Expression<Func<IProjection>> projectionDefinition, params string[] becauseOf)
        {
            ProjectionDefinition = projectionDefinition;
            _becauseOf = new List<string>(becauseOf);
            _becauseOfType = new List<Type>();
        }
        
        public ProjectToDescription(Expression<Func<IProjection>> projectionDefinition, Type becauseOf)
        {
            ProjectionDefinition = projectionDefinition;
            _becauseOf = new List<string> { (becauseOf.AssemblyQualifiedName ?? becauseOf.FullName) ?? becauseOf.Name};
            _becauseOfType = new List<Type> {becauseOf};
        }

        public ProjectToDescription AndWith(Assembly assembly)
        {
            var sourceType = typeof(ISource);
            var sources = assembly.GetTypes().Where(t => sourceType.IsAssignableFrom(sourceType)).ToList();
            _becauseOf.AddRange(sources.Select(s => (s.AssemblyQualifiedName ?? s.FullName) ?? s.Name));
            _becauseOfType.AddRange(sources);
            return this;
        }
        
        public ProjectToDescription AndWith<TSource>() where TSource : ISource
        {
            var type = typeof(TSource);
            _becauseOf.Add((type.AssemblyQualifiedName ?? type.FullName) ?? type.Name);
            _becauseOfType.Add(type);
            return this;
        }
    }
    
    /// <summary>
    /// Declares the projection type that is dispatched for a given set of causes/reasons.
    /// </summary>
    public class ProjectToDescription<TActor> : ProjectToDescription where TActor : Actor, IProjection
    {
        /// <summary>
        /// Gets a new <see cref="ProjectToDescription{TActor}"/> with <paramref name="projectionDefinition"/> for matches on types in <code>BecauseOf</code>.
        /// </summary>
        /// <param name="projectionDefinition"></param>
        /// <returns>A new <see cref="ProjectToDescription{TActor}"/></returns>
        public static ProjectToDescription<TActor> With<TSource>(Expression<Func<IProjection>> projectionDefinition)
            where TSource : ISource =>
            new ProjectToDescription<TActor>(projectionDefinition, typeof(TSource));
        
        public static ProjectToDescription<TActor> With(Expression<Func<IProjection>> projectionDefinition, params string[] becauseOf) =>
            new ProjectToDescription<TActor>(projectionDefinition, becauseOf);

        /// <summary>
        /// Construct my default state.
        /// </summary>
        /// <param name="projectionDefinition">The expression tree of the projection that must be an Actor</param>
        /// <param name="becauseOf">The array causes/reasons that the projectionType handles</param>
        private ProjectToDescription(Expression<Func<IProjection>> projectionDefinition, Type becauseOf) : base(projectionDefinition, becauseOf)
        {
        }
        
        private ProjectToDescription(Expression<Func<IProjection>> projectionDefinition, params string[] becauseOf) : base(projectionDefinition, becauseOf)
        {
        }
    }
}