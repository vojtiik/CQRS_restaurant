using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using EventStore.ClientAPI;
using Newtonsoft.Json;

namespace CQRS_restaurant.Handlers
{
    public class QueuedHandler<T> : IStartable, IMonitor, IHandler<T> where T : IMessage
    {
        private readonly IHandler<T> _handler;
        private readonly string _name;
        private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();

        public QueuedHandler(IHandler<T> handler, string name)
        {
            _handler = handler;
            _name = name;

            EventStoreConnection._connection.SubscribeToStreamAsync(_name, false, eventAppeared).Wait();
        }

        private void eventAppeared(EventStoreSubscription arg1, ResolvedEvent arg2)
        {
            var s = UnicodeEncoding.UTF8.GetString(arg2.Event.Data);
            var type = Type.GetType(arg2.Event.EventType);
            var message = (T)JsonConvert.DeserializeObject(s, type,new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });
            
            try
            {
                _handler.Handle(message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Handler failed placing the order back to the queue" + e.Message);
                //_queue.Enqueue(message);
                Enqueue(message);
            }
        }

        public void Handle(T message)
        {
            // _queue.Enqueue(message);
            Enqueue(message);
        }

        public void Enqueue(T message)
        {
            var sermessage = JsonConvert.SerializeObject(message, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });

            EventStoreConnection._connection.AppendToStreamAsync(_name,
                           ExpectedVersion.Any,
                           new EventData(Guid.NewGuid(), message.GetType().FullName, true, UnicodeEncoding.UTF8.GetBytes(sermessage), new byte[0])
                        ).Wait();
        }



        public void Start()
        {
            //var t = new Thread(ClearTheQueue);
            //t.Start();
        }

        public void ClearTheQueue()
        {
            while (true)
            {
                T message;
                if (!_queue.TryDequeue(out message))
                {
                    Thread.Sleep(17);

                }
                else
                {
                    try
                    {
                        _handler.Handle(message);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Handler failed placing the order back to the queue" + e.Message);
                        //_queue.Enqueue(message);
                        Enqueue(message);

                    }
                }
            }
        }

        public string Status()
        {
            return _name + " count in the queue : " + _queue.Count();
        }

        public int GetQueueCount()
        {
            return _queue.Count();
        }
    }
}