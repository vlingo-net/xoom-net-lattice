// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Wire.Message;
using Vlingo.Xoom.Wire.Nodes;

namespace Vlingo.Xoom.Lattice.Grid.Application;

public interface IApplicationMessageHandler
{
    void Handle(RawMessage message);

    void InformNodeIsHealthy(Id id, bool isHealthy);
}