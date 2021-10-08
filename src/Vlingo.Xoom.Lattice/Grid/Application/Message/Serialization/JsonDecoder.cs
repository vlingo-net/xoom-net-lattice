// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Newtonsoft.Json;
using Vlingo.Xoom.Common.Expressions;
using Vlingo.Xoom.Common.Serialization;

namespace Vlingo.Xoom.Lattice.Grid.Application.Message.Serialization
{
    public class JsonDecoder : IDecoder
    {
        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            ContractResolver = new BestMatchConstructorResolver(),
            Converters = new List<JsonConverter> { new MessageConverter() },
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Ignore
        };
        
        public TMessage Decode<TMessage>(byte[] bytes) where TMessage : IMessage =>
            JsonSerialization.Deserialized<TMessage>(Encoding.UTF8.GetString(bytes), _settings);
    }
    
    public class MessageConverter : JsonConverter<LambdaExpression>
    {
        public override void WriteJson(JsonWriter writer, LambdaExpression? value, JsonSerializer serializer)
        {
            var expressionNode = ExpressionSerialization.Serialize(value);
            writer.WriteValue(expressionNode);
        }

        public override LambdaExpression ReadJson(JsonReader reader, Type objectType, LambdaExpression? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var s = (string)reader.Value!;
            var expressionSerializationInfo = ExpressionSerialization.Deserialize(s);
            return MakeFrom(expressionSerializationInfo, objectType);
        }

        public override bool CanRead => true;
        
        public override bool CanWrite => true;

        private LambdaExpression MakeFrom(ExpressionSerializationInfo info, Type objectType)
        {
            if (info.Parameters.Length != 1)
            {
                throw new ArgumentException("Can't deserialize expression with multiple parameters");
            }
            var parameter = Expression.Parameter(info.Parameters[0].Type, info.Parameters[0].Name);
            var types = info.FlattenTypes();
            var callExpressions = new List<Expression>(info.ArgumentTypes.Length);
            for (var i = 0; i < types.Length; i++)
            {
                var argExpression = Expression.Constant(info.ArgumentValues[i], info.ArgumentTypes[i]!);
                callExpressions.Add(argExpression);
            }

            var mi = info.Parameters[0].Type.GetMethod(info.MethodName);

            var call = Expression.Call(parameter, mi!, callExpressions.ToArray());
            return Expression.Lambda(objectType.GetGenericArguments()[0], call, parameter);
        }
    }
}