// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
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

namespace Vlingo.Xoom.Lattice.Grid.Application.Message
{
    [Serializable]
    public class Deliver<T> : IMessage
    {
        public IAddress Address { get; }
        public Definition.SerializationProxy<T> Definition { get; }
        public Expression<Action<T>> Consumer { get; }
        public Guid AnswerCorrelationId { get; }
        public string Representation { get; }

        public static Func<Actors.IMessage, Deliver<T>> From(Action<Guid, UnAckMessage<T>> correlation, Id receiver)
        {
            return message =>
            {
                var localMessage = (LocalMessage<T>) message;
                var returns = Optional.OfNullable(localMessage.Completes);
                
                var answerCorrelationId = returns
                    .Map(completes => Guid.NewGuid())
                    .OrElse(Guid.Empty);
                
                var deliver = new Deliver<T>(
                    localMessage.Protocol,
                    localMessage.Actor.Address,
                    Vlingo.Xoom.Actors.Definition.SerializationProxy<T>.From(localMessage.Actor.Definition),
                    localMessage.SerializableConsumer!,
                    answerCorrelationId,
                    localMessage.Representation);
                
                if (answerCorrelationId != null)
                {
                    correlation(answerCorrelationId, new UnAckMessage<T>(receiver, (ICompletes<T>) returns.Get(), deliver));
                }

                return deliver;
            };
        }
        
        public Deliver(
            Type protocol,
            IAddress address,
            Definition.SerializationProxy<T> definition,
            Expression<Action<T>> consumer,
            string representation) : this(protocol, address, definition, consumer, Guid.Empty, representation)
        {
        }

        public Deliver(
            Type protocol,
            IAddress address,
            Definition.SerializationProxy<T> definition,
            Expression<Action<T>> consumer,
            Guid answerCorrelationId,
            string representation) {
            Address = address;
            Definition = definition;
            Consumer = consumer;
            AnswerCorrelationId = answerCorrelationId;
            Representation = representation;
        }
        
        public void Accept(Id receiver, Id sender, IVisitor visitor) => visitor.Visit(receiver, sender, this);

        public override string ToString() =>
            $"Deliver(protocol='{typeof(T).Name}', address='{Address}', definitionProxy='{Definition}', consumer='{Consumer}', representation='{Representation}')";
    }
}