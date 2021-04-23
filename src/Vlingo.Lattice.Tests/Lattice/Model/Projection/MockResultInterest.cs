// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Vlingo.Xoom.Common;
using Vlingo.Symbio;
using Vlingo.Symbio.Store;
using Vlingo.Symbio.Store.Dispatch;
using Vlingo.Symbio.Store.State;
using Vlingo.Xoom.Actors.TestKit;

namespace Vlingo.Tests.Lattice.Model.Projection
{
    public class MockResultInterest : IReadResultInterest, IWriteResultInterest, IConfirmDispatchedResultInterest
    {
        private readonly AtomicInteger _confirmDispatchedResultedIn = new AtomicInteger(0);
        private readonly AtomicInteger _readTextResultedIn = new AtomicInteger(0);
        private readonly AtomicInteger _writeTextResultedIn = new AtomicInteger(0);

        private readonly AtomicRefValue<Result> _textReadResult = new AtomicRefValue<Result>();
        private readonly AtomicRefValue<Result> _textWriteResult = new AtomicRefValue<Result>();
        private readonly ConcurrentQueue<Result> _textWriteAccumulatedResults = new ConcurrentQueue<Result>();
        private readonly AtomicReference<object> _stateHolder = new AtomicReference<object>();
        private readonly AtomicReference<Metadata> _metadataHolder = new AtomicReference<Metadata>();
        private readonly ConcurrentQueue<Exception> _errorCauses = new ConcurrentQueue<Exception>();
        
        public AccessSafely Access { get; private set; } = AccessSafely.AfterCompleting(0);
        
        public void ReadResultedIn<TState>(IOutcome<StorageException, Result> outcome, string? id, TState state, int stateVersion, Metadata? metadata, object? @object)
        {
            outcome
                .AndThen(result => {
                    Access.WriteUsing("readTextResultedIn", 1);
                    Access.WriteUsing("textReadResult", result);
                    Access.WriteUsing("stateHolder", state);
                    Access.WriteUsing("metadataHolder", metadata);
                    return result;
                })
                .Otherwise(cause => {
                    Access.WriteUsing("readTextResultedIn", 1);
                    Access.WriteUsing("textReadResult", cause.Result);
                    Access.WriteUsing("stateHolder", state);
                    Access.WriteUsing("metadataHolder", metadata);
                    Access.WriteUsing("errorCauses", cause);
                    return cause.Result;
                });
        }

        public void ReadResultedIn<TState>(IOutcome<StorageException, Result> outcome, IEnumerable<TypedStateBundle> bundles, object? @object)
        {
        }

        public void WriteResultedIn<TState, TSource>(IOutcome<StorageException, Result> outcome, string id, TState state, int stateVersion, IEnumerable<TSource> sources, object? @object)
        {
            outcome
                .AndThen(result => {
                    Access.WriteUsing("writeTextResultedIn", 1);
                    Access.WriteUsing("textWriteResult", result);
                    Access.WriteUsing("stateHolder", state);
                    Access.WriteUsing("textWriteAccumulatedResults", result);
                    return result;
                })
                .Otherwise(cause => {
                    Access.WriteUsing("writeTextResultedIn", 1);
                    Access.WriteUsing("textWriteResult", cause.Result);
                    Access.WriteUsing("stateHolder", state);
                    Access.WriteUsing("textWriteAccumulatedResults", cause.Result);
                    Access.WriteUsing("errorCauses", cause);
                    return cause.Result;
                });
        }

        public void ConfirmDispatchedResultedIn(Result result, string dispatchId) => 
            Access.WriteUsing("confirmDispatched", 1);

        public AccessSafely AfterCompleting(int times)
        {
            Access = AccessSafely.AfterCompleting(times);

            Access
                .WritingWith<int>("confirmDispatched", i => _confirmDispatchedResultedIn.IncrementAndGet())
                .WritingWith<int>("readTextResultedIn", i => _readTextResultedIn.IncrementAndGet())
                .WritingWith<int>("writeTextResultedIn", i => _writeTextResultedIn.IncrementAndGet())
                .WritingWith<Result>("textReadResult", i => _textReadResult.Set(i))
                .WritingWith<Result>("textWriteResult", i => _textWriteResult.Set(i))
                .WritingWith<object>("stateHolder", i => _stateHolder.Set(i))
                .WritingWith<Result>("textWriteAccumulatedResults", i => _textWriteAccumulatedResults.Enqueue(i))
                .WritingWith<Metadata>("metadataHolder", i => _metadataHolder.Set(i))
                .WritingWith<StorageException>("errorCauses", i => _errorCauses.Enqueue(i))
                .ReadingWith("confirmDispatched", () => _confirmDispatchedResultedIn.Get())
                .ReadingWith("readTextResultedIn", () => _readTextResultedIn.IncrementAndGet())
                .ReadingWith("writeTextResultedIn", () => _writeTextResultedIn.IncrementAndGet())
                .ReadingWith("textReadResult", () => _textReadResult.Get())
                .ReadingWith("textWriteResult", () => _textWriteResult.Get())
                .ReadingWith("stateHolder", () => _stateHolder.Get())
                .ReadingWith("textWriteAccumulatedResults", () =>
                { 
                    _textWriteAccumulatedResults.TryDequeue(out var writeResult);
                    return writeResult;
                })
                .ReadingWith("metadataHolder", () => _metadataHolder.Get())
                .ReadingWith("errorCauses", () =>
                {
                    _errorCauses.TryDequeue(out var exception);
                    return exception;
                });

            return Access;
        }
    }
}