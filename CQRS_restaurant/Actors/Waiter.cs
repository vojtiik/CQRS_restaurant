using System;
using System.Collections.Generic;

namespace CQRS_restaurant.Actors
{
    public class Waiter
    {
        private readonly IPublisher _publisher;

        public Waiter(IPublisher publisher)
        {
            _publisher = publisher;
        }

        public string PlaceOrder(IEnumerable<Item> items)
        {
            var order = new Order { OrderId = Guid.NewGuid().ToString() };
          

            foreach (var item in items)
            {
                order.AddItem(item);
            }

            _publisher.Publish(new CookFood()
            {
                Order = order,
                LiveUntil = DateTime.Now.AddSeconds(10)
                
            });

            return order.OrderId;
        }
    }
}