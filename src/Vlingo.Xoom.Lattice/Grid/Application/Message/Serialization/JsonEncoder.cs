// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Vlingo.Xoom.Common.Serialization;

namespace Vlingo.Xoom.Lattice.Grid.Application.Message.Serialization;

public class JsonEncoder : IEncoder
{
    private readonly JsonSerializerSettings _settings = new JsonSerializerSettings
    {
        Converters = new List<JsonConverter> { new MessageConverter() },
        TypeNameHandling = TypeNameHandling.Objects,
        Formatting = Formatting.Indented,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    };
        
    public byte[] Encode(IMessage message)
    {
        var serialized = JsonSerialization.Serialized(message, _settings);
        return Encoding.UTF8.GetBytes(serialized);
    }
}