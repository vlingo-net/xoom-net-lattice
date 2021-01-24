// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Text;
using Vlingo.Actors;
using Vlingo.Common;
using Vlingo.Lattice.Model.Sourcing;
using Vlingo.Symbio;
using Vlingo.Symbio.Store;
using Vlingo.Symbio.Store.Object;
using Vlingo.Symbio.Store.State;

namespace Vlingo.Lattice.Model.Stateful
{
    /// <summary>
    /// Abstract base for all entity types that require the id-clob/blob state storage
    /// typical of a CQRS Command Model and CQRS Query Model. Therefore, extend me
    /// for both your Command Model and CQRS Query Model, or for your CQRS Query Model
    /// only when your Command Model uses the <see cref="EventSourced"/>.
    /// </summary>
    /// <typeparam name="T">The type of the underlying state entry.</typeparam>
    public abstract class StatefulEntity<T> : EntityActor, IReadResultInterest, IWriteResultInterest where T : IEntry
    {
        protected string Id;

        private int _currentVersion;
        private readonly Info<T> _info;
        private readonly IReadResultInterest _readInterest;
        private readonly IWriteResultInterest _writeInterest;
        
        /// <summary>
        /// Construct my default state using my <code>address</code> as my <id>id</id>.
        /// </summary>
        protected StatefulEntity() : this(null)
        {
        }
        
        /// <summary>
        /// Construct my default state.
        /// </summary>
        /// <param name="id">The unique identity of this entity</param>
        protected StatefulEntity(string? id)
        {
            Id = id ?? Address.IdString;
            _currentVersion = 0;
            _info = Info();
            _readInterest = SelfAs<IReadResultInterest>();
            _writeInterest = SelfAs<IWriteResultInterest>();
        }

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
                Apply<string, T>(newState.Item1, newState.Item2, (Func<T>?) null);
            }
        }

        public void ReadResultedIn<TState>(IOutcome<StorageException, Result> outcome, string? id, TState state, int stateVersion, Metadata? metadata, object? @object)
        {
            outcome
                .AndThen(result => 
                {
                    State((T)(object) state!); //TODO: ugly but there is no direct conversion between TState and T
                    _currentVersion = stateVersion;
                    DisperseStowedMessages();
                    return result; 
                })
                .Otherwise(cause => 
                { 
                    DisperseStowedMessages();
                    var ignoreNotFound = (bool) @object!;
                    if (!ignoreNotFound) {
                       var message =
                           $"State not restored for: {GetType().Name}({id}) because: {cause.Result} with: {cause.Message}";
                        Logger.Error(message, cause);
                        throw new InvalidOperationException(message, cause);
                    }
                    return cause.Result;
                });
        }

        public void ReadResultedIn<TState>(IOutcome<StorageException, Result> outcome, IEnumerable<TypedStateBundle> bundles, object? @object)
        {
            foreach (var stateBundle in bundles)
            {
                ReadResultedIn(outcome, stateBundle.Id, stateBundle.State, stateBundle.StateVersion, stateBundle.Metadata, @object);
            }
        }

        public void WriteResultedIn<TState, TSource>(IOutcome<StorageException, Result> outcome, string id, TState state, int stateVersion, IEnumerable<Source<TSource>> sources, object? @object)        {
            outcome
                .AndThen(result =>
                {
                    State((T)(object) state!); //TODO: ugly but there is no direct conversion between TState and T
                    _currentVersion = stateVersion;
                    AfterApply();
                    CompleteUsing(@object);
                    DisperseStowedMessages();
                    return result;
                })
                .Otherwise(cause =>
                {
                    DisperseStowedMessages();
                    var message = $"State not applied for: {GetType().Name}({id}) because: {cause.Result} with: {cause.Message}";
                    Logger.Error(message, cause);
                    throw new InvalidOperationException(message, cause);
                });
        }

        /// <summary>
        ///     Gets <see cref="ICompletes{TResult}" />, applying <paramref name="state" /> and <paramref name="metadataValue" />,
        ///     that was modified due to the descriptive <paramref name="operation"/>, along with <paramref name="sources"/>, and
        ///     eventual outcome by means of the given <code>andThen</code> function.
        /// </summary>
        /// <param name="state">The state to apply.</param>
        /// <param name="sources">The <see cref="T:IEnumerable{Source{TSource}}" /> instances to apply.</param>
        /// <param name="metadataValue">The Metadata to apply along with the state.</param>
        /// <param name="operation">The descriptive name of the operation that caused the state modification.</param>
        /// <param name="andThen">
        ///     The <see cref="CompletionSupplier{TReturn}" /> that will provide the fully updated state following this operation,
        ///     and which will used to answer an eventual outcome to the client of this entity
        /// </param>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TResult">The return type of the Supplier function, which is the type of the completed state.</typeparam>
        /// <returns><see cref="ICompletes{TResult}" />.</returns>
        protected ICompletes<TResult> Apply<TSource, TResult>(T state, IEnumerable<Source<TSource>> sources, string? metadataValue, string? operation, Func<TResult>? andThen)
        {
            var metadata = Metadata.With(state, metadataValue ?? "", operation ?? "");
            var completionSupplier = CompletionSupplier<T>.SupplierOrNull(andThen, CompletesEventually());
            var completes = andThen == null ? null : Completes();
            StowMessages(typeof(IWriteResultInterest));
            _info.Store.Write(Id, state, NextVersion(), sources, metadata, _writeInterest, completionSupplier);
            return (ICompletes<TResult>) completes!;
        }

        /// <summary>
        ///     Gets <see cref="ICompletes{TResult}" />, applying <paramref name="state" /> and <paramref name="metadataValue" />,
        ///     that was modified due to the descriptive <paramref name="operation"/> and
        ///     eventual outcome by means of the given <code>andThen</code> function.
        /// </summary>
        /// <param name="state">The state to apply.</param>
        /// <param name="metadataValue">The Metadata to apply along with the state.</param>
        /// <param name="operation">The descriptive name of the operation that caused the state modification.</param>
        /// <param name="andThen">
        ///     The <see cref="CompletionSupplier{TReturn}" /> that will provide the fully updated state following this operation,
        ///     and which will used to answer an eventual outcome to the client of this entity
        /// </param>
        /// <typeparam name="TResult">The return type of the Supplier function, which is the type of the completed state.</typeparam>
        /// <returns><see cref="ICompletes{TResult}" />.</returns>
        protected ICompletes<TResult> Apply<TResult>(T state, string? metadataValue, string? operation, Func<TResult>? andThen)
        {
            var metadata = Metadata.With(state, metadataValue ?? "", operation ?? "");
            var completionSupplier = CompletionSupplier<T>.SupplierOrNull(andThen, CompletesEventually());
            var completes = andThen == null ? null : Completes();
            StowMessages(typeof(IWriteResultInterest));
            _info.Store.Write(Id, state, NextVersion(), metadata, _writeInterest, completionSupplier);
            return (ICompletes<TResult>) completes!;
        }

        /// <summary>
        ///     Gets <see cref="ICompletes{TResult}" />, applying <paramref name="state" />,
        ///     that was modified due to the descriptive <paramref name="operation"/>, along with <paramref name="sources"/>, and
        ///     eventual outcome by means of the given <code>andThen</code> function.
        /// </summary>
        /// <param name="state">The state to apply.</param>
        /// <param name="sources">The <see cref="T:IEnumerable{Source{TSource}}" /> instances to apply.</param>
        /// <param name="operation">The descriptive name of the operation that caused the state modification.</param>
        /// <param name="andThen">
        ///     The <see cref="CompletionSupplier{TReturn}" /> that will provide the fully updated state following this operation,
        ///     and which will used to answer an eventual outcome to the client of this entity
        /// </param>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TResult">The return type of the Supplier function, which is the type of the completed state.</typeparam>
        /// <returns><see cref="ICompletes{TResult}" />.</returns>
        protected ICompletes<TResult> Apply<TSource, TResult>(T state, IEnumerable<Source<TSource>> sources, string? operation, Func<TResult>? andThen)
            => Apply(state, sources, string.Empty, operation, andThen);

        /// <summary>
        ///     Gets <see cref="ICompletes{TResult}" />, applying <paramref name="state" />,
        ///     that was modified due to the descriptive <paramref name="operation"/>, and
        ///     eventual outcome by means of the given <code>andThen</code> function.
        /// </summary>
        /// <param name="state">The state to apply.</param>
        /// <param name="operation">The descriptive name of the operation that caused the state modification.</param>
        /// <param name="andThen">
        ///     The <see cref="CompletionSupplier{TReturn}" /> that will provide the fully updated state following this operation,
        ///     and which will used to answer an eventual outcome to the client of this entity
        /// </param>
        /// <typeparam name="TResult">The return type of the Supplier function, which is the type of the completed state.</typeparam>
        /// <returns><see cref="ICompletes{TResult}" />.</returns>
        protected ICompletes<TResult> Apply<TResult>(T state, string? operation, Func<TResult>? andThen)
            => Apply<TResult>(state, string.Empty, operation, andThen);
        
        /// <summary>
        ///     Gets <see cref="ICompletes{TResult}" />, applying <paramref name="state" />,
        ///     along with <paramref name="sources"/>, and eventual outcome by means of the given <code>andThen</code> function.
        /// </summary>
        /// <param name="state">The state to apply.</param>
        /// <param name="sources">The <see cref="T:IEnumerable{Source{TSource}}" /> instances to apply.</param>
        /// <param name="andThen">
        ///     The <see cref="CompletionSupplier{TReturn}" /> that will provide the fully updated state following this operation,
        ///     and which will used to answer an eventual outcome to the client of this entity
        /// </param>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TResult">The return type of the Supplier function, which is the type of the completed state.</typeparam>
        /// <returns><see cref="ICompletes{TResult}" />.</returns>
        protected ICompletes<TResult> Apply<TSource, TResult>(T state, IEnumerable<Source<TSource>> sources, Func<TResult>? andThen)
            => Apply(state, sources, string.Empty, string.Empty, andThen);

        /// <summary>
        ///     Gets <see cref="ICompletes{TResult}" />, applying <paramref name="state" />,
        ///     and eventual outcome by means of the given <code>andThen</code> function.
        /// </summary>
        /// <param name="state">The state to apply.</param>
        /// <param name="andThen">
        ///     The <see cref="CompletionSupplier{TReturn}" /> that will provide the fully updated state following this operation,
        ///     and which will used to answer an eventual outcome to the client of this entity
        /// </param>
        /// <typeparam name="TResult">The return type of the Supplier function, which is the type of the completed state.</typeparam>
        /// <returns><see cref="ICompletes{TResult}" />.</returns>
        protected ICompletes<TResult> Apply<TResult>(T state, Func<TResult>? andThen)
            => Apply<TResult>(state, string.Empty, string.Empty, andThen);

        /// <summary>
        ///     Gets <see cref="ICompletes{TResult}" />, applying <paramref name="state" /> and <paramref name="metadataValue" />,
        ///     that was modified due to the descriptive <paramref name="operation"/>, along with <paramref name="sources"/>.
        /// </summary>
        /// <param name="state">The state to apply.</param>
        /// <param name="sources">The <see cref="T:IEnumerable{Source{TSource}}" /> instances to apply.</param>
        /// <param name="metadataValue">The Metadata to apply along with the state.</param>
        /// <param name="operation">The descriptive name of the operation that caused the state modification.</param>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <returns><see cref="ICompletes{TResult}" />.</returns>
        protected ICompletes<TSource> Apply<TSource>(T state, IEnumerable<Source<TSource>> sources, string? metadataValue, string? operation)
            => Apply(state, sources, metadataValue, operation, (Func<TSource>) null!);

        /// <summary>
        ///     Gets <see cref="ICompletes{TResult}" />, applying <paramref name="state" /> and <paramref name="metadataValue" />,
        ///     that was modified due to the descriptive <paramref name="operation"/>.
        /// </summary>
        /// <param name="state">The state to apply.</param>
        /// <param name="metadataValue">The Metadata to apply along with the state.</param>
        /// <param name="operation">The descriptive name of the operation that caused the state modification.</param>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <returns><see cref="ICompletes{TResult}" />.</returns>
        protected ICompletes<TSource> Apply<TSource>(T state, string? metadataValue, string? operation)
            => Apply<TSource>(state, metadataValue, operation, null);
        
        /// <summary>
        ///     Gets <see cref="ICompletes{TResult}" />, applying <paramref name="state" />,
        ///     that was modified due to the descriptive <paramref name="operation"/>, along with <paramref name="sources"/>.
        /// </summary>
        /// <param name="state">The state to apply.</param>
        /// <param name="sources">The <see cref="T:IEnumerable{Source{TSource}}" /> instances to apply.</param>
        /// <param name="operation">The descriptive name of the operation that caused the state modification.</param>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <returns><see cref="ICompletes{TResult}" />.</returns>
        protected ICompletes<TSource> Apply<TSource>(T state, IEnumerable<Source<TSource>> sources, string? operation)
            => Apply<TSource>(state, string.Empty, operation, null);

        /// <summary>
        ///     Gets <see cref="ICompletes{TResult}" />, applying <paramref name="state" />,
        ///     that was modified due to the descriptive <paramref name="operation"/>.
        /// </summary>
        /// <param name="state">The state to apply.</param>
        /// <param name="operation">The descriptive name of the operation that caused the state modification.</param>
        /// <typeparam name="TSource">The return type of the source.</typeparam>
        /// <returns><see cref="ICompletes{TResult}" />.</returns>
        protected ICompletes<TSource> Apply<TSource>(T state, string? operation)
            => Apply<TSource>(state, string.Empty, operation, null);

        /// <summary>
        ///     Gets <see cref="ICompletes{TResult}" />, applying <paramref name="state" />,
        ///     along with <paramref name="sources"/>.
        /// </summary>
        /// <param name="state">The state to apply.</param>
        /// <param name="sources">The <see cref="T:IEnumerable{Source{TSource}}" /> instances to apply.</param>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <returns><see cref="ICompletes{TResult}" />.</returns>
        protected ICompletes<TSource> Apply<TSource>(T state, IEnumerable<Source<TSource>> sources)
            => Apply(state, sources, string.Empty, string.Empty, (Func<TSource>?) null);
        
        /// <summary>
        ///     Gets <see cref="ICompletes{TResult}" />, applying <paramref name="state" />,
        ///     along with <paramref name="source"/>.
        /// </summary>
        /// <param name="state">The state to apply.</param>
        /// <param name="source">The <see cref="T:Source{TSource}" /> instance to apply.</param>
        /// <param name="andThen">
        ///     The <see cref="CompletionSupplier{TReturn}" /> that will provide the fully updated state following this operation,
        ///     and which will used to answer an eventual outcome to the client of this entity
        /// </param>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TResult">The return type of the Supplier function, which is the type of the completed state.</typeparam>
        /// <returns><see cref="ICompletes{TResult}" />.</returns>
        protected ICompletes<TResult> Apply<TSource, TResult>(T state, Source<TSource> source, Func<TResult>? andThen)
            => Apply(state, AsList(source), string.Empty, string.Empty, andThen);
        
        /// <summary>
        ///     Gets <see cref="ICompletes{TResult}" />, applying <paramref name="state" />,
        ///     along with <paramref name="source"/>.
        /// </summary>
        /// <param name="state">The state to apply.</param>
        /// <param name="source">The <see cref="T:Source{TSource}" /> instance to apply.</param>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <returns><see cref="ICompletes{TResult}" />.</returns>
        protected ICompletes<TSource> Apply<TSource>(T state, Source<TSource> source)
            => Apply(state, AsList(source), string.Empty, string.Empty, (Func<TSource>?) null);

        /// <summary>
        ///     Gets <see cref="ICompletes{TResult}" />, applying <paramref name="state" />.
        /// </summary>
        /// <param name="state">The state to apply.</param>
        /// <typeparam name="TResult">The return type of the Supplier function, which is the type of the completed state.</typeparam>
        /// <returns><see cref="ICompletes{TResult}" />.</returns>
        protected ICompletes<TResult> Apply<TResult>(T state)
            => Apply<TResult>(state, string.Empty, string.Empty, null);
        
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
        
        /// <summary>
        /// Received by my extender when my current state has been applied and restored.
        /// Must be overridden by my extender.
        /// </summary>
        /// <param name="state">The <typeparamref name="T"/> typed state</param>
        protected abstract void State(T state);

        private Info<T> Info()
        {
            try
            {
                var registry = Stage.World.ResolveDynamic<StatefulTypeRegistry<T>>(StatefulTypeRegistry<T>.InternalName);
                var info = registry.Info();
                return info;
            }
            catch (Exception e)
            {
                var message = $"{GetType().Name}: Info not registered with StatefulTypeRegistry.";
                Logger.Error(message, e);
                throw new InvalidOperationException(message);
            }
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
        /// Gets my <code>nextVersion</code>, which is one greater than my <code>currentVersion</code>.
        /// </summary>
        /// <returns></returns>
        private int NextVersion() => _currentVersion + 1;
        
        /// <summary>
        ///     Cause state restoration and indicate whether a not found
        ///     condition can be safely ignored.
        /// </summary>
        /// <param name="ignoreNotFound">The boolean indicating whether or not a not found condition may be ignored</param>
        private void Restore(bool ignoreNotFound)
        {
            StowMessages(typeof(IQueryResultInterest));
            _info.Store.Read<T>(Id, _readInterest, ignoreNotFound);
        }
    }
}