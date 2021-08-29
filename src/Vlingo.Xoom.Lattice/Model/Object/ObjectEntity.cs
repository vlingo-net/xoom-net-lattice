// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Text;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common;
using Vlingo.Xoom.Symbio;
using Vlingo.Xoom.Symbio.Store;
using Vlingo.Xoom.Symbio.Store.Object;

namespace Vlingo.Xoom.Lattice.Model.Object
{
    /// <summary>
    ///     Abstract base type used to preserve and restore object state
    ///     by means of the <see cref="IObjectStore" />. The <see cref="IObjectStore" />
    ///     is typically backed by some form of object-relational mapping,
    ///     whether formally or informally implemented.
    /// </summary>
    /// <typeparam name="T">The type of persistent object</typeparam>
    public abstract class ObjectEntity<T> : EntityActor, IPersistResultInterest, IQueryResultInterest
        where T : StateObject
    {
        private readonly Info<T> _info;
        private readonly IPersistResultInterest _persistResultInterest;
        private readonly IQueryResultInterest _queryResultInterest;
        private QueryExpression? _queryExpression;

        /// <summary>
        ///     Construct my default state using my <see cref="IAddress" /> as my <code>Id</code>
        /// </summary>
        protected ObjectEntity() : this(null)
        {
        }

        /// <summary>
        ///     Construct my default state.
        /// </summary>
        /// <param name="id">Unique identity of this entity</param>
        protected ObjectEntity(string? id)
        {
            Id = !string.IsNullOrWhiteSpace(id) ? id : Address.IdString;
            _info = Info();
            _persistResultInterest = SelfAs<IPersistResultInterest>();
            _queryResultInterest = SelfAs<IQueryResultInterest>();
        }

        protected string? Id { get; set; }

        /// <summary>
        ///     Gets the <see cref="Metadata" />. Must override if <see cref="Metadata" /> is to be supported.
        /// </summary>
        protected Metadata Metadata => Metadata.NullMetadata();

        /// <summary>
        ///     Received by my extender when I must access its state object.
        ///     Must be overridden by my extender.
        /// </summary>
        protected abstract T StateObject { get; }

        //=====================================
        // FOR INTERNAL USE ONLY.
        //=====================================

        public void PersistResultedIn(IOutcome<StorageException, Result> outcome, object? stateObject, int possible, int actual, object? @object)
        {
            outcome
                .AndThen(result =>
                {
                    OnStateObject((T) stateObject!);
                    AfterApply();
                    CompleteUsing(@object);
                    DisperseStowedMessages();
                    return result;
                })
                .Otherwise(cause =>
                {
                    DisperseStowedMessages();
                    var message = $"State not preserved for: {GetType()}({Id}) because: {cause.Result} with: {cause.Message}";
                    Logger.Error(message, cause);
                    throw new InvalidOperationException(message, cause);
                });
        }

        public void QueryAllResultedIn(IOutcome<StorageException, Result> outcome, QueryMultiResults results, object? @object)
        {
            throw new NotImplementedException("Must be unreachable: QueryAllResultedIn()");
        }

        public void QueryObjectResultedIn(IOutcome<StorageException, Result> outcome, QuerySingleResult queryResult, object? @object)
        {
            outcome
                .AndThen(result =>
                {
                    OnStateObject((T) queryResult.StateObject!);
                    DisperseStowedMessages();
                    return result;
                })
                .Otherwise(cause =>
                {
                    DisperseStowedMessages();
                    var ignoreNotFound = (bool) @object!;
                    if (!ignoreNotFound)
                    {
                        var message =
                            $"State not restored for: {GetType()}({Id}) because: {cause.Result} with: {cause.Message}";
                        Logger.Error(message, cause);
                        throw new InvalidOperationException(message, cause);
                    }

                    return cause.Result;
                });
        }

        /// <inheritdoc />
        public override void Start()
        {
            base.Start();

            var newState = WhenNewState();

            if (newState == null)
            {
                Restore(true); // ignore not found (possible first time start)
            }
            else
            {
                Apply<string, T>(newState.Item1, newState.Item2, null);
            }
        }

        /// <summary>
        ///     Gets <see cref="ICompletes{TResult}" />, applying <paramref name="state" /> and <paramref name="sources" />,
        ///     dispatching to <code>State(T state)</code> when completed, and supply an
        ///     eventual outcome by means of the given <code>andThen</code> function.
        /// </summary>
        /// <param name="state">The state to preserve.</param>
        /// <param name="sources">The <see cref="T:IEnumerable{Source{TSource}}" /> instances to apply.</param>
        /// <param name="metadata">The Metadata to apply along with source</param>
        /// <param name="andThen">
        ///     The <see cref="CompletionSupplier{TResult}" /> that will provide the fully updated state following this operation,
        ///     and which will used to answer an eventual outcome to the client of this entity
        /// </param>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TResult">The return type of the Supplier function, which is the type of the completed state.</typeparam>
        /// <returns><see cref="ICompletes{TResult}" />.</returns>
        protected ICompletes<TResult> Apply<TSource, TResult>(T state, IEnumerable<Source<TSource>> sources, Metadata metadata, Func<TResult>? andThen)
        {
            var completionSupplier = CompletionSupplier<TResult>.SupplierOrNull(andThen, CompletesEventually());
            var completes = andThen == null ? null : Completes();
            StowMessages(typeof(IPersistResultInterest));
            _info.Store.Persist(StateSources<T, Source<TSource>>.Of(state, sources), metadata, _persistResultInterest, completionSupplier);
            return (ICompletes<TResult>) completes!;
        }

        /// <summary>
        ///     Gets <see cref="ICompletes{TResult}" />, applying <paramref name="state" /> and <paramref name="sources" />,
        ///     dispatching to <code>State(T state)</code> when completed, and supply an
        ///     eventual outcome by means of the given <code>andThen</code> function.
        /// </summary>
        /// <param name="state">The state to preserve.</param>
        /// <param name="sources">The <see cref="T:IEnumerable{Source{TSource}}" /> instances to apply.</param>
        /// <param name="andThen">
        ///     The <see cref="CompletionSupplier{TResult}" /> that will provide the fully updated state following this operation,
        ///     and which will used to answer an eventual outcome to the client of this entity
        /// </param>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TResult">The return type of the Supplier function, which is the type of the completed state.</typeparam>
        /// <returns><see cref="ICompletes{TResult}" />.</returns>
        protected ICompletes<TResult> Apply<TSource, TResult>(T state, IEnumerable<Source<TSource>> sources, Func<TResult>? andThen) =>
            Apply(state, sources, Metadata, andThen);

        /// <summary>
        ///     Gets <see cref="ICompletes{TResult}" />, applying <paramref name="state" /> and <paramref name="source" />,
        ///     dispatching to <code>State(T state)</code> when completed, and supply an
        ///     eventual outcome by means of the given <code>andThen</code> function.
        /// </summary>
        /// <param name="state">The state to preserve.</param>
        /// <param name="source">The <see cref="Source{TSource}" /> instance to apply.</param>
        /// <param name="metadata">The Metadata to apply along with source</param>
        /// <param name="andThen">
        ///     The <see cref="CompletionSupplier{TResult}" /> that will provide the fully updated state following this operation,
        ///     and which will used to answer an eventual outcome to the client of this entity
        /// </param>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TResult">The return type of the Supplier function, which is the type of the completed state.</typeparam>
        /// <returns><see cref="ICompletes{TResult}" />.</returns>
        protected ICompletes<TResult> Apply<TSource, TResult>(T state, Source<TSource> source, Metadata metadata, Func<TResult>? andThen)
        {
            var completionSupplier = CompletionSupplier<TResult>.SupplierOrNull(andThen, CompletesEventually());
            var completes = andThen == null ? null : Completes();
            StowMessages(typeof(IPersistResultInterest));
            _info.Store.Persist(StateSources<T, Source<TSource>>.Of(state, source), metadata, _persistResultInterest, completionSupplier);
            return (ICompletes<TResult>) completes!;
        }

        /// <summary>
        ///     Gets <see cref="ICompletes{TResult}" />, applying <paramref name="state" /> and <paramref name="source" />,
        ///     dispatching to <code>State(T state)</code> when completed, and supply an
        ///     eventual outcome by means of the given <code>andThen</code> function.
        /// </summary>
        /// <param name="state">The state to preserve.</param>
        /// <param name="source">The <see cref="Source{TSource}" /> instance to apply.</param>
        /// <param name="andThen">
        ///     The <see cref="CompletionSupplier{TResult}" /> that will provide the fully updated state following this operation,
        ///     and which will used to answer an eventual outcome to the client of this entity
        /// </param>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TResult">The return type of the Supplier function, which is the type of the completed state.</typeparam>
        /// <returns><see cref="ICompletes{TResult}" />.</returns>
        protected ICompletes<TResult> Apply<TSource, TResult>(T state, Source<TSource> source, Func<TResult>? andThen)
            => Apply(state, source, Metadata, andThen);

        /// <summary>
        ///     Gets <see cref="ICompletes{TResult}" />, applying <paramref name="state" />,
        ///     dispatching to <code>State(T state)</code> when completed, and supply an
        ///     eventual outcome by means of the given <code>andThen</code> function.
        /// </summary>
        /// <param name="state">The state to preserve.</param>
        /// <param name="andThen">
        ///     The <see cref="CompletionSupplier{TResult}" /> that will provide the fully updated state following this operation,
        ///     and which will used to answer an eventual outcome to the client of this entity
        /// </param>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TResult">The return type of the Supplier function, which is the type of the completed state.</typeparam>
        /// <returns><see cref="ICompletes{TResult}" />.</returns>
        protected ICompletes<TResult> Apply<TSource, TResult>(T state, Func<TResult>? andThen)
        {
            var completionSupplier = CompletionSupplier<TResult>.SupplierOrNull(andThen, CompletesEventually());
            var completes = andThen == null ? null : Completes();
            StowMessages(typeof(IPersistResultInterest));
            _info.Store.Persist(StateSources<T, Source<TSource>>.Of(state), _persistResultInterest, completionSupplier);
            return (ICompletes<TResult>) completes!;
        }

        /// <summary>
        ///     Applies the current <paramref name="state" /> and <paramref name="sources" />.
        /// </summary>
        /// <param name="state">The state to preserve.</param>
        /// <param name="sources">The <see cref="T:IEnumerable{Source{TSource}}" /> instances to apply.</param>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <returns><see cref="ICompletes{TResult}" />.</returns>
        protected ICompletes<TSource> Apply<TSource>(T state, IEnumerable<Source<TSource>> sources)
            => Apply(state, sources, Metadata, null as Func<TSource>);

        /// <summary>
        ///     Applies the current <paramref name="state" /> and <paramref name="source" />.
        /// </summary>
        /// <param name="state">The state to preserve.</param>
        /// <param name="source">The <see cref="Source{TSource}" /> instance to apply.</param>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <returns><see cref="ICompletes{TResult}" />.</returns>
        protected ICompletes<TSource> Apply<TSource>(T state, Source<TSource> source)
            => Apply(state, new List<Source<TSource>> {source}, Metadata, null as Func<TSource>);

        /// <summary>
        ///     Received after the full asynchronous evaluation of each <code>Apply()</code>.
        ///     Override if notification is desired.
        /// </summary>
        protected virtual void AfterApply()
        {
        }

        /// <summary>
        ///     Converts as single instance of <see cref="Source{T}" /> to the stream of sources
        /// </summary>
        /// <param name="source">The source to return as collection</param>
        /// <typeparam name="TNewSource">The type of the underlying source</typeparam>
        /// <returns>The stream of <see cref="T:IEnumerable{Source{T}}" /></returns>
        protected IEnumerable<Source<TNewSource>> AsList<TNewSource>(Source<TNewSource> source) => new List<Source<TNewSource>> {source};

        /// <summary>
        ///     Answer a representation of a number of segments as a
        ///     composite id. The implementor of <code>Id</code> would use
        ///     this method if the its id is built from segments.
        /// </summary>
        /// <param name="separator">The string separator the insert between segments</param>
        /// <param name="idSegments">String params of one or more segments.</param>
        /// <returns>Segments as single id.</returns>
        protected string IdFrom(string separator, params string[] idSegments)
        {
            var builder = new StringBuilder();
            builder.Append(idSegments[0]);
            for (var idx = 1; idx < idSegments.Length; ++idx) builder.Append(separator).Append(idSegments[idx]);
            return builder.ToString();
        }

        /// <summary>
        ///     Answer my new <code>state</code> and <code>sources</code> as a
        ///     <see cref="T:Tuple{T1, IEnumerable{Source{string}}}" />,
        ///     or <code>null</code> if not new. Used each time I am started to determine
        ///     whether restoration is necessary or otherwise initial state persistence.
        ///     By default I always attempt to restore my state while ignoring non-existence.
        /// </summary>
        /// <returns>
        ///     <see cref="T:Tuple{T, IEnumerable{Source{string}}}" />
        /// </returns>
        protected Tuple<T, IEnumerable<Source<string>>>? WhenNewState() => null;

        /// <summary>
        ///     Received by my extender when my state object has been preserved and restored.
        ///     Must be overridden by my extender.
        /// </summary>
        /// <param name="stateObject">The T typed state object.</param>
        protected abstract void OnStateObject(T stateObject);

        protected override void Restore() => Restore(false);

        private Info<T> Info()
        {
            try
            {
                var registry = ObjectTypeRegistry.ResolveObjectTypeRegistry(Stage.World);
                var info = registry.Info<T>();
                return info;
            }
            catch (Exception e)
            {
                var message = $"{GetType().Name}: Info not registered with ObjectTypeRegistry.";
                Logger.Error(message, e);
                throw new InvalidOperationException(message);
            }
        }

        private QueryExpression QueryExpression()
        {
            if (_queryExpression == null)
            {
                if (_info.QueryObjectExpression.IsListQueryExpression)
                    _queryExpression =
                        ListQueryExpression.Using(
                            _info.QueryObjectExpression.Type,
                            _info.QueryObjectExpression.Query,
                            StateObject.QueryList());
                else if (_info.QueryObjectExpression.IsMapQueryExpression)
                    _queryExpression =
                        MapQueryExpression.Using(
                            _info.QueryObjectExpression.Type,
                            _info.QueryObjectExpression.Query,
                            StateObject.QueryMap());
                else
                    throw new InvalidOperationException($"Unknown QueryExpression type: {_queryExpression}");
            }

            return _queryExpression;
        }

        /// <summary>
        /// Dispatches to the <paramref name="supplier"/> to complete my protocol
        /// </summary>
        /// <param name="supplier">The supplier to dispatch to.</param>
        private void CompleteUsing(object? supplier)
        {
            if (supplier != null)
            {
                ((CompletionSupplier<T>) supplier).Complete();
            }
        }
        
        /// <summary>
        ///     Cause state restoration and indicate whether a not found
        ///     condition can be safely ignored.
        /// </summary>
        /// <param name="ignoreNotFound">The boolean indicating whether or not a not found condition may be ignored</param>
        private void Restore(bool ignoreNotFound)
        {
            StowMessages(typeof(IQueryResultInterest));
            _info.Store.QueryObject(QueryExpression(), _queryResultInterest, ignoreNotFound);
        }
    }
}