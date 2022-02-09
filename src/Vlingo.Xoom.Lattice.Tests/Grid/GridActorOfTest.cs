// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Actors.Plugin.Logging.Console;
using Vlingo.Xoom.Actors.TestKit;
using Vlingo.Xoom.Cluster;
using Vlingo.Xoom.Common;
using Xunit;
using Xunit.Abstractions;

namespace Vlingo.Xoom.Lattice.Tests.Grid
{
    public class GridActorOfTest : IDisposable
    {
        private readonly Lattice.Grid.Grid _grid;
        protected readonly World World;
        protected readonly TestWorld TestWorld;
        
        [Fact]
        public void TestThatNonExistingActorCreates()
        {
            var address = _grid.AddressFactory.Unique();

            var valueHolder = new AtomicInteger(0);
            var access = AccessSafely.AfterCompleting(1);
            access.WritingWith<int>("value", value => valueHolder.Set(value));
            access.ReadingWith("value", () => valueHolder.Get());

            var proxy1 = _grid.ActorOf<INoProtocol>(address).Await();

            Assert.Null(proxy1);
            Assert.Equal(0, valueHolder.Get());

            var proxy2 = _grid.ActorOf<INoProtocol>(address, () => new NoExistingActor( 1, access)).Await();

            var value = access.ReadFrom<int>("value");

            Assert.NotNull(proxy2);
            Assert.Equal(1, value);
            Assert.Equal(value, valueHolder.Get());
        }

        [Fact]
        public void TestThatActorOfAddressIsPingedByThreeClients()
        {
            var address = _grid.AddressFactory.Unique();

            var valueHolder = new AtomicInteger(0);

            var access = AccessSafely.AfterCompleting(3);

            access.WritingWith<int>("value", value => valueHolder.Set(value));
            access.ReadingWith("value", () => valueHolder.Get());

            var proxy1 = _grid.ActorOf<IRingDing>(address, () => new RingDingActor(access)).Await();
            Assert.NotNull(proxy1);
            proxy1.RingDing();

            var proxy2 = _grid.ActorOf<IRingDing>(address, () => new RingDingActor(access)).Await();
            Assert.NotNull(proxy2);
            proxy2.RingDing();

            var proxy3 = _grid.ActorOf<IRingDing>(address, () => new RingDingActor(access)).Await();
            Assert.NotNull(proxy3);
            proxy3.RingDing();

            var value = access.ReadFrom<int>("value");

            Assert.Equal(3, value);
            Assert.Equal(value, valueHolder.Get());
        }
        
        public GridActorOfTest(ITestOutputHelper output)
        {
            var converter = new Converter(output);
            Console.SetOut(converter);
            
            var configuration =
                Configuration
                    .Define()
                    .With(ConsoleLoggerPluginConfiguration
                        .Define()
                        .WithDefaultLogger()
                        .WithName("xoom-actors"));

            TestWorld = TestWorld.Start("test", configuration);
            World = TestWorld.World;

            var properties = ClusterProperties.OneNode();

            _grid = Lattice.Grid.Grid.Start(World, properties, "node1");
            _grid.QuorumAchieved();
        }

        public void Dispose() => TestWorld?.Terminate();
    }

    public interface IRingDing
    {
        void RingDing();
    }
    
    public class RingDingActor : Actor, IRingDing
    {
        private readonly AccessSafely _access;
        private int _value;
        
        public RingDingActor(AccessSafely access)
        {
            _access = access;
            _value = 0;

            if (!(Stage is Lattice.Grid.Grid))
            {
                throw new ArgumentException("The Stage should be of Grid type");
            }
        }
        
        public void RingDing() => _access.WriteUsing("value", ++_value);
    }

    public class NoExistingActor : Actor, INoProtocol
    {
        public NoExistingActor(int value, AccessSafely access)
        {
            access.WriteUsing("value", value);

            if (!(Stage is Lattice.Grid.Grid))
            {
                throw new ArgumentException("The Stage should be of Grid type");
            }
        }
    }
}