// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Xoom.Lattice.Model.Process;

/// <summary>
/// State machine state management.
/// </summary>
/// <typeparam name="TState">The type of state</typeparam>
public class Chronicle<TState>
{
    public TState State { get; }
        
    public Chronicle(TState state) => State = state;

    public Chronicle<TState> TransitionTo(TState state) => new Chronicle<TState>(state);
}