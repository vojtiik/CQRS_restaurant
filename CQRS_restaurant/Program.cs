using System;
using System.Collections;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CQRS_restaurant
{
    class Program
    {
        private static void Main(string[] args)
        {
            var orderHandler = new ConsolePrintingOrderHandler();
         
            var cashier = new Cashier(orderHandler);

            var assistant = new Assistantmanager(cashier);
       
            var cook = new Cook(assistant);
               
            var waiter = new Waiter(cook);


            var items = new[]
            {
                new Item {Name = "Soup", Qty = 2, Price = 3.50m},
                new Item {Name = "Goulash", Qty = 2, Price = 3.50m}
            };

            var orderId = waiter.PlaceOrder(items);

            Console.WriteLine("Your order id is: {0}", orderId);

            Console.ReadLine();
            cashier.Pay(orderId);
            Console.ReadLine();
        }
    }

    public class Waiter
    {
        private readonly IHandlerOrder _orderHandler;

        public Waiter(IHandlerOrder orderHandler)
        {
            _orderHandler = orderHandler;
        }

        public string PlaceOrder(IEnumerable<Item> items)
        {
            var order = new Order { OrderId = Guid.NewGuid().ToString()};
            foreach (var item in items)
            {
                order.AddItem(item);
            }
            _orderHandler.HandleAnOrder(order);
            return order.OrderId;
        }
    }

    public interface IHandlerOrder
    {
        void HandleAnOrder(Order order);
    }

    public class ConsolePrintingOrderHandler : IHandlerOrder
    {
        public void HandleAnOrder(Order order)
        {
            Console.WriteLine(JsonConvert.SerializeObject(order));
        }
    }

    public class Cook : IHandlerOrder
    {
        private IHandlerOrder _nextHandler;
        Dictionary<string, string[]> ingredients = new Dictionary<string, string[]>
        {
            {"Goulash", new[]{"meat", "onion"}},
            {"Soup", new[]{"tears", "elves"}},
            {"Pie", new[]{"beef", "broken dreams"}},
        };

        public Cook(IHandlerOrder nextHandler)
        {
            _nextHandler = nextHandler;
        }

        public void HandleAnOrder(Order order)
        {
            foreach (var item in order.Items.ToList())
            foreach (var ingredient in ingredients[item.Name].ToList())
            {
                order.AddIngredient(ingredient);
            }
             Console.WriteLine(JsonConvert.SerializeObject(order.Ingredients) + "added.");

            Thread.Sleep(TimeSpan.FromSeconds(2));

            _nextHandler.HandleAnOrder(order);
        }
    }

    public class Assistantmanager : IHandlerOrder
    {
        private IHandlerOrder _nextHandler;
        Dictionary<string, string[]> _items = new Dictionary<string, string[]>
        {
            {"Goulash", new[]{"meat", "onion"}},
            {"Soup", new[]{"tears", "elves"}},
            {"Pie", new[]{"beef", "broken dreams"}},
        };

        public Assistantmanager(IHandlerOrder nextHandler)
        {
            _nextHandler = nextHandler;
        }

        public void HandleAnOrder(Order order)
        {
            order.SubTotal = order.Items.Sum(x => x.Price*x.Qty);
            order.Tax = order.SubTotal*0.2m;
            order.Total = order.SubTotal + order.Tax;

            _nextHandler.HandleAnOrder(order);
        }
    }

    public class Cashier : IHandlerOrder
    {
        private IHandlerOrder _nextHandler;
        private Dictionary<string, Order> _items = new Dictionary<string, Order>();
        
        public Cashier(IHandlerOrder nextHandler)
        {
            _nextHandler = nextHandler;
        }

        public void HandleAnOrder(Order order)
        {
            _items.Add(order.OrderId, order);
            _nextHandler.HandleAnOrder(order);
        }

        public void Pay(string orderId)
        {
            _items[orderId].Paid = true;
        }
    }

    public class Item
    {
        public string Name { get; set; }
        public int Qty { get; set; }
        public decimal Price { get; set; }
    }
}
