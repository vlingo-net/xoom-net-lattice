// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Symbio;
using Vlingo.Symbio.Store.State;

namespace Vlingo.Lattice.Model.Stateful
{
    /// <summary>
    /// Holder of registration information.
    /// </summary>
    /// <typeparam name="T">The native type of the state</typeparam>
    public class Info<T>
    {
        public IStateStore<T> Store { get; }
        public string StoreName { get; }

        public Type StoreType => typeof(T);

        /// <summary>
        /// Construct my default state.
        /// </summary>
        /// <param name="storeName">The string name of the Store</param>
        /// <param name="store">The store</param>
        public Info(IStateStore<T> store, string storeName)
        {
            Store = store;
            StoreName = storeName;
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
    /// <typeparam name="T">The native type of the state</typeparam>
    public class BinaryInfo<T> : Info<T> where T : IEntry
    {
        public BinaryInfo(IStateStore<T> store, string storeName) : base(store, storeName)
        {
        }

        public override bool IsBinary => true;
    }
    
    /// <summary>
    /// Holder of text registration information.
    /// </summary>
    /// <typeparam name="T">The native type of the state</typeparam>
    public class TextInfo<T> : Info<T> where T : IEntry
    {
        public TextInfo(IStateStore<T> store, string storeName) : base(store, storeName)
        {
        }

        public override bool IsText => true;
    }
}