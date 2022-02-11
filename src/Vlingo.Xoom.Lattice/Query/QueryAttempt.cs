// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Symbio.Store;
using Vlingo.Xoom.Symbio.Store.Object;

namespace Vlingo.Xoom.Lattice.Query;

public class QueryAttemptBase
{
    public QueryAttemptBase(Cardinality cardinality, QueryExpression query, object untypedCompletionTranslator)
    {
        Cardinality = cardinality;
        Query = query;
        UntypedCompletionTranslator = untypedCompletionTranslator;
    }

    public Cardinality Cardinality { get; }
    public object UntypedCompletionTranslator { get; }
    public QueryExpression Query { get; }
}
    
/// <summary>
/// The elements used in the attempted <code>QueryAll()</code> or <code>QueryObject()</code>
/// </summary>
/// <typeparam name="TObjectState">The type of the <see cref="StateObject"/> being queried</typeparam>
/// <typeparam name="TOutcome">The type of the outcome of the query</typeparam>
/// <typeparam name="TResult">The final result, being a <code>TObjectState</code> or <code>IEnumerable{TObjectState}</code></typeparam>
public class QueryAttempt<TObjectState, TOutcome, TResult> : QueryAttemptBase
{
    public QueryAttempt(Cardinality cardinality, QueryExpression query, CompletionTranslator<TObjectState, TResult> completionTranslator)
        : base(cardinality, query, completionTranslator) =>
        CompletionTranslator = completionTranslator;

    public static QueryAttempt<TObjectState, TOutcome, TResult> From(object attempt)
    {
        var typed = (QueryAttempt<TObjectState, TOutcome, TResult>) attempt;
        return typed;
    }

    public static QueryAttempt<TObjectState, TOutcome, TResult> With(Cardinality cardinality, QueryExpression query, CompletionTranslator<TObjectState, TResult> completionTranslator) => 
        new QueryAttempt<TObjectState, TOutcome, TResult>(cardinality, query, completionTranslator);

    public CompletionTranslator<TObjectState, TResult> CompletionTranslator { get; }
}

public enum Cardinality
{
    All,
    Object
}