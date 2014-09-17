using System;

namespace CQRS_restaurant.Handlers
{
    public class TimeToLiveHandler<T> : IHandler<T> where T : IMessage
    {
        private readonly IHandler<T> _handler;

        public TimeToLiveHandler(IHandler<T> handler)
        {
            _handler = handler;
        }

        public void Handle(T message)
        {

            var foo = message as IWontWaitForEver;

            if (foo != null && (foo.LiveUntil == DateTime.MinValue || foo.LiveUntil > DateTime.Now))
            {
                _handler.Handle(message);
            }
            else
            {
                // drop
                Console.WriteLine("dropping order.");

            }
        }
    }
}