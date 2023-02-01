// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Xoom.Lattice.Exchange;

public abstract class DefaultExchangeAdapter<TLocal, TExternal, TExchange> : IExchangeAdapter<TLocal, TExternal, TExchange>
{
    public object? FromExchange(object exchangeMessage) => FromExchange((TExchange) exchangeMessage);

    public object? ToExchange(object localMessage) => ToExchange((TLocal) localMessage);

    public abstract bool Supports(object? exchangeMessage);

    public abstract TLocal FromExchange(TExchange exchangeMessage);

    public abstract TExchange ToExchange(TLocal localMessage);
}