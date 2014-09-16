using System;

namespace CQRS_restaurant.Handlers
{
    public class ConsolePrintingHandler : IHandler<OrderPaid>
    {
        public void Handle(OrderPaid message)
        {
            Console.WriteLine("Order completed : " + message.Order.OrderId);
            // Console.WriteLine(JsonConvert.SerializeObject(order));
        }
    }
}