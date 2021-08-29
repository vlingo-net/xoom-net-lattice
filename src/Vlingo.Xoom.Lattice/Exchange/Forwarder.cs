// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Vlingo.Xoom.Lattice.Exchange
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

        public void ForwardToReceiver(object exchangeMessage) =>
            ForwardToAllReceivers(exchangeMessage, OfExchangeMessage(exchangeMessage));


        public void ForwardToAllReceivers(object exchangeMessage) => 
            ForwardToAllReceivers(exchangeMessage, OfExchangeMessage(exchangeMessage));

        /// <summary>
        /// Forward the <typeparamref name="TLocal"/>} local object as an exchange message to the sender.
        /// </summary>
        /// <param name="local">The local object to send.</param>
        /// <typeparam name="TLocal">The local object type</typeparam>
        public void ForwardToSender<TLocal>(TLocal local)
        {
            var covey = OfObjectType(local?.GetType());
            var exchangeTypedMessage = covey.Adapter.ToExchange(local!);
            covey.Sender.Send(exchangeTypedMessage!);
        }
        
        public void Register<TLocal, TExternal, TExchange>(Covey<TLocal, TExternal, TExchange> covey) => _coveys.Add(covey);

        private void ForwardToAllReceivers(object exchangeMessage, IEnumerable<Covey> coveys)
        {
            coveys.ToList().ForEach(covey =>
            {
                var localObject = covey.Adapter.FromExchange(exchangeMessage);
                covey.Receiver.Receive(localObject!);
            });
        }
        
        /// <summary>
        /// Answer a <see cref="Covey{TLocal,TExternal,TExchange}"/> collection of the <paramref name="exchangeMessage"/>.
        /// </summary>
        /// <param name="exchangeMessage">The exchange message to match</param>
        /// <returns><see cref="Covey"/></returns>
        private List<Covey> OfExchangeMessage(object exchangeMessage)
        {
            var matched = _coveys
                .Cast<Covey>()
                .Where(covey => covey.Adapter.Supports(exchangeMessage))
                .ToList();

            if (!matched.Any())
            {
                throw new ArgumentException($"Not a supported message type: {exchangeMessage.GetType().Name}");
            }

            return matched;
        }

        private Covey OfObjectType(Type? objectType)
        {
            foreach (Covey covey in _coveys)
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