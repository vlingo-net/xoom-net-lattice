// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Symbio.Store;

namespace Vlingo.Xoom.Lattice.Exchange.Feeds;

/// <summary>
/// The default implementation of <see cref="Feed"/>. See the default behaviors
/// in <see cref="Feed"/> for specific defaults.
/// </summary>
public class DefaultFeed : Feed
{
    private readonly string _exchangeName;
    private readonly Type _feederType;
    private readonly IEntryReader _entryReaderType;
    private readonly IFeeder _feeder;

    public override IEntryReader EntryReaderType => _entryReaderType;
    public override Type FeederType => _feederType;
    public override IFeeder Feeder => _feeder;
    public override string ExchangeName => _exchangeName;

    /// <summary>
    /// Construct my default state.
    /// </summary>
    /// <param name="stage">This <see cref="Stage"/> of actors I create</param>
    /// <param name="exchangeName">The name of the exchange I feed</param>
    /// <param name="feederType">The type of the <see cref="IFeeder"/></param>
    /// <param name="entryReaderType">The <see cref="IEntryReader"/> used by the <see cref="IFeeder"/></param>
    internal DefaultFeed(Stage stage, string exchangeName, Type feederType, IEntryReader entryReaderType)
    {
        _exchangeName = exchangeName;
        _feederType = feederType;
        _entryReaderType = entryReaderType;
        _feeder = stage.ActorFor<IFeeder>(feederType, this, entryReaderType);
    }
}