using System;
using System.Collections.Generic;

namespace CQRS_restaurant.Handlers
{
    public class RandomlyDropOrder<T> : IHandler<T> where T : IMessage
    {
        private readonly IHandler<T> _handler;
        private readonly int _dropRatio;
        private readonly Random rnd = new Random();

        public RandomlyDropOrder(IHandler<T> handlers, int dropRatio)
        {
            _handler = handlers;
            _dropRatio = dropRatio;
        }

        public void Handle(T message)
        {
            if (rnd.Next(0, _dropRatio * 2) < _dropRatio)
            {
                Console.WriteLine("Order dropped randomly");
                return;
            }

            _handler.Handle(message);

        }
    }
}