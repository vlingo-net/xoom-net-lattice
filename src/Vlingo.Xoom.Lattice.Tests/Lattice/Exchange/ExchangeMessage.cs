// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Common.Message;
using Vlingo.Xoom.Common.Serialization;
using Vlingo.Xoom.Common.Version;

namespace Vlingo.Tests.Lattice.Exchange
{
    public class ExchangeMessage : IMessage
    {
        private readonly string _payload;
        
        public ExchangeMessage(string type, string payload)
        {
            Type = type;
            _payload = payload;
        }
        
        public string Id { get; }
        public DateTimeOffset OccurredOn { get; }
        public T Payload<T>() => JsonSerialization.Deserialized<T>(_payload);

        public string Type { get; }
        public string Version { get; }
        public SemanticVersion SemanticVersion { get; }

        public override string ToString() => "ExchangeMessage[type=" + Type + " payload=" + _payload + "]";
    }
}