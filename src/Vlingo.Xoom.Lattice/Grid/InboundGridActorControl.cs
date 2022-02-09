// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common;
using Vlingo.Xoom.Lattice.Grid.Application;
using Vlingo.Xoom.Lattice.Grid.Application.Message;
using Vlingo.Xoom.Wire.Nodes;
using IMessage = Vlingo.Xoom.Lattice.Grid.Application.Message.IMessage;

namespace Vlingo.Xoom.Lattice.Grid
{
    public class InboundGridActorControl : Actor, IInbound
    {
        private readonly IGridRuntime _gridRuntime;

        private readonly Func<Guid, UnAckMessage> _gridMessagesCorrelation;
        private readonly Func<Guid, ICompletes> _actorMessagesCorrelation;
        
        public InboundGridActorControl(
            IGridRuntime gridRuntime,
            Func<Guid, UnAckMessage> gridMessagesCorrelation,
            Func<Guid, ICompletes> actorMessagesCorrelation)
        {
            _gridRuntime = gridRuntime;
            _gridMessagesCorrelation = gridMessagesCorrelation;
            _actorMessagesCorrelation = actorMessagesCorrelation;
        }
        
        public void Start(Id recipient, Id sender, Type protocol, IAddress address, Definition.SerializationProxy definitionProxy)
        {
            Logger.Debug("Processing: Received application message: Start");

            var stage = _gridRuntime.AsStage();

            var actor = stage.RawLookupOrStart(
                    Definition.From(stage, definitionProxy, stage.World.DefaultLogger),
                    address);

            if (GridActorOperations.IsSuspendedForRelocation(actor))
            {
                Logger.Debug($"Resuming thunk found at {address} with definition='{actor.Definition}'");

                GridActorOperations.ResumeFromRelocation(actor);
            }
        }

        public void GridDeliver(
            Id recipient,
            Id sender,
            ICompletes? returns,
            Type protocol,
            IAddress address,
            Definition.SerializationProxy definitionProxy,
            LambdaExpression consumer,
            string representation)
        {
            Logger.Debug("Processing: Received application message: GridDeliver");

            var stage = _gridRuntime.AsStage();

            var actor = stage.ActorLookupOrStartThunk(
                    Definition.From(stage, definitionProxy, stage.World.DefaultLogger),
                    address);

            actor?.ActorMailbox(actor).Send(actor, protocol, consumer, returns, representation);

            if (GridActorOperations.IsSuspendedForRelocation(actor!))
            {
                // this case is happening when a message is retried on a different node and above actor is created 'on demand'
                Logger.Debug($"Resuming thunk found at {address} with definition='{actor!.Definition}'");

                GridActorOperations.ResumeFromRelocation(actor);
            }
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
            Logger.Debug("Processing: Received application message: Deliver2");

            var grid = (Grid) _gridRuntime.AsStage();

            var actor = actorProvider(grid);

            actor.ActorMailbox(actor).Send(actor, protocol, consumer, returns, representation);
        }

        public void Answer<T>(Id receiver, Id sender, Answer<T> answer)
        {
            // same Answer is used for both GridDeliver and Deliver2 messages
            Logger.Debug("GRID: Processing application message: Answer");
            var clientReturns = Optional
                .OfNullable(_gridMessagesCorrelation(answer.CorrelationId))
                .Map(message => message.Completes as ICompletes<T>)
                .OrElse(null);
            if (clientReturns == null)
            {
                clientReturns = _actorMessagesCorrelation(answer.CorrelationId) as ICompletes<T>;
                if (clientReturns == null)
                {
                    Logger.Warn($"GRID: Answer from {sender} for Returns with {answer.CorrelationId} didn't match a Returns on this node!");
                    return;
                }
            }
            if (answer.Error == null)
            {
                var result = ActorProxyBase.Thunk(_gridRuntime.AsStage(), answer.Result);
                clientReturns.With(result);
            }
            else
            {
                clientReturns.Failed(new Exception("Remote actor call failed", answer.Error));
            }
        }

        public void Forward(Id receiver, Id sender, IMessage message) => 
            throw new NotSupportedException("Should have been handled in GridApplicationMessageHandler.Accept(Id, Id, Forward) by dispatching the visitor to the enclosed Message");

        public void Relocate(
            Id receiver,
            Id sender,
            Definition.SerializationProxy definitionProxy,
            IAddress address,
            object snapshot,
            IEnumerable<Actors.IMessage> pending)
        {
            Logger.Debug("Processing: Received application message: Relocate");

            var stage = _gridRuntime.AsStage();

            var actor =
                stage.ActorLookupOrStartThunk(
                    Definition.From(stage, definitionProxy, stage.World.DefaultLogger),
                    address);

            GridActorOperations.ApplyRelocationSnapshot(stage, actor!, snapshot);

            var mailbox = actor?.ActorMailbox(actor);

            pending.ToList().ForEach(pendingMessage => {
                var message = pendingMessage;
                message.Set(actor!, message.Protocol, message.SerializableConsumer, message.Completes, message.Representation);
                mailbox?.Send(message);
            });

            GridActorOperations.ResumeFromRelocation(actor!);
        }

        public void InformNodeIsHealthy(Id id, bool isHealthy) => throw new NotImplementedException("InformNodeIsHealthy handled in ApplicationMessageHandler");
    }
}