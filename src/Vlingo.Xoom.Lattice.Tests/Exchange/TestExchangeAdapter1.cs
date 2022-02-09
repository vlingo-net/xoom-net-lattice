// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Common.Serialization;
using Vlingo.Xoom.Lattice.Exchange;

namespace Vlingo.Xoom.Lattice.Tests.Exchange
{
    public class TestExchangeAdapter1 : DefaultExchangeAdapter<LocalType1, ExternalType1, ExchangeMessage>
    {
        private readonly IExchangeMapper<LocalType1, ExternalType1> _mapper = new TestExchangeMapper1();

        public override bool Supports(object exchangeMessage)
        {
            if (typeof(ExchangeMessage) != exchangeMessage?.GetType())
            {
                return false;
            }
            
            return nameof(ExternalType1).Equals(((ExchangeMessage) exchangeMessage).Type);
        }

        public override LocalType1 FromExchange(ExchangeMessage exchangeMessage)
        {
            var external = exchangeMessage.Payload<ExternalType1>();
            var local = _mapper.ExternalToLocal(external);
            return local;
        }

        public override ExchangeMessage ToExchange(LocalType1 localMessage)
        {
            var external = _mapper.LocalToExternal(localMessage);
            var payload = JsonSerialization.Serialized(external);
            return new ExchangeMessage(nameof(ExternalType1), payload);
        }
    }
}