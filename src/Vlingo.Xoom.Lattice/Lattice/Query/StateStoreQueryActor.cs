// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common;
using Vlingo.Xoom.Symbio;
using Vlingo.Xoom.Symbio.Store;
using Vlingo.Xoom.Symbio.Store.State;

namespace Vlingo.Xoom.Lattice.Query
{
    /// <summary>
    /// A building-block <see cref="Actor"/> that queries asynchronously for state by id.
    /// </summary>
    public abstract class StateStoreQueryActor<T> : Actor, ICompositeIdentitySupport, IReadResultInterest, IScheduled<RetryContext<T>>, IScheduled<RetryContext<ObjectState<T>>>
    {
        private readonly IReadResultInterest _readInterest;
        private readonly IStateStore _stateStore;
        
        /// <summary>
        /// Construct my final state with the <see cref="IStateStore"/>, which must
        /// be provided by my concrete extenders.
        /// </summary>
        /// <param name="stateStore">The <see cref="IStateStore"/> from which states are read</param>
        protected StateStoreQueryActor(IStateStore stateStore)
        {
            _stateStore = stateStore;
            _readInterest = SelfAs<IReadResultInterest>();
        }
        
        /// <summary>
        /// Returns a <code>ICompletes{ObjectState{T}}</code> of the eventual result of querying
        /// for the state of the <typeparamref name="T"/> and identified by <paramref name="id"/>.
        ///
        /// If the state is found, the <code>ObjectState{T}</code> will contain a valid <code>state</code>
        /// of the <typeparamref name="T"/>, the <code>stateVersion</code>, and <code>metadata</code>. The contents of
        /// the <code>metadata</code> depends on whether or not it as included in the corresponding <code>Write()</code> operation.
        ///
        /// If the state is not found, the <code>ObjectState{T}</code> will be <code>ObjectState{T}.Null</code>.
        /// 
        /// </summary>
        /// <param name="id">The unique identity of the state to query</param>
        /// <returns><see cref="ICompletes{TResult}"/></returns>
        protected ICompletes<ObjectState<T>> QueryObjectStateFor(string id) =>
            QueryFor(id, ResultType.ObjectState, ObjectState<T>.Null);

        /// <summary>
        /// Returns a <code>ICompletes{ObjectState{T}}</code> of the eventual result of querying
        /// for the state of the <typeparamref name="T"/> and identified by <paramref name="id"/>.
        /// 
        /// If the state is found, the <code>ObjectState{T}</code> will contain a valid <code>state</code>
        /// of the <typeparamref name="T"/>, the <code>stateVersion</code>, and <code>metadata</code>. The contents of
        /// the <code>metadata</code> depends on whether or not it as included in the corresponding <code>Write()</code> operation.
        /// 
        /// If the state is not found, the <code>ObjectState{T}</code> will be <paramref name="notFoundState"/>.
        /// 
        /// </summary>
        /// <param name="id">The unique identity of the state to query</param>
        /// <param name="notFoundState">The <typeparamref name="T"/> state to answer if the query doesn't find the desired state</param>
        /// <returns><see cref="ICompletes{TResult}"/></returns>
        protected ICompletes<ObjectState<T>> QueryObjectStateFor(string id, ObjectState<T> notFoundState) => 
            QueryFor(id, ResultType.ObjectState, notFoundState);

        /// <summary>
        /// Returns a <code>ICompletes{ObjectState{T}}</code> of the eventual result of querying
        /// for the state of the <typeparamref name="T"/> and identified by <paramref name="id"/>.
        /// 
        /// If the state is found, the <code>ObjectState{T}</code> will contain a valid <code>state</code>
        /// of the <typeparamref name="T"/>, the <code>stateVersion</code>, and <code>metadata</code>. The contents of
        /// the <code>metadata</code> depends on whether or not it as included in the corresponding <code>Write()</code> operation.
        /// 
        /// If the state is not found, the <code>ObjectState{T}</code> will be <code>ObjectState{T}.Null</code>.
        ///
        /// If the state is not found, the query will be retried up to <paramref name="retryTotal"/> times in <paramref name="retryInterval"/> intervals.
        /// 
        /// </summary>
        /// <param name="id">The unique identity of the state to query</param>
        /// <param name="retryInterval">The interval for retries if state is not found at first</param>
        /// <param name="retryTotal">The maximum number of retries if state is not found at first</param>
        /// <returns><see cref="ICompletes{TResult}"/></returns>
        protected ICompletes<ObjectState<T>> QueryObjectStateFor(string id, int retryInterval, int retryTotal) => 
            QueryObjectStateFor(id, null, retryInterval, retryTotal);

        /// <summary>
        /// Returns a <code>ICompletes{ObjectState{T}}</code> of the eventual result of querying
        /// for the state of the <typeparamref name="T"/> and identified by <paramref name="id"/>.
        /// 
        /// If the state is found, the <code>ObjectState{T}</code> will contain a valid <code>state</code>
        /// of the <typeparamref name="T"/>, the <code>stateVersion</code>, and <code>metadata</code>. The contents of
        /// the <code>metadata</code> depends on whether or not it as included in the corresponding <code>Write()</code> operation.
        /// 
        /// If the state is not found, the <code>ObjectState{T}</code> will be <paramref name="notFoundState"/>.
        /// 
        /// If the state is not found, the query will be retried up to <paramref name="retryTotal"/> times in <paramref name="retryInterval"/> intervals.
        /// 
        /// </summary>
        /// <param name="id">The unique identity of the state to query</param>
        /// <param name="notFoundState">The <typeparamref name="T"/> state to answer if the query doesn't find the desired state</param>
        /// <param name="retryInterval">The interval for retries if state is not found at first</param>
        /// <param name="retryTotal">The maximum number of retries if state is not found at first</param>
        /// <returns><see cref="ICompletes{TResult}"/></returns>
        protected ICompletes<ObjectState<T>> QueryObjectStateFor(string id, ObjectState<T>? notFoundState, int retryInterval, int retryTotal)
        {
            QueryWithRetries(new RetryContext<ObjectState<T>>(
                CompletesEventually(), 
                answer => 
                        QueryFor(id, ResultType.ObjectState, notFoundState, answer!), 
                notFoundState!, retryInterval, retryTotal));
            return (ICompletes<ObjectState<T>>) Completes();
        }
        
        protected ICompletes<IEnumerable<TResult>> AllOf<TResult>(IEnumerable<TResult> all) => QueryAllOf(all.ToList());

        /// <summary>
        /// Gets a <see cref="ICompletes{TResult}"/> of the eventual result of querying
        /// for the state of the <typeparamref name="T"/> and identified by <paramref name="id"/>.
        ///
        /// If the state is found, the outcome is the <typeparamref name="T"/> instance.
        ///
        /// If the state is not found, the outcome is <code>null</code>.
        /// 
        /// </summary>
        /// <param name="id">The unique identity of the state to query</param>
        /// <returns><see cref="ICompletes{TResult}"/></returns>
        protected ICompletes<T> QueryStateFor(string id) => QueryFor<T>(id, ResultType.Unwrapped, (T)(object) null!);

        /// <summary>
        /// Gets a <see cref="ICompletes{TResult}"/> of the eventual result of querying
        /// for the state of the <typeparamref name="T"/> and identified by <paramref name="id"/>.
        /// 
        /// If the state is found, the outcome is the <typeparamref name="T"/> instance.
        /// 
        /// If the state is not found, the outcome is <paramref name="notFoundState"/>.
        /// 
        /// </summary>
        /// <param name="id">The unique identity of the state to query</param>
        /// <param name="notFoundState">The <typeparamref name="T"/> state to answer if the query doesn't find the desired state</param>
        /// <returns><see cref="ICompletes{TResult}"/></returns>
        protected ICompletes<T> QueryStateFor(string id, T notFoundState) => QueryFor(id, ResultType.Unwrapped, notFoundState);

        /// <summary>
        /// Gets a <see cref="ICompletes{TResult}"/> of the eventual result of querying
        /// for the state of the <typeparamref name="T"/> and identified by <paramref name="id"/>.
        /// 
        /// If the state is found, the outcome is the <typeparamref name="T"/> instance.
        /// 
        /// If the state is not found, the outcome is <code>null</code>.
        ///
        /// If the state is not found, the query will be retried up to <paramref name="retryTotal"/> times in <paramref name="retryInterval"/> intervals.
        /// 
        /// </summary>
        /// <param name="id">The unique identity of the state to query</param>
        /// <param name="retryInterval">The interval for retries if state is not found at first</param>
        /// <param name="retryTotal">The maximum number of retries if state is not found at first</param>
        /// <returns><see cref="ICompletes{TResult}"/></returns>
        protected ICompletes<T> QueryStateFor(string id, int retryInterval, int retryTotal) =>
            QueryStateFor(id, (T)(object) null!, retryInterval, retryTotal);

        /// <summary>
        /// Gets a <see cref="ICompletes{TResult}"/> of the eventual result of querying
        /// for the state of the <typeparamref name="T"/> and identified by <paramref name="id"/>.
        /// 
        /// If the state is found, the outcome is the <typeparamref name="T"/> instance.
        /// 
        /// If the state is not found, the outcome is <paramref name="notFoundState"/>.
        /// 
        /// If the state is not found, the query will be retried up to <paramref name="retryTotal"/> times in <paramref name="retryInterval"/> intervals.
        /// 
        /// </summary>
        /// <param name="id">The unique identity of the state to query</param>
        /// <param name="notFoundState">The <typeparamref name="T"/> state to answer if the query doesn't find the desired state</param>
        /// <param name="retryInterval">The interval for retries if state is not found at first</param>
        /// <param name="retryTotal">The maximum number of retries if state is not found at first</param>
        /// <returns><see cref="ICompletes{TResult}"/></returns>
        protected ICompletes<TResult> QueryStateFor<TResult>(string id, TResult notFoundState, int retryInterval, int retryTotal)
        {
            QueryWithRetries(new RetryContext<TResult>(CompletesEventually(), answer => QueryFor(id, ResultType.Unwrapped, notFoundState, answer), notFoundState, retryInterval, retryTotal));
            return (ICompletes<TResult>) Completes();
        }

        public string DataIdFrom(string separator, params string[] idSegments) => CompositeIdentitySupport.DataIdFrom(separator, idSegments);

        public IEnumerable<string> DataIdSegmentsFrom(string separator, string dataId) => CompositeIdentitySupport.DataIdSegmentsFrom(separator, dataId);

        //==================================
        // ReadResultInterest
        //==================================
        
        public void ReadResultedIn<TState>(IOutcome<StorageException, Result> outcome, string? id, TState state, int stateVersion, Metadata? metadata, object? @object)
        {
            outcome.AndThen(result =>
            {
                QueryResultHandler<TState>.From(@object!).CompleteFoundWith(id!, state, stateVersion, metadata!);
                return result;
            }).Otherwise(cause =>
            {
                if (cause.Result == Result.NotFound)
                {
                    QueryResultHandler<TState>.From(@object!).CompleteNotFound();
                } 
                else
                {
                    Logger.Info($"Query state not read for update because: {cause.Message}", cause);
                }
                return cause.Result;
            });
        }

        public void ReadResultedIn<TState>(IOutcome<StorageException, Result> outcome, IEnumerable<TypedStateBundle> bundles, object? @object) => 
            throw new NotImplementedException();

        public void IntervalSignal(IScheduled<RetryContext<T>> scheduled, RetryContext<T> data) => QueryWithRetries(data);
        
        public void IntervalSignal(IScheduled<RetryContext<ObjectState<T>>> scheduled, RetryContext<ObjectState<T>> data)
            => QueryWithRetries(data);
        
        private void QueryWithRetries<TResult>(RetryContext<TResult> context)
        {
            Action<TResult> answer = maybeFoundState =>
            {
                if (context.NeedsRetry(maybeFoundState))
                {
                    Scheduler.ScheduleOnce(
                        SelfAs<IScheduled<RetryContext<TResult>>>(), context.NextTry(), TimeSpan.Zero, TimeSpan.FromMilliseconds(context.RetryInterval));
                }
                else
                {
                    context.Completes.With(maybeFoundState);
                }
            };
            
            context.Query(answer);
        }

        private ICompletes<TResult> QueryFor<TResult>(string id, ResultType resultType, TResult notFoundState)
        {
            var completes = CompletesEventually();
            QueryFor(id, resultType, notFoundState, maybeFoundState => completes.With(maybeFoundState));
            return (ICompletes<TResult>) Completes();
        }

        private void QueryFor<TResult>(string id, ResultType resultType, TResult notFoundState, Action<TResult> answer) => 
            _stateStore.Read<TResult>(id, _readInterest, new QueryResultHandler<TResult>(answer, resultType, notFoundState));
        
        private ICompletes<IEnumerable<TResult>> QueryAllOf<TResult>(List<TResult> all)
        {
            Action<StateBundle> populator = state => all.Add((TResult) state.Object!);

            var completes = CompletesEventually();
            Action<IEnumerable<TResult>> collector = collected => completes.With(collected);

            // final TerminalOperationConsumerSink sink =
            //     new TerminalOperationConsumerSink(populator, all, collector);

            //_stateStore.ReadAll<TResult>( .andFinallyConsume(stream -> stream.flowInto(sink));

            return (ICompletes<IEnumerable<TResult>>)Completes();
        }
    }

    public enum ResultType
    {
        ObjectState,
        Unwrapped
    }

    public class QueryResultHandler<T>
    {
        public Action<T> Consumer { get; }
        public T NotFoundState { get; }
        public ResultType ResultType { get; }
        
        public static QueryResultHandler<T> From(object handler) => (QueryResultHandler<T>) handler;

        public QueryResultHandler(Action<T> consumer, ResultType resultType, T notFoundState)
        {
            Consumer = consumer;
            ResultType = resultType;
            NotFoundState = notFoundState;
        }
        
        public void CompleteNotFound() => Consumer(NotFoundState);
        
        public void CompleteFoundWith(string id, T state, int stateVersion, Metadata metadata)
        {
            switch (ResultType)
            {
                case ResultType.ObjectState:
                    Consumer(state);
                    //Consumer((T)(object) new ObjectState<T>(id, typeof(T), 1, state, stateVersion, metadata));
                    break;
                case ResultType.Unwrapped:
                    Consumer(state);
                    break;
            }
        }
    }

    public class RetryContext<T>
    {
        public ICompletesEventually Completes { get; }
        public Action<Action<T>> Query { get; }
        public T NotFoundState { get; }
        public int RetryInterval { get; }
        public int RetriesLeft { get; }
        
        public RetryContext(ICompletesEventually completes, Action<Action<T>> query, T notFoundState, int retryInterval, int retriesLeft)
        {
            Completes = completes;
            Query = query;
            NotFoundState = notFoundState;
            RetryInterval = retryInterval;
            RetriesLeft = retriesLeft;
        }
        
        public RetryContext<T> NextTry() => new RetryContext<T>(Completes, Query, NotFoundState, RetryInterval, RetriesLeft - 1);

        public bool NeedsRetry(T maybeFoundState) => maybeFoundState == null || RetriesLeft > 0 && maybeFoundState.Equals(NotFoundState);
    }
}