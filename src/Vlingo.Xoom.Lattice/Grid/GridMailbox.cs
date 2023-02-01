// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common;
using Vlingo.Xoom.Lattice.Grid.Application;
using Vlingo.Xoom.Lattice.Grid.Hashring;
using Vlingo.Xoom.Wire.Nodes;

namespace Vlingo.Xoom.Lattice.Grid;

public class GridMailbox : IMailbox
{
    private readonly ILogger _logger;
    private readonly IMailbox _local;
    private readonly Id _localId;
    private readonly IAddress _address;

    private readonly IHashRing<Id> _hashRing;

    private readonly IOutbound _outbound;
        
    private static readonly ISet<Type> Overrides = new HashSet<Type> {typeof(IStoppable) };
        
    public GridMailbox(IMailbox local, Id localId, IAddress address, IHashRing<Id> hashRing, IOutbound outbound, ILogger logger)
    {
        _local = local;
        _localId = localId;
        _address = address;
        _hashRing = hashRing;
        _outbound = outbound;
        _logger = logger;
    }
        
    private void DelegateUnlessIsRemote(Action<Id> remote, IRunnable consumer)
    {
        if (!_address.IsDistributable)
        {
            consumer.Run();
            return;
        }
            
        var nodeOf = _hashRing.NodeOf(_address.IdString);
        if (nodeOf == null || nodeOf.Equals(_localId))
        {
            consumer.Run();
        }
        else
        {
            remote(nodeOf);
        }
    }
        
    private void DelegateUnlessIsRemote(Action<Id> remote, Action consumer)
    {
        if (!_address.IsDistributable)
        {
            consumer();
            return;
        }
            
        var nodeOf = _hashRing.NodeOf(_address.IdString);
        if (nodeOf == null || nodeOf.Equals(_localId))
        {
            consumer();
        }
        else
        {
            remote(nodeOf);
        }
    }
        
    private TResult DelegateUnlessIsRemote<TResult>(Func<Id, TResult> remote, Func<TResult> supplier)
    {
        if (!_address.IsDistributable)
        {
            return supplier();
        }
            
        var nodeOf = _hashRing.NodeOf(_address.IdString);
        if (nodeOf == null || nodeOf.Equals(_localId))
        {
            return supplier();
        }

        return remote(nodeOf);
    }

    public void Run()
    {
        DelegateUnlessIsRemote(nodeOf => {
            _logger.Debug($"Remote.Run on: {nodeOf}");
            _local.Run();
        }, _local);
    }

    public void Close()
    {
        DelegateUnlessIsRemote(nodeOf => {
            _logger.Debug($"Remote.Close on: {nodeOf}");
            _local.Close();
        }, () => _local.Close());
    }

    public TaskScheduler TaskScheduler => _local.TaskScheduler;

    public bool IsClosed => DelegateUnlessIsRemote(nodeOf => {
        _logger.Debug($"Remote.IsClosed on: {nodeOf}");
        return _local.IsClosed;
    }, () => _local.IsClosed);
        
    public bool IsDelivering => DelegateUnlessIsRemote(nodeOf => {
        _logger.Debug($"Remote.IsDelivering on: {nodeOf}");
        return _local.IsDelivering;
    }, () => _local.IsDelivering);

    public int ConcurrencyCapacity => DelegateUnlessIsRemote(nodeOf => {
        _logger.Debug($"Remote.ConcurrencyCapacity on: {nodeOf}");
        return _local.ConcurrencyCapacity;
    }, () => _local.ConcurrencyCapacity);

    public void Resume(string name) =>
        DelegateUnlessIsRemote(nodeOf => {
            _logger.Debug($"Remote.Resume on: {nodeOf}");
            _local.Resume(name);
        }, () => _local.Resume(name));

    public void Send(IMessage message)
    {
        DelegateUnlessIsRemote(nodeOf => {
            _logger.Debug($"Remote.Send(Message) on: {nodeOf}");
            if (Overrides.Contains(message.Protocol))
            {
                _local.Send(message);
            }
            _outbound.GridDeliver(
                nodeOf, _localId, message.Completes, message.Protocol,
                _address, Definition.SerializationProxy.From(message.Actor.Definition),
                message.SerializableConsumer!, message.Representation);
        }, () => _local.Send(message));
    }
        
    public void Send<T>(Actor actor, Action<T> consumer, ICompletes? completes, string representation)
    {
        DelegateUnlessIsRemote(nodeOf => {
            _logger.Debug($"Remote.Send(Actor, ...) on: {nodeOf}");
            if (Overrides.Contains(typeof(T)))
            {
                _local.Send(actor, consumer, completes, representation);
            }
            _outbound.GridDeliver(nodeOf, _localId, completes, typeof(T),
                _address, Definition.SerializationProxy.From(actor.Definition),
                consumer.ToSerializableExpression(), representation);
        }, () => _local.Send(actor, consumer, completes, representation));
    }
        
    public void SuspendExceptFor(string name, params Type[] overrides) => _local.SuspendExceptFor(name, overrides);

    public bool IsSuspendedFor(string name) => _local.IsSuspendedFor(name);
        
    public bool IsSuspended => DelegateUnlessIsRemote(nodeOf => false, () => _local.IsSuspended);

    public IMessage Receive() =>
        DelegateUnlessIsRemote(nodeOf => {
            _logger.Debug($"Remote.Receive on: {nodeOf}");
            return _local.Receive();
        }, () => _local.Receive())!;

    public int PendingMessages =>
        DelegateUnlessIsRemote(nodeOf => {
            _logger.Debug($"Remote.PendingMessages on: {nodeOf}");
            return _local.PendingMessages;
        }, () => _local.PendingMessages);
        
    public bool IsPreallocated => _local.IsPreallocated;

    public void Send(Actor actor, Type protocol, LambdaExpression consumer, ICompletes? completes, string representation)
    {
        DelegateUnlessIsRemote(nodeOf => {
            _logger.Debug($"Remote.Send(Actor, ...) on: {nodeOf}");
            if (Overrides.Contains(protocol))
            {
                _local.Send(actor, protocol, consumer, completes, representation);
            }
            _outbound.GridDeliver(nodeOf, _localId, completes, protocol,
                _address, Definition.SerializationProxy.From(actor.Definition),
                consumer, representation);
        }, () => _local.Send(actor, protocol, consumer, completes, representation));
    }
}