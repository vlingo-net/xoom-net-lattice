// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Symbio;

namespace Vlingo.Xoom.Lattice.Exchange.Feed
{
    /// <summary>
    /// A message that is provided through a feed with a <code>body</code> that must be a <see cref="ISource"/>.
    /// </summary>
    public class FeedMessage
    {
        /// <summary>
        /// Gets the message source body payload
        /// </summary>
        public FeedMessageBody Body { get; }
        
        /// <summary>
        /// Gets the unique id of the this message
        /// </summary>
        public string FeedMessageId { get; }
        
        /// <summary>
        /// Gets the type of this message
        /// </summary>
        public string TypeName { get; }
        
        /// <summary>
        /// Gets the version of the source type
        /// </summary>
        public int Version { get; }

        /// <summary>
        /// Gets a new <see cref="FeedMessage"/> with the given properties.
        /// </summary>
        /// <param name="feedMessageId">The id to assign to this message</param>
        /// <param name="body">The payload</param>
        /// <param name="typeName">The the name of the type of message</param>
        /// <param name="typeVersion">The version of the type of message</param>
        /// <returns><see cref="FeedMessage"/></returns>
        public static FeedMessage With(string feedMessageId, FeedMessageBody body, string typeName, int typeVersion) =>
            new FeedMessage(feedMessageId, body, typeName, typeVersion);

        /// <summary>
        /// Construct my state with <paramref name="feedMessageId"/> and <code>source</code>.
        /// </summary>
        /// <param name="feedMessageId">The id to assign to this message</param>
        /// <param name="body">The payload</param>
        /// <param name="typeName">The the name of the type of message</param>
        /// <param name="typeVersion">The version of the type of message</param>
        /// <exception cref="ArgumentNullException"></exception>
        public FeedMessage(string feedMessageId, FeedMessageBody? body, string typeName, int typeVersion)
        {
            if (feedMessageId == null || string.IsNullOrEmpty(feedMessageId))
            {
                throw new ArgumentNullException(nameof(feedMessageId));
            }

            if (typeName == null || string.IsNullOrEmpty(typeName))
            {
                throw new ArgumentNullException(nameof(typeName));
            }
            
            FeedMessageId = feedMessageId;
            Body = body ?? throw new ArgumentNullException(nameof(body));
            TypeName = typeName;
            Version = typeVersion;
        }
    }
}