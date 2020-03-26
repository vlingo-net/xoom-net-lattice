// Copyright © 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;
using Vlingo.Actors;
using Vlingo.Common;
using Vlingo.Lattice.Lattice.Grid.Application;
using Vlingo.Lattice.Lattice.Grid.Hashring;
using Vlingo.Wire.Node;

namespace Vlingo.Lattice.Actors
{
    public class GridMailbox : IMailbox
    {
        //private static ILogger log = LoggerFactory.getLogger(GridMailbox.class);

        private IMailbox local;
        private Id localId;
        private Address address;

        private IHashRing<Id> hashRing;

        private IOutbound outbound;
        
        public void Run()
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }
        
        public TaskScheduler TaskScheduler { get; }

        public bool IsClosed { get; }
        public bool IsDelivering { get; }
        public void Resume(string name)
        {
            throw new NotImplementedException();
        }

        public void Send(IMessage message)
        {
            throw new NotImplementedException();
        }

        public void SuspendExceptFor(string name, params Type[] overrides)
        {
            throw new NotImplementedException();
        }

        public bool IsSuspended { get; }
        public IMessage? Receive()
        {
            throw new NotImplementedException();
        }

        public int PendingMessages { get; }
        public bool IsPreallocated { get; }
        public void Send<T>(Actor actor, Action<T> consumer, ICompletes? completes, string representation)
        {
            throw new NotImplementedException();
        }
    }
}