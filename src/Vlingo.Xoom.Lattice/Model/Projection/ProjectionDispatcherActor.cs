// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Symbio.Store;
using Vlingo.Xoom.Symbio.Store.Dispatch;
using IDispatcher = Vlingo.Xoom.Symbio.Store.Dispatch.IDispatcher;

namespace Vlingo.Xoom.Lattice.Model.Projection
{
    public abstract class ProjectionDispatcherActor : AbstractProjectionDispatcherActor, IDispatcher, IConfirmDispatchedResultInterest
    {
        private readonly long _multiConfirmationsExpiration;
        private readonly IConfirmDispatchedResultInterest _interest;
        private IMultiConfirming? _multiConfirming;
        private IProjectionControl? _multiConfirmingProjectionControl;
        private readonly Func<IDispatcherControl, IProjectionControl> _projectionControlFactory;
        private IProjectionControl? _projectionControl;

        protected ProjectionDispatcherActor() : this(Enumerable.Empty<ProjectToDescription>(), MultiConfirming.DefaultExpirationLimit)
        {
        }
        
        protected ProjectionDispatcherActor(IEnumerable<ProjectToDescription> projectToDescriptions, long multiConfirmationsExpiration) : base(projectToDescriptions)
        {
            _multiConfirmationsExpiration = multiConfirmationsExpiration;
            _interest = SelfAs<IConfirmDispatchedResultInterest>();
            _projectionControlFactory = control => new DefaultProjectionControl(control, _interest, RequiresDispatchedConfirmation, Logger);
        }
        
        //=====================================
        // Dispatcher
        //=====================================
        
        public void ControlWith(IDispatcherControl control)
        {
            _projectionControl = _projectionControlFactory(control);
            var protocols = ChildActorFor(new[] {typeof(IMultiConfirming), typeof(IProjectionControl)},
                Definition.Has(() =>
                    new MultiConfirmingProjectionControlActor(_projectionControlFactory(control), _multiConfirmationsExpiration)));

            _multiConfirming = protocols.Get<IMultiConfirming>(0);
            _multiConfirmingProjectionControl = protocols.Get<IProjectionControl>(1);
        }

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
                _multiConfirming?.ManageConfirmationsFor(projectable, count);
            }

            foreach (var projection in projections)
            {
                projection.ProjectWith(projectable, count > 1 ? _multiConfirmingProjectionControl! : _projectionControl!);
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