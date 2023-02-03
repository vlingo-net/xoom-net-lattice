// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Linq.Expressions;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common;
using Vlingo.Xoom.Wire.Nodes;

namespace Vlingo.Xoom.Lattice.Grid.Application.Message;

[Serializable]
public class GridDeliver : IMessage
{
    public Type Protocol { get; }
    public IAddress Address { get; }
    public Definition.SerializationProxy Definition { get; }
    public LambdaExpression Consumer { get; }
    public Guid AnswerCorrelationId { get; }
    public string Representation { get; }

    public static Func<Actors.IMessage, GridDeliver> From(Action<Guid, UnAckMessage> correlation, Id receiver)
    {
        return message =>
        {
            var returns = Optional.OfNullable(message.Completes);

            var answerCorrelationId = returns
                .Map(_ => Guid.NewGuid())
                .OrElse(Guid.Empty);
                
            var deliver = new GridDeliver(
                message.Protocol,
                message.Actor.Address,
                Vlingo.Xoom.Actors.Definition.SerializationProxy.From(message.Actor.Definition),
                message.SerializableConsumer!,
                answerCorrelationId,
                message.Representation);
                
            if (answerCorrelationId != Guid.Empty)
            {
                correlation(answerCorrelationId, new UnAckMessage(message.Protocol, receiver, returns.Get(), deliver));
            }

            return deliver;
        };
    }
        
    public GridDeliver(
        Type protocol,
        IAddress address,
        Definition.SerializationProxy definition,
        LambdaExpression consumer,
        string representation) : this(protocol, address, definition, consumer, Guid.Empty, representation)
    {
    }

    public GridDeliver(
        Type protocol,
        IAddress address,
        Definition.SerializationProxy definition,
        LambdaExpression consumer,
        Guid answerCorrelationId,
        string representation)
    {
        Protocol = protocol;
        Address = address;
        Definition = definition;
        Consumer = consumer;
        AnswerCorrelationId = answerCorrelationId;
        Representation = representation;
    }
        
    public override string ToString() =>
        $"GridDeliver(protocol='{Protocol.Name}', address='{Address}', definitionProxy='{Definition}', consumer='{Consumer}', representation='{Representation}')";
}