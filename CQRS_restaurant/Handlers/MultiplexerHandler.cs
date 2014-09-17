using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CQRS_restaurant.Handlers
{
    public class MultiplexerHandler<T> : IHandler<T> where T : IMessage
    {
        private readonly List<IHandler<T>> _handlers;

        public MultiplexerHandler(IEnumerable<IHandler<T>> handlers)
        {
            _handlers = handlers.ToList();
        }

        public void Handle(T order)
        {
            foreach (var handler in _handlers)
            {
                handler.Handle(order);
            }
        }

        public void AddHandler(IHandler<T> handler)
        {
            _handlers.Add(handler);
        }
    }
}