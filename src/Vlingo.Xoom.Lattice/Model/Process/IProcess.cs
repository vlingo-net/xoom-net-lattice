// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Vlingo.Xoom.Common;
using Vlingo.Xoom.Symbio;
using Vlingo.Xoom.Symbio.Store.Journal;

namespace Vlingo.Xoom.Lattice.Model.Process;

/// <summary>
/// Definition for a long-running process.
/// </summary>
/// <typeparam name="TState">The type of the state.</typeparam>
public interface IProcess<TState>
{
    /// <summary>
    /// Answer my state as a <see cref="T:Chronicle{TState}"/>
    /// </summary>
    Chronicle<TState> Chronicle { get; }
        
    /// <summary>
    /// Gets id, which is used for correlation among my collaborators.
    /// </summary>
    string ProcessId { get; }

    /// <summary>
    /// Cause the <code>command</code> to be processed by persisting it as a <see cref="ProcessMessage"/>.
    /// Uses the underlying persistence mechanism to ensure the <code>command</code> is permanent, enabling
    /// a backing {@code Exchange} message to be enqueued <see cref="Exchange"/> message to be enqueued
    /// with guaranteed delivery semantics.
    /// </summary>
    /// <param name="command">The Command to apply</param>
    void Process(Command command);
        
    /// <summary>
    /// Answer <see cref="ICompletes{T}"/>, while causing the <code>command</code> to be processed by persisting
    /// it as a <see cref="ProcessMessage"/>, followed by the execution of a possible <code>andThen</code>.
    /// Uses the underlying persistence mechanism to ensure the <code>command</code> is permanent, enabling
    /// a backing <see cref="Exchange"/> message to be enqueued with guaranteed delivery semantics.
    /// </summary>
    /// <param name="command">The command to apply</param>
    /// <param name="andThen">The function executed following the application of command</param>
    /// <typeparam name="T">The return type of the <code>andThen</code> function</typeparam>
    /// <returns><see cref="ICompletes{TResult}"/></returns>
    ICompletes<T> Process<T>(Command command, Func<T> andThen);
        
    /// <summary>
    /// Cause the <code>event</code> to be processed by persisting it as a <see cref="ProcessMessage"/>.
    /// Uses the underlying persistence mechanism to ensure the <code>event</code> is permanent, enabling
    /// a backing <see cref="Exchange"/> message to be enqueued with guaranteed delivery semantics.
    /// </summary>
    /// <param name="event">The DomainEvent to apply</param>
    void Process(DomainEvent @event);

    /// <summary>
    /// Answer <see cref="ICompletes{T}"/>, while causing the <code>event</code> to be processed by persisting
    /// it as a <see cref="ProcessMessage"/>, followed by the execution of a possible <code>andThen</code>.
    /// Uses the underlying persistence mechanism to ensure the <code>event</code> is permanent, enabling
    /// a backing <see cref="Exchange"/> message to be enqueued with guaranteed delivery semantics.
    /// </summary>
    /// <param name="event">The event to apply</param>
    /// <param name="andThen">The function executed following the application of event</param>
    /// <typeparam name="T">The return type of the <code>andThen</code> function</typeparam>
    /// <returns><see cref="ICompletes{TResult}"/></returns>
    ICompletes<T> Process<T>(DomainEvent @event, Func<T> andThen);
        
    /// <summary>
    /// Cause the <see cref="T:IEnumerable{Source{TSource}}"/> to be processed by persisting each as a <see cref="ProcessMessage"/>.
    /// Uses the underlying persistence mechanism to ensure the <see cref="T:IEnumerable{Source{TSource}}"/> are permanent, enabling
    /// a backing <see cref="Exchange"/> message to be enqueued with guaranteed delivery semantics.
    /// </summary>
    /// <param name="sources">The collection of sources instances to apply</param>
    /// <typeparam name="TSource">The type of the sources</typeparam>
    void ProcessAll<TSource>(IEnumerable<Source<TSource>> sources);
        
    /// <summary>
    /// Answer <see cref="ICompletes{T}"/>, while causing the <see cref="T:IEnumerable{Source{TSource}}"/> to be processed by persisting
    /// each as a <see cref="ProcessMessage"/>, followed by the execution of a possible <code>andThen</code>.
    /// Uses the underlying persistence mechanism to ensure the <see cref="T:IEnumerable{Source{TSource}}"/> are permanent, enabling
    /// a backing <see cref="Exchange"/> message to be enqueued with guaranteed delivery semantics.
    /// Emit all <see cref="T:IEnumerable{Source{TSource}}"/> by applying them to myself, followed by the execution of a possible <code>andThen</code>.
    /// </summary>
    /// <param name="sources">The collection of source instances to apply</param>
    /// <param name="andThen">The function executed following the application of sources</param>
    /// <typeparam name="T">The return ype of the <paramref name="andThen"/> function</typeparam>
    /// <typeparam name="TSource">The type of the sources</typeparam>
    /// <returns><see cref="ICompletes{T}"/></returns>
    ICompletes<T> ProcessAll<T, TSource>(IEnumerable<Source<TSource>> sources, Func<T> andThen);

    /// <summary>
    /// Send the <code>command</code> to my collaborators via my Exchange.
    /// Note that this is <strong>not</strong> expected to initially persist the <code>command</code> to the underlying <see cref="Journal{T}"/>.
    /// Thus, the <code>command</code> is subject to the limitations of the underlying <see cref="Exchange"/> mechanism, such as downed nodes
    /// and network partitions/disconnections.
    /// </summary>
    /// <param name="command">The command to send</param>
    void Send(Command command);
        
    /// <summary>
    /// Send the <code>event</code> to my collaborators via my Exchange.
    /// Note that this is <strong>not</strong> expected to initially persist the <code>event</code> to the underlying <see cref="Journal{T}"/>.
    /// Thus, the <code>event</code> is subject to the limitations of the underlying <see cref="Exchange"/> mechanism, such as downed nodes
    /// and network partitions/disconnections.
    /// </summary>
    /// <param name="event">The event to send</param>
    void Send(DomainEvent @event);
}