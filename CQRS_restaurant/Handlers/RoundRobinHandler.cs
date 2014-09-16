using System;
using System.Collections.Generic;

namespace CQRS_restaurant.Handlers
{
    public class RoundRobinHandler<T> : IHandler<T> where T : IMessage
    {
        private readonly Queue<IHandler<T>> _roundRobinHandlers;

        public void Handle(T order)
        {
            var handle = _roundRobinHandlers.Dequeue();
            try
            {
                handle.Handle(order);
            }
            catch (Exception)
            {
                Console.WriteLine("Handler failed");
            }
            finally
            {
                _roundRobinHandlers.Enqueue(handle);
            }
        }

        public RoundRobinHandler(IEnumerable<IHandler<T>> handlers)
        {
            _roundRobinHandlers = new Queue<IHandler<T>>(handlers);
        }
    }
}