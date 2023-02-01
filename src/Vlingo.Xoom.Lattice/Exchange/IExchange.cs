// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Xoom.Lattice.Exchange;

/// <summary>
/// Defines a message exchange, such as a queue or topic, through which any number of related
/// <see cref="IExchangeSender{T}"/>, <see cref="IExchangeReceiver{T}"/>, and <see cref="IExchangeAdapter{TLocal,TExternal,TExchange}"/> components
/// are registered, and messages are sent.
/// </summary>
public interface IExchange
{
    /// <summary>
    /// Close this Exchange and any underlying resources.
    /// </summary>
    void Close();

    /// <summary>
    /// Gets the channel, which is implementation dependent.
    /// </summary>
    /// <typeparam name="T">The type of the channel</typeparam>
    /// <returns>Channel of type <typeparamref name="T"/></returns>
    T Channel<T>();
        
    /// <summary>
    /// Gets the connection, which is implementation dependent.
    /// </summary>
    /// <typeparam name="T">The type of the connection</typeparam>
    /// <returns>Connection of type <typeparamref name="T"/></returns>
    T Connection<T>();
        
    /// <summary>
    /// Gets the name of the exchange
    /// </summary>
    string Name { get; }
        
    /// <summary>
    /// Registers a <see cref="Covey{TLocal,TExternal,TExchange}"/> with this Exchange.
    /// </summary>
    /// <param name="covey">The <see cref="Covey{TLocal,TExternal,TExchange}"/> to register</param>
    /// <typeparam name="TLocal">The local object type</typeparam>
    /// <typeparam name="TExternal">The external object type</typeparam>
    /// <typeparam name="TExchange">The exchange message type</typeparam>
    /// <returns>The exchange</returns>
    IExchange Register<TLocal, TExternal, TExchange>(Covey<TLocal, TExternal, TExchange> covey);
        
    /// <summary>
    /// Sends the <typeparamref name="TLocal"/> message to the exchange after first adapting it to a exchange message.
    /// </summary>
    /// <param name="local">The <typeparamref name="TLocal"/> local message to send as an exchange message</param>
    /// <typeparam name="TLocal">The local type</typeparam>
    void Send<TLocal>(TLocal local);
}