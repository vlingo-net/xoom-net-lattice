// Copyright © 2012-2020 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using System.Text;

namespace Vlingo.Xoom.Lattice
{
    /// <summary>
    /// Composite identity support mix-in.
    /// </summary>
    public interface ICompositeIdentitySupport
    {
        /// <summary>
        /// Gets data id from the <paramref name="idSegments"/> separated by <paramref name="separator"/>.
        /// </summary>
        /// <param name="separator">The string used to separate the segments</param>
        /// <param name="idSegments">The array of identities to compose</param>
        /// <returns>String id</returns>
        string DataIdFrom(string separator, params string[] idSegments);

        /// <summary>
        /// Gets the collection of segments from the <paramref name="dataId"/> that are
        /// separated by the <paramref name="separator"/>.
        /// </summary>
        /// <param name="separator">The string that separates the segments</param>
        /// <param name="dataId">The string composite identity</param>
        /// <returns>Id Segments</returns>
        IEnumerable<string> DataIdSegmentsFrom(string separator, string dataId);
    }

    public static class CompositeIdentitySupport
    {
        public static string DataIdFrom(string separator, params string[] idSegments)
        {
            var builder = new StringBuilder();
            builder.Append(idSegments[0]);
            for (var idx = 1; idx < idSegments.Length; ++idx)
            {
                builder.Append(separator).Append(idSegments[idx]);
            }
            return builder.ToString();
        }

        public static IEnumerable<string> DataIdSegmentsFrom(string separator, string dataId) => dataId.Split(separator.ToCharArray());
    }
}