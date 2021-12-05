// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Linq;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Lattice.Grid.Spaces
{
    public class SpaceItemFactoryRelay : ISpace
    {
        private readonly Grid _grid;
        private readonly ISpace _space;

        public SpaceItemFactoryRelay(Grid grid, ISpace space)
        {
            _grid = grid;
            _space = space;
        }
            
        public ICompletes<T> ItemFor<T>(Type actorType, params object[] parameters)
        {
            var actor = _grid.ActorFor<T>(Definition.Has(actorType, parameters.ToArray()), _grid.AddressFactory.Unique());
            return Completes.WithSuccess(actor);
        }

        public ICompletes<KeyItem<T>> Put<T>(IKey key, Item<T> item) => _space.Put(key, item);

        public ICompletes<Optional<KeyItem<T>>> Get<T>(IKey key, Period until) => _space.Get<T>(key, until);

        public ICompletes<Optional<KeyItem<T>>> Take<T>(IKey key, Period until) => _space.Take<T>(key, until);
    }
}