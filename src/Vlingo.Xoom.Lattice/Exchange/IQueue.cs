// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Xoom.Lattice.Exchange;

/// <summary>
/// Defines a message exchange, as a queue, through which any number of related
/// <see cref="IExchangeSender{T}"/>, <see cref="IExchangeReceiver{T}"/>, and <see cref="IExchangeAdapter{TLocal,TExternal,TExchange}"/> components
/// </summary>
public interface IQueue : IExchange
{
}