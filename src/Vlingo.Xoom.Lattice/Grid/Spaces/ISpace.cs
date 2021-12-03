// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Lattice.Grid.Spaces
{
    public interface ISpace
    {
        ICompletes<T> ItemFor<T>(Type actorType, params object[] parameters);
        ICompletes<KeyItem<T>> Put<T>(IKey key, Item<T> item);
        ICompletes<Optional<KeyItem<T>>> Get<T>(IKey key, Period until);
        ICompletes<Optional<KeyItem<T>>> Take<T>(IKey key, Period until);
    }
}