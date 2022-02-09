// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common;
using Vlingo.Xoom.Lattice.Grid.Application.Message;
using Vlingo.Xoom.Lattice.Util;
using Vlingo.Xoom.Wire.Fdx.Outbound;
using Vlingo.Xoom.Wire.Message;
using Vlingo.Xoom.Wire.Nodes;
using IMessage = Vlingo.Xoom.Lattice.Grid.Application.Message.IMessage;

namespace Vlingo.Xoom.Lattice.Grid.Application
{
    public class OutboundGridActorControl : Actor, IOutbound
    {
        private readonly Id _localNodeId;
        private IApplicationOutboundStream? _stream;
        private readonly IEncoder _encoder;
        private readonly Action<Guid, IMessage> _gridMessagesCorrelationConsumer;
        private readonly Action<Guid, ICompletes> _actorMessagesCorrelationConsumer;

        private readonly OutBuffers _outBuffers; // buffer messages for unhealthy nodes
        private readonly ConcurrentDictionary<Id, bool> _nodesHealth;
        
        public OutboundGridActorControl(
            Id localNodeId,
            IEncoder encoder,
            Action<Guid, IMessage> gridMessagesCorrelationConsumer,
            Action<Guid, ICompletes> actorMessagesCorrelationConsumer,
            OutBuffers outBuffers) : this(localNodeId, null, encoder, gridMessagesCorrelationConsumer, actorMessagesCorrelationConsumer, outBuffers)
        {
        }
            
        public OutboundGridActorControl(
            Id localNodeId,
            IApplicationOutboundStream? stream,
            IEncoder encoder,
            Action<Guid, IMessage> gridMessagesCorrelationConsumer,
            Action<Guid, ICompletes> actorMessagesCorrelationConsumer,
            OutBuffers outBuffers)
        {
            _localNodeId = localNodeId;
            _stream = stream;
            _encoder = encoder;
            _gridMessagesCorrelationConsumer = gridMessagesCorrelationConsumer;
            _actorMessagesCorrelationConsumer = actorMessagesCorrelationConsumer;
            _outBuffers = outBuffers;
            _nodesHealth = new ConcurrentDictionary<Id, bool>();
        }
        
        public void Start(Id recipient, Id sender, Type protocol, IAddress address, Definition.SerializationProxy definitionProxy) => 
            Send(recipient, new Start(protocol, address, definitionProxy));

        public void GridDeliver(Id recipient, Id sender, ICompletes? returns, Type protocol, IAddress address, Definition.SerializationProxy definitionProxy, LambdaExpression consumer, string representation)
        {
            GridDeliver gridDeliver;
            if (returns == null)
            {
                gridDeliver = new GridDeliver(protocol, address, definitionProxy, consumer, representation);
            } 
            else
            {
                var answerCorrelationId = Guid.NewGuid();
                gridDeliver = new GridDeliver(protocol, address, definitionProxy, consumer, answerCorrelationId, representation);
                _gridMessagesCorrelationConsumer(answerCorrelationId, new UnAckMessage(protocol, recipient, returns, gridDeliver));
            }
            Send(recipient, gridDeliver);
        }
        
        public void ActorDeliver(
            Id recipient,
            Id sender,
            ICompletes? returns,
            Type protocol,
            Func<Grid, Actor> actorProvider,
            LambdaExpression consumer,
            string representation)
        {
            ActorDeliver actorDeliver;
            if (returns == null)
            {
                actorDeliver = new ActorDeliver(protocol, actorProvider, consumer, representation);
            }
            else
            {
                var answerCorrelationId = Guid.NewGuid();
                actorDeliver = new ActorDeliver(protocol, actorProvider, consumer, answerCorrelationId, representation);
                _actorMessagesCorrelationConsumer(answerCorrelationId, returns);
            }

            Send(recipient, actorDeliver);
        }

        public void Answer<T>(Id receiver, Id sender, Answer<T> answer) => Send(receiver, answer);

        public void Forward(Id receiver, Id sender, IMessage message) => Send(receiver, new Forward(sender, message));

        public void Relocate(Id receiver, Id sender, Definition.SerializationProxy definitionProxy, IAddress address, object snapshot, IEnumerable<Actors.IMessage> pending)
        {
            var messages =
                pending
                    .Select(Message.GridDeliver.From(_gridMessagesCorrelationConsumer, receiver))
                    .ToList();

            Send(receiver, new Relocate(messages.GetType().GetGenericArguments().First(), address, definitionProxy, snapshot, messages));
        }

        public void InformNodeIsHealthy(Id id, bool isHealthy)
        {
            _nodesHealth.AddOrUpdate(id, isHealthy, (oldVal, newVal) => isHealthy);
            if (isHealthy)
            {
                Disburse(id);
            }
        }

        public void UseStream(IApplicationOutboundStream? outbound) => _stream = outbound;

        private void Send(Id recipient, IMessage message)
        {
            Logger.Debug($"Buffering message {message} to {recipient}");
            ThreadStart sendFunction = () => {
                Logger.Debug($"Sending message {message} to {recipient}");
                byte[] payload = _encoder.Encode(message);
                var raw = RawMessage.From(_localNodeId.Value, -1, payload.Length);
                raw.PutRemaining(new MemoryStream(payload));
                _stream?.SendTo(raw, recipient);
            };

            if (_nodesHealth.ContainsKey(recipient) && _nodesHealth[recipient])
            {
                sendFunction(); // send the message immediately, node is healthy
            }
            else
            {
                _outBuffers.Enqueue(recipient, new Thread(sendFunction)); // enqueue the message, node is unhealthy
            }
        }
        
        private void Disburse(Id id)
        {
            var buffer = _outBuffers.Queue(id);
            if (buffer.Count == 0)
            {
                return;
            }

            Logger.Debug($"Disbursing {buffer.Count} buffered messages to node {id}");
            Thread? next;
            do
            {
                next = buffer.Poll();
                if (next != null)
                {
                    next.Start();
                }
            } while (next != null);
        }
    }
}