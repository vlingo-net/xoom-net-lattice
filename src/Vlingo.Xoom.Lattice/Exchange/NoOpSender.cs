// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Xoom.Lattice.Exchange
{
    /// <summary>
    /// For when you are a consumer only.
    /// </summary>
    public class NoOpSender : IExchangeSender<object>
    {
        void IExchangeSender<object>.Send(object message)
        {
        }

        void IExchangeSender.Send(object message)
        {
        }
    }
}