// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections;
using System.Collections.Generic;
using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Lattice.Util;

public class WeakQueue<T> where T : class
{
    private readonly AtomicBoolean _idle;

    private Queue<WeakReference<T>> _delegate;

    public WeakQueue() : this(new Queue<WeakReference<T>>())
    {
    }

    private WeakQueue(Queue<WeakReference<T>> @delegate)
    {
        _idle = new AtomicBoolean(true);
        _delegate = @delegate;
    }
        
    private Queue<WeakReference<T>> GetDelegate()
    {
        ExpungeStaleEntries();
        return _delegate;
    }

    private void ExpungeStaleEntries()
    {
        var tempQueue = new Queue<WeakReference<T>>();

        foreach (var weak in _delegate)
        {
            if (weak.TryGetTarget(out _))
            {
                tempQueue.Enqueue(weak);
            }
        }

        _delegate = tempQueue;
    }

    private void Atomic(Action supplier)
    {
        try
        {
            while (_idle.CompareAndSet(true, false)) ;
            supplier();
        }
        finally
        {
            _idle.Set(true);
        }
    }
        
    private TA Atomic<TA>(Func<TA> supplier)
    {
        try
        {
            while (_idle.CompareAndSet(true, false)) ;
            return supplier();
        }
        finally
        {
            _idle.Set(true);
        }
    }

    private T ExpungeStaleEntryOnSupply(Func<WeakReference<T>> supplier)
    {
        return Optional.OfNullable(supplier())
            .Map(w =>
            {
                T? t = null;
                if (w.TryGetTarget(out var target)) t = target;

                return Optional.OfNullable(t)
                    .OrElseGet(() =>
                        ExpungeStaleEntryOnSupply(supplier));
            })
            .OrElse(default!);
    }
        
    public void Enqueue(T t)
    {
        if (t == null)
        {
            throw new ArgumentNullException(nameof(t), "Null entries not allowed");
        }
            
        Atomic(() => GetDelegate().Enqueue(new WeakReference<T>(t)));
    }

    public void EnqueueAll(IEnumerable<T> values)
    {
        foreach (var value in values)
        {
            Enqueue(value);
        }
    }

    public T? Poll()
    {
        try
        {
            return Atomic(() => ExpungeStaleEntryOnSupply(GetDelegate().Dequeue));
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }
        
    public T Dequeue() => Atomic(() => ExpungeStaleEntryOnSupply(GetDelegate().Dequeue));

    public T? Peek()
    {
        try
        {
            return Atomic(() => ExpungeStaleEntryOnSupply(_delegate.Peek));
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    public IEnumerator<T> GetEnumerator() => new ExpungingEnumerator(this, _delegate.GetEnumerator());

    public int Count => _delegate.Count;

    private class ExpungingEnumerator : IEnumerator<T>
    {
        private readonly WeakQueue<T> _queue;
        private Queue<WeakReference<T>>.Enumerator _enumerator;

        private T _next = null!;

        public ExpungingEnumerator(WeakQueue<T> queue, Queue<WeakReference<T>>.Enumerator enumerator)
        {
            _queue = queue;
            _enumerator = enumerator;
        }

        public bool MoveNext()
        {
            _next = _queue.ExpungeStaleEntryOnSupply(Iterate);
            return _next != null;
        }

        public void Reset() => throw new InvalidOperationException("Cannot call Reset() on this queue.");

        public T Current => _next;

        object? IEnumerator.Current => Current;

        public void Dispose() => _enumerator.Dispose();

        private WeakReference<T> Iterate()
        {
            if (_enumerator.MoveNext())
            {
                return _enumerator.Current;
            }

            return null!;
        }
    }
}