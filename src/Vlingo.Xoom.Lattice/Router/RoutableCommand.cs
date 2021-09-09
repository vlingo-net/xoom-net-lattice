// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common;
using Vlingo.Xoom.Lattice.Model;

namespace Vlingo.Xoom.Lattice.Router
{
    /// <summary>
    /// A <see cref="Command"/> that may be routed through a defined <see cref="ICommandRouter"/>.
    /// </summary>
    /// <typeparam name="TProtocol">The protocol type</typeparam>
    /// <typeparam name="TCommand">The command type</typeparam>
    /// <typeparam name="TAnswer">The answer (outcome) type</typeparam>
    public class RoutableCommand<TProtocol, TCommand, TAnswer> : Command where TCommand : Command
    {
        private Type? _actorType;
        private string? _address;
        private string _name = "";
        private List<object> _creationParameters;
        private ICommandDispatcher<TProtocol, TCommand, ICompletes<TAnswer>>? _handler;
        private TimeSpan _timeout = TimeSpan.Zero;
        
        public TCommand? Command { get; private set; }
        public ICompletes<TAnswer>? Answer { get; private set; }


        /// <summary>
        /// Gets a new <see cref="RoutableCommand{TProtocol,TCommand,TAnswer}"/> that speaks the <typeparam name="TProtocol"></typeparam>.
        /// </summary>
        /// <returns><see cref="RoutableCommand{TProtocol,TCommand,TAnswer}"/></returns>
        public static RoutableCommand<TProtocol, TCommand, TAnswer> Speaks() => 
            new RoutableCommand<TProtocol, TCommand, TAnswer>();
        
        /// <summary>
        /// Gets myself after assigning my <typeparamref name="TActor"/>.
        /// </summary>
        /// <typeparam name="TActor">The type of the actor.</typeparam>
        /// <returns><see cref="RoutableCommand{TProtocol,TCommand,TAnswer}"/></returns>
        public RoutableCommand<TProtocol, TCommand, TAnswer> To<TActor>() where TActor : Actor
        {
            _actorType = typeof(TActor);
            return this;
        }
        
        /// <summary>
        /// Gets myself after assigning my <paramref name="address"/>.
        /// </summary>
        /// <param name="address">The address of the actor.</param>
        /// <returns><see cref="RoutableCommand{TProtocol,TCommand,TAnswer}"/></returns>
        /// <exception cref="ArgumentOutOfRangeException">Throws an exception when address is equal to -1L</exception>
        public RoutableCommand<TProtocol, TCommand, TAnswer> At(long address)
        {
            if (address == -1L)
            {
                throw new ArgumentOutOfRangeException(nameof(address), "The address cannot have -1L value");
            }
            
            _address = address.ToString();
            return this;
        }
        
        /// <summary>
        /// Gets myself after assigning my <paramref name="address"/>.
        /// </summary>
        /// <param name="address">The address of the actor.</param>
        /// <returns><see cref="RoutableCommand{TProtocol,TCommand,TAnswer}"/></returns>
        /// <exception cref="ArgumentNullException">Throws an exception when address is null or empty</exception>
        public RoutableCommand<TProtocol, TCommand, TAnswer> At(string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                throw new ArgumentNullException(nameof(address), "The address cannot be null or empty");
            }
            
            _address = address;
            return this;
        }
        
        /// <summary>
        /// Gets myself after assigning my <paramref name="creationParameters"/>.
        /// </summary>
        /// <param name="creationParameters">Constructor arguments</param>
        /// <returns><see cref="RoutableCommand{TProtocol,TCommand,TAnswer}"/></returns>
        /// <exception cref="ArgumentNullException">Throws an exception when <paramref name="creationParameters"/> are null</exception>
        public RoutableCommand<TProtocol, TCommand, TAnswer> CreatesWith(params object[] creationParameters)
        {
            if (creationParameters == null)
            {
                throw new ArgumentNullException(nameof(creationParameters), "The creationParameters cannot be null.");
            }
            
            _creationParameters = creationParameters.ToList();
            return this;
        }
        
        /// <summary>
        /// Gets myself after assigning my actor <paramref name="name"/>
        /// </summary>
        /// <param name="name">The name of the actor.</param>
        /// <returns><see cref="RoutableCommand{TProtocol,TCommand,TAnswer}"/></returns>
        /// <exception cref="ArgumentNullException">Throws an exception when name is null or empty</exception>
        public RoutableCommand<TProtocol, TCommand, TAnswer> Named(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name), "The name cannot be null or empty");
            }
            
            _name = name;
            return this;
        }
        
        /// <summary>
        /// Gets myself after assigning my <paramref name="command"/>.
        /// </summary>
        /// <param name="command">The command</param>
        /// <returns><see cref="RoutableCommand{TProtocol,TCommand,TAnswer}"/></returns>
        /// <exception cref="ArgumentNullException">Throws an exception when command is null</exception>
        public RoutableCommand<TProtocol, TCommand, TAnswer> Delivers(TCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command), "The command cannot be null or empty");
            }
            
            Command = command;
            return this;
        }
        
        /// <summary>
        /// Gets myself after assigning my <paramref name="timeout"/>.
        /// </summary>
        /// <param name="timeout">The timeout expressed in ms</param>
        /// <returns><see cref="RoutableCommand{TProtocol,TCommand,TAnswer}"/></returns>
        public RoutableCommand<TProtocol, TCommand, TAnswer> Timeout(long timeout)
        {
            _timeout = TimeSpan.FromMilliseconds(timeout);
            return this;
        }
        
        /// <summary>
        /// Gets myself after assigning my <paramref name="timeout"/>.
        /// </summary>
        /// <param name="timeout">The timeout</param>
        /// <returns><see cref="RoutableCommand{TProtocol,TCommand,TAnswer}"/></returns>
        public RoutableCommand<TProtocol, TCommand, TAnswer> Timeout(TimeSpan timeout)
        {
            _timeout = timeout;
            return this;
        }
        
        /// <summary>
        /// Gets the copy of myself
        /// </summary>
        /// <returns><see cref="RoutableCommand{TProtocol,TCommand,TAnswer}"/></returns>
        public RoutableCommand<TProtocol, TCommand, TAnswer> Copy() => 
            new RoutableCommand<TProtocol, TCommand, TAnswer>(_actorType, _address, Command, Answer, _handler, _creationParameters);

        public override string Id => Command == null || string.IsNullOrEmpty(Command.Id) ? base.Id : Command.Id;
        
        /// <summary>
        /// Gets myself after setting my <paramref name="answer"/>.
        /// </summary>
        /// <param name="answer">The <typeparamref name="TAnswer"/> that serves as my means to answer</param>
        /// <typeparam name="TNewProtocol">The protocol type</typeparam>
        /// <typeparam name="TNewCommand">The command type</typeparam>
        /// <typeparam name="TNewAnswer">The answer type</typeparam>
        /// <returns><see cref="RoutableCommand{TProtocol,TCommand,TAnswer}"/></returns>
        /// <exception cref="ArgumentNullException">Throws an exception when answer is null</exception>
        public RoutableCommand<TNewProtocol, TNewCommand, TNewAnswer> Answers<TNewProtocol, TNewCommand, TNewAnswer>(ICompletes<TNewAnswer> answer) where TNewCommand : Command
        {
            if (answer == null)
            {
                throw new ArgumentNullException(nameof(answer), "The answer cannot be null");
            }
            
            Answer = (ICompletes<TAnswer>)answer;
            return (RoutableCommand<TNewProtocol, TNewCommand, TNewAnswer>)(object) this;
        }
        
        public RoutableCommand<TProtocol, TCommand, TAnswer> Answers(ICompletes<TAnswer> answer)
        {
            if (answer == null)
            {
                throw new ArgumentNullException(nameof(answer), "The answer cannot be null");
            }
            
            Answer = answer;
            return this;
        }
        
        /// <summary>
        /// Gets myself after assigning my <paramref name="handler"/>.
        /// </summary>
        /// <param name="handler">The <see cref="ICommandDispatcher{TProtocol,TCommand,TAnswer}"/> to assign as my handler</param>
        /// <returns><see cref="RoutableCommand{TProtocol,TCommand,TAnswer}"/></returns>
        /// <exception cref="ArgumentNullException">Throws an exception when handler is null</exception>
        public RoutableCommand<TProtocol, TCommand, TAnswer> HandledBy(ICommandDispatcher<TProtocol, TCommand, ICompletes<TAnswer>> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler), "The handler cannot be null");
            }
            _handler = handler;
            return this;
        }
        
        public void HandleWithin(Stage stage)
        {
            Check();

            var actorAddress = stage.AddressFactory.From(_address!, _name);

            stage.ActorOf<TProtocol>(actorAddress)
                .AndThenConsume(_timeout, actor =>
                {
                    _handler?.Accept(actor, Command!, Answer!);
                })
                .Otherwise<TProtocol>(noActor => {
                    var actor = stage.ActorFor<TProtocol>(Definition.Has(_actorType, _creationParameters, _name), actorAddress);
                    _handler?.Accept(actor, Command!, Answer!);
                    return actor;
                });
        }
        
        protected RoutableCommand() => _creationParameters = Definition.NoParameters;
        
        protected RoutableCommand(
        Type? actorType,
        string? address,
        TCommand? command,
        ICompletes<TAnswer>? answer,
        ICommandDispatcher<TProtocol, TCommand, ICompletes<TAnswer>>? handler,
        params object[] creationParameters) {
            _actorType = actorType;
            _address = address;
            Command = command;
            Answer = answer;
            _handler = handler;
            _creationParameters = creationParameters.ToList();
        }

        private void Check()
        {
            ThrowsNull(nameof(_actorType), _actorType);
            ThrowsNull(nameof(_address), _address);
            ThrowsNull(nameof(Command), Command);
            ThrowsNull(nameof(_handler), _handler);
        }

        private void ThrowsNull(string name, object? @object)
        {
            if (@object == null)
            {
                throw new ArgumentNullException(name, $"The {name} cannot be null");
            }
        }
    }
}