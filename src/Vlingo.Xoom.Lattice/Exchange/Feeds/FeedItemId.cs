// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Xoom.Lattice.Exchange.Feeds;

/// <summary>
///     The identity of a <see cref="FeedItem" />.
/// </summary>
public sealed class FeedItemId
{
    internal static readonly FeedItemId Unknown = new FeedItemId("");

    private readonly string _value;

    /// <summary>
    ///     Construct my state.
    /// </summary>
    /// <param name="id">The identity to assign as internal value</param>
    public FeedItemId(long id) => _value = id.ToString();

    /// <summary>
    ///     Construct my state.
    /// </summary>
    /// <param name="id">The identity to assign as internal value</param>
    public FeedItemId(string id) => _value = id;

    /// <summary>
    ///     Gets a new <see cref="FeedItemId"/> having <paramref name="id"/> as its <code>value</code>.
    /// </summary>
    /// <param name="id">The identity to assign as internal value</param>
    /// <returns><see cref="FeedItemId"/></returns>
    public static FeedItemId With(long id) => new FeedItemId(id);

    /// <summary>
    ///     Gets a copy of me; a new <see cref="FeedItemId"/> with my <code>value</code>.
    /// </summary>
    /// <returns><see cref="FeedItemId"/></returns>
    public FeedItemId Copy() => new FeedItemId(_value);

    /// <summary>
    ///     Gets my <code>value</code> as a <code>long</code>.
    /// </summary>
    /// <returns><see cref="FeedItemId"/></returns>
    public long ToLong() => long.Parse(_value);

    /// <summary>
    ///     Gets the next identity.
    /// </summary>
    /// <returns><see cref="FeedItemId"/></returns>
    public FeedItemId Next() => new FeedItemId(ToLong() + 1L);

    /// <summary>
    ///     Gets whether or not there is a previous identity.
    /// </summary>
    public bool HasPrevious => ToLong() > 0;

    /// <summary>
    ///     Gets the previous identity.
    /// </summary>
    /// <returns><see cref="FeedItemId"/></returns>
    public FeedItemId Previous()
    {
        var id = ToLong();
        if (id == 0)
        {
            throw new InvalidOperationException("No previous identity.");
        }

        return new FeedItemId(id - 1L);
    }

    /// <summary>
    ///     Gets whether or not I am the unknown identity.
    /// </summary>
    /// <returns><code>true</code> if id is unknown</returns>
    public bool IsUnknown => Equals(Unknown);

    public override int GetHashCode() => 31 * _value.GetHashCode();

    public override bool Equals(object? obj)
    {
        if (this == obj)
        {
            return true;
        }

        if (obj == null || obj.GetType() != typeof(FeedItemId))
        {
            return false;
        }

        return _value.Equals(((FeedItemId) obj)._value);
    }

    public override string ToString() => $"FeedItemId[value={_value}]";
}