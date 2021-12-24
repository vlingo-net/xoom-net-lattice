// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Linq.Expressions;
using Vlingo.Xoom.Actors;

namespace Vlingo.Xoom.Lattice.Grid.Application.Message
{
    public class ActorDeliver : IMessage
    {
        public Type Protocol { get; }
        public Func<Grid, Actor> ActorProvider { get; }
        public LambdaExpression Consumer { get; }
        public Guid AnswerCorrelationId { get; }
        public string Representation { get; }

        public ActorDeliver(
            Type protocol,
            Func<Grid, Actor> actorProvider,
            LambdaExpression consumer,
            string representation) : this(protocol, actorProvider, consumer, Guid.Empty, representation)
        {
        }

        public ActorDeliver(
            Type protocol,
            Func<Grid, Actor> actorProvider,
            LambdaExpression consumer,
            Guid answerCorrelationId,
            string representation)
        {
            Protocol = protocol;
            ActorProvider = actorProvider;
            Consumer = consumer;
            AnswerCorrelationId = answerCorrelationId;
            Representation = representation;
        }
        
        public override string ToString() =>
            $"Space(protocol='{Protocol}', actorProvider='{ActorProvider.GetType()}', consumer='{Consumer.GetType()}', representation='{Representation}'";
    
    }
}