// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Common.Serialization;
using Vlingo.Xoom.Lattice.Exchange;

namespace Vlingo.Xoom.Lattice.Tests.Lattice.Exchange
{
    public class TestExchangeAdapter2 : DefaultExchangeAdapter<LocalType2, ExternalType2, ExchangeMessage>
    {
        private readonly IExchangeMapper<LocalType2, ExternalType2> _mapper = new TestExchangeMapper2();

        public override bool Supports(object? exchangeMessage)
        {
            if (typeof(ExchangeMessage) != exchangeMessage?.GetType())
            {
                return false;
            }
            
            return nameof(ExternalType2).Equals(((ExchangeMessage) exchangeMessage).Type);
        }

        public override LocalType2 FromExchange(ExchangeMessage exchangeMessage)
        {
            var external = exchangeMessage.Payload<ExternalType2>();
            var local = _mapper.ExternalToLocal(external);
            return local;
        }

        public override ExchangeMessage ToExchange(LocalType2 localMessage)
        {
            var external = _mapper.LocalToExternal(localMessage);
            var payload = JsonSerialization.Serialized(external);
            return new ExchangeMessage(nameof(ExternalType2), payload);
        }
    }
}