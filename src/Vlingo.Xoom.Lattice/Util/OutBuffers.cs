// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Concurrent;
using System.Threading;
using Vlingo.Xoom.Wire.Nodes;

namespace Vlingo.Xoom.Lattice.Util;

public class OutBuffers
{
    private static readonly WeakQueue<Thread> Empty = new WeakQueue<Thread>();
        
    private readonly Func<WeakQueue<Thread>> _queueInitializer;
    private readonly ConcurrentDictionary<Id, WeakQueue<Thread>> _buffers;
        
    private readonly IHardRefHolder _holder;

    public OutBuffers(IHardRefHolder holder) : this(() => new WeakQueue<Thread>(), holder)
    {
    }

    public OutBuffers(Func<WeakQueue<Thread>> queueInitializer, IHardRefHolder holder, int size = 5, int concurrencyLevel = 4)
    {
        _queueInitializer = queueInitializer;
        _holder = holder;
        _buffers = new ConcurrentDictionary<Id, WeakQueue<Thread>>(concurrencyLevel, size);
    }


    public void Enqueue(Id id, Thread task)
    {
        if (!_buffers.ContainsKey(id))
        {
            _buffers.AddOrUpdate(id, valueId => _queueInitializer(), (updateId, queue) => _queueInitializer());
        }

        _holder?.HoldOnTo(task);
        _buffers.GetOrAdd(id, _queueInitializer()).Enqueue(task);
    }

    public WeakQueue<Thread> Queue(Id id) => _buffers.GetOrAdd(id, Empty);
}