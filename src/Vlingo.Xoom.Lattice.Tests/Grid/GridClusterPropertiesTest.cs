// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Lattice.Grid;
using Xunit;

namespace Vlingo.Xoom.Lattice.Tests.Grid;

/// <summary>
/// Properties that are predefined for single-node and three-node grid clusters. These
/// are useful for development and test, but provided for access
/// by tests outside of <code>xoom-net-lattice</code> and <code>xoom-net-cluster</code>.
/// </summary>
public class GridClusterPropertiesTest
{
    [Fact]
    public void ShouldConfigureThreeNodeGrid()
    {
        var properties = GridClusterProperties.AllNodes();

        // common (other common properties are tested in xoom-cluster

        Assert.Equal(typeof(GridNode).Name, properties.GetString("cluster.app.class", ""));

        var seedNodes = properties.GetString("cluster.seedNodes", "").Split(",");

        Assert.Equal(3, seedNodes.Length);
        Assert.Equal("node1", seedNodes[0]);
        Assert.Equal("node2", seedNodes[1]);
        Assert.Equal("node3", seedNodes[2]);

        // node specific
        Assert.Equal("1", properties.GetString("node.node1.id", ""));
        Assert.Equal("node1", properties.GetString("node.node1.name", ""));
        Assert.Equal("localhost", properties.GetString("node.node1.host", ""));
        Assert.Equal("37371", properties.GetString("node.node1.op.port", ""));
        Assert.Equal("37372", properties.GetString("node.node1.app.port", ""));

        Assert.Equal("2", properties.GetString("node.node2.id", ""));
        Assert.Equal("node2", properties.GetString("node.node2.name", ""));
        Assert.Equal("localhost", properties.GetString("node.node2.host", ""));
        Assert.Equal("37373", properties.GetString("node.node2.op.port", ""));
        Assert.Equal("37374", properties.GetString("node.node2.app.port", ""));

        Assert.Equal("3", properties.GetString("node.node3.id", ""));
        Assert.Equal("node3", properties.GetString("node.node3.name", ""));
        Assert.Equal("localhost", properties.GetString("node.node3.host", ""));
        Assert.Equal("37375", properties.GetString("node.node3.op.port", ""));
        Assert.Equal("37376", properties.GetString("node.node3.app.port", ""));

        Assert.Equal("", properties.GetString("node.node4.id", ""));
        Assert.Equal("", properties.GetString("node.node4.name", ""));
        Assert.Equal("", properties.GetString("node.node4.host", ""));
    }

    [Fact]
    public void ShouldConfigureSingleNodeGrid()
    {
        var properties = GridClusterProperties.OneNode();

        // common (other common properties are tested in xoom-cluster

        Assert.Equal(typeof(GridNode).Name, properties.GetString("cluster.app.class", ""));

        var seedNodes = properties.GetString("cluster.seedNodes", "").Split(",");

        Assert.Single(seedNodes);
        Assert.Equal("node1", seedNodes[0]);

        // node specific
        Assert.Equal("1", properties.GetString("node.node1.id", ""));
        Assert.Equal("node1", properties.GetString("node.node1.name", ""));
        Assert.Equal("localhost", properties.GetString("node.node1.host", ""));
        Assert.Equal("37371", properties.GetString("node.node1.op.port", ""));
        Assert.Equal("37372", properties.GetString("node.node1.app.port", ""));

        Assert.Equal("", properties.GetString("node.node2.id", ""));
        Assert.Equal("", properties.GetString("node.node2.name", ""));
        Assert.Equal("", properties.GetString("node.node2.host", ""));
    }
}