// Copyright © 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Actors;

namespace Vlingo.Lattice.Actors
{
    [Serializable]
    public class GridAddress : IAddress
    {
        private readonly Guid _id;
        private string? _name;

        public long Id => _id.ToLeastSignificantBits();
        
        public long IdSequence => _id.ToLeastSignificantBits();
        
        public string IdSequenceString => _id.ToLeastSignificantBits().ToString();
        
        public string IdString => _id.ToLeastSignificantBits().ToString();
        
        public T IdTyped<T>() => (T)(object)IdString;

        public string Name => string.IsNullOrEmpty(_name) ? IdString : _name!;
        
        public bool IsDistributable => true;

        public int CompareTo(IAddress other) => Id.CompareTo(other.Id);

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(GridAddress))
            {
                return false;
            }
            
            return _id.Equals(((GridAddress) obj)._id);
        }

        public override int GetHashCode() => _id.GetHashCode();

        public override string ToString() => $"GridAddress[id={_id}, name={(string.IsNullOrEmpty(_name) ? "(none)" : _name)}]";

        internal GridAddress(Guid reservedId) : this(reservedId, null, false)
        {
        }

        internal GridAddress(Guid reservedId, string? name) : this(reservedId, name, false)
        {
        }

        internal GridAddress(Guid reservedId, string? name, bool prefixName)
        {
            _id = reservedId;
            _name = name == null ? null : prefixName ? name + _id : name;
        }
    }
}