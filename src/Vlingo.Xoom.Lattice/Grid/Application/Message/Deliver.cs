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
        public IAddress address;
        public Definition.SerializationProxy<T> definition;
        public Expression<Action<T>> consumer;
        public Guid answerCorrelationId;
        public string representation;
        
        public static Func<Actors.IMessage, Deliver<T>> From(Action<Guid, UnAckMessage<T>> correlation, Id receiver)
        {
            return message =>
            {
                // var __message = (LocalMessage<T>) message;
                // var returns = Optional.OfNullable(__message.Returns());
                //
                // var answerCorrelationId = returns
                //     .Map(_return => Guid.NewGuid())
                //     .OrElse(Guid.Empty);
                //
                // var deliver = new Deliver(
                //     __message.Protocol,
                //     __message.Actor.Address,
                //     Definition.SerializationProxy<T>.From(__message.Actor.Definition),
                //     __message.Consumer(),
                //     answerCorrelationId,
                //     __message.Representation);
                //
                // if (answerCorrelationId != null)
                // {
                //     correlation(answerCorrelationId, new UnAckMessage(receiver, returns.get(), deliver));
                // }

                // return deliver;

                return null; // TODO: replace with concrete implementation (commented above)
            };
        }
        
        public Deliver(
            IAddress address,
            Definition.SerializationProxy<T> definition,
            Expression<Action<T>> consumer,
            string representation) : this(address, definition, consumer, Guid.Empty, representation)
        {
        }

        public Deliver(
            IAddress address,
            Definition.SerializationProxy<T> definition,
            Expression<Action<T>> consumer,
            Guid answerCorrelationId,
            string representation) {
            this.address = address;
            this.definition = definition;
            this.consumer = consumer;
            this.answerCorrelationId = answerCorrelationId;
            this.representation = representation;
        }
        
        public void Accept(Id receiver, Id sender, IVisitor visitor) => visitor.Visit(receiver, sender, this);

        public override string ToString() =>
            $"Deliver(protocol='{typeof(T).Name}', address='{address}', definitionProxy='{definition}', consumer='{consumer}', representation='{representation}')";
    }
}