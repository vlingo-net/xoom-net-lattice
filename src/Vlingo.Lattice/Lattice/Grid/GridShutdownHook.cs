// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Threading;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Cluster.Model;

namespace Vlingo.Lattice.Grid
{
    internal class GridShutdownHook
    {
        private readonly IClusterSnapshotControl _control;
        private readonly ILogger _logger;
        private readonly string _nodeName;

        internal GridShutdownHook(string nodeName, (IClusterSnapshotControl, ILogger) _)
        {
            _nodeName = nodeName;
            (_control, _logger) = _;
        }
        
        internal void Register()
        {
            AppDomain.CurrentDomain.ProcessExit += (s, e) =>
            {
                _logger.Info("\n==========");
                _logger.Info($"Stopping node: '{_nodeName}' ...");
                _control.ShutDown();
                Pause();
                _logger.Info($"Stopped node: '{_nodeName}'");
            };
        }
        
        private void Pause()
        {
            try
            {
                Thread.Sleep(1000);
            }
            catch 
            {
                // ignore
            }
        }
    }
}