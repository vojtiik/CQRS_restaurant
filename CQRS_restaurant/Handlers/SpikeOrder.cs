using System;

namespace CQRS_restaurant.Handlers
{
    public class SpikeOrder : IHandler<CQRS_restaurant.SpikeOrder>
    {
        public void Handle(CQRS_restaurant.SpikeOrder message)
        {
            Console.WriteLine("Order completed");
     
        }
    }
}