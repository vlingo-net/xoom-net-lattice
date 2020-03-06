// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Lattice.Exchange.Local
{
    public class LocalExchangeAdapter<TLocal, TExternal> : IExchangeAdapter<TLocal, TExternal, LocalExchangeMessage>
    {
        public TLocal FromExchange(LocalExchangeMessage exchangeMessage) => exchangeMessage.Payload<TLocal>();

        public LocalExchangeMessage ToExchange(TLocal localMessage) => 
            new LocalExchangeMessage(localMessage!.GetType().FullName!, localMessage);

        public bool Supports(object? exchangeMessage) => exchangeMessage != null && ((LocalExchangeMessage) exchangeMessage).RawPayload.GetType() == typeof(TLocal);
    }
}