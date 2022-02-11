// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using System.Linq;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Lattice.Model.Projection;

public class MultiConfirmingProjectionControlActor : Actor, IMultiConfirming, IProjectionControl, IScheduled<object>
{
    private readonly long _expiration;
    private readonly Dictionary<string, Confirmable> _confirmables;
    private readonly IProjectionControl _projectionControl;
        
    public MultiConfirmingProjectionControlActor(IProjectionControl projectionControl, long expiration)
    {
        _projectionControl = projectionControl;
        _confirmables = new Dictionary<string, Confirmable>();
        _expiration = expiration;
    }
        
    //==========================
    // MultiConfirming
    //==========================
        
    public void ManageConfirmationsFor(IProjectable projectable, int count)
    {
        var expiresBy = DateTimeHelper.CurrentTimeMillis() + _expiration;

        _confirmables.Add(projectable.ProjectionId, new Confirmable(projectable, count, 0, expiresBy));
    }

    //==========================
    // ProjectionControl
    //==========================
        
    public ICompletes<IEnumerable<IProjectable>> ManagedConfirmations()
    {
        var managedConfirmations = _confirmables.Values.Select(confirmable => confirmable.Projectable);

        return Completes().With(managedConfirmations);
    }

    public Confirmer ConfirmerFor(IProjectable projectable, IProjectionControl control) => ProjectionControl.ConfirmerFor(projectable, control);

    public void ConfirmProjected(string projectionId)
    {
        if (!_confirmables.TryGetValue(projectionId, out var confirmable)) return; // too many confirms possible

        var total = confirmable.Total + 1;

        if (confirmable.Count > total)
        {
            _confirmables.Add(projectionId, confirmable.IncrementTotal());
        }
        else
        {
            ProjectionControl.ConfirmerFor(confirmable.Projectable, _projectionControl).Confirm();
            _confirmables.Remove(projectionId);
        }
    }

    //==========================
    // Scheduled
    //==========================
        
    public void IntervalSignal(IScheduled<object> scheduled, object data)
    {
        var currentTime = DateTimeHelper.CurrentTimeMillis();
        var expiredKeys = new List<string>();

        foreach (var projectionId in _confirmables.Keys)
        {
            var confirmable = _confirmables[projectionId];
            if (confirmable.ExpiresBy <= currentTime)
            {
                expiredKeys.Add(projectionId);
            }
        }

        foreach (var projectionId in expiredKeys)
        {
            Logger.Info($"Removing expired confirmable: {projectionId}");
            _confirmables.Remove(projectionId);
        }
    }
        
    private class Confirmable
    {
        public IProjectable Projectable { get; }
        public int Count { get; }
        public int Total { get; }
        public long ExpiresBy { get; }

        public Confirmable(IProjectable projectable, int count, int total, long expiresBy)
        {
            Projectable = projectable;
            Count = count;
            Total = total;
            ExpiresBy = expiresBy;
        }

        public Confirmable IncrementTotal() => new Confirmable(Projectable, Count, Total + 1, ExpiresBy);
    }
}