// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using Vlingo.Xoom.Actors.TestKit;
using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Lattice.Tests.Model.Sourcing
{
    public class Result
    {
        private AccessSafely _access;
        private readonly List<object> _applied = new List<object>();
        private readonly AtomicBoolean _tested1 = new AtomicBoolean(false);
        private readonly AtomicBoolean _tested2 = new AtomicBoolean(false);
        private readonly AtomicBoolean _tested3 = new AtomicBoolean(false);

        public Result() => _access = AfterCompleting(0);

        public AccessSafely Access() => _access;

        public AccessSafely AfterCompleting(int times)
        {
            _access = AccessSafely
                .AfterCompleting(times)
                .WritingWith<object>("applied", obj => _applied.Add(obj))
                .ReadingWith("applied", () => _applied)
                .ReadingWith("appliedCount", () => _applied.Count)
                .ReadingWith<int, object>("appliedAt", index => _applied[index])
                .WritingWith<bool>("tested1", trueOrFalse => _tested1.Set(trueOrFalse))
                .ReadingWith("tested1", () => _tested1.Get())
                .WritingWith<bool>("tested2", trueOrFalse => _tested2.Set(trueOrFalse))
                .ReadingWith("tested2", () => _tested2.Get())
                .WritingWith<bool>("tested3", trueOrFalse => _tested3.Set(trueOrFalse))
                .ReadingWith("tested3", () => _tested3.Get());

            return _access;
        }
    }
}