// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Symbio;
using Vlingo.Xoom.Symbio.Store.Journal;

namespace Vlingo.Xoom.Lattice.Model.Sourcing;

/// <summary>
/// Holder of registration information.
/// </summary>
public class Info
{
    public EntryAdapterProvider EntryAdapterProvider { get; }
    public StateAdapterProvider StateAdapterProvider { get; }
    public IJournal Journal { get; }
    public string SourcedName => SourcedType.Name;
    public Type SourcedType { get; }

    public bool IsBinary => false;
    public bool IsObject => false;
    public bool IsText => false;

    public static Info RegisterSourced<TSourced>(IJournal journal) => 
        RegisterSourced(journal, typeof(TSourced));
        
    public static Info RegisterSourced(IJournal journal, Type sourcedType) => 
        new Info(journal, sourcedType);

    /// <summary>
    /// Construct my default state.
    /// </summary>
    /// <param name="journal">The <see cref="IJournal{T}"/> of the registration</param>
    /// <param name="sourcedType">The type of the registered source</param>
    private Info(IJournal journal, Type sourcedType)
    {
        Journal = journal;
        SourcedType = sourcedType;
        EntryAdapterProvider = new EntryAdapterProvider();
        StateAdapterProvider = new StateAdapterProvider();
    }

    /// <summary>
    /// Answer myself after registering the <paramref name="adapter"/>.
    /// </summary>
    /// <param name="adapter"><see cref="IEntryAdapter"/> to register</param>
    /// <returns><see cref="Info"/></returns>
    public Info RegisterEntryAdapter(IEntryAdapter adapter)
    {
        EntryAdapterProvider.RegisterAdapter(adapter);
        return this;
    }

    /// <summary>
    /// Answer myself after registering the <paramref name="adapter"/>.
    /// </summary>
    /// <param name="adapter"><see cref="IEntryAdapter"/> to register</param>
    /// <param name="consumer">The consumer being registered</param>
    /// <returns><see cref="Info"/></returns>
    public Info RegisterEntryAdapter(IEntryAdapter adapter, Action<IEntryAdapter> consumer)
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
    /// <returns><see cref="Info"/></returns>
    public Info RegisterStateAdapter<TSource, TState>(IStateAdapter<Source<TSource>, State<TState>> adapter)
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
    /// <returns><see cref="Info"/></returns>
    public Info RegisterStateAdapter<TSource, TState>(IStateAdapter<Source<TSource>, State<TState>> adapter, Action<Type, IStateAdapter<Source<TSource>, State<TState>>> consumer)
    {
        StateAdapterProvider.RegisterAdapter(adapter, consumer);
        return this;
    }
}