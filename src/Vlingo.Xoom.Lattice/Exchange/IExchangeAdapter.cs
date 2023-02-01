// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Xoom.Lattice.Exchange;

public interface IExchangeAdapter
{
    object? FromExchange(object exchangeMessage);
      
    object? ToExchange(object localMessage);
        
    /// <summary>
    /// Gets whether or not this adapter supports the <paramref name="exchangeMessage"/>.
    /// </summary>
    /// <param name="exchangeMessage">The possibly supported exchange message</param>
    /// <returns>True if supports <paramref name="exchangeMessage"/></returns>
    bool Supports(object? exchangeMessage);
}
    
/// <summary>
/// Adapts the local messages of type <typeparamref name="TLocal"/> to exchange messages
/// of type <typeparamref name="TExchange"/> that hold external type <typeparamref name="TExternal"/>. This may involve
/// mapping, in which case the underlying implementation must arrange a for
/// <see cref="IExchangeMapper{TLocal,TExternal}"/> to be established. Note that the <typeparamref name="TLocal"/>
/// and <typeparamref name="TExchange"/> types may be different between <see cref="IExchangeAdapter{TLocal,TExternal,TExchange}"/>
/// and the <see cref="IExchangeMapper{TLocal,TExternal}"/>
/// </summary>
/// <typeparam name="TLocal">The local object type</typeparam>
/// <typeparam name="TExternal">The external object type</typeparam>
/// <typeparam name="TExchange">The exchange message type</typeparam>
public interface IExchangeAdapter<TLocal, TExternal, TExchange> : IExchangeAdapter
{
    /// <summary>
    /// Gets the <typeparamref name="TLocal"/> typed local message from the <paramref name="exchangeMessage"/>
    /// of type <typeparamref name="TExchange"/>.
    /// </summary>
    /// <param name="exchangeMessage">the <typeparamref name="TExchange"/> typed exchange message</param>
    /// <returns>The message of type <typeparamref name="TLocal"/></returns>
    TLocal FromExchange(TExchange exchangeMessage);
        
    /// <summary>
    /// Gets the <typeparamref name="TExchange"/> typed exchange message from the <paramref name="localMessage"/>
    /// of type <typeparamref name="TLocal"/>.
    /// </summary>
    /// <param name="localMessage">The message of type <typeparamref name="TLocal"/></param>
    /// <returns>The message of type <typeparamref name="TExchange"/></returns>
    TExchange ToExchange(TLocal localMessage);
}