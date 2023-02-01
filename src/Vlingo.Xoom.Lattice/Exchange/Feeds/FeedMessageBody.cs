// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Xoom.Lattice.Exchange.Feeds;

/// <summary>
/// A message body that is provided by <code>body</code>.
/// </summary>
public class FeedMessageBody
{
    /// <summary>
    /// The value representation of the body
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Construct default state
    /// </summary>
    /// <param name="body">The representation to assign as my body</param>
    /// <exception cref="ArgumentNullException">If <paramref name="body"/> is null or empty</exception>
    public FeedMessageBody(string body)
    {
        if (string.IsNullOrEmpty(body))
        {
            throw new ArgumentNullException(nameof(body));
        }

        Value = body;
    }

    /// <summary>
    /// Gets a new FeedMessageBody with <paramref name="body"/>.
    /// </summary>
    /// <param name="body">The representation of the body</param>
    /// <returns><see cref="FeedMessageBody"/></returns>
    public static FeedMessageBody With(string body) => new FeedMessageBody(body);
}