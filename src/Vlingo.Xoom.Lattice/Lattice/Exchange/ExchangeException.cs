// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Runtime.Serialization;

namespace Vlingo.Xoom.Lattice.Exchange
{
    public class ExchangeException : Exception
    {
        /// <summary>
        /// Gets <code>true</code> or <code>false</code> whether or not retry is set. Retry can be used by a MessageListener
        /// when it wants the message it has attempted to handle to be re-queued rather
        /// than rejected, so that it can re-attempt handling later.
        /// </summary>
        public bool Retry { get; }

        public ExchangeException()
        {
        }

        protected ExchangeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ExchangeException(string? message, bool retry = false) : base(message) => Retry = retry;

        public ExchangeException(string? message, Exception? innerException, bool retry = false) : base(message, innerException) =>
            Retry = retry;
    }
}