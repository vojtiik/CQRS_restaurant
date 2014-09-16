using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CQRS_restaurant
{
    public class WireUp
    {
        List<ThreadedHandler> startables;
        public void Start()
        {

            var orderHandler = new ConsolePrintingOrderHandler();

            var cashier = new Cashier(orderHandler);

            var assistant = new Assistantmanager(cashier);


            startables = new List<ThreadedHandler>()
            {
                new ThreadedHandler(new Cook(assistant)),
                new ThreadedHandler(new Cook(assistant))
            };
         
            foreach (var startable in startables)
            {
                startable.Start();
            }

            //var waiter = new Waiter(cook);
            var waiter = new Waiter(new RoundRobin(new List<IHandlerOrder>()
            {
               startables[0],
               startables[1]
            }));

            var thread = new Thread(Monitor);
            thread.Start();


            var items = new[]
            {
                new Item {Name = "Soup", Qty = 2, Price = 3.50m},
                new Item {Name = "Goulash", Qty = 2, Price = 3.50m}
            };

            var orderId = waiter.PlaceOrder(items);
            Console.WriteLine("Your order id is: {0}", orderId);

            orderId = waiter.PlaceOrder(items);
            Console.WriteLine("Your order id is: {0}", orderId);


            Console.ReadLine();
            cashier.Pay(orderId);
            Console.ReadLine();
        }

        public void Monitor()
        {
            while (true)
            {
                Thread.Sleep(2000);
                foreach (var startable in startables)
                {
                    Console.WriteLine(startable.Status());
                }
            }
        }
    }

    class Program
    {
        private static void Main(string[] args)
        {
            var v = new WireUp();
            v.Start();
        }
    }

    public class ThreadedHandler : IHandlerOrder, IStartable , IMonitor
    {
        private readonly IHandlerOrder _handler;
        private readonly ConcurrentQueue<Order> _queue = new ConcurrentQueue<Order>();

        public ThreadedHandler(IHandlerOrder handler)
        {
            _handler = handler;
        }

        public void HandleAnOrder(Order order)
        {
            _queue.Enqueue(order);
        }

        public void Start()
        {
            var t = new Thread(ClearTheQueue);
            t.Start();
        }

        public void ClearTheQueue()
        {
            while (true)
            {
                Order order;
                if (!_queue.TryDequeue(out order))
                {
                    Thread.Sleep(17);

                }
                else
                {
                    try
                    {
                        _handler.HandleAnOrder(order);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Handler failed placing the order back to the queue");
                        _queue.Enqueue(order);
                    }
                }
            }
        }


        public string Status()
        {
            return "" + "count in the queue" + _queue.Count();
        }
    }

    public interface IMonitor
    {
        string Status();
    }

    public interface IStartable
    {
        void Start();
    }

    public class RoundRobin : IHandlerOrder
    {
        private readonly Queue<IHandlerOrder> _roundRobinHandlers;

        public void HandleAnOrder(Order order)
        {
            var handle = _roundRobinHandlers.Dequeue();
            try
            {
                handle.HandleAnOrder(order);
            }
            catch (Exception)
            {
                Console.WriteLine("Handler failed");
            }
            finally
            {
                _roundRobinHandlers.Enqueue(handle);
            }
        }
        public RoundRobin(IEnumerable<IHandlerOrder> handlers)
        {
            _roundRobinHandlers = new Queue<IHandlerOrder>(handlers);
        }


    }

    public class Multiplexer : IHandlerOrder
    {
        private readonly IEnumerable<IHandlerOrder> _handlers;

        public Multiplexer(IEnumerable<IHandlerOrder> handlers)
        {
            _handlers = handlers;
        }

        public void HandleAnOrder(Order order)
        {
            foreach (var handler in _handlers)
            {
                handler.HandleAnOrder(order);
            }
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

    public class Waiter
    {
        private readonly IHandlerOrder _orderHandler;

        public Waiter(IHandlerOrder orderHandler)
        {
            _orderHandler = orderHandler;
        }

        public string PlaceOrder(IEnumerable<Item> items)
        {
            Console.WriteLine("waiter : place order");

            var order = new Order { OrderId = Guid.NewGuid().ToString() };
            foreach (var item in items)
            {
                order.AddItem(item);
            }
            _orderHandler.HandleAnOrder(order);
            return order.OrderId;
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
            Console.WriteLine("cook : handle order");
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

        public Assistantmanager(IHandlerOrder nextHandler)
        {
            _nextHandler = nextHandler;
        }

        public void HandleAnOrder(Order order)
        {
            Console.WriteLine("assistant : handle order");
            order.SubTotal = order.Items.Sum(x => x.Price * x.Qty);
            order.Tax = order.SubTotal * 0.2m;
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
            Console.WriteLine("cashier : handle order");
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
