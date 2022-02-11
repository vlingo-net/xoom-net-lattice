// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common;
using Vlingo.Xoom.Lattice.Grid;
using Vlingo.Xoom.Lattice.Grid.Application.Message;
using Vlingo.Xoom.Lattice.Grid.Application.Message.Serialization;
using Vlingo.Xoom.Wire.Nodes;
using Xunit;

namespace Vlingo.Xoom.Lattice.Tests.Grid.Message.Serialization;

public class SerializationTest
{
    [Fact]
    public void TestThatEncodesDecodesAnswerMessage()
    {
        var answer = new Answer<int>(Guid.NewGuid(), 5);
        var encoder = new JsonEncoder();
        var decoder = new JsonDecoder();

        var encoded = encoder.Encode(answer);

        var decoded = (Answer<int>) decoder.Decode(encoded);
            
        Assert.Equal(answer.CorrelationId, decoded?.CorrelationId);
        Assert.Equal(answer.Result, decoded?.Result);
        Assert.Null(decoded?.Error);
    }
        
    [Fact]
    public void TestThatEncodesDecodesAnswerWithExceptionMessage()
    {
        var answer = new Answer<int>(Guid.NewGuid(), new Exception("Answer exception test"));
        var encoder = new JsonEncoder();
        var decoder = new JsonDecoder();

        var encoded = encoder.Encode(answer);

        var decoded = (Answer<int>)decoder.Decode(encoded);
            
        Assert.Equal(answer.CorrelationId, decoded?.CorrelationId);
        Assert.Equal(answer.Error.Message, decoded?.Error.Message);
        Assert.Equal(0, decoded?.Result);
    }
        
    [Fact]
    public void TestThatEncodesDecodesDeliverMessage()
    {
        Expression<Action<ISimpleParametersInterface>> consumer = actor => actor.DoSomething("Test Consumer", 100);
        var deliver = new GridDeliver(
            typeof(ISimpleParametersInterface),
            new GridAddress(Guid.NewGuid()),
            Definition.SerializationProxy.From(
                Definition.Has<ISimpleParametersInterface>(() => new ActorTest())),
            consumer,
            Guid.NewGuid(),
            "GridDeliver representation");
        var encoder = new JsonEncoder();
        var decoder = new JsonDecoder();

        var encoded = encoder.Encode(deliver);

        var decoded = (GridDeliver) decoder.Decode(encoded);
            
        Assert.Equal(deliver.Address, decoded?.Address);
        Assert.Equal(deliver.Representation, decoded?.Representation);
        Assert.Equal(deliver.AnswerCorrelationId, decoded?.AnswerCorrelationId);
        Assert.Equal(typeof(Action<ISimpleParametersInterface>).FullName, decoded?.Consumer.Type.FullName);
    }
        
    [Fact]
    public void TestThatEncodesForwardWithAnswerMessage()
    {
        var answer = new Answer<int>(Guid.NewGuid(), 5);
        var forward = new Forward(Id.Of(50), answer);
        var encoder = new JsonEncoder();
        var decoder = new JsonDecoder();

        var encoded = encoder.Encode(forward);

        var decoded = (Forward) decoder.Decode(encoded);

        var decodedAnswer = (Answer<int>)decoded?.Message;
            
        Assert.Equal(forward.OriginalSender, decoded?.OriginalSender);
        Assert.Equal(answer.CorrelationId, decodedAnswer?.CorrelationId);
        Assert.Equal(answer.Result, decodedAnswer?.Result);
        Assert.Null(decodedAnswer?.Error);
    }
        
    [Fact]
    public void TestThatEncodesForwardWithDeliverMessage()
    {
        Expression<Action<ISimpleParametersInterface>> consumer = actor => actor.DoSomething("Test Consumer", 100);
        var deliver = new GridDeliver(
            typeof(ISimpleParametersInterface),
            new GridAddress(Guid.NewGuid()),
            Definition.SerializationProxy.From(
                Definition.Has<ISimpleParametersInterface>(() => new ActorTest())),
            consumer,
            Guid.NewGuid(),
            "GridDeliver representation");
        var forward = new Forward(Id.Of(50), deliver);
        var encoder = new JsonEncoder();
        var decoder = new JsonDecoder();

        var encoded = encoder.Encode(forward);

        var decoded = (Forward) decoder.Decode(encoded);

        var decodedDeliver = (GridDeliver)decoded?.Message;
            
        Assert.Equal(forward.OriginalSender, decoded?.OriginalSender);
        Assert.Equal(deliver.Address, decodedDeliver?.Address);
        Assert.Equal(deliver.Representation, decodedDeliver?.Representation);
        Assert.Equal(deliver.AnswerCorrelationId, decodedDeliver?.AnswerCorrelationId);
        Assert.Equal(typeof(Action<ISimpleParametersInterface>).FullName, decodedDeliver?.Consumer.Type.FullName);
    }
        
    [Fact]
    public void TestThatEncodesRelocateMessage()
    {
        Expression<Action<ISimpleParametersInterface>> consumer = actor => actor.DoSomething("Test Consumer", 100);
        var deliver1 = new GridDeliver(
            typeof(ISimpleParametersInterface),
            new GridAddress(Guid.NewGuid()),
            Definition.SerializationProxy.From(
                Definition.Has<ISimpleParametersInterface>(() => new ActorTest())),
            consumer,
            Guid.NewGuid(),
            "GridDeliver representation 1");
        var deliver2 = new GridDeliver(
            typeof(ISimpleParametersInterface),
            new GridAddress(Guid.NewGuid()),
            Definition.SerializationProxy.From(
                Definition.Has<ISimpleParametersInterface>(() => new ActorTest())),
            consumer,
            Guid.NewGuid(),
            "GridDeliver representation 2");
        var delivers = new List<GridDeliver>
        {
            deliver1,
            deliver2
        };
        var relocate = new Relocate(
            typeof(ISimpleParametersInterface),
            new GridAddress(Guid.NewGuid()),
            Definition.SerializationProxy
                .From(Definition.Has<ISimpleParametersInterface>(() => new ActorTest())),
            new object(),
            delivers);
        var encoder = new JsonEncoder();
        var decoder = new JsonDecoder();

        var encoded = encoder.Encode(relocate);

        var decoded = (Relocate)decoder.Decode(encoded);

        Assert.Equal(relocate.Address, decoded?.Address);
        Assert.Equal(relocate.Definition, decoded?.Definition);
        Assert.Equal(2, decoded?.Pending.Count);
        Assert.NotNull(decoded?.Snapshot);
        var decoded1 = decoded.Pending[0];
        Assert.Equal(deliver1.Address, decoded1.Address);
        Assert.Equal(deliver1.Representation, decoded1.Representation);
        Assert.Equal(deliver1.AnswerCorrelationId, decoded1.AnswerCorrelationId);
        Assert.Equal(typeof(Action<ISimpleParametersInterface>).FullName, decoded1.Consumer.Type.FullName);
        var decoded2 = decoded.Pending[1];
        Assert.Equal(deliver2.Address, decoded2.Address);
        Assert.Equal(deliver2.Representation, decoded2.Representation);
        Assert.Equal(deliver2.AnswerCorrelationId, decoded2.AnswerCorrelationId);
        Assert.Equal(typeof(Action<ISimpleParametersInterface>).FullName, decoded2.Consumer.Type.FullName);
    }
        
    [Fact]
    public void TestThatEncodesStartMessage()
    {
        var start = new Start(
            typeof(ISimpleParametersInterface),
            new GridAddress(Guid.NewGuid()),
            Definition.SerializationProxy
                .From(Definition.Has<ISimpleParametersInterface>(() => new ActorTest())));
        var encoder = new JsonEncoder();
        var decoder = new JsonDecoder();

        var encoded = encoder.Encode(start);

        var decoded = (Start)decoder.Decode(encoded);

        Assert.Equal(start.Address, decoded?.Address);
        Assert.Equal(start.Definition, decoded?.Definition);
    }
        
    [Fact]
    public void TestThatEncodesUnAckMessage()
    {
        Expression<Action<ISimpleParametersInterface>> consumer = actor => actor.DoSomething("Test Consumer", 100);
        var deliver = new GridDeliver(
            typeof(ISimpleParametersInterface),
            new GridAddress(Guid.NewGuid()),
            Definition.SerializationProxy.From(
                Definition.Has<ISimpleParametersInterface>(() => new ActorTest())),
            consumer,
            Guid.NewGuid(),
            "GridDeliver representation");
        var unack = new UnAckMessage(
            typeof(ISimpleParametersInterface),
            Id.Of(50),
            new BasicCompletes<ISimpleParametersInterface>(new SimpleParametersInterface(null)), deliver);
        var encoder = new JsonEncoder();
        var decoder = new JsonDecoder();

        var encoded = encoder.Encode(unack);

        var decoded = (UnAckMessage) decoder.Decode(encoded);

        var decodedDeliver = decoded?.Message;
            
        Assert.Equal(unack.Receiver, decoded?.Receiver);
        Assert.Equal(deliver.Address, decodedDeliver?.Address);
        Assert.Equal(deliver.Representation, decodedDeliver?.Representation);
        Assert.Equal(deliver.AnswerCorrelationId, decodedDeliver?.AnswerCorrelationId);
        Assert.Equal(typeof(Action<ISimpleParametersInterface>).FullName, decodedDeliver?.Consumer.Type.FullName);
    }
}
    
public interface ISimpleParametersInterface
{
    void DoSomething(string message, int count);
}
    
public class SimpleParametersInterface : ISimpleParametersInterface
{
    public SimpleParametersInterface(string complex)
    {
            
    }
        
    public string Message { get; private set; }
    public int Count { get; private set; }
        
    public void DoSomething(string message, int count)
    {
        Message = message;
        Count = count;
    }
}
    
public interface IParameterlessInterface
{
    void DoSomething();
}

public interface IComplexParameterInterface
{
    void DoSomething(int count, ComplexParameters parameters);
}
    
public class ActorTest : Actor, IParameterlessInterface, ISimpleParametersInterface, IComplexParameterInterface
{
    public bool WasRun { get; private set; }
    public string Message { get; private set; }
    public int Count { get; private set; }
    public ComplexParameters Parameters { get; private set; }
        
    public void DoSomething() => WasRun = true;

    public void DoSomething(string message, int count)
    {
        Message = message;
        Count = count;
    }

    public void DoSomething(int count, ComplexParameters parameters)
    {
        Count = count;
        Parameters = parameters;
    }
}

[Serializable]
public class ComplexParameters
{
    public int Int { get; set; }
        
    public string Message { get; set; }
        
    public ComplexParameters InnerParameters { get; set; }
        
    protected bool Equals(ComplexParameters other) => 
        Int == other.Int 
        && Message == other.Message
        && Equals(InnerParameters, other.InnerParameters);

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ComplexParameters) obj);
    }

    public override int GetHashCode() => HashCode.Combine(Int, Message, InnerParameters);
}