// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Symbio;
using Vlingo.Symbio.Store.Journal;

namespace Vlingo.Lattice.Model.Sourcing
{
    /// <summary>
    /// Holder of registration information.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="IJournal{T}"/> of the registration</typeparam>
    public class Info<T>
    {
        public EntryAdapterProvider EntryAdapterProvider { get; }
        public StateAdapterProvider StateAdapterProvider { get; }
        public IJournal<T> Journal { get; }
        public string? SourcedName { get; }
        public Type SourcedType => typeof(Sourced<T>);

        public bool IsBinary => false;
        public bool IsObject => false;
        public bool IsText => false;

        /// <summary>
        /// Construct my default state.
        /// </summary>
        /// <param name="journal">The <see cref="IJournal{T}"/> of the registration</param>
        /// <param name="sourcedName">The name of the sourced</param>
        public Info(IJournal<T> journal, string? sourcedName)
        {
            Journal = journal;
            SourcedName = sourcedName;
            EntryAdapterProvider = new EntryAdapterProvider();
            StateAdapterProvider = new StateAdapterProvider();
        }
        
        /// <summary>
        /// Answer myself after registering the <paramref name="adapter"/>.
        /// </summary>
        /// <param name="adapter"><see cref="IEntryAdapter{TSource,TEntry}"/> to register</param>
        /// <typeparam name="TSource">The <see cref="Source{T}"/> extender being registered</typeparam>
        /// <typeparam name="TEntry">The <see cref="IEntry{T}"/> extender being registered</typeparam>
        /// <returns><see cref="Info{T}"/></returns>
        public Info<T> RegisterEntryAdapter<TSource, TEntry>(IEntryAdapter<Source<TSource>, IEntry<TEntry>> adapter)
        {
            EntryAdapterProvider.RegisterAdapter(adapter);
            return this;
        }

        /// <summary>
        /// Answer myself after registering the <paramref name="adapter"/>.
        /// </summary>
        /// <param name="adapter"><see cref="IEntryAdapter{TSource,TEntry}"/> to register</param>
        /// <param name="consumer">The consumer being registered</param>
        /// <typeparam name="TSource">The <see cref="Source{T}"/> extender being registered</typeparam>
        /// <typeparam name="TEntry">The <see cref="IEntry{T}"/> extender being registered</typeparam>
        /// <returns><see cref="Info{T}"/></returns>
        public Info<T> RegisterEntryAdapter<TSource, TEntry>(IEntryAdapter<Source<TSource>, IEntry<TEntry>> adapter, Action<Type, IEntryAdapter<Source<TSource>, IEntry<TEntry>>> consumer)
        {
            EntryAdapterProvider.RegisterAdapter(adapter, consumer);
            return this;
        }
        
        /// <summary>
        /// Answer myself after registering the <paramref name="adapter"/>.
        /// </summary>
        /// <param name="adapter"><see cref="IStateAdapter{TSource,TEntry}"/> to register</param>
        /// <typeparam name="TSource">The <see cref="Source{T}"/> extender being registered</typeparam>
        /// <typeparam name="TState">The <see cref="State{T}"/> extender being registered</typeparam>
        /// <returns><see cref="Info{T}"/></returns>
        public Info<T> RegisterStateAdapter<TSource, TState>(IStateAdapter<Source<TSource>, State<TState>> adapter)
        {
            StateAdapterProvider.RegisterAdapter(adapter);
            return this;
        }
        
        /// <summary>
        /// Answer myself after registering the <paramref name="adapter"/>.
        /// </summary>
        /// <param name="adapter"><see cref="IStateAdapter{TSource,TEntry}"/> to register</param>
        /// <param name="consumer">The consumer being registered</param>
        /// <typeparam name="TSource">The <see cref="Source{T}"/> extender being registered</typeparam>
        /// <typeparam name="TState">The <see cref="State{T}"/> extender being registered</typeparam>
        /// <returns><see cref="Info{T}"/></returns>
        public Info<T> RegisterStateAdapter<TSource, TState>(IStateAdapter<Source<TSource>, State<TState>> adapter, Action<Type, IStateAdapter<Source<TSource>, State<TState>>> consumer)
        {
            StateAdapterProvider.RegisterAdapter(adapter, consumer);
            return this;
        }
    }
}