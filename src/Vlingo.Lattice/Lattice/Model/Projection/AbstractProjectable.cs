// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using Vlingo.Common;
using Vlingo.Symbio;

namespace Vlingo.Lattice.Model.Projection
{
    public abstract class AbstractProjectable : IProjectable
    {
        private readonly List<IEntry> _entries;
        private readonly string _projectionId;
        private readonly IState? _state;
        private int _index;

        public AbstractProjectable(IState state, IEnumerable<IEntry> entries, string projectionId)
        {
            _entries = entries.ToList();
            _projectionId = projectionId;
            _state = state;
        }
        
        public string[] BecauseOf()
        {
            var count = (_state != null ? 1:0) + _entries.Count;
            var becauseOf = new string[count];
            _index = 0;
            if (_state != null) {
                becauseOf[0] = _state.Metadata.Operation;
                ++_index;
            }

            _entries.Select(entry => entry.TypeName).ToList().ForEach(type => becauseOf[_index++] = type);

            return becauseOf;
        }

        public virtual byte[] DataAsBytes() => throw new NotImplementedException("Projectable data is not binary compatible.");

        public virtual string DataAsText() => throw new NotImplementedException("Projectable data is not text compatible.");

        public int DataVersion()
        {
            if (_state == null)
            {
                if (HasEntries)
                {
                    return LastEntry.EntryVersion;
                }
                
                return -1;
            }
            return _state.DataVersion;
        }

        public string DataId
        {
            get
            {
                if (_state == null)
                {
                    return string.Empty;
                }
                return _state.Id;
            }
        }

        public IEnumerable<IEntry> Entries => _entries;
        public bool HasEntries => _entries != null && _entries.Any();

        public string Metadata
        {
            get
            {
                if (_state == null)
                {
                    return string.Empty;
                }
                return _state.Metadata.Value;
            }
        }

        public bool HasObject
        {
            get
            {
                if (_state == null)
                {
                    return false;
                }
                
                return _state.Metadata.HasObject;
            }
        }

        public T Object<T>()
        {
            if (_state == null)
            {
                return default!;
            }
            return (T) _state.Metadata.Object;
        }

        public Optional<T> OptionalObject<T>()
        {
            if (_state == null)
            {
                return Optional.Empty<T>();
            }
            return (Optional<T>)(object) _state.Metadata.OptionalObject;
        }

        public string ProjectionId => _projectionId;

        public bool HasState => _state != null;

        public string Type
        {
            get
            {
                if (_state == null)
                {
                    return "null";
                }
                return _state.Type;
            }
        }

        public int TypeVersion
        {
            get
            {
                if (_state == null)
                {
                    return -1;
                }
                
                return _state.TypeVersion;
            }
        }

        public override string ToString() => $"Projectable [projectionId={_projectionId}, state={_state}, entries={_entries}, index={_index}]";
        
        protected State<byte[]> BinaryState => (State<byte[]>) _state!;
        
        protected State<string> TextState => (State<string>) _state!;

        private IEntry LastEntry => _entries.Aggregate((first, second) => second);
    }
}