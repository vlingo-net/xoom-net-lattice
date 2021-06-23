// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Xoom.Lattice.Query
{
    /// <summary>
    /// An Exception used to indicate the failure of an attempt to <code>QueryAll()</code> or to <code>QueryObject()</code>.
    /// </summary>
    public class ObjectQueryFailedException : Exception
    {
        private readonly object _queryAttempt;

        public ObjectQueryFailedException(object queryAttempt) => _queryAttempt = queryAttempt;

        public ObjectQueryFailedException(object queryAttempt, string message, Exception cause) : base(message, cause) =>
            _queryAttempt = queryAttempt;

        public ObjectQueryFailedException(object queryAttempt, string message) : base(message) => _queryAttempt = queryAttempt;

        public QueryAttempt<TObjectState, TOutcome, TResult> QueryAttempt<TObjectState, TOutcome, TResult>() => 
            (QueryAttempt<TObjectState, TOutcome, TResult>) _queryAttempt;
        
        public QueryAttemptBase QueryAttempt() => (QueryAttemptBase) _queryAttempt;
    }
}