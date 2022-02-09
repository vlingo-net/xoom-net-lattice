// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Xoom.Lattice.Grid.Cache
{
    public class Cache
    {
        private const string DefaultCacheName = "__defaultCache";
        private string _name;

        public static Cache Of(string name) => new Cache(name);

        public static Cache DefaultCache() => new Cache();

        public Cache(string name) => _name = name;

        public Cache() => _name = DefaultCacheName;
    }
}