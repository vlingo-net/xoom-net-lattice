// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Lattice.Grid.Spaces;

internal class ExpirableQuery : IScheduledScannable<ExpirableQuery>, IComparable<ExpirableQuery>
{
    internal ICompletesEventually Completes { get; }
    internal DateTime ExpiresOn { get; }
    internal IKey Key { get; }
    internal Period Period { get; }
    internal bool RetainItem { get; }

    internal ExpirableQuery(IKey key, bool retainItem, DateTime expiresOn, Period period, ICompletesEventually completes)
    {
        Key = key;
        RetainItem = retainItem;
        ExpiresOn = expiresOn;
        Period = period;
        Completes = completes;
    }
        
    internal bool IsMaximumExpiration => ExpiresOn.GetCurrentSeconds() == DateTime.MaxValue.GetCurrentSeconds();
        
    public int CompareTo(ExpirableQuery? other) => Key.Compare(Key, other?.Key!);
        
    public ExpirableQuery Scannable() => this;
}