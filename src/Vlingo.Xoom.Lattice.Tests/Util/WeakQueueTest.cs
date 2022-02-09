// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using Vlingo.Xoom.Lattice.Util;
using Xunit;

namespace Vlingo.Xoom.Lattice.Tests.Util
{
    public class WeakQueueTest
    {
        // Test sample generation and utilities

        private List<Sample> GenerateSamples(int size) => 
            Enumerable.Range(0, size).Select(i => new Sample(i, new object())).ToList();

        private List<T> FMap<T>(List<Sample> samples, Func<Sample, T> fun) => samples.Select(fun).ToList();
        
        // Generic assertion methods

        private void AssertFifo<T>(List<T> elements, Func<T> testFun) =>
            elements.ForEach(e => Assert.Equal(e, testFun()));
        
        // WeakQueue should not permit nulls

        [Fact]
        public void TestEnqueuePreventsNulls()
        {
            var queue = new WeakQueue<object>();
            Assert.Throws<ArgumentNullException>(() => queue.Enqueue(null));
        }

        [Fact]
        public void TestEnqueueAllPreventsNulls()
        {
            var queue = new WeakQueue<object>();
            var addAll = new List<object>(2);
            addAll.Add(new object());
            addAll.Add(null);
            Assert.Throws<ArgumentNullException>(() => queue.EnqueueAll(addAll));
        }
        
        // WeakQueue should never throw on enqueue of non-null item

        [Fact]
        public void TestEnqueueNeverThrowsOnNonNullArgument()
        {
            var queue = new WeakQueue<object>();
            GenerateSamples(1000).ForEach(queue.Enqueue);
        }
        
        [Fact]
        public void TestEnqueueAllNeverThrowsOnNonNullArgument()
        {
            var queue = new WeakQueue<object>();
            queue.EnqueueAll(GenerateSamples(1000));
        }
        
        // WeakQueue should always present elements in FIFO order

        [Fact]
        public void TestPollPresentsElementsInFIFOOrder()
        {
            var queue = new WeakQueue<object>();
            var samples = FMap(GenerateSamples(1000), s => s.Object);
            queue.EnqueueAll(samples);
            AssertFifo(samples, queue.Poll);
        }
        
        // WeakQueue should obey the Queue API protocol

        [Fact]
        public void TestPollReturnsNullOnEmptyQueue()
        {
            var queue = new WeakQueue<object>();
            Assert.Null(queue.Poll());
        }
        
        [Fact]
        public void TestPollRetrievesAndRemoves()
        {
            var queue = new WeakQueue<object>();
            var @object = new object();
            queue.Enqueue(@object);
            Assert.Equal(@object, queue.Poll());
            Assert.Null(queue.Poll());
        }
        
        [Fact]
        public void TestDequeueThrowsOnEmptyQueue()
        {
            var queue = new WeakQueue<object>();
            Assert.Throws<InvalidOperationException>(() => queue.Dequeue());
        }
        
        [Fact]
        public void TestDequeueRetrievesAndDequeue()
        {
            var queue = new WeakQueue<object>();
            var @object = new object();
            queue.Enqueue(@object);
            Assert.Equal(@object, queue.Dequeue());
            Assert.Null(queue.Poll());
        }
        
        [Fact]
        public void TestPeekReturnsNullOnEmptyQueue()
        {
            var queue = new WeakQueue<object>();
            Assert.Null(queue.Peek());
        }
        
        [Fact]
        public void TestPeekRetrievesButDoesNotRemove()
        {
            var queue = new WeakQueue<object>();
            var @object = new object();
            queue.Enqueue(@object);
            Assert.Equal(@object, queue.Peek());
            Assert.Equal(@object, queue.Peek());
        }
        
        // WeakQueue should not present Garbage Collected elements

        [Fact]
        public void TestPollExpungesGCedElementsPreservingFIFOOrder()
        {
            var queue = new WeakQueue<object>();

            var samples = GenerateSamples(1000);

            // keep hard references to only half of the samples
            var hard = samples
                .Where(p => p.Index % 2 == 0)
                .Select(p => p.Object)
                .ToList();

            samples.ForEach(p => queue.Enqueue(p.Object));

            // dereference samples
            foreach (var sample in samples)
            {
                sample.Object = null;
            }
            samples = null;

            // force GC
            GC.Collect();
            GC.WaitForPendingFinalizers();

            AssertFifo(hard, queue.Poll);
        }

        // WeakQueue should obey the Collection API protocol TODO

        // WeakQueue should be thread-safe TODO
        
        // Internal helper classes
        
        private class Sample
        {
            public int Index { get; }
            public object Object { get; set; }

            internal Sample(int index, object @object)
            {
                Index = index;
                Object = @object;
            }
        }
    }
}