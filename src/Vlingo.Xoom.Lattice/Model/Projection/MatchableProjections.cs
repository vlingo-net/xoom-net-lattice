// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using System.Linq;

namespace Vlingo.Xoom.Lattice.Model.Projection;

/// <summary>
/// Maintains any number of <see cref="IProjection"/>s that are used to project
/// when an <code>actualCauses</code> happens. Supports simple wildcards.
/// </summary>
public class MatchableProjections
{
    private const string Wildcard = "*";
    private readonly Dictionary<Cause, List<IProjection>> _mappedProjections = new Dictionary<Cause, List<IProjection>>();

    /// <summary>
    /// Gets the <see cref="IEnumerable{IProjection}"/> matching the <paramref name="actualCauses"/> requiring projection(s).
    /// </summary>
    /// <param name="actualCauses">The string describing why any number of <see cref="IProjection"/>s are required.</param>
    /// <returns>Matching <see cref="IProjection"/>s for <paramref name="actualCauses"/></returns>
    public IEnumerable<IProjection> MatchProjections(params string[] actualCauses) => 
        _mappedProjections.Keys.Where(c => c.Matches(actualCauses)).SelectMany(c => _mappedProjections[c]);

    /// <summary>
    /// Registers all causes, which may be combined and separated by semicolons (;) to
    /// the projection such that later matches will be dispatched to the projection.
    /// For example: string whenMatchingCauses = "User:new;User:contact;User:name";
    /// </summary>
    /// <param name="projection">The <see cref="IProjection"/> to which matches are dispatched</param>
    /// <param name="whenMatchingCauses">The array with one or more cause patterns separated by semicolons.</param>
    public void MayDispatchTo(IProjection projection, string[] whenMatchingCauses)
    {
        foreach (var whenMatchingCause in whenMatchingCauses)
        {
            var cause = Cause.DetermineFor(whenMatchingCause);

            if (!_mappedProjections.TryGetValue(cause, out var projections))
            {
                projections = new List<IProjection>();
                _mappedProjections.Add(cause, projections);
            }

            projections.Add(projection);
        }
    }

    /// <summary>
    /// Abstract base for kinds of wildcard matching of causes.
    /// </summary>
    private abstract class Cause
    {
        protected readonly string Value;
            
        /// <summary>
        /// Gets a <see cref="Cause"/>
        /// </summary>
        /// <param name="matchableCause">Pattern for clause to match.</param>
        /// <returns>Returns a concrete implementation of <see cref="Cause"/> handling specific scenario</returns>
        public static Cause DetermineFor(string matchableCause)
        {
            var beginsWithWildcard = matchableCause.StartsWith(Wildcard);
            var endsWithWildcard = matchableCause.EndsWith(Wildcard);

            if (beginsWithWildcard && endsWithWildcard) return new ContainsCause(matchableCause);
            if (beginsWithWildcard) return new EndsWithCause(matchableCause);
            if (endsWithWildcard) return new BeginsWithCause(matchableCause);
            return new EntireCause(matchableCause);
        }

        /// <summary>
        /// Construct my default state.
        /// </summary>
        /// <param name="value">The string segment to match on, which may specify one or more wildcards</param>
        public Cause(string value) => Value = value.Replace("*", "");
            
        /// <summary>
        /// Answer whether or not I match the <paramref name="actualCauses"/>.
        /// </summary>
        /// <param name="actualCauses">The text strings describing the cause to match on</param>
        /// <returns>True if cause match the actual causes.</returns>
        public abstract bool Matches(params string[] actualCauses);

        public override bool Equals(object? obj)
        {
            if (obj == null || obj.GetType() != GetType()) return false;
            return Value.Equals(((Cause) obj).Value);
        }

        public override int GetHashCode() => 31 * Value.GetHashCode();

        public override string ToString() => $"{GetType().Name}[value={Value}]";
    }
        
    /// <summary>
    /// A cause that matches on the beginning of a description value.
    /// </summary>
    private class BeginsWithCause : Cause
    {
        public BeginsWithCause(string value) : base(value)
        {
        }

        public override bool Matches(params string[] actualCauses)
        {
            foreach (var actualCause in actualCauses)
            {
                if (actualCause.StartsWith(Value))
                {
                    return true;
                }
            }
                
            return false;
        }
    }
        
    /// <summary>
    /// A cause that matches on any segment of a description value.
    /// </summary>
    private class ContainsCause : Cause
    {
        public ContainsCause(string value) : base(value)
        {
        }

        public override bool Matches(params string[] actualCauses)
        {
            foreach (var actualCause in actualCauses)
            {
                if (actualCause.Contains(Value))
                {
                    return true;
                }
            }
                
            return false;
        }
    }
        
    /// <summary>
    /// A cause that matches on the end of a description value.
    /// </summary>
    private class EndsWithCause : Cause
    {
        public EndsWithCause(string value) : base(value)
        {
        }

        public override bool Matches(params string[] actualCauses)
        {
            foreach (var actualCause in actualCauses)
            {
                if (actualCause.EndsWith(Value))
                {
                    return true;
                }
            }
                
            return false;
        }
    }
        
    /// <summary>
    /// A cause that matches exactly a description value.
    /// </summary>
    private class EntireCause : Cause
    {
        public EntireCause(string value) : base(value)
        {
        }

        public override bool Matches(params string[] actualCauses)
        {
            foreach (var actualCause in actualCauses)
            {
                if (actualCause.Equals(Value))
                {
                    return true;
                }
            }
                
            return false;
        }
    }
}