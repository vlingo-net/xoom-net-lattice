// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Lattice.Grid.Spaces;

[Serializable]
public class Period
{
    public static Period Forever = Of(DateTime.MaxValue.GetCurrentSeconds());
    public static Period None = Of(0);

    public TimeSpan Duration { get; }

    public static Period Of(TimeSpan duration) => new Period(duration);

    public static Period Of(long duration) => new Period(TimeSpan.FromSeconds(duration));

    public bool IsForever() => ToMilliseconds() == Forever.ToMilliseconds();

    public DateTime ToFutureDateTime() => IsForever() ? DateTime.MaxValue : DateTime.Now.Add(Duration);

    public long ToMilliseconds() => Convert.ToInt64(Duration.TotalMilliseconds);

    protected Period(TimeSpan duration) => Duration = duration;
}