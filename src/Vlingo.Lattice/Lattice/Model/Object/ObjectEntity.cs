// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Vlingo.Actors;
using Vlingo.Common;
using Vlingo.Symbio;
using Vlingo.Symbio.Store;
using Vlingo.Symbio.Store.Object;

namespace Vlingo.Lattice.Lattice.Model.Object
{
    /// <summary>
    /// Abstract base type used to preserve and restore object state
    /// by means of the <see cref="IObjectStore"/>. The <see cref="IObjectStore"/>
    /// is typically backed by some form of object-relational mapping,
    /// whether formally or informally implemented.
    /// </summary>
    /// <typeparam name="T">The type of persistent object</typeparam>
    public abstract class ObjectEntity<T> : EntityActor, IPersistResultInterest, IQueryResultInterest where T : StateObject
    {
        protected string Id { get; set; }

        private Info _info;
        private IPersistResultInterest _persistResultInterest;
        private readonly IQueryResultInterest _queryResultInterest;
        private QueryExpression? _queryExpression;

        /// <summary>
        /// Construct my default state using my <see cref="IAddress"/> as my <code>Id</code>
        /// </summary>
        protected ObjectEntity() : this(null)
        {
        }

        /// <summary>
        /// Construct my default state.
        /// </summary>
        /// <param name="id">Unique identity of this entity</param>
        protected ObjectEntity(string? id)
        {
            Id = !string.IsNullOrWhiteSpace(id) ? id : Address.IdString;
            _info = Info();
            _persistResultInterest = SelfAs<IPersistResultInterest>();
            _queryResultInterest = SelfAs<IQueryResultInterest>();
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
                Apply<string, T>(newState.Item1, newState.Item2, null);
            }
        }
        
        protected ICompletes? Apply<TSource, TResult>(T state, IEnumerable<Source<TSource>> sources, Metadata metadata, Func<TResult>? andThen)
        {
            var completionSupplier = CompletionSupplier<TResult>.SupplierOrNull(andThen, CompletesEventually());
            var completes = andThen == null ? null : Completes();
            StowMessages(typeof(IPersistResultInterest));
            //_info.Store.Persist(StateSources.of(state,sources), metadata, _persistResultInterest, completionSupplier);
            return completes;
        }
        
        protected ICompletes? Apply<TSource, TResult>(T state, IEnumerable<Source<TSource>> sources, Func<TResult>? andThen)
            => Apply(state, sources, Metadata, andThen);

        /// <summary>
        /// Answer my new <code>state</code> and <code>sources</code> as a <see cref="T:Tuple{T1, IEnumerable{Source{string}}}"/>,
        /// or <code>null</code> if not new. Used each time I am started to determine
        /// whether restoration is necessary or otherwise initial state persistence.
        /// By default I always attempt to restore my state while ignoring non-existence.
        /// </summary>
        /// <returns><see cref="T:Tuple{T, IEnumerable{Source{string}}}"/></returns>
        protected Tuple<T, IEnumerable<Source<string>>>? WhenNewState() => null;
        
        /// <summary>
        /// Gets the <see cref="Metadata"/>. Must override if <see cref="Metadata"/> is to be supported.
        /// </summary>
        protected Metadata Metadata => Metadata.NullMetadata();
        
        /// <summary>
        /// Received by my extender when I must access its state object.
        /// Must be overridden by my extender.
        /// </summary>
        protected abstract T StateObject { get; }

        //=====================================
        // FOR INTERNAL USE ONLY.
        //=====================================
        
        public void PersistResultedIn(IOutcome<StorageException, Result> outcome, object? stateObject, int possible, int actual, object? @object)
        {
            throw new NotImplementedException();
        }

        public void QueryAllResultedIn(IOutcome<StorageException, Result> outcome, QueryMultiResults results, object? @object)
        {
            throw new NotImplementedException();
        }

        public void QueryObjectResultedIn(IOutcome<StorageException, Result> outcome, QuerySingleResult result, object? @object)
        {
            throw new NotImplementedException();
        }
        
        /// <summary>
        /// Gets type state object type received by my extender when I must know it's state object type.
        /// Must be overridden by my extender.
        /// </summary>
        protected abstract Type StateObjectType { get; }
        
        private Info Info()
        {
            try
            {
                var registry = Stage.World.ResolveDynamic<ObjectTypeRegistry>(ObjectTypeRegistry.InternalName);
                var info = registry.Info(StateObjectType);
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
                {
                    _queryExpression =
                        ListQueryExpression.Using(
                        _info.QueryObjectExpression.Type,
                        _info.QueryObjectExpression.Query,
                        StateObject.QueryList());
                }
                else if (_info.QueryObjectExpression.IsMapQueryExpression)
                {
                    _queryExpression =
                        MapQueryExpression.Using(
                        _info.QueryObjectExpression.Type,
                        _info.QueryObjectExpression.Query,
                        StateObject.QueryMap());
                }
                else
                {
                    throw new InvalidOperationException($"Unknown QueryExpression type: {_queryExpression}");
                }
            }
            return _queryExpression;
        }
        
        /// <summary>
        /// Cause state restoration and indicate whether a not found
        /// condition can be safely ignored.
        /// </summary>
        /// <param name="ignoreNotFound">The boolean indicating whether or not a not found condition may be ignored</param>
        private void Restore(bool ignoreNotFound)
        {
            StowMessages(typeof(IQueryResultInterest));
            _info.Store.QueryObject(QueryExpression(), _queryResultInterest, ignoreNotFound);
        }
    }
}