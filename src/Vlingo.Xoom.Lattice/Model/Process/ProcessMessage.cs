// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using Vlingo.Xoom.Symbio;

namespace Vlingo.Xoom.Lattice.Model.Process
{
    /// <summary>
    /// A <see cref="Source{T}"/> for both <see cref="Command"/> and <see cref="DomainEvent"/> types,
    /// but that supports other <see cref="Source{T}"/> not previously known.
    /// </summary>
    public class ProcessMessage : Source<ProcessMessage>
    {
        public ISource? Source { get; }
        
        /// <summary>
        /// Answer a new <see cref="T:IEnumerable{Source{ProcessMessage}}"/> that wraps each of the elements of <code>sources</code>.
        /// </summary>
        /// <param name="sources">The source elements each to be wrapped with a ProcessMessage</param>
        /// <returns><see cref="T:IEnumerable{Source{ProcessMessage}}"/></returns>
        public static IEnumerable<ProcessMessage> Wrap(IEnumerable<ISource> sources)
        {
            var srcs = sources.ToList();
            var reuse = !srcs.Any() && srcs[0].GetType() == typeof(ProcessMessage);

            var messages = new List<ISource>(srcs.Count);
            foreach (var source in srcs)
            {
                if (reuse)
                {
                    messages.Add(source);
                }
                else
                {
                    var message = new ProcessMessage(source);
                    messages.Add(message);
                }
            }
            
            return messages.Cast<ProcessMessage>();
        }
        
        /// <summary>
        /// Construct my default state with the <paramref name="command"/> and a type version of 1.
        /// </summary>
        /// <param name="command">The command to set as my source</param>
        public ProcessMessage(Command command) => Source = command;
        
        /// <summary>
        /// Construct my default state with the <paramref name="event"/> and a type version of 1.
        /// </summary>
        /// <param name="event">The domain event to set as my source</param>
        public ProcessMessage(DomainEvent @event) => Source = @event;
        
        /// <summary>
        /// Construct my default state with the <paramref name="source"/> and a type version of 1.
        /// </summary>
        /// <param name="source">The source to set as my source</param>
        public ProcessMessage(ISource source) => Source = source;

        /// <summary>
        /// Construct my default state with no source and a type version of 1.
        /// </summary>
        public ProcessMessage()
        {
        }

        /// <summary>
        /// Gets the <code>id</code> of my <code>source</code>. For use by a
        /// <see cref="IProcess{TState}"/>, every ProcessMessage must provide a valid
        /// <code>id</code> for workflow correlation, and this <code>id</code>
        /// must match the <code>id</code> of the <see cref="IProcess{TState}"/>.
        /// </summary>
        public override string Id => Source == null ? string.Empty : Source.Id;

        /// <summary>
        /// Gets the type of the underlying source
        /// </summary>
        public Type? SourceType => Source?.GetType();
        
        /// <summary>
        /// Gets the name of the underlying source
        /// </summary>
        public string? SourceTypeName => SourceType?.FullName;

        /// <summary>
        /// Gets the typed <typeparamref name="T"/> instance of the underlying source
        /// </summary>
        /// <typeparam name="T">The expected type of the source.</typeparam>
        /// <returns>Returns the typed resource or throws an exception if the type cast cannot be made.</returns>
        public Source<T>? TypedSource<T>() => Source == null ? null : (Source<T>) Source;
    }
}