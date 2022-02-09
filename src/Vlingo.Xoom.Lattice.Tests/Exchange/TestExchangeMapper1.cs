// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Lattice.Exchange;

namespace Vlingo.Xoom.Lattice.Tests.Exchange
{
    public class TestExchangeMapper1 : IExchangeMapper<LocalType1, ExternalType1>
    {
        public ExternalType1 LocalToExternal(LocalType1 local) => new ExternalType1(local.Attribute1, local.Attribute2);

        public LocalType1 ExternalToLocal(ExternalType1 external) => new LocalType1(external.Field1, int.Parse(external.Field2));
    }
}