// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Linq;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Lattice.Grid.Spaces;

public class SpaceItemFactoryRelay : ISpace
{
    private readonly Stage _localStage;
    private readonly ISpace _space;

    public SpaceItemFactoryRelay(Stage localStage, ISpace space)
    {
        _localStage = localStage;
        _space = space;
    }
            
    public ICompletes<T> ItemFor<T>(Type actorType, params object[] parameters)
    {
        var actor = _localStage.ActorFor<T>(actorType, Definition.Has(actorType, parameters.ToArray()), _localStage.AddressFactory.Unique());
        return Completes.WithSuccess(actor);
    }

    public ICompletes<KeyItem> Put(IKey key, Item item) => _space.Put(key, item);

    public ICompletes<Optional<KeyItem>> Get(IKey key, Period until) => _space.Get(key, until);

    public ICompletes<Optional<KeyItem>> Take(IKey key, Period until) => _space.Take(key, until);
}