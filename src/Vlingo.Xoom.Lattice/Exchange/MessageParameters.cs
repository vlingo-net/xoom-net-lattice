// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using System.Linq;
using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Lattice.Exchange
{
    /// <summary>
    ///     A builder and set of metadata attributes, any number of which are common
    ///     to associate with messages as parameters. Use and ignore as appropriate.
    ///     The chosen parameters are built using the fluent interface.
    /// </summary>
    public class MessageParameters
    {
        /// <summary>
        ///     Name of the exchange.
        /// </summary>
        public string? ExchangeName { get; private set; }

        /// <summary>
        ///     Identity specific to the application or service.
        /// </summary>
        public string? ApplicationId { get; private set; }

        /// <summary>
        ///     Encoding used for this message, defaulting to UTF_8.
        /// </summary>
        public string? ContentEncoding { get; private set; }

        /// <summary>
        ///     Type of content of the message, defaulting to \"text/plain\".
        /// </summary>
        public string? ContentType { get; private set; }

        /// <summary>
        ///     Identity used to correlate with other messages.
        /// </summary>
        public string? CorrelationId { get; private set; }

        /// <summary>
        ///     Delivery identity.
        /// </summary>
        public string? DeliveryId { get; private set; }

        /// <summary>
        ///     Delivery mode, either Durable or Transient, defaulting to Transient.
        /// </summary>
        public DeliveryMode Mode { get; private set; }

        /// <summary>
        ///     Key-value headers to attach to the messages.
        /// </summary>
        public IReadOnlyDictionary<string, object>? Headers { get; private set; }

        /// <summary>
        ///     Unique identity specific to the message.
        /// </summary>
        public string? MessageId { get; private set; }

        /// <summary>
        ///     Extra parameter 1.
        /// </summary>
        public string? Other1 { get; private set; }

        /// <summary>
        ///     Extra parameter 2.
        /// </summary>
        public string? Other2 { get; private set; }

        /// <summary>
        ///     Extra parameter 3.
        /// </summary>
        public string? Other3 { get; private set; }

        /// <summary>
        ///     Priority of the message, defaulting to Normal.
        /// </summary>
        public Priority Priority { get; private set; }

        /// <summary>
        ///     Name of the queue.
        /// </summary>
        public string? QueueName { get; private set; }

        /// <summary>
        ///     Re-delivery indicator.
        /// </summary>
        public bool Redeliver { get; private set; }

        /// <summary>
        ///     Identification for the receiver to reply to the sender.
        /// </summary>
        public string? ReplyTo { get; private set; }

        /// <summary>
        ///     Return address for the receiver to reply to the sender.
        /// </summary>
        public string? ReturnAddress { get; private set; }

        /// <summary>
        ///     Routing information.
        /// </summary>
        public IEnumerable<string>? Routing { get; private set; }

        /// <summary>
        ///     Tag metadata.
        /// </summary>
        public string? Tag { get; private set; }

        /// <summary>
        ///     Time that the message was created, defaulting to the current time.
        /// </summary>
        public long Timestamp { get; private set; }

        /// <summary>
        ///     Time that the message is valid for delivery, defaulting to {@code Long.MAX_VALUE}.
        /// </summary>
        public long TimeToLive { get; private set; }

        /// <summary>
        ///     Message type code.
        /// </summary>
        public string? TypeCode { get; private set; }

        /// <summary>
        ///     Message type name.
        /// </summary>
        public string? TypeName { get; private set; }

        /// <summary>
        ///     Identity of the user sending the message.
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        ///     Answer a new MessageParameters with no preset values.
        ///     @return MessageParameters
        /// </summary>
        public static MessageParameters Bare()
        {
            return new MessageParameters();
        }

        /// <summary>
        ///     Gets a new MessageParameters with preset defaults.
        /// </summary>
        /// <returns>The <see cref="MessageParameters" /></returns>
        public static MessageParameters WithDefaults()
        {
            return new MessageParameters()
                .WithContentEncoding("UTF8")
                .WithContentType("text/plain")
                .WithDeliveryMode(DeliveryMode.Transient)
                .WithHeaders(new Dictionary<string, object>())
                .WithPriority(Priority.Normal)
                .WithTimestamp(DateTimeHelper.CurrentTimeMillis())
                .WithTimeToLive(long.MaxValue);
        }

        public MessageParameters WithApplicationId(string applicationId)
        {
            ApplicationId = applicationId;
            return this;
        }

        public MessageParameters WithContentEncoding(string contentEncoding)
        {
            ContentEncoding = contentEncoding;
            return this;
        }

        public MessageParameters WithContentType(string contentType)
        {
            ContentType = contentType;
            return this;
        }

        public MessageParameters WithCorrelationId(string correlationId)
        {
            CorrelationId = correlationId;
            return this;
        }

        public MessageParameters WithDeliveryId(string deliveryId)
        {
            DeliveryId = deliveryId;
            return this;
        }

        public MessageParameters WithDeliveryMode(DeliveryMode deliveryMode)
        {
            Mode = deliveryMode;
            return this;
        }

        public bool IsDurableDeliveryMode() => Mode == DeliveryMode.Durable;

        public bool IsTransientDeliveryMode() => Mode == DeliveryMode.Transient;

        public MessageParameters WithExchangeName(string exchangeName)
        {
            ExchangeName = exchangeName;
            return this;
        }

        public MessageParameters WithHeaders(Dictionary<string, object> headers)
        {
            Headers = headers;
            return this;
        }

        public MessageParameters WithMessageId(string messageId)
        {
            MessageId = messageId;
            return this;
        }

        public MessageParameters WithOther1(string other1)
        {
            Other1 = other1;
            return this;
        }

        public MessageParameters WithOther2(string other2)
        {
            Other2 = other2;
            return this;
        }

        public MessageParameters WithOther3(string other3)
        {
            Other3 = other3;
            return this;
        }

        public MessageParameters WithPriority(Priority priority)
        {
            Priority = priority;
            return this;
        }

        public MessageParameters WithQueueName(string queueName)
        {
            QueueName = queueName;
            return this;
        }

        public MessageParameters WithRedeliver(bool redeliver)
        {
            Redeliver = redeliver;
            return this;
        }

        public MessageParameters WithReplyTo(string replyTo)
        {
            ReplyTo = replyTo;
            return this;
        }

        public MessageParameters WithReturnAddress(string returnAddress)
        {
            ReturnAddress = returnAddress;
            return this;
        }

        public MessageParameters WithRouting(params string[] routings)
        {
            Routing = routings.ToList();
            return this;
        }

        public MessageParameters WithTag(string tag)
        {
            Tag = tag;
            return this;
        }

        public MessageParameters WithTimestamp(long timestamp)
        {
            Timestamp = timestamp;
            return this;
        }

        public MessageParameters WithTimeToLive(long timeToLive)
        {
            TimeToLive = timeToLive;
            return this;
        }

        public MessageParameters WithTypeCode(string typeCode)
        {
            TypeCode = typeCode;
            return this;
        }

        public MessageParameters WithTypeName(string typeName)
        {
            TypeName = typeName;
            return this;
        }

        public MessageParameters WithUserId(string userId)
        {
            UserId = userId;
            return this;
        }
    }

    public enum DeliveryMode
    {
        Durable,
        Transient
    }

    public enum Priority
    {
        High,
        Normal,
        Medium,
        Low,
        P0,
        P1,
        P2,
        P3,
        P4,
        P5,
        P6,
        P7,
        P8,
        P9
    }
}