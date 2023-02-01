// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Xoom.Lattice.Model;

/// <summary>
/// Provides the means to request the identity of the <see cref="DomainEvent"/>.
/// </summary>
public abstract class IdentifiedDomainEvent : DomainEvent
{
    /// <summary>
    /// Construct my default state with a type version of 1.
    /// </summary>
    protected IdentifiedDomainEvent()
    {
    }
        
    /// <summary>
    /// Construct my default state with a <paramref name="eventTypeVersion"/> greater than 1.
    /// </summary>
    /// <param name="eventTypeVersion">The int version of this command type</param>
    protected IdentifiedDomainEvent(int eventTypeVersion) : base(eventTypeVersion)
    {
    }
        
    /// <summary>
    /// Gets the <code>string</code> identity of this <see cref="DomainEvent"/>.
    /// </summary>
    public abstract string Identity { get; }

    /// <summary>
    /// Gets the <code>string</code> identity of this <see cref="DomainEvent"/>.
    /// Must be overridden for supplying meaningful values.
    /// </summary>
    public virtual string ParentIdentity => string.Empty;
}