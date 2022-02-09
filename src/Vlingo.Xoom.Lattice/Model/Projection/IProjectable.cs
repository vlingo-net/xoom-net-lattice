// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using Vlingo.Xoom.Common;
using Vlingo.Xoom.Symbio;

namespace Vlingo.Xoom.Lattice.Model.Projection
{
    /// <summary>
    /// Provides parts of the state to be projected.
    /// </summary>
    public interface IProjectable
    {
        /// <summary>
        /// Gets reason(s) for projection.
        /// </summary>
        /// <returns>An array of reasons.</returns>
        string[] BecauseOf();
        
        /// <summary>
        /// Gets my state as binary/bytes.
        /// </summary>
        /// <returns>Data as byte array.</returns>
        byte[] DataAsBytes();
        
        /// <summary>
        /// Get my state as text.
        /// </summary>
        /// <returns>Data as text.</returns>
        string DataAsText();
        
        /// <summary>
        /// Gets the version of data.
        /// </summary>
        /// <returns>The version of the data.</returns>
        int DataVersion();
        
        /// <summary>
        /// Gets the unique id of data.
        /// </summary>
        string DataId { get; }
        
        /// <summary>
        /// Gets all the entries.
        /// </summary>
        IEnumerable<IEntry> Entries { get; }
        
        /// <summary>
        /// Gets <code>true</code> or <code>false</code> whether or not there are entries.
        /// </summary>
        bool HasEntries { get; }
        
        /// <summary>
        /// Gets the associated metadata.
        /// </summary>
        string Metadata { get; }
        
        /// <summary>
        /// Gets <code>true</code> or <code>false</code> whether or no my <code>object</code> exists.
        /// </summary>
        bool HasObject { get; }

        /// <summary>
        /// Gets data as a specific <typeparamref name="T"/> typed object.
        /// </summary>
        /// <typeparam name="T">The type of the expected object.</typeparam>
        /// <returns>Gets the typed object <typeparamref name="T"/></returns>
        T Object<T>();
        
        /// <summary>
        /// Gets data as a specific optional <typeparamref name="T"/> typed object.
        /// </summary>
        /// <typeparam name="T">The type of the expected object.</typeparam>
        /// <returns>Gets the optional typed object <typeparamref name="T"/></returns>
        Optional<T> OptionalObject<T>();
        
        /// <summary>
        /// Gets the unique identity associated with the projection operation.
        /// </summary>
        string ProjectionId { get; }
        
        /// <summary>
        /// Gets <code>true</code> or <code>false</code> whether or not <code>state</code> is non-null.
        /// </summary>
        bool HasState { get; }
        
        /// <summary>
        /// Gets the projection type as text.
        /// </summary>
        string Type { get; }
        
        /// <summary>
        /// Gets the version of my type.
        /// </summary>
        int TypeVersion { get; }
    }
}