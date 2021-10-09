// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Linq;
using System.Threading;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common;
using Vlingo.Xoom.Lattice.Grid.Application.Message;
using Vlingo.Xoom.Lattice.Grid.Application.Message.Serialization;
using Vlingo.Xoom.Lattice.Grid.Hashring;
using Vlingo.Xoom.Lattice.Util;
using Vlingo.Xoom.Wire.Message;
using Vlingo.Xoom.Wire.Nodes;
using IMessage = Vlingo.Xoom.Lattice.Grid.Application.Message.IMessage;

namespace Vlingo.Xoom.Lattice.Grid.Application
{
    public class GridApplicationMessageHandler : IApplicationMessageHandler
    {
        private readonly Id _localNode;
        private readonly AtomicBoolean _isClusterHealthy = new AtomicBoolean(false);
        private readonly IDecoder _decoder;
        private readonly IVisitor _visitor;
        private readonly ILogger _logger;
        
        private readonly IHardRefHolder _holder;
        private readonly WeakQueue<ThreadStart> _buffer = new WeakQueue<ThreadStart>(); // buffer messages when cluster is not healthy
        
        public GridApplicationMessageHandler(
            Id localNode,
            IHashRing<Id> hashRing,
            IInbound inbound,
            IOutbound outbound,
            IHardRefHolder holder,
            Scheduler scheduler,
            ILogger logger) : this(localNode, hashRing, inbound, outbound, new JsonDecoder(), holder, scheduler, logger)
        {
        }

        public GridApplicationMessageHandler(
            Id localNode,
            IHashRing<Id> hashRing,
            IInbound inbound,
            IOutbound outbound,
            IDecoder decoder,
            IHardRefHolder holder,
            Scheduler scheduler,
            ILogger logger)
        {
            _localNode = localNode;
            _decoder = decoder;
            _holder = holder;
            _logger = logger;
            _visitor = new ControlMessageVisitor(inbound, outbound, hashRing, scheduler);
        }
        
        public void Handle(RawMessage raw)
        {
            try
            {
                var message = (IMessage) _decoder.Decode(raw.AsBinaryMessage)!;
                var sender = Id.Of(raw.Header().NodeId);
                _logger.Debug($"Buffering message {message} from {sender}");
                ThreadStart runnable = () =>
                {
                    _logger.Debug($"Handling message {message} from {sender}");
                    message.Accept(_localNode, sender, _visitor);
                };

                if (_isClusterHealthy.Get())
                {
                    runnable(); // incoming messages are dispatched immediately
                }
                else
                {
                    _buffer.Enqueue(runnable); // buffer messages; cluster is not healthy
                }

                if (_holder != null)
                {
                    _holder.HoldOnTo(runnable);
                }
            }
            catch (Exception e)
            {
                _logger.Error($"Failed to process message {raw}", e);
            }
        }

        public void InformNodeIsHealthy(Id id, bool isHealthy)
        {
            _isClusterHealthy.Set(isHealthy);
            if (isHealthy)
            {
                Disburse(id);
            }
        }
        
        private void Disburse(Id id)
        {
            if (!id.Equals(_localNode))
            {
                return;
            }

            if (_buffer.Count == 0)
            {
                return;
            }
            
            _logger.Debug($"Disbursing {_buffer.Count} buffered messages");
            
            ThreadStart? next;
            do
            {
                next = _buffer.Poll();
                if (next != null)
                {
                    new Thread(next).Start();
                }
            } while (next != null);
        }
        
        private class ControlMessageVisitor : IVisitor
        {
            private readonly IInbound _inbound;
            private readonly IOutbound _outbound;
            private readonly IHashRing<Id> _hashRing;
            private readonly Scheduler _scheduler;

            public ControlMessageVisitor(IInbound inbound, IOutbound outbound, IHashRing<Id> hashRing, Scheduler scheduler)
            {
                _inbound = inbound;
                _outbound = outbound;
                _hashRing = hashRing;
                _scheduler = scheduler;
            }
            
            public void Visit<T>(Id receiver, Id sender, Answer<T> answer) => _inbound.Answer(receiver, sender, answer);

            public void Visit<T>(Id receiver, Id sender, Deliver<T> deliver)
            {
                var recipient = Receiver(receiver, deliver.Address);
                if (recipient.Equals(receiver))
                {
                    _inbound.Deliver(
                        receiver, sender,
                        ReturnsAnswer(receiver, sender, deliver),
                        deliver.Address, deliver.Definition, deliver.Consumer, deliver.Representation);
                }
                else
                {
                    _outbound.Forward(recipient, sender, deliver);
                }
            }

            public void Visit<T>(Id receiver, Id sender, Start<T> start)
            {
                var recipient = Receiver(receiver, start.Address);
                if (recipient == receiver)
                {
                    _inbound.Start(receiver, sender, start.Address, start.Definition);
                }
                else
                {
                    _outbound.Forward(recipient, sender, start);
                }
            }

            public void Visit<T>(Id receiver, Id sender, Relocate<T> relocate)
            {
                var recipient = Receiver(receiver, relocate.Address);
                if (recipient == receiver)
                {
                    var pending = relocate.Pending
                        .Select(deliver =>
                            new LocalMessage<T>(null, deliver.Consumer.Compile(),
                                ReturnsAnswer(receiver, sender, deliver), deliver.Representation));
                    _inbound.Relocate(receiver, sender, relocate.Definition, relocate.Address, relocate.Snapshot, pending);
                }
                else
                {
                    _outbound.Forward(recipient, sender, relocate);
                }
            }

            public void Visit(Id receiver, Id sender, Forward forward) => forward.Message.Accept(receiver, forward.OriginalSender, this);

            private Id Receiver(Id receiver, IAddress address)
            {
                var recipient = _hashRing.NodeOf(address.IdString);
                if (recipient == null || recipient.Equals(receiver))
                {
                    return receiver;
                }
                
                return recipient;
            }
            
            private ICompletes<T>? ReturnsAnswer<T>(Id receiver, Id sender, Deliver<T> deliver)
            {
                if (deliver.AnswerCorrelationId == Guid.Empty)
                {
                    return null;
                }

                var completes = Completes.Using<T>(_scheduler);
                completes.AndThen(result => new Answer<T>(deliver.AnswerCorrelationId, result))
                    .RecoverFrom(error => new Answer<T>(deliver.AnswerCorrelationId, error))
                    .Otherwise<Answer<T>>(ignored => new Answer<T>(deliver.AnswerCorrelationId, new TimeoutException()))
                    .AndThenConsume(TimeSpan.FromMilliseconds(4000), answer => _outbound.Answer(sender, receiver, answer));

                return completes;
            }
        }
    }
}