using System.Linq;
using CQRS_restaurant.Handlers;

namespace CQRS_restaurant.Actors
{
    public class Assistantmanager : IHandler<OrderCooked>
    {
        private readonly IPublisher _publisher;

        public Assistantmanager(IPublisher publisher)
        {
            _publisher = publisher;

        }

        public void Handle(OrderCooked message)
        {
            var order = message.Order;

            order.SubTotal = order.Items.Sum(x => x.Price * x.Qty);
            order.Tax = order.SubTotal * 0.2m;
            order.Total = order.SubTotal + order.Tax;

            _publisher.Publish(new OrderPriced() { Order = message.Order });
        }
    }
}