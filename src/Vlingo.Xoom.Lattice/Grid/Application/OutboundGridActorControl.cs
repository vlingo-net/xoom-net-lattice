// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
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
        private readonly Action<Guid, IMessage> _correlation;

        private readonly OutBuffers _outBuffers; // buffer messages for unhealthy nodes
        private readonly ConcurrentDictionary<Id, bool> _nodesHealth;
        
        public OutboundGridActorControl(
            Id localNodeId,
            IEncoder encoder,
            Action<Guid, IMessage> correlation,
            OutBuffers outBuffers) : this(localNodeId, null, encoder, correlation, outBuffers)
        {
        }
            
        public OutboundGridActorControl(
            Id localNodeId,
            IApplicationOutboundStream? stream,
            IEncoder encoder,
            Action<Guid, IMessage> correlation,
            OutBuffers outBuffers)
        {
            _localNodeId = localNodeId;
            _stream = stream;
            _encoder = encoder;
            _correlation = correlation;
            _outBuffers = outBuffers;
            _nodesHealth = new ConcurrentDictionary<Id, bool>();
        }
        
        public void Start(Id recipient, Id sender, Type protocol, IAddress address, Definition.SerializationProxy definitionProxy) => 
            Send(recipient, new Start(protocol, address, definitionProxy));

        public void Deliver(Id recipient, Id sender, ICompletes? returns, Type protocol, IAddress address, Definition.SerializationProxy definitionProxy, LambdaExpression consumer, string representation)
        {
            Deliver deliver;
            if (returns == null)
            {
                deliver = new Deliver(protocol, address, definitionProxy, consumer, representation);
            } 
            else
            {
                var answerCorrelationId = Guid.NewGuid();
                deliver = new Deliver(protocol, address, definitionProxy, consumer, answerCorrelationId, representation);
                _correlation(answerCorrelationId, new UnAckMessage(protocol, recipient, returns, deliver));
            }
            Send(recipient, deliver);
        }

        public void Answer<T>(Id receiver, Id sender, Answer<T> answer) => Send(receiver, answer);

        public void Forward(Id receiver, Id sender, IMessage message) => Send(receiver, new Forward(sender, message));

        public void Relocate(Id receiver, Id sender, Definition.SerializationProxy definitionProxy, IAddress address, object snapshot, IEnumerable<Actors.IMessage> pending)
        {
            var messages =
                pending
                    .Select(Message.Deliver.From(_correlation, receiver))
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

        public void UseStream(IApplicationOutboundStream outbound) => _stream = outbound;

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