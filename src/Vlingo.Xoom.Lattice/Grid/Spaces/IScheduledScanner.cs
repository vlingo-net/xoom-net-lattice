// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Xoom.Lattice.Grid.Spaces;

public interface IScheduledScanner<T>
{
    void Scan();
    void ScheduleBy(IScheduledScannable<T> scannable);
}

public interface IScheduledScannable
{
}

public interface IScheduledScannable<out T> : IScheduledScannable
{
    T Scannable();
}