// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Cluster;
using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Lattice.Grid
{
    public class GridClusterProperties
    {
        private static int DefaultStartPort = 37371;
        private static int DefaultTotalNodes = 3;

        public static Cluster.Model.Properties AllNodes() => AllNodes(DefaultStartPort);

        public static Cluster.Model.Properties AllNodes(int startPort) => AllNodes(startPort, DefaultTotalNodes);

        public static Cluster.Model.Properties AllNodes(int startPort, int totalNodes) => 
            ClusterProperties.AllNodes(new AtomicInteger(startPort - 1), totalNodes, typeof(GridNode).Name);

        public static Cluster.Model.Properties OneNode() => OneNode(DefaultStartPort);

        public static Cluster.Model.Properties OneNode(int startPort) => 
            ClusterProperties.OneNode(new AtomicInteger(startPort - 1), typeof(GridNode).Name);
    }
}