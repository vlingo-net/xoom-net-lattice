// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Common.Message;
using Vlingo.Xoom.Common.Version;

namespace Vlingo.Xoom.Lattice.Tests.Exchange;

public class ExternalType2 : IMessage
{
    public ExternalType2(string value1, int value2)
    {
        Field1 = value1;
        Field2 = value2.ToString();
    }
        
    public string Field1 { get; set; }
    public string Field2 { get; set; }
        
    public string Id { get; }
    public DateTimeOffset OccurredOn { get; }
    public T Payload<T>() => default;

    public string Type { get; }
    public string Version { get; }
    public SemanticVersion SemanticVersion { get; }

    public override string ToString() => $"ExternalType[field1={Field1} field2={Field2}]";
}