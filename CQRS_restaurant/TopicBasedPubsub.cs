using System;
using System.Collections.Generic;
using CQRS_restaurant.Handlers;

namespace CQRS_restaurant
{
    public class TopicBasedPubsub : IPublisher
    {
        private Dictionary<string, MultiplexerHandler<IMessage>> mutiplexers = new Dictionary<string, MultiplexerHandler<IMessage>>();

        public void Subscribe<T>(IHandler<T> handler) where T : IMessage
        {
            MultiplexerHandler<IMessage> multi;
            if (mutiplexers.TryGetValue(typeof(T).FullName, out multi))
            {
                multi.AddHandler(new Widener<IMessage, T>(handler));
            }
            else
            {
                mutiplexers.Add(typeof(T).FullName,
                    new MultiplexerHandler<IMessage>(new List<IHandler<IMessage>> { new Widener<IMessage, T>(handler) }));
            }
        }

        public void Subscribe<T>(IHandler<T> handler, string correlationId) where T : IMessage
        {
            MultiplexerHandler<IMessage> multi;
            if (mutiplexers.TryGetValue(correlationId, out multi))
            {
                multi.AddHandler(new Widener<IMessage, T>(handler));
            }
            else
            {
                mutiplexers.Add(correlationId,
                    new MultiplexerHandler<IMessage>(new List<IHandler<IMessage>> { new Widener<IMessage, T>(handler) }));
            }
        }

        public void Publish<T>(string topic, T message) where T : IMessage
        {
            MultiplexerHandler<IMessage> multi;
            if (mutiplexers.TryGetValue(topic, out multi))
            {
                multi.Handle(message);
            }
        }

        public void Publish<T>(T message) where T : IMessage
        {
            Publish(typeof(T).FullName, message);
            Publish(message.CorrelationId, message);

            Publish(typeof(IMessage).FullName, message);
           // var interfaces = typeof(T).GetInterface("IMessage");
        }

        public void UnSubscribe(string topic)
        {
            mutiplexers.Remove(topic);
            Console.WriteLine("midget died");
        }

    }
}