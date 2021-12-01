// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections;
using System.Collections.Generic;
using Vlingo.Xoom.Actors;

namespace Vlingo.Xoom.Lattice.Grid
{
    public class GridActorOperations
    {
        private const string Resume = "GridActor.Resume";
        
        public static void ApplyRelocationSnapshot<S>(Stage stage, Actor actor, S snapshot)
        {
            var consumer = stage.ActorAs<IRelocatable>(actor);
            consumer.StateSnapshot(snapshot);
        }

        public static S SupplyRelocationSnapshot<S>(Actor actor) => actor.StateSnapshot<S>();
        
        public static object SupplyRelocationSnapshot(Actor actor) => actor.StateSnapshot();
        
        public static IEnumerable<IMessage> Pending(Actor actor)
        {
            var mailbox = actor.ActorMailbox(actor);

            var messages = new PendingMessageEnumerable(new PendingMessageIterator(mailbox));

            foreach (var message in messages)
            {
                yield return message;
            }
        }
        
        public static bool IsSuspendedForRelocation(Actor actor) => actor.ActorMailbox(actor).IsSuspendedFor(Resume);
        
        public static void SuspendForRelocation(Actor actor) =>
            actor.ActorMailbox(actor).SuspendExceptFor(Resume, typeof(IRelocatable));

        private class PendingMessageIterator : IEnumerator<IMessage>
        {
            private readonly IMailbox _mailbox;

            private IMessage? _current;

            public PendingMessageIterator(IMailbox mailbox) => _mailbox = mailbox;

            public bool MoveNext()
            {
                if (_current == null)
                {
                    _current = _mailbox.Receive();
                    return true;
                }

                return _current != null;
            }

            public void Reset() => _current = null;

            public IMessage Current => _current!;

            object? IEnumerator.Current => Current;

            public void Dispose()
            {
            }
        }
        
        public class PendingMessageEnumerable : IEnumerable<IMessage>
        {
            private readonly IEnumerator<IMessage> _enumerator;
            
            public PendingMessageEnumerable(IEnumerator<IMessage> e) => _enumerator = e;

            public IEnumerator<IMessage> GetEnumerator() => _enumerator;
            
            IEnumerator IEnumerable.GetEnumerator()
            {
                _enumerator.Reset();
                return _enumerator;
            }
        }
    }
}