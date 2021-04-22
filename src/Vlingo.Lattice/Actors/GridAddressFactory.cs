// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Common.Identity;
using Vlingo.Xoom.UUID;

namespace Vlingo.Actors
{
    public class GridAddressFactory : GuidAddressFactory
    {
        private static IAddress Empty = new GridAddress(Guid.Empty, "(Empty)");

        public GridAddressFactory(IdentityGeneratorType type) : base(type)
        {
        }

        public override IAddress FindableBy<T>(T id) => 
            Guid.TryParse(id!.ToString(), out var parsed) ? 
                new GridAddress(parsed) :
                new GridAddress(long.Parse(id!.ToString()).ToGuid());

        public override IAddress From(long reservedId, string name) =>
            Guid.TryParse(reservedId!.ToString(), out var parsed) ? 
                new GridAddress(parsed, name) :
                new GridAddress(long.Parse(reservedId!.ToString()).ToGuid(), name);

        public override IAddress From(string idString) =>
            Guid.TryParse(idString, out var parsed) ? 
                new GridAddress(parsed) :
                new GridAddress(long.Parse(idString).ToGuid());

        public override IAddress From(string idString, string name) =>
            Guid.TryParse(idString, out var parsed) ? 
                new GridAddress(parsed, name) :
                new GridAddress(long.Parse(idString).ToGuid(), name);

        public override IAddress None() => Empty;

        public override IAddress WithHighId() => throw new NotImplementedException("Unsupported for GridAddress.");

        public override IAddress WithHighId(string? name) => throw new NotImplementedException("Unsupported for GridAddress.");
    }
}