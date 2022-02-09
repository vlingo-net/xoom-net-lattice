// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Symbio;
using Vlingo.Xoom.Symbio.Store.State;

namespace Vlingo.Xoom.Lattice.Model.Stateful
{
    /// <summary>
    /// Holder of registration information.
    /// </summary>
    public class Info
    {
        public IStateStore Store { get; }
        public string StoreName { get; }

        public Type StoreType { get; }
        public EntryAdapterProvider EntryAdapterProvider { get; }
        public StateAdapterProvider StateAdapterProvider { get; }

        /// <summary>
        /// Construct my default state.
        /// </summary>
        /// <param name="stateType">The type of state store state</param>
        /// <param name="storeName">The string name of the Store</param>
        /// <param name="store">The store</param>
        public Info(IStateStore store, Type stateType, string storeName)
        {
            Store = store;
            StoreType = stateType;
            StoreName = storeName;
            EntryAdapterProvider = new EntryAdapterProvider();
            StateAdapterProvider = new StateAdapterProvider();
        }
        
        public Info RegisterEntryAdapter(IEntryAdapter adapter)
        {
            EntryAdapterProvider.RegisterAdapter(adapter);
            return this;
        }
        
        public Info RegisterEntryAdapter(IEntryAdapter adapter, Action<IEntryAdapter> consumer)
        {
            EntryAdapterProvider.RegisterAdapter(adapter, consumer);
            return this;
        }
        
        public Info RegisterStateAdapter<TSource, TState>(IStateAdapter<Source<TSource>, State<TState>> adapter)
        {
            StateAdapterProvider.RegisterAdapter(adapter);
            return this;
        }
        
        public Info RegisterStateAdapter<TSource, TState>(IStateAdapter<Source<TSource>, State<TState>> adapter, Action<Type, IStateAdapter<Source<TSource>, State<TState>>> consumer)
        {
            StateAdapterProvider.RegisterAdapter(adapter, consumer);
            return this;
        }

        /// <summary>
        /// Gets whether or not I am a binary type.
        /// </summary>
        public virtual bool IsBinary => false;

        /// <summary>
        /// Gets whether or not I am a text type.
        /// </summary>
        public virtual bool IsText => false;
    }
    
    /// <summary>
    /// Holder of binary registration information.
    /// </summary>
    public class BinaryInfo : Info
    {
        public BinaryInfo(IStateStore store, string storeName) : base(store, typeof(BinaryInfo), storeName)
        {
        }

        public override bool IsBinary => true;
    }
    
    /// <summary>
    /// Holder of text registration information.
    /// </summary>
    public class TextInfo : Info
    {
        public TextInfo(IStateStore store, string storeName) : base(store, typeof(TextInfo), storeName)
        {
        }

        public override bool IsText => true;
    }
}