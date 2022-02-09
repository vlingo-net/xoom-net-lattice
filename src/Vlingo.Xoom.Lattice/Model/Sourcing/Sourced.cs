// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Actors.TestKit;
using Vlingo.Xoom.Common;
using Vlingo.Xoom.Symbio;
using Vlingo.Xoom.Symbio.Store;
using Vlingo.Xoom.Symbio.Store.Journal;

namespace Vlingo.Xoom.Lattice.Model.Sourcing
{
    /// <summary>
    /// Abstract base for all concrete types that support journaling and application of
    /// <see cref="Source{T}"/> instances. Provides abstracted <see cref="Journal{T}"/> and and state
    /// transition control for my concrete extender.
    /// </summary>
    /// <typeparam name="T">The concrete type that is being sourced</typeparam>
    public abstract class Sourced<T> : EntityActor, IAppendResultInterest
    {
        private readonly ConcurrentDictionary<Type, Dictionary<Type, Delegate>>
            _registeredConsumers = new ConcurrentDictionary<Type, Dictionary<Type, Delegate>>();

        private TestContext? _testContext;
        private int _currentVersion;
        private readonly Info? _journalInfo;
        private readonly IAppendResultInterest _interest;

        protected string StreamName { get; }

        /// <summary>
        /// Register the means to apply <typeparamref name="TSource"/> instances for state transition
        /// by means of a given <paramref name="consumer"/>.
        /// </summary>
        /// <param name="consumer">The consumer used to perform the application of <typeparamref name="TSource"/></param>
        /// <typeparam name="TSource">The <see cref="Source{T}"/> of the source to be applied</typeparam>
        public void RegisterConsumer<TSource>(Action<TSource> consumer) where TSource : ISource
        {
            if (!_registeredConsumers.TryGetValue(GetType(), out var sourcedTypeMap))
            {
                sourcedTypeMap = new Dictionary<Type, Delegate>();
                _registeredConsumers.AddOrUpdate(GetType(), sourcedTypeMap, (type, funcs) => sourcedTypeMap);
            }

            sourcedTypeMap.Add(typeof(TSource), consumer);
        }

        /// <summary>
        /// Construct my default state using my <code>address</code> as my <code>streamName</code>.
        /// </summary>
        protected Sourced() : this(null)
        {
        }

        /// <summary>
        /// Construct my default state.
        /// </summary>
        /// <param name="streamName">the string unique identity of this entity</param>
        protected Sourced(string? streamName)
        {
            StreamName = streamName != null ? streamName : Address.IdString;
            _currentVersion = 0;
            _journalInfo = Info();
            _interest = SelfAs<IAppendResultInterest>();
        }

        public override void Start()
        {
            base.Start();
            
            Restore();
        }

        public override void ViewTestStateInitialization(TestContext? context)
        {
            if (context != null)
            {
                _testContext = context;
                _testContext.InitialReferenceValueOf(new List<Source<T>>());
            }
        }

        public override TestState ViewTestState()
        {
            var testState = new TestState();
            if (_testContext != null) testState.PutValue("applied", _testContext.ReferenceValue<List<Source<T>>>());
            return testState;
        }

        public void AppendResultedIn<TSource, TSnapshotState>(IOutcome<StorageException, Result> outcome,
            string streamName, int streamVersion, TSource source,
            Optional<TSnapshotState> snapshot, object @object) where TSource : ISource =>
            AppendResultedIn(outcome, streamName, streamVersion, source, Metadata.NullMetadata(), snapshot, @object);

        public void AppendResultedIn<TSource, TSnapshotState>(IOutcome<StorageException, Result> outcome,
            string streamName, int streamVersion, TSource source,
            Metadata metadata, Optional<TSnapshotState> snapshot, object @object) where TSource : ISource
        {
            //TODO handle metadata
            outcome
                .AndThen(result => {
                    RestoreSnapshot(snapshot, _currentVersion);
                    ApplyResultVersioned(source);
                    AfterApply();
                    CompleteUsing(@object);
                    DisperseStowedMessages();
                    return result;
            })
            .Otherwise(cause => {
                var applicable = new Applicable<TSnapshotState>(default!, new ISource[] { source }, metadata, (CompletionSupplier<TSnapshotState>) @object);
                var message = $"Source (count 1) not appended for: {GetType().Name}({StreamName}) because: {cause.Result} with: {cause.Message}";
                var exception = new ApplyFailedException<TSnapshotState>(applicable, message, cause);
                var maybeException = AfterApplyFailed(exception);
                DisperseStowedMessages();
                if (maybeException.IsPresent)
                {
                    Logger.Error(message, maybeException.Get());
                    throw maybeException.Get();
                }
                Logger.Error(message, exception);
                return cause.Result;
            }); 
        }

        public void AppendAllResultedIn<TSource, TSnapshotState>(IOutcome<StorageException, Result> outcome,
            string streamName, int streamVersion,
            IEnumerable<TSource> sources, Optional<TSnapshotState> snapshot, object @object) where TSource : ISource =>
            AppendAllResultedIn(outcome, streamName, streamVersion, sources, Metadata.NullMetadata(), snapshot, @object);

        public void AppendAllResultedIn<TSource, TSnapshotState>(IOutcome<StorageException, Result> outcome,
            string streamName, int streamVersion,
            IEnumerable<TSource> sources, Metadata metadata, Optional<TSnapshotState> snapshot, object @object)
            where TSource : ISource
        {
            //TODO handle metadata
            outcome
                .AndThen(result => {
                    RestoreSnapshot(snapshot, _currentVersion);
                    foreach (var source in sources)
                    {
                        ApplyResultVersioned(source);
                    }
                    AfterApply();
                    CompleteUsing(@object);
                    DisperseStowedMessages();
                    return result;
            })
            .Otherwise(cause => {
                var listSources = sources.ToList();
                var applicable = new Applicable<TSnapshotState>(default!, listSources.Cast<ISource>(), metadata, (CompletionSupplier<TSnapshotState>) @object);
                var message = $"Source (count {listSources.Count}) not appended for: {GetType().Name}({StreamName}) because: {cause.Result} with: {cause.Message}";
                var exception = new ApplyFailedException<TSnapshotState>(applicable, message, cause);
                var maybeException = AfterApplyFailed(exception);
                DisperseStowedMessages();
                if (maybeException.IsPresent)
                {
                    Logger.Error(message, maybeException.Get());
                    throw maybeException.Get();
                }
                Logger.Error(message, exception);
                return cause.Result;
            });
        }
        
        /// <summary>
        /// Apply all of the given <paramref name="source"/> to myself, which includes appending
        /// them to my journal and reflecting the representative changes to my state.
        /// </summary>
        /// <param name="source">Source to apply</param>
        protected void Apply(ISource source) => Apply(Wrap(source));
        
        /// <summary>
        /// Apply all of the given <paramref name="sources"/> to myself, which includes appending
        /// them to my journal and reflecting the representative changes to my state.
        /// </summary>
        /// <param name="sources">Sources to apply</param>
        protected void Apply(IEnumerable<ISource> sources) => Apply(sources, Metadata, (Func<object>) null!);
        
        /// <summary>
        /// Answer <see cref="ICompletes{TResult}"/>, applying all of the given <paramref name="sources"/> to myself,
        /// which includes appending them to my journal and reflecting the representative changes
        /// to my state, followed by the execution of a possible <paramref name="andThen"/>.
        /// </summary>
        /// <param name="sources">Sources to apply</param>
        /// <param name="andThen">The function executed following the application of sources</param>
        /// <typeparam name="TResult"></typeparam>
        /// <returns><see cref="ICompletes{TResult}"/></returns>
        protected ICompletes<TResult> Apply<TResult>(IEnumerable<ISource> sources, Func<TResult> andThen) => Apply(sources, Metadata, andThen);

        /// <summary>
        /// Answer <see cref="ICompletes{TResult}"/>, applying all of the given <paramref name="source"/> to myself,
        /// which includes appending them to my journal and reflecting the representative changes
        /// to my state, followed by the execution of a possible <paramref name="andThen"/>.
        /// </summary>
        /// <param name="source">Source to apply</param>
        /// <param name="andThen">The function executed following the application of sources</param>
        /// <typeparam name="TResult"></typeparam>
        /// <returns><see cref="ICompletes{TResult}"/></returns>
        protected ICompletes<TResult> Apply<TResult>(ISource source, Func<TResult> andThen) => Apply(Wrap(source), Metadata, andThen);
        
        /// <summary>
        /// Answer <see cref="ICompletes{TResult}"/>, applying all of the given <paramref name="sources"/> to myself,
        /// which includes appending them to my journal and reflecting the representative changes
        /// to my state, followed by the execution of a possible <paramref name="andThen"/>.
        /// </summary>
        /// <param name="sources">Sources to apply</param>
        /// <param name="metadata">The Metadata to apply along with source</param>
        /// <param name="andThen">The function executed following the application of sources</param>
        /// <typeparam name="TResult">The return type of the function andThen</typeparam>
        /// <returns><see cref="ICompletes{TResult}"/></returns>
        protected ICompletes<TResult> Apply<TResult>(IEnumerable<ISource> sources, Metadata metadata, Func<TResult>? andThen)
        {
            var listSources = sources.ToList();
            BeforeApply<ISource>(listSources);
            var journal = _journalInfo?.Journal;
            var completionSupplier = CompletionSupplier<TResult>.SupplierOrNull(andThen, CompletesEventually());
            var completes = andThen == null ? null : Completes();
            StowMessages(typeof(IAppendResultInterest));
            journal?.AppendAllWith<ISource, TResult>(StreamName, NextVersion, listSources, metadata, Snapshot<TResult>(), _interest, completionSupplier!);
            return (ICompletes<TResult>) completes!;
        }
        
        protected virtual void BeforeApply<TSource>(IEnumerable<ISource> sources)
        {
            // override to be informed prior to apply evaluation
            if (_testContext != null)
            {
                var all = _testContext.ReferenceValue<List<ISource>>();
                all.AddRange(sources);
                _testContext.ReferenceValueTo(all);
            }
        }

        /// <summary>
        /// Received after the full asynchronous evaluation of each <see cref="M:Apply()"/>.
        /// Override if notification is desired.
        /// </summary>
        protected virtual void AfterApply()
        {
        }
        
        /// <summary>
        /// Answer <see cref="Optional{T}"/> that should be thrown
        /// and handled by my <code>Supervisor</code>, unless it is empty. The default
        /// behavior is to answer the given <code>exception</code>, which will be thrown.
        /// Must override to change default behavior.
        /// </summary>
        /// <param name="exception">The exception</param>
        /// <returns><see cref="Optional{T}"/></returns>
        protected Optional<ApplyFailedException<TException>> AfterApplyFailed<TException>(ApplyFailedException<TException> exception) => Optional.Of(exception);

        /// <summary>
        /// Answer a valid <typeparamref name="TSnapshot"/> state instance if a snapshot should
        /// be taken and persisted along with applied sources instance(s).
        /// Must override if snapshots are to be supported.
        /// </summary>
        /// <typeparam name="TSnapshot">The type of the snapshot</typeparam>
        /// <returns>The snapshot</returns>
        protected virtual TSnapshot Snapshot<TSnapshot>() => default!;

        protected virtual void RestoreSnapshot<TSnapshot>(TSnapshot snapshot, int currentVersion)
        {
            // OVERRIDE FOR SNAPSHOT SUPPORT
        }
        
        /// <summary>
        /// Answer a representation of a number of segments as a
        /// composite stream name. The implementor of <code>StreamName</code>
        /// would use this method if the its stream name is built from segments.
        /// </summary>
        /// <param name="separator">the string separator the insert between segments</param>
        /// <param name="streamNameSegments">the varargs string of one or more segments</param>
        /// <returns>The string representation</returns>
        protected string StreamNameFrom(string separator, params string[] streamNameSegments)
        {
            var builder = new StringBuilder();
            builder.Append(streamNameSegments[0]);
            for (var idx = 1; idx < streamNameSegments.Length; ++idx)
            {
                builder.Append(separator).Append(streamNameSegments[idx]);
            }
            return builder.ToString();
        }

        protected string[] StreamNameSegmentsFrom(char separator, string streamName) => streamName.Split(separator);

        /// <summary>
        /// Answer my currentVersion, which if zero indicates that the receiver
        /// is being initially constructed or reconstituted.
        /// </summary>
        protected int CurrentVersion => _currentVersion;

        /// <summary>
        /// Gets <see cref="Metadata"/>
        /// Must override if <see cref="Metadata"/> is to be supported.
        /// </summary>
        protected virtual Metadata Metadata => Metadata.NullMetadata();
        
        /// <summary>
        /// Answer my next version, which if one greater then my currentVersion.
        /// </summary>
        protected int NextVersion => _currentVersion + 1;

        //==================================
        // internal implementation
        //==================================

        /// <summary>
        /// Restore the state of my concrete extender from a possibly snapshot and stream of events.
        /// </summary>
        /// <exception cref="StorageException"></exception>
        protected override void Restore()
        {
            StowMessages(typeof(IStoppable));

            _journalInfo?.Journal.StreamReader(GetType().Name)
                .AndThenTo(reader => reader?.StreamFor(StreamName)!)
                .AndThenConsume(stream =>
                {
                    RestoreSnapshot(stream.Snapshot);
                    RestoreFrom(_journalInfo.EntryAdapterProvider.AsSources<ISource, BaseEntry<T>>(stream.Entries.Cast<BaseEntry<T>>().ToList()).ToList(), stream.StreamVersion);
                    DisperseStowedMessages();
                })
                .OtherwiseConsume(stream => DisperseStowedMessages())
            .RecoverFrom(cause => {
                DisperseStowedMessages();
                var message = $"Stream not recovered for: {GetType().Name}({StreamName}) because: {cause.Message}";
                throw new StorageException(Result.Failure, message, cause);
            });
        }

        /// <summary>
        /// Apply an individual <paramref name="source"/> onto my concrete extender by means of
        /// the <see cref="Action"/> of its registered <code>_sourcedTypeMap</code>.
        /// </summary>
        /// <param name="source">The sources to apply</param>
        private void ApplyResultVersioned(ISource source)
        {
            ApplySource(source);
            ++_currentVersion;
        }

        private void ApplySource(ISource source)
        {
            var type = GetType();

            var consumerFound = false;
            while (type != null)
            {
                if (_registeredConsumers.TryGetValue(type!, out var sourcedTypeMap))
                {
                    if (sourcedTypeMap.TryGetValue(source.GetType(), out var consumer))
                    {
                        consumer.DynamicInvoke(source);
                        consumerFound = true;
                        break;
                    }
                }

                type = type.BaseType;
            }

            if (!consumerFound)
            {
                throw new InvalidOperationException("No such Sourced type.");
            }
        }

        /// <summary>
        /// Given that the <paramref name="supplier"/> is non-null, execute it by completing the <see cref="CompletionSupplier{T}"/>.
        /// </summary>
        /// <param name="supplier">The <see cref="CompletionSupplier{T}"/> or null</param>
        private void CompleteUsing(object? supplier)
        {
            if (supplier != null)
            {
                ((CompletionSupplier<T>) supplier).Complete();
            }
        }

        private Info? Info()
        {
            try
            {
                return SourcedTypeRegistry.ResolveSourcedTypeRegistry(Stage.World).Info(GetType());
            }
            catch (Exception e)
            {
                var message = $"{GetType().Name}: Info not registered with SourcedTypeRegistry.";
                Logger.Error(message, e);
                throw new InvalidOperationException(message);
            }
        }

        /// <summary>
        /// Restore the state of my concrete extender from the <paramref name="stream"/> and
        /// set my <paramref name="currentVersion"/>.
        /// </summary>
        /// <param name="stream">The <paramref name="stream"/> from which state is restored</param>
        /// <param name="currentVersion">The int to set as my currentVersion</param>
        private void RestoreFrom(List<ISource> stream, int currentVersion)
        {
            foreach (var source in stream)
            {
                ApplySource(source);
            }

            _currentVersion = currentVersion;
        }

        /// <summary>
        /// Restores the initial state of the receiver by means of the <paramref name="snapshot"/>.
        /// </summary>
        /// <param name="snapshot">the snapshot holding the <see cref="Sourced{T}"/> initial state</param>
        private void RestoreSnapshot(IState? snapshot)
        {
            if (snapshot != null && !snapshot.IsNull)
            {
                RestoreSnapshot(_journalInfo!.StateAdapterProvider.FromRaw<T, IState>(snapshot), _currentVersion);
            }
        }

        private List<ISource> Wrap(ISource source) => new List<ISource> {source};
    }
}