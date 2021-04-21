// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Actors;
using Vlingo.Common.Identity;
using Vlingo.Xoom.UUID;
using Xunit;

namespace Vlingo.Tests.Actors
{
    public class GridAddressTest
    {
        [Fact]
        public void TestNameGiven()
        {
            var addressFactory = new GridAddressFactory(IdentityGeneratorType.Random);
            
            var address = addressFactory.UniqueWith("test-address");
    
            Assert.NotNull(address);
            Assert.NotNull(address.IdString);
            Assert.Equal("test-address", address.Name);
    
            var another = addressFactory.UniqueWith("another-address");
    
            Assert.NotEqual(another, address);
            Assert.NotEqual(0, address.CompareTo(another));
            Assert.Equal(address.IdTyped(s => s), address.IdString);
        }
        
        [Fact]
        public void TestNameAndGuidIdGiven()
        {
            var addressFactory = new GridAddressFactory(IdentityGeneratorType.Random);

            var id1 = Guid.NewGuid().ToString();
    
            var address = addressFactory.From(id1, "test-address");
    
            Assert.NotNull(address);
            Assert.Equal(new Guid(id1).ToLeastSignificantBits(), address.Id);
            Assert.Equal("test-address", address.Name);
    
            var id2 = Guid.NewGuid().ToString();
            var another = addressFactory.From(id2, "test-address");
    
            Assert.NotEqual(another, address);
            Assert.NotEqual(0, address.CompareTo(another));
            Assert.Equal(address, addressFactory.From(id1, "test-address"));
            Assert.Equal(0, address.CompareTo(addressFactory.From(id1, "test-address")));
        }

        [Fact]
        public void TestNameAndLongIdGiven()
        {
            var addressFactory = new GridAddressFactory(IdentityGeneratorType.Random);

            var id = 123;
    
            var address = addressFactory.From(id, "test-address");
    
            Assert.NotNull(address);
            Assert.Equal(123, address.Id);
            Assert.Equal("test-address", address.Name);
    
            var another = addressFactory.From(456, "test-address");
    
            Assert.NotEqual(another, address);
            Assert.NotEqual(0, address.CompareTo(another));
            Assert.Equal(address, addressFactory.From(id, "test-address"));
            Assert.Equal(0, address.CompareTo(addressFactory.From(id, "test-address")));
        }

        [Fact]
        public void TestTimeBasedOrdering()
        {
            var addressFactory = new GridAddressFactory(IdentityGeneratorType.TimeBased);

            var ordered = new IAddress[10];
            var reversed = new IAddress[10];
            for (var idx = 0; idx < ordered.Length; ++idx)
            {
                ordered[idx] = addressFactory.Unique();
                reversed[reversed.Length - idx - 1] = ordered[idx];
            }
            Array.Sort(reversed);
            Assert.Equal(ordered, reversed);
            Array.Sort(ordered);
            Assert.Equal(reversed, ordered);
        }
    }
}