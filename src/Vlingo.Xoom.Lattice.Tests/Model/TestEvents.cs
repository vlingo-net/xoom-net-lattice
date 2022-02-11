// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Symbio;

namespace Vlingo.Xoom.Lattice.Tests.Model;

public static class TestEvents
{
    public abstract class Event : Source<Event>
    {
        public static Event1 Event1 => new Event1();
            
        public static Event2 Event2 => new Event2();
            
        public static Event3 Event3 => new Event3();
            
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }
                
            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
        
    public class Event1 : Event
    {
    }
        
    public class Event2 : Event
    {
    }
        
    public class Event3 : Event
    {
    }
}