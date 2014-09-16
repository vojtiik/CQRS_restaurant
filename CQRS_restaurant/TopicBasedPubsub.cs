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

        public void Publish<T>(string topic, T message) where T : IMessage
        {
            MultiplexerHandler<IMessage> multi;
            if (mutiplexers.TryGetValue(typeof(T).FullName, out multi))
            {
                multi.Handle(message);
            }
            else
            {
                Console.WriteLine("no matching multiplexer." + topic);
            }
        }

        public void Publish<T>(T message) where T : IMessage
        {
            Publish(typeof(T).FullName, message);
        }

    }
}