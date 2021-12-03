// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Xoom.Lattice.Grid.Spaces
{
    public class Period
    {
        public static Period Forever = Of(GetCurrentMillis(DateTime.MaxValue));
        public static Period None = Of(0);

        public TimeSpan Duration { get; }

        public static Period Of(TimeSpan duration) => new Period(duration);

        public static Period Of(long duration) => new Period(TimeSpan.FromSeconds(duration));

        public bool IsForever() => ToMilliseconds() == Forever.ToMilliseconds();

        public DateTime ToNow() => IsForever() ? DateTime.MaxValue : DateTime.Now.Add(Duration);

        public long ToMilliseconds() => Convert.ToInt64(Duration.TotalMilliseconds);

        protected Period(TimeSpan duration) => Duration = duration;
        
        protected static long GetCurrentMillis(DateTime dateTime)
        {
            var jan1970 = new DateTime(1970, 1, 1, 0, 0,0, DateTimeKind.Utc);
            var javaSpan = dateTime - jan1970;
            return Convert.ToInt64(javaSpan.TotalSeconds);
        }
    }
}