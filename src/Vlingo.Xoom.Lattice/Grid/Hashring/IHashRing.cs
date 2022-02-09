// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Xoom.Lattice.Grid.Hashring
{
    public interface IHashRing<T>
    {
        void Dump();
        IHashRing<T> ExcludeNode(T nodeIdentifier);
        IHashRing<T> IncludeNode(T nodeIdentifier);
        T NodeOf(object id);
        IHashRing<T> Copy();
    }
}