// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Common.Message;
using Vlingo.Xoom.Common.Version;

namespace Vlingo.Tests.Lattice.Exchange
{
    public class ExternalType1 : IMessage
    {
        private readonly string _field1;
        private readonly string _field2;
        
        public ExternalType1(string value1, int value2)
        {
            _field1 = value1;
            _field2 = value2.ToString();
        }
        
        public string Id { get; }
        public DateTimeOffset OccurredOn { get; }
        public T Payload<T>() => default;

        public string Type { get; }
        public string Version { get; }
        public SemanticVersion SemanticVersion { get; }

        public override string ToString() => $"ExternalType[field1={_field1} field2={_field2}]";
    }
}