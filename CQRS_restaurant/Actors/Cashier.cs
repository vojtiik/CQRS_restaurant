using System;
using System.Collections.Generic;
using System.Linq;
using CQRS_restaurant.Handlers;

namespace CQRS_restaurant.Actors
{
    public class Cashier : IHandler<TakePayment>
    {
        private readonly IPublisher _publisher;

        private Dictionary<string, Order> _orders = new Dictionary<string, Order>();

        public Cashier(IPublisher publisher)
        {
            _publisher = publisher;
        }

        public void Handle(TakePayment message)
        {
            var order = message.Order;
            _orders.Add(order.OrderId, order);
            _publisher.Publish(new OrderPaid()
            {
                Order = order
            });
        }

        public void Pay(string orderId)
        {
            _orders[orderId].Paid = true;
            Console.WriteLine("Paid for " + orderId);
        }

        public IList<string> GetOutstandingOrders()
        {
            var orders = _orders.Where(x => x.Value.Paid == false).Select(x => x.Key).ToList();
            return orders;
        }
    }
}