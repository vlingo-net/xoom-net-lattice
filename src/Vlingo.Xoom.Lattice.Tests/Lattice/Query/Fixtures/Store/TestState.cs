// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Tests.Lattice.Query.Fixtures.Store
{
    public class TestState
    {
        public static readonly string MISSING = "(missing)";
        public string Name { get; }

        private TestState(string name) => Name = name;

        public static TestState Named(string name) => new TestState(name);

        public static TestState Missing() => new TestState(MISSING);
    }
}