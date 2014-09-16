using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

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
        }

        public void Handle(T message)
        {
            _queue.Enqueue(message);
        }

        public void Start()
        {
            var t = new Thread(ClearTheQueue);
            t.Start();
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
                        _queue.Enqueue(message);
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