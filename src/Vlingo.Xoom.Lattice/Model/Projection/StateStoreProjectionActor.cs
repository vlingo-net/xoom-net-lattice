// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
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

namespace Vlingo.Xoom.Lattice.Model.Projection;

/// <summary>
/// Abstract <see cref="Actor"/> base class used by <see cref="IProjection"/> types to handle
/// <code>ProjectWith()</code> into the <see cref="IStateStore"/>. Concrete extenders must:
/// - Provide the <see cref="IStateStore"/> for construction
/// - Implement <code>StateStoreProjectionActor.CurrentDataFor(IProjectable)</code>
/// - Implement <code>StateStoreProjectionActor.Merge(T, int, T, int)</code>
/// - Invoke <code>StateStoreProjectionActor.UpsertFor(IProjectable, IProjectionControl)</code> to cause the upsert
/// </summary>
/// <typeparam name="T">The type to be persisted in the <see cref="IStateStore"/></typeparam>
public abstract class StateStoreProjectionActor<T> : Actor, IProjection, ICompositeIdentitySupport, IReadResultInterest, IWriteResultInterest
{
    private readonly List<ISource> _adaptedSources;
    private readonly IEntryAdapter _entryAdapter;
    private readonly IStateAdapter _stateAdapter;
    private readonly IReadResultInterest _readInterest;
    private readonly IWriteResultInterest _writeInterest;
    private readonly IStateStore _stateStore;

    public StateStoreProjectionActor(IStateStore stateStore) : this(stateStore, DefaultTextStateAdapter(), DefaultTextEntryAdapter())
    {
    }
        
    /// <summary>
    /// Construct my final state with the <see cref="IStateStore"/>, which must
    /// be provided by my concrete extenders, as well as with a
    /// <see cref="IStateAdapter"/> and a <see cref="IEntryAdapter{TSource,TEntry}"/>.
    /// </summary>
    /// <param name="stateStore">The <see cref="IStateStore"/> from which previous state is read and merged current state is written</param>
    /// <param name="stateAdapter">The <see cref="IStateAdapter"/> used by my extenders to adapt persistent state</param>
    /// <param name="entryAdapter">The <see cref="IEntryAdapter{TSource,TEntry}"/> used by my extenders to adapt persistent entries</param>
    public StateStoreProjectionActor(IStateStore stateStore, IStateAdapter stateAdapter, IEntryAdapter entryAdapter)
    {
        _stateStore = stateStore;
        _stateAdapter = stateAdapter;
        _entryAdapter = entryAdapter;
        _readInterest = SelfAs<IReadResultInterest>();
        _writeInterest = SelfAs<IWriteResultInterest>();

        _adaptedSources = new List<ISource>();
    }
        
    public void ProjectWith(IProjectable projectable, IProjectionControl control) => UpsertFor(projectable, control);

    public string DataIdFrom(string separator, params string[] idSegments) =>
        CompositeIdentitySupport.DataIdFrom(separator, idSegments);

    public IEnumerable<string> DataIdSegmentsFrom(string separator, string dataId) =>
        CompositeIdentitySupport.DataIdSegmentsFrom(separator, dataId);
        
    //==================================
    // ReadResultInterest
    //==================================

    public void ReadResultedIn<TState>(IOutcome<StorageException, Result> outcome, string? id, TState state, int stateVersion, Metadata? metadata, object? @object)
    {
        outcome.AndThen(result =>
        {
            ((Action<TState, int>) @object!)(state, stateVersion);
            return result;
        }).Otherwise(cause =>
        {
            if (cause.Result == Result.NotFound)
            {
                ((Action<TState, int>) @object!)(default!, -1);
            }
            else
            {
                // log but don't retry, allowing re-delivery of Projectable
                Logger.Info($"Query state not read for update because: {cause.Message}", cause);
            }
            return cause.Result;
        });
    }

    public void ReadResultedIn<TState>(IOutcome<StorageException, Result> outcome, IEnumerable<TypedStateBundle> bundles, object? @object)
    {
    }

    //==================================
    // WriteResultInterest
    //==================================
        
    public void WriteResultedIn<TState, TSource>(IOutcome<StorageException, Result> outcome, string id, TState state, int stateVersion, IEnumerable<TSource> sources, object? @object)
    {
        outcome.AndThen(result => 
        {
            ConfirmProjection((Confirmer) @object!);
            return result;
        }).Otherwise(cause =>
        {
            DisperseStowedMessages();
            // log but don't retry, allowing re-delivery of Projectable
            Logger.Info($"Query state not written for update because: {cause.Message}", cause);
            return cause.Result;
        });
    }
        
    /// <summary>
    /// Gets whether to always write or to compare the <code>currentData</code> with
    /// the <code>previousData</code> and only write if the two are different. The answer
    /// is <code>true</code> by default, meaning that the write will always happen, even
    /// if the <code>currentData</code> isn't different from the <code>previousData</code>.
    /// Override to get <code>false</code> to cause a comparison to qualify the write.
    /// </summary>
    protected virtual bool AlwaysWrite => true;
        
    /// <summary>
    /// Gets the <see cref="IEnumerable{T}"/> that are adapted from the current <code>Projectable.Entries</code>.
    /// </summary>
    protected IEnumerable<ISource> Sources => _adaptedSources;

    /// <summary>
    /// Gets the <typeparamref name="T"/> typed current data from the <paramref name="projectable"/>, which
    /// is <code>projectable.ObjectT}()</code> by default. Override this if determining the
    /// <code>currentData</code> is more complex.
    /// </summary>
    /// <param name="projectable">the <see cref="IProjectable"/> from which the current data is retrieved</param>
    /// <returns>The <typeparamref name="T"/> typed object</returns>
    protected virtual T CurrentDataFor(IProjectable projectable) => projectable.Object<T>();

    /// <summary>
    /// Answer the current data version. By default this method answers in one of two
    /// conditional ways: (1) when my <code>AlwaysWrite</code> answers <code>true</code> then
    /// the answer is the <code>projectable.DataVersion()</code> of the received
    /// <paramref name="projectable"/>; or (2) when my <code>AlwaysWrite</code> answers <code>false</code>
    /// the <code>previousVersion + 1</code>. Override for specialized behavior.
    /// </summary>
    /// <param name="projectable">The <see cref="IProjectable"/> containing state and/or entries to be projected</param>
    /// <param name="previousData">The <typeparamref name="T"/> typed previous data from storage</param>
    /// <param name="previousVersion">The int previous version from storage</param>
    /// <returns>The current data version</returns>
    protected virtual int CurrentDataVersionFor(IProjectable projectable, T previousData, int previousVersion) => 
        AlwaysWrite ? projectable.DataVersion() : previousVersion == -1 ? 1 : previousVersion + 1;

    /// <summary>
    /// Gets the id to be associated with the data being projected.
    /// </summary>
    /// <param name="projectable">The <see cref="IProjectable"/> from which the data id is retrieved</param>
    /// <returns>string id.</returns>
    protected string DataIdFor(IProjectable projectable)
    {
        var dataId = projectable.DataId;

        if (string.IsNullOrWhiteSpace(dataId))
        {
            try
            {
                dataId = TypedToIdentifiedDomainEvent(Sources.First()).Identity;
            }
            catch
            {
                // ignore; fall through
            }
        }

        return dataId;
    }

    /// <summary>
    /// Gets the <see cref="IEntryAdapter{TSource,TEntry}"/> previously registered by construction.
    /// </summary>
    /// <typeparam name="TSource">The source type</typeparam>
    /// <typeparam name="TEntry">The entry type</typeparam>
    /// <returns>Entry adapter if the instance used in construction can be casted. Otherwise throws an exception</returns>
    protected IEntryAdapter EntryAdapter() => _entryAdapter;

    /// <summary>
    /// Answer the <typeparamref name="T"/> result of merging the <typeparamref name="T"/> typed <paramref name="previousData"/> and <paramref name="currentData"/>,
    /// which will be written into the <see cref="IStateStore"/>. By default the <paramref name="currentData"/> is returned.
    /// Override for specialize behavior. The receiver may simply answer the <paramref name="currentData"/> when no merging
    /// is required, resulting in <paramref name="currentData"/> being written. If projecting from full state, this is the
    /// better override.
    /// </summary>
    /// <param name="previousData">The <typeparamref name="T"/> data read from the <see cref="IStateStore"/></param>
    /// <param name="previousVersion">The version of the previous data</param>
    /// <param name="currentData">The <typeparamref name="T"/> data being projected</param>
    /// <param name="currentVersion">The version of the current data.</param>
    /// <returns>The version of the currentData</returns>
    protected virtual T Merge(T previousData, int previousVersion, T currentData, int currentVersion) => currentData;

    /// <summary>
    /// Answer the <typeparamref name="T"/> result of merging the <typeparamref name="T"/> typed <paramref name="previousData"/>, <paramref name="currentData"/> and/or <paramref name="sources"/>, 
    /// which will be written into the <see cref="IStateStore"/>. By default this method delegates
    /// to the lesser <code>StateStoreProjectionActor.Merge(object, int, object, int)</code>.
    /// Override for specialize behavior. If projecting from Event Sourcing, this is the better override.
    /// </summary>
    /// <param name="previousData">The <typeparamref name="T"/> data read from the <see cref="IStateStore"/></param>
    /// <param name="previousVersion">The version of the previous data</param>
    /// <param name="currentData">The <typeparamref name="T"/> data being projected</param>
    /// <param name="currentVersion">The version of the current data.</param>
    /// <param name="sources">Sources adapted from the <see cref="IProjectable"/> entries</param>
    /// <returns><typeparamref name="T"/> type data.</returns>
    protected virtual T Merge(T previousData, int previousVersion, T currentData, int currentVersion, IEnumerable<ISource> sources) => 
        Merge(previousData, previousVersion, currentData, currentVersion);

    /// <summary>
    /// Prepare for the merge. Override this behavior for specialized implementation.
    /// </summary>
    /// <param name="projectable">The <see cref="IProjectable"/> used for merge preparation</param>
    protected virtual void PrepareForMergeWith(IProjectable projectable)
    {
        _adaptedSources.Clear();

        foreach (var entry in projectable.Entries)
        {
            _adaptedSources.Add(_entryAdapter.AnyTypeFromEntry(entry));
        }
    }

    /// <summary>
    /// Gets the current state adapter
    /// </summary>
    public IStateAdapter StateAdapter => _stateAdapter;

    /// <summary>
    /// Upsert the <paramref name="projectable"/> into the <see cref="IStateStore"/>, which may be an insert of
    /// new data or an update of new data merged with previous data.
    /// </summary>
    /// <param name="projectable">The <see cref="IProjectable"/> to upsert</param>
    /// <param name="control">The <see cref="IProjectionControl"/> with <see cref="Confirmer"/> use to confirm projection is completed</param>
    protected void UpsertFor(IProjectable projectable, IProjectionControl control)
    {
        var currentData = CurrentDataFor(projectable);

        PrepareForMergeWith(projectable);

        var dataId = DataIdFor(projectable);

        Action<T, int> upserter = (previousData, previousVersion) =>
        {
            var currentDataVersion = CurrentDataVersionFor(projectable, previousData, previousVersion);
            var data = Merge(previousData, previousVersion, currentData, currentDataVersion, Sources);
            var confirmer = ProjectionControl.ConfirmerFor(projectable, control);
            if (data != null && (AlwaysWrite || !data.Equals(previousData)))
            {
                _stateStore.Write(dataId, data, currentDataVersion, _writeInterest, confirmer);
            }
            else
            {
                ConfirmProjection(confirmer);
            }
        };
            
        StowMessages(typeof(IReadResultInterest), typeof(IWriteResultInterest));
            
        _stateStore.Read<T>(dataId, _readInterest, upserter);
    }
        
    /// <summary>
    /// Gets the <typeparamref name="TState"/> typed state from the abstract <paramref name="state"/>.
    /// </summary>
    /// <param name="state">The object to cast to type <typeparamref name="TState"/></param>
    /// <typeparam name="TState">The concrete type of the state</typeparam>
    /// <returns>The <typeparamref name="TState"/> typed state.</returns>
    protected TState Typed<TState>(object state) => (TState) state;
        
    /// <summary>
    /// Gets the <typeparamref name="TEvent"/> typed event from the abstract <paramref name="event"/>.
    /// </summary>
    /// <param name="event">The object to cast to type <typeparamref name="TEvent"/></param>
    /// <typeparam name="TEvent">The concrete type of the event</typeparam>
    /// <returns>The <typeparamref name="TEvent"/> typed event.</returns>
    protected TEvent Typed<TEvent>(DomainEvent @event) where TEvent : DomainEvent => (TEvent) @event;
        
    /// <summary>
    /// Gets the <typeparamref name="TSource"/> typed source from the abstract <paramref name="source"/>.
    /// </summary>
    /// <param name="source">The object to cast to type <typeparamref name="TSource"/></param>
    /// <typeparam name="TSource">The concrete type of the source</typeparam>
    /// <returns>The <typeparamref name="TSource"/> typed source.</returns>
    protected TSource Typed<TSource>(ISource source) => (TSource) source;

    /// <summary>
    /// Gets the <see cref="IdentifiedDomainEvent"/> typed from the abstract <paramref name="source"/>.
    /// </summary>
    /// <param name="source">The source to cast to <see cref="IdentifiedDomainEvent"/></param>
    /// <returns><see cref="IdentifiedDomainEvent"/></returns>
    protected IdentifiedDomainEvent TypedToIdentifiedDomainEvent(ISource source) => (IdentifiedDomainEvent) source;
        
    //==================================
    // Internal Implementation
    //==================================
        
    private static IEntryAdapter DefaultTextEntryAdapter() => new DefaultTextEntryAdapter<ISource>();
        
    private static IStateAdapter DefaultTextStateAdapter() => new DefaultTextStateAdapter();
        
    private void ConfirmProjection(Confirmer confirmer)
    {
        confirmer.Confirm();
        DisperseStowedMessages();
    }
}