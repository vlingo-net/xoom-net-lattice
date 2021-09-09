// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Wire.Nodes;

namespace Vlingo.Xoom.Lattice.Grid.Application.Message
{
    public interface IVisitor
    {
        void Visit<T>(Id receiver, Id sender, Answer<T> answer);
        void Visit<T>(Id receiver, Id sender, Deliver<T> deliver);
        void Visit<T>(Id receiver, Id sender, Start<T> start);
        void Visit<T>(Id receiver, Id sender, Relocate<T> relocate);
        void Visit(Id receiver, Id sender, Forward forward);
    }
}