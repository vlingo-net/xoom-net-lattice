// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Lattice.Query;
using Vlingo.Xoom.Symbio.Store;
using Xunit;

namespace Vlingo.Xoom.Lattice.Tests.Query
{
    public class ObjectQueryFailedExceptionTest
    {
        [Fact]
        public void TestThatFailedHasAttempt()
        {
            var queryAttempt = new QueryAttempt<object, object, object>(Cardinality.All, QueryExpression.Using<object>(""), CompletionTranslator<object, object>.TranslatorOrNull(o => null, null));
            var e = new ObjectQueryFailedException(queryAttempt);

            Assert.NotNull(e);
            Assert.NotNull(e.QueryAttempt());
            Assert.Equal(Cardinality.All, e.QueryAttempt().Cardinality);
            Assert.NotNull(e.QueryAttempt().Query);
            Assert.NotNull(e.QueryAttempt().UntypedCompletionTranslator);
        }
        
        [Fact]
        public void TestThatFailedHasExceptionInfo()
        {
            var cause = new Exception("TestInner", new Exception());
            var queryAttempt = new QueryAttempt<object, object, object>(Cardinality.All, QueryExpression.Using<object>(""), CompletionTranslator<object, object>.TranslatorOrNull(o => null, null));
            var e = new ObjectQueryFailedException(queryAttempt, "TestOuter", cause);

            Assert.NotNull(e);
            Assert.NotNull(e.QueryAttempt());
            Assert.Equal("TestOuter", e.Message);
            Assert.NotNull(e.InnerException);
            Assert.Equal("TestInner", e.InnerException.Message);
            Assert.NotNull(e.Message);
            Assert.NotNull(e.InnerException.InnerException);
        }
    }
}