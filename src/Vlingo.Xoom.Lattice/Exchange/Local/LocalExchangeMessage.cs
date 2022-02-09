// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Common.Message;
using Vlingo.Xoom.Common.Version;

namespace Vlingo.Xoom.Lattice.Exchange.Local
{
    public class LocalExchangeMessage : IMessage
    {
        public LocalExchangeMessage(string id, string type, string version, DateTimeOffset occurredOn, object payload)
        {
            RawPayload = payload;
            Id = id;
            Type = type;
            Version = version;
            OccurredOn = occurredOn;
        }

        public LocalExchangeMessage(string type, object payload) : this(Guid.NewGuid().ToString(), type, "1.0.0", DateTimeOffset.Now, payload)
        {
        }
        
        public string Id { get; }
        
        public DateTimeOffset OccurredOn { get; }
        
        public T Payload<T>() => (T) RawPayload;
        
        public object RawPayload { get; }

        public string Type { get; }
        
        public string Version { get; }

        public SemanticVersion SemanticVersion => this.From();

        public override string ToString() => $"LocalExchangeMessage[type={Type} payload={RawPayload}]";
    }
}