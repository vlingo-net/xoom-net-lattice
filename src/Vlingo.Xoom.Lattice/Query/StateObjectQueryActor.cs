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
using Vlingo.Xoom.Symbio.Store;
using Vlingo.Xoom.Symbio.Store.Object;

namespace Vlingo.Xoom.Lattice.Query;

/// <summary>
/// An building-block <see cref="Actor"/> that queries asynchronously and provides a translated outcome.
/// </summary>
/// <typeparam name="TState">The type of the <see cref="StateObject"/> being queried</typeparam>
/// <typeparam name="TOutcome">The type of the outcome of the query</typeparam>
/// <typeparam name="TResult">The final result, being a <see cref="IEnumerable{T}"/></typeparam>
public abstract class StateObjectQueryActor<TState, TOutcome, TResult> : Actor, IQueryResultInterest
{
    private readonly IObjectStore _objectStore;
    private readonly IQueryResultInterest _queryResultInterest;
        
    /// <summary>
    /// Constructs my default state
    /// </summary>
    /// <param name="objectStore">The <see cref="IObjectStore"/> for the query</param>
    protected StateObjectQueryActor(IObjectStore objectStore)
    {
        _objectStore = objectStore;
        _queryResultInterest = SelfAs<IQueryResultInterest>();
    }
        
    /// <summary>
    /// Answer <see cref="Optional{ObjectQueryFailedException}"/> that should be thrown
    /// and handled by my <code>Supervisor</code>, unless it is empty. The default
    /// behavior is to answer the given <paramref name="exception"/>, which will be thrown.
    /// Must override to change default behavior.
    /// </summary>
    /// <param name="exception">The <see cref="ObjectQueryFailedException"/></param>
    /// <returns><see cref="Optional{ObjectQueryFailedException}"/></returns>
    protected Optional<ObjectQueryFailedException> AfterQueryFailed(ObjectQueryFailedException exception) => Optional.Of(exception);

    /// <summary>
    /// Answer the <see cref="ICompletes{TResult}"/> through which the queried and translated result is provided.
    /// </summary>
    /// <param name="query">The <see cref="QueryExpression"/> used to execute the query</param>
    /// <param name="andThen">The function used to translate the <typeparamref name="TState"/> outcome to the <typeparamref name="TResult"/> result</param>
    /// <returns><see cref="ICompletes{TResult}"/></returns>
    protected ICompletes<TResult> QueryAll(QueryExpression query, Func<TState, TResult> andThen)
    {
        _objectStore.QueryAll(
            query,
            _queryResultInterest,
            QueryAttempt<TState, TOutcome, TResult>.With(Cardinality.All, query, CompletionTranslator<TState, TResult>.TranslatorOrNull(andThen, CompletesEventually())!));

        return (ICompletes<TResult>) Completes();
    }
        
    protected ICompletes<TResult> QueryObject(QueryExpression query, Func<TState, TResult> andThen)
    {
        _objectStore.QueryObject(
            query,
            _queryResultInterest,
            QueryAttempt<TState, TOutcome, TResult>.With(Cardinality.Object, query, CompletionTranslator<TState, TResult>.TranslatorOrNull(andThen, CompletesEventually())!));

        return (ICompletes<TResult>) Completes();
    }
        
    public void QueryAllResultedIn(IOutcome<StorageException, Result> outcome, QueryMultiResults queryResults, object? attempt)
    {
        outcome
            .AndThen(result =>
            {
                CompleteUsing(attempt, (TState) queryResults.StateObjects);
                return result;
            })
            .Otherwise(cause => 
            {
                switch (cause.Result)
                {
                    case Result.NotFound:
                        CompleteUsing(attempt, (TState) Enumerable.Empty<object>());
                        return cause.Result;
                }
                var message = $"Query failed because: {cause.Result} with: {cause.Message}";
                var exception = new ObjectQueryFailedException(attempt!, message, cause);
                var maybeException = AfterQueryFailed(exception);
                if (maybeException.IsPresent)
                {
                    Logger.Error(message, maybeException.Get());
                    throw maybeException.Get();
                }
                    
                Logger.Error(message, exception);
                return cause.Result;
            });
    }

    public void QueryObjectResultedIn(IOutcome<StorageException, Result> outcome, QuerySingleResult queryResult, object? attempt)
    {
        outcome
            .AndThen(result =>
            {
                CompleteUsing(attempt, (TState) queryResult.StateObject!);
                return result;
            })
            .Otherwise(cause =>
            {
                switch (cause.Result)
                {
                    case Result.NotFound:
                        CompleteUsing(attempt, (TState) queryResult.StateObject!);
                        return cause.Result;
                }
                var message = $"Query failed because: {cause.Result} with: {cause.Message}";
                var exception = new ObjectQueryFailedException(QueryAttempt<TState, TOutcome, TResult>.From(attempt!), message, cause);
                var maybeException = AfterQueryFailed(exception);
                if (maybeException.IsPresent)
                {
                    Logger.Error(message, maybeException.Get());
                    throw maybeException.Get();
                }
                    
                Logger.Error(message, exception);
                return cause.Result;
            });
    }
        
    private void CompleteUsing(object? attempt, TState outcome)
    {
        if (attempt != null)
        {
            QueryAttempt<TState, TOutcome, TResult>.From(attempt).CompletionTranslator.Complete(outcome);
        }
    }
}