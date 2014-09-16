using System.Collections.Generic;
using System.Threading;

namespace CQRS_restaurant.Handlers
{
    public class MoreFairDispatcherHandler<T> : IHandler<T> where T : IMessage
    {
        private readonly List<QueuedHandler<T>> _handlers;

        public MoreFairDispatcherHandler(IEnumerable<QueuedHandler<T>> handlers)
        {
            _handlers = new List<QueuedHandler<T>>(handlers); ;
        }

        public void Handle(T message)
        {
            while (true)
            {

                foreach (var handle in _handlers)
                {
                    var queuecount = handle.GetQueueCount();
                    if (queuecount < 5)
                    {
                        handle.Handle(message);
                        return;
                    }
                }

                Thread.Sleep(1);
            }
        }
    }
}