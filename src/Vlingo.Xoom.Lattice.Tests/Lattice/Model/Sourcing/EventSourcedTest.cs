// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Actors.TestKit;
using Vlingo.Xoom.Lattice.Model;
using Vlingo.Xoom.Lattice.Model.Sourcing;
using Vlingo.Xoom.Symbio;
using Vlingo.Xoom.Symbio.Store.Journal;
using Vlingo.Xoom.Symbio.Store.Journal.InMemory;
using Xunit;
using Xunit.Abstractions;

namespace Vlingo.Xoom.Lattice.Tests.Lattice.Model.Sourcing
{
    public class EventSourcedTest
    {
        private readonly MockJournalDispatcher _dispatcher;
        private readonly IEntity _entity;
        private readonly Result _result;
        private readonly World _world;
        
        [Fact]
        public void TestThatCtorEmits()
        {
            var resultAccess = _result.AfterCompleting(2);
            var dispatcherAccess = _dispatcher.AfterCompleting(1);

            _entity.DoTest1();

            Assert.True(resultAccess.ReadFrom<bool>("tested1"));
            Assert.Equal(1, resultAccess.ReadFrom<int>("appliedCount"));
            Assert.Equal(1, dispatcherAccess.ReadFrom<int>("entriesCount"));
            var appliedAt0 = resultAccess.ReadFrom<int, object>("appliedAt", 0);
            Assert.NotNull(appliedAt0);
            Assert.Equal(typeof(Test1Happened), appliedAt0.GetType());
            var appendeAt0 = dispatcherAccess.ReadFrom<int, IEntry>("appendedAt", 0);
            Assert.NotNull(appendeAt0);
            Assert.Equal(nameof(Test1Happened), InnerToSimple(appendeAt0.TypeName));
            Assert.False(resultAccess.ReadFrom<bool>("tested2"));
        }
        
        [Fact]
        public void TestThatCommandEmits()
        {
            var resultAccess = _result.AfterCompleting(2);
            var dispatcherAccess = _dispatcher.AfterCompleting(1);

            _entity.DoTest1();

            Assert.True(resultAccess.ReadFrom<bool>("tested1"));
            Assert.False(resultAccess.ReadFrom<bool>("tested2"));
            Assert.Equal(1, resultAccess.ReadFrom<int>("appliedCount"));
            Assert.Equal(1, dispatcherAccess.ReadFrom<int>("entriesCount"));
            var appliedAt0 = resultAccess.ReadFrom<int, object>("appliedAt", 0);
            Assert.NotNull(appliedAt0);
            Assert.Equal(typeof(Test1Happened), appliedAt0.GetType());
            var appendeAt0 = dispatcherAccess.ReadFrom<int, IEntry>("appendedAt", 0);
            Assert.NotNull(appendeAt0);
            Assert.Equal(nameof(Test1Happened), InnerToSimple(appendeAt0.TypeName));

            var resultAccess2 = _result.AfterCompleting(2);
            var dispatcherAccess2 = _dispatcher.AfterCompleting(1);

            _entity.DoTest2();

            Assert.Equal(2, resultAccess2.ReadFrom<int>("appliedCount"));
            Assert.Equal(2, dispatcherAccess.ReadFrom<int>("entriesCount"));
            var appliedAt1 = resultAccess2.ReadFrom<int, object>("appliedAt", 1);
            Assert.NotNull(appliedAt1);
            Assert.Equal(typeof(Test2Happened), appliedAt1.GetType());
            var appendeAt1 = dispatcherAccess2.ReadFrom<int, IEntry>("appendedAt", 1);
            Assert.NotNull(appendeAt1);
            Assert.Equal(nameof(Test2Happened), InnerToSimple(appendeAt1.TypeName));
        }

        [Fact]
        public void TestThatOutcomeCompletes()
        {
            var resultAccess = _result.AfterCompleting(2);
            var dispatcherAccess = _dispatcher.AfterCompleting(1);

            _entity.DoTest1();
            
            Assert.True(resultAccess.ReadFrom<bool>("tested1"));
            Assert.False(resultAccess.ReadFrom<bool>("tested3"));
            Assert.Equal(1, resultAccess.ReadFrom<int>("appliedCount"));
            Assert.Equal(1, dispatcherAccess.ReadFrom<int>("entriesCount"));
            var appliedAt0 = resultAccess.ReadFrom<int, object>("appliedAt", 0);
            Assert.NotNull(appliedAt0);
            Assert.Equal(typeof(Test1Happened), appliedAt0.GetType());
            var appendeAt0 = dispatcherAccess.ReadFrom<int, IEntry>("appendedAt", 0);
            Assert.NotNull(appendeAt0);
            Assert.Equal(nameof(Test1Happened), InnerToSimple(appendeAt0.TypeName));

            var resultAccess2 = _result.AfterCompleting(2);
            var dispatcherAccess2 = _dispatcher.AfterCompleting(1);

            _entity.DoTest3().AndThenConsume(greeting => Assert.Equal("hello", greeting));

            Assert.Equal(2, resultAccess2.ReadFrom<int>("appliedCount"));
            Assert.Equal(2, dispatcherAccess2.ReadFrom<int>("entriesCount"));
            var appliedAt1 = resultAccess2.ReadFrom<int, object>("appliedAt", 1);
            Assert.NotNull(appliedAt1);
            Assert.Equal(typeof(Test3Happened), appliedAt1.GetType());
            var appendeAt1 = dispatcherAccess.ReadFrom<int, IEntry>("appendedAt", 1);
            Assert.NotNull(appendeAt1);
            Assert.Equal(nameof(Test3Happened), InnerToSimple(appendeAt1.TypeName));
        }

        [Fact] 
        public void TestBaseClassBehavior() 
        {
            var product = _world.ActorFor<IProduct>(() => new ProductEntity());

            var access = _dispatcher.AfterCompleting(4);

            product.Define("dice", "fuz", "dice-fuz-1", "Fuzzy dice.", 999);

            product.DeclareType("Type1");

            product.Categorize("Category2");

            product.ChangeName("Fuzzy, fuzzy dice!");

            var entries = access.ReadFrom<List<IEntry>>("entries");

            Assert.Equal(nameof(ProductDefined), InnerToSimple(entries[0].TypeName));
            Assert.Equal(nameof(ProductTyped), InnerToSimple(entries[1].TypeName));
            Assert.Equal(nameof(ProductCategorized), InnerToSimple(entries[2].TypeName));
            Assert.Equal(nameof(ProductNameChanged), InnerToSimple(entries[3].TypeName));
        }

        public EventSourcedTest(ITestOutputHelper output)
        {
            var converter = new Converter(output);
            Console.SetOut(converter);
            
            var testWorld = TestWorld.StartWithDefaults("test-es");

            _world = testWorld.World;

            _dispatcher = new MockJournalDispatcher();

            var entryAdapterProvider = EntryAdapterProvider.Instance(_world);

            entryAdapterProvider.RegisterAdapter(new Test1HappenedAdapter());
            entryAdapterProvider.RegisterAdapter(new Test2HappenedAdapter());
            entryAdapterProvider.RegisterAdapter(new Test3HappenedAdapter());

            var journal = _world.ActorFor<IJournal<string>>(() => new InMemoryJournalActor<string>(_dispatcher));

            var registry = new SourcedTypeRegistry(_world);
            registry.Register(Info.RegisterSourced<TestEventSourcedEntity>(journal));
            registry.Register(Info.RegisterSourced<ProductEntity>(journal));
            registry.Register(Info.RegisterSourced<ProductParent>(journal));
            registry.Register(Info.RegisterSourced<ProductGrandparent>(journal));

            _result = new Result();
            _entity = _world.ActorFor<IEntity>(() => new TestEventSourcedEntity(_result));
        }
        
        private string InnerToSimple(string assemblyQualifiedName)
        {
            var simpleName = Type.GetType(assemblyQualifiedName)?.Name;
            return simpleName;
        }
    }
    
    //===========================
    // HIERARCHICAL TEST TYPES
    //===========================

    public interface IProduct
    {
        void Define(string type, string category, string name, string description, long price);
        void DeclareType(string type);
        void Categorize(string category);
        void ChangeDescription(string description);
        void ChangeName(string name);
        void ChangePrice(long price);
    }
    
    public abstract class ProductGrandparent : EventSourced, IProduct
    {
        private string _type;
        
        protected ProductGrandparent(string streamName) : base(streamName) => RegisterConsumer<ProductTyped>(WhenProductTyped);

        private void WhenProductTyped(ProductTyped @event) => _type = @event.Type;

        public abstract void Define(string type, string category, string name, string description, long price);

        public void DeclareType(string type) => Apply(new ProductTyped(type));

        public abstract void Categorize(string category);

        public abstract void ChangeDescription(string description);

        public abstract void ChangeName(string name);

        public abstract void ChangePrice(long price);

        public override string ToString() => $"Grandparent [type={_type}]";
    }
    
    public abstract class ProductParent : ProductGrandparent
    {
        private string _category;
        
        public ProductParent(string streamName) : base(streamName) => RegisterConsumer<ProductCategorized>(WhenProductCategorized);

        private void WhenProductCategorized(ProductCategorized @event) => _category = @event.Category;

        public override void Categorize(string category) => Apply(new ProductCategorized(category));

        public override string ToString() => $"ProductParent [category={_category}]";
    }

    public class ProductEntity : ProductParent
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public long Price { get; private set; }
        
        public ProductEntity() : base(null)
        {
            RegisterConsumer<ProductDefined>(WhenProductDefined);
            RegisterConsumer<ProductDescriptionChanged>(WhenProductDescriptionChanged);
            RegisterConsumer<ProductNameChanged>(WhenProductNameChanged);
            RegisterConsumer<ProductPriceChanged>(WhenProductPriceChanged);
        }

        public override void Define(string type, string category, string name, string description, long price)
            => Apply(new ProductDefined(name, description, price));

        public override void ChangeDescription(string description)
            => Apply(new ProductDescriptionChanged(description));

        public override void ChangeName(string name) => Apply(new ProductNameChanged(name));

        public override void ChangePrice(long price) => Apply(new ProductPriceChanged(price));
        
        public void WhenProductDefined(ProductDefined @event) 
        {
            Name = @event.Name;
            Description = @event.Description;
            Price = @event.Price;
        }

        public void WhenProductDescriptionChanged(ProductDescriptionChanged @event) {
            Description = @event.Description;
        }

        public void WhenProductNameChanged(ProductNameChanged @event) {
            Name = @event.Name;
        }

        public void WhenProductPriceChanged(ProductPriceChanged @event) {
            Price = @event.Price;
        }
    }

    public class ProductTyped : DomainEvent
    {
        public ProductTyped(string category) => Type = category;

        public string Type { get; }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(ProductTyped)) return false;

            var otherProductPriceChanged = (ProductTyped) obj;

            return Type == otherProductPriceChanged.Type;
        }

        protected bool Equals(ProductTyped other) => base.Equals(other) && Type == other.Type;

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Type);
    }
    
    public class ProductCategorized : DomainEvent
    {
        public ProductCategorized(string category) => Category = category;

        public string Category { get; }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(ProductCategorized)) return false;

            var otherProductPriceChanged = (ProductCategorized) obj;

            return Category == otherProductPriceChanged.Category;
        }

        protected bool Equals(ProductCategorized other) =>
            base.Equals(other) && Category == other.Category;

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Category);
    }

    public class ProductDefined : DomainEvent
    {
        public string Description { get; }
        public string Name { get; }
        public DateTime OccurredOn { get; }
        public long Price { get; }
        public int Version { get; }

        public ProductDefined(string name, string description, long price)
        {
            Name = name;
            Description = description;
            Price = price;
            OccurredOn = DateTime.Now;
            Version = 1;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(ProductDefined))
            {
                return false;
            }

            var otherProductDefined = (ProductDefined) obj;

            return Name.Equals(otherProductDefined.Name) &&
                   Description.Equals(otherProductDefined.Description) &&
                   Price == otherProductDefined.Price &&
                   Version == otherProductDefined.Version;
        }

        protected bool Equals(ProductDefined other) => 
            base.Equals(other) && Description == other.Description && Name == other.Name && OccurredOn.Equals(other.OccurredOn) && Price == other.Price && Version == other.Version;

        public override int GetHashCode() => 
            HashCode.Combine(base.GetHashCode(), Description, Name, OccurredOn, Price, Version);
    }

    public class ProductDescriptionChanged : DomainEvent
    {
        public ProductDescriptionChanged(string description)
        {
            Description = description;
            OccurredOn = DateTime.Now;
            Version = 1;
        }

        public string Description { get; }
        public DateTime OccurredOn { get; }
        public int Version { get; }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(ProductDescriptionChanged)) return false;

            var otherProductPriceChanged = (ProductDescriptionChanged) obj;

            return Description == otherProductPriceChanged.Description && Version == otherProductPriceChanged.Version;
        }

        protected bool Equals(ProductDescriptionChanged other) =>
            base.Equals(other) && Description == other.Description && OccurredOn.Equals(other.OccurredOn) && Version == other.Version;

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Description, OccurredOn, Version);
    }

    public class ProductNameChanged : DomainEvent
    {
        public ProductNameChanged(string name)
        {
            Name = name;
            OccurredOn = DateTime.Now;
            Version = 1;
        }

        public string Name { get; }
        public DateTime OccurredOn { get; }
        public int Version { get; }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(ProductNameChanged)) return false;

            var otherProductPriceChanged = (ProductNameChanged) obj;

            return Name == otherProductPriceChanged.Name && Version == otherProductPriceChanged.Version;
        }

        protected bool Equals(ProductNameChanged other) =>
            base.Equals(other) && Name == other.Name && OccurredOn.Equals(other.OccurredOn) && Version == other.Version;

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Name, OccurredOn, Version);
    }

    public class ProductPriceChanged : DomainEvent
    {
        public ProductPriceChanged(long price)
        {
            Price = price;
            OccurredOn = DateTime.Now;
            Version = 1;
        }

        public long Price { get; }
        public DateTime OccurredOn { get; }
        public int Version { get; }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(ProductPriceChanged)) return false;

            var otherProductPriceChanged = (ProductPriceChanged) obj;

            return Price == otherProductPriceChanged.Price && Version == otherProductPriceChanged.Version;
        }

        protected bool Equals(ProductPriceChanged other) =>
            base.Equals(other) && Price == other.Price && OccurredOn.Equals(other.OccurredOn) && Version == other.Version;

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Price, OccurredOn, Version);
    }
}