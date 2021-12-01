// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        private readonly IHashRing<Id> _hashRing;
        private readonly IInbound _inbound;
        private readonly IOutbound _outbound;
        private readonly Id _localNode;
        private readonly AtomicBoolean _isClusterHealthy = new AtomicBoolean(false);
        private readonly IDecoder _decoder;
        private readonly ILogger _logger;
        
        private readonly IHardRefHolder _holder;
        private readonly Scheduler _scheduler;
        private readonly WeakQueue<ThreadStart> _buffer = new WeakQueue<ThreadStart>(); // buffer messages when cluster is not healthy
        private IEnumerable<MethodInfo> _privateHandleMethods;

        public GridApplicationMessageHandler(
            Id localNode,
            IHashRing<Id> hashRing,
            IInbound inbound,
            IOutbound outbound,
            IHardRefHolder holder,
            Scheduler scheduler,
            ILogger logger) : this(localNode, hashRing, inbound, outbound, new JsonDecoder(), holder, scheduler, logger)
        {
            _hashRing = hashRing;
            _inbound = inbound;
            _outbound = outbound;
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
            _hashRing = hashRing;
            _inbound = inbound;
            _outbound = outbound;
            _decoder = decoder;
            _holder = holder;
            _scheduler = scheduler;
            _logger = logger;

            _privateHandleMethods = GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => m.IsGenericMethod && m.Name.StartsWith("Handle"));
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
                    //message.Accept(_localNode, sender, _visitor);
                    ConcreteHandler(_localNode, sender, message);
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
        
        private void HandleAnswer<T>(Id receiver, Id sender, Answer<T> answer) => _inbound.Answer(receiver, sender, answer);

        private void HandleDeliver<T>(Id receiver, Id sender, Deliver deliver)
        {
            var recipient = Receiver(receiver, deliver.Address);
            if (recipient.Equals(receiver))
            {
                _inbound.Deliver(
                    receiver, sender,
                    ReturnsAnswer<T>(receiver, sender, deliver),
                    deliver.Protocol,
                    deliver.Address, deliver.Definition, deliver.Consumer, deliver.Representation);
            }
            else
            {
                _outbound.Forward(recipient, sender, deliver);
            }
        }

        private void HandleStart<T>(Id receiver, Id sender, Start start)
        {
            var recipient = Receiver(receiver, start.Address);
            if (recipient == receiver)
            {
                _inbound.Start(receiver, sender, start.Protocol, start.Address, start.Definition);
            }
            else
            {
                _outbound.Forward(recipient, sender, start);
            }
        }

        private void HandleRelocate<T>(Id receiver, Id sender, Relocate relocate)
        {
            var recipient = Receiver(receiver, relocate.Address);
            if (recipient == receiver)
            {
                var pending = relocate.Pending
                    .Select(deliver =>
                        new LocalMessage<T>(null, (Action<T>) deliver.Consumer.Compile(),
                            ReturnsAnswer<T>(receiver, sender, deliver), deliver.Representation));
                _inbound.Relocate(receiver, sender, relocate.Definition, relocate.Address, relocate.Snapshot, pending);
            }
            else
            {
                _outbound.Forward(recipient, sender, relocate);
            }
        }

        private void HandleForward<T>(Id receiver, Id sender, Forward forward) => ConcreteHandler(receiver, forward.OriginalSender, forward.Message);

        private Id Receiver(Id receiver, IAddress address)
        {
            var recipient = _hashRing.NodeOf(address.IdString);
            if (recipient == null || recipient.Equals(receiver))
            {
                return receiver;
            }
            
            return recipient;
        }
        
        private ICompletes ReturnsAnswer<T>(Id receiver, Id sender, Deliver deliver)
        {
            if (deliver.AnswerCorrelationId == Guid.Empty)
            {
                return null!;
            }

            var completes = Completes.Using<T>(_scheduler);
            completes.AndThen(result => new Answer<T>(deliver.AnswerCorrelationId, result))
                .RecoverFrom(error => new Answer<T>(deliver.AnswerCorrelationId, error))
                .Otherwise<Answer<T>>(ignored => new Answer<T>(deliver.AnswerCorrelationId, new TimeoutException()))
                .AndThenConsume(TimeSpan.FromMilliseconds(4000), answer => _outbound.Answer(sender, receiver, answer));

            return completes;
        }

        private void ConcreteHandler(Id receiver, Id sender, IMessage message)
        {
            switch (message)
            {
                case Deliver d:
                    _privateHandleMethods.First(m => m.Name.Contains("Deliver"))
                        .Invoke(this, new object[] { receiver, sender, d });
                    break;
                case Start s:
                    _privateHandleMethods.First(m => m.Name.Contains("Start"))
                        .Invoke(this, new object[] { receiver, sender, s });
                    break;
                case Relocate r:
                    _privateHandleMethods.First(m => m.Name.Contains("Relocate"))
                        .Invoke(this, new object[] { receiver, sender, r });
                    break;
                case Forward f:
                    _privateHandleMethods.First(m => m.Name.Contains("Forward"))
                        .Invoke(this, new object[] { receiver, sender, f });
                    break;
                default:
                    _privateHandleMethods.First(m => m.Name.Contains("Answer"))
                        .Invoke(this, new object[] { receiver, sender, message });
                    break;
            }
        }
    }
}