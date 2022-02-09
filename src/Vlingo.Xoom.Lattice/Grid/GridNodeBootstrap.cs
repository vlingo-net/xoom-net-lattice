// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Cluster.Model;

namespace Vlingo.Xoom.Lattice.Grid
{
    public class GridNodeBootstrap
    {
        private readonly Tuple<IClusterSnapshotControl, ILogger> _clusterSnapshotControl;

        public static GridNodeBootstrap Boot(IGridRuntime grid, string nodeName, bool embedded) => 
            Boot(grid, nodeName, Cluster.Model.Properties.Instance, embedded);
        
        public static GridNodeBootstrap Boot(IGridRuntime grid, string nodeName, Cluster.Model.Properties properties, bool embedded)
        {
            properties.ValidateRequired(nodeName);

            var controlLogger = Cluster.Model.Cluster.ControlFor(
                grid.World,
                node => new GridNode(grid, node),
                properties,
                nodeName);

            var instance = new GridNodeBootstrap(controlLogger, nodeName);

            var (_, logger) = controlLogger;
            logger.Info($"Successfully started cluster node: '{nodeName}'");

            if (!embedded)
            {
                logger.Info("==========");
            }

            return instance;
        }

        public IClusterSnapshotControl ClusterSnapshotControl => _clusterSnapshotControl.Item1;
        
        private GridNodeBootstrap(Tuple<IClusterSnapshotControl, ILogger> control, string nodeName)
        {
            _clusterSnapshotControl = control;

            var shutdownHook = new GridShutdownHook(nodeName, control);
            shutdownHook.Register();
        }
    }
}