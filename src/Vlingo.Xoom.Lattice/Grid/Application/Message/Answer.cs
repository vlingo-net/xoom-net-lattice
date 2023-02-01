// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Xoom.Lattice.Grid.Application.Message;

[Serializable]
public class Answer<T> : IMessage
{
    public Guid CorrelationId { get; }
    public T Result { get; }
    public Exception Error { get; }

    public Answer(Guid correlationId, T result) : this(correlationId, result, null!)
    {
    }

    public Answer(Guid correlationId, Exception error) : this(correlationId, default!, error)
    {
    }

    private Answer(Guid correlationId, T result, Exception error)
    {
        CorrelationId = correlationId;
        Result = result;
        Error = error;
    }
            
    public override string ToString() => $"Answer(correlationId='{CorrelationId}', result='{Result}', error='{Error}')";
}