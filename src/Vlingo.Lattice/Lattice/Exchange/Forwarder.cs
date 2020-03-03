// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Vlingo.Lattice.Exchange
{
    /// <summary>
    /// Forwarder of all local and exchange messages. Registers Covey instances
    /// through which forwarding is accomplished.
    /// </summary>
    public class Forwarder
    {
        private readonly List<object> _coveys;

        /// <summary>
        /// Constructs this Forwarder.
        /// </summary>
        public Forwarder() => _coveys = new List<object>();

        public void ForwardToReceiver<TLocal, TExternal, TExchange>(TExchange exchangeMessage) =>
            ForwardToAllReceivers(exchangeMessage, OfExchangeMessage<TLocal, TExternal, TExchange>(exchangeMessage));


        public void ForwardToAllReceivers<TLocal, TExternal, TExchange>(TExchange exchangeMessage) => 
            ForwardToAllReceivers(exchangeMessage, OfExchangeMessage<TLocal, TExternal, TExchange>(exchangeMessage));

        /// <summary>
        /// Forward the <typeparamref name="TLocal"/>} local object as an <typeparamref name="TExchange"/> exchange message to the sender.
        /// </summary>
        /// <param name="local">The local object to send.</param>
        /// <typeparam name="TLocal">The local object type</typeparam>
        /// <typeparam name="TExternal">The external object type</typeparam>
        /// <typeparam name="TExchange">The exchange message type</typeparam>
        public void ForwardToSender<TLocal, TExternal, TExchange>(TLocal local)
        {
            var covey = OfObjectType<TLocal, TExternal, TExchange>(local?.GetType());
            var exchangeTypedMessage = covey.Adapter.ToExchange(local);
            covey.Sender.Send(exchangeTypedMessage);
        }
        
        public void Register<TLocal, TExternal, TExchange>(Covey<TLocal, TExternal, TExchange> covey) => _coveys.Add(covey);

        private void ForwardToAllReceivers<TLocal, TExternal, TExchange>(TExchange exchangeMessage, IEnumerable<Covey<TLocal, TExternal, TExchange>> coveys)
        {
            coveys.ToList().ForEach(covey =>
            {
                var localObject = covey.Adapter.FromExchange(exchangeMessage);
                covey.Receiver.Receive(localObject);
            });
        }
        
        /// <summary>
        /// Answer a <see cref="Covey{TLocal,TExternal,TExchange}"/> collection of the <paramref name="exchangeMessage"/>.
        /// </summary>
        /// <param name="exchangeMessage">The exchange message to match</param>
        /// <typeparam name="TLocal">The local object type</typeparam>
        /// <typeparam name="TExternal">The external object type</typeparam>
        /// <typeparam name="TExchange">The exchange message type</typeparam>
        /// <returns><see cref="Covey{TLocal,TExternal,TExchange}"/></returns>
        /// <exception cref="ArgumentException">The <typeparamref name="TExchange"/> is not supported.</exception>
        private List<Covey<TLocal, TExternal, TExchange>> OfExchangeMessage<TLocal, TExternal, TExchange>(TExchange exchangeMessage)
        {
            var matched = _coveys
                .Cast<Covey<TLocal, TExternal, TExchange>>()
                .Where(covey => covey.Adapter.Supports(exchangeMessage))
                .ToList();

            if (!matched.Any())
            {
                throw new ArgumentException($"Not a supported message type: {exchangeMessage?.GetType().Name}");
            }

            return matched;
        }

        private Covey<TLocal, TExternal, TExchange> OfObjectType<TLocal, TExternal, TExchange>(Type? objectType)
        {
            foreach (Covey<TLocal, TExternal, TExchange> covey in _coveys)
            {
                if (covey.ExternalType == objectType || covey.ExternalType.IsAssignableFrom(objectType) 
                                                     || covey.LocalType == objectType || covey.LocalType.IsAssignableFrom(objectType))
                {
                    return covey;
                }
            }

            throw new ArgumentException($"Not a supported object type: {objectType?.Name}");
        }
    }
}