// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Actors.Plugin.Logging.Console;

namespace Vlingo.Xoom.Lattice.Exchange
{
    /// <summary>
    /// Exchange that does nothing.
    /// </summary>
    public class NullExchange : IExchange
    {
        private readonly ILogger _logger = ConsoleLogger.BasicInstance();
        
        public static readonly NullExchange Instance = new NullExchange();
        
        public void Close()
        {
        }

        public T Channel<T>() => default!;

        public T Connection<T>() => default!;

        public string Name => "NullExchange";

        public IExchange Register<TLocal, TExternal, TExchange>(Covey<TLocal, TExternal, TExchange> covey) => this;

        public void Send<TLocal>(TLocal local) => _logger.Error($"NullExchange: Sending nowhere: {local}");
    }
}