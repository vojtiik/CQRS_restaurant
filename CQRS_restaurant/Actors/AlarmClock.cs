using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CQRS_restaurant.Handlers;

namespace CQRS_restaurant.Actors
{
    public class AlarmClock : IHandler<SendToMeInX>, IStartable
    {
        private readonly IPublisher _publisher;
        private List<KeyValuePair<DateTime,SendToMeInX>> scheduled = new List<KeyValuePair<DateTime,SendToMeInX>>();
           private Thread thread;
        
        public AlarmClock(IPublisher publisher)
        {
            _publisher = publisher;

            thread = new Thread(Spin);
        }

        public void Start()
        {
            thread.Start();
        }

        public void Handle(SendToMeInX message)
        {
            scheduled.Add(new KeyValuePair<DateTime, SendToMeInX>(DateTime.Now.AddSeconds(message.Seconds), message));
        }

        public void Spin()
        {
            while (true)
            {
                Thread.Sleep(1000);
                var expired = scheduled.Where(x => x.Key < DateTime.Now).ToList();

                foreach (var keyValuePair in expired)
                {
                    var message = keyValuePair.Value;
                    _publisher.Publish(message.Message);
                }
            }
        }
    }
}