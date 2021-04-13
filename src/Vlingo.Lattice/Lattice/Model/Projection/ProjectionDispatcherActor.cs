// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using Vlingo.Actors;
using Vlingo.Symbio.Store;
using Vlingo.Symbio.Store.Dispatch;
using IDispatcher = Vlingo.Symbio.Store.Dispatch.IDispatcher;

namespace Vlingo.Lattice.Model.Projection
{
    public abstract class ProjectionDispatcherActor : AbstractProjectionDispatcherActor, IDispatcher, IConfirmDispatchedResultInterest
    {
        private readonly IConfirmDispatchedResultInterest _interest;
        private IDispatcherControl? _control;
        private readonly IMultiConfirming _multiConfirming;
        private readonly IProjectionControl _multiConfirmingProjectionControl;
        private readonly IProjectionControl _projectionControl;

        protected ProjectionDispatcherActor() : this(Enumerable.Empty<ProjectToDescription>(), MultiConfirming.DefaultExpirationLimit)
        {
        }
        
        protected ProjectionDispatcherActor(IEnumerable<ProjectToDescription> projectToDescriptions, long multiConfirmationsExpiration) : base(projectToDescriptions)
        {
            _interest = SelfAs<IConfirmDispatchedResultInterest>();
            _projectionControl = new DefaultProjectionControl(_control, _interest, RequiresDispatchedConfirmation, Logger);
            var protocols = ChildActorFor(new[] {typeof(IMultiConfirming), typeof(IProjectionControl)},
                Definition.Has(() =>
                    new MultiConfirmingProjectionControlActor(_projectionControl, multiConfirmationsExpiration)));

            _multiConfirming = protocols.Get<IMultiConfirming>(0);
            _multiConfirmingProjectionControl = protocols.Get<IProjectionControl>(1);
        }
        
        //=====================================
        // Dispatcher
        //=====================================
        
        public void ControlWith(IDispatcherControl control) => _control = control;

        public abstract void Dispatch(Dispatchable dispatchable);

        //=====================================
        // ConfirmDispatchedResultInterest
        //=====================================
        
        public void ConfirmDispatchedResultedIn(Result result, string dispatchId)
        {
        }
        
        //=====================================
        // internal implementation
        //=====================================

        protected abstract bool RequiresDispatchedConfirmation();

        protected virtual void Dispatch(string dispatchId, IProjectable projectable)
        {
            var projections = ProjectionsFor(projectable.BecauseOf()).ToList();

            var count = projections.Count;

            if (count > 1)
            {
                _multiConfirming.ManageConfirmationsFor(projectable, count);
            }

            foreach (var projection in projections)
            {
                projection.ProjectWith(projectable, count > 1 ? _multiConfirmingProjectionControl : _projectionControl);
            }
        }
    }
    
    public class DefaultProjectionControl : IProjectionControl
    {
        private readonly IDispatcherControl? _control;
        private readonly IConfirmDispatchedResultInterest _confirmDispatchedResultInterest;
        private readonly Func<bool> _requiresDispatchedConfirmation;
        private readonly ILogger _logger;

        public DefaultProjectionControl(IDispatcherControl? control, IConfirmDispatchedResultInterest confirmDispatchedResultInterest, Func<bool> requiresDispatchedConfirmation, ILogger logger)
        {
            _control = control;
            _confirmDispatchedResultInterest = confirmDispatchedResultInterest;
            _requiresDispatchedConfirmation = requiresDispatchedConfirmation;
            _logger = logger;
        }
        
        public Confirmer ConfirmerFor(IProjectable projectable, IProjectionControl control) => 
            ProjectionControl.ConfirmerFor(projectable, control);

        public void ConfirmProjected(string projectionId)
        {
            if (_control != null)
            {
                _control.ConfirmDispatched(projectionId, _confirmDispatchedResultInterest);
            }
            else if (_requiresDispatchedConfirmation())
            {
                _logger.Error($"WARNING: ProjectionDispatcher control is not set; unconfirmed: {projectionId}");
            }
        }
    }
}