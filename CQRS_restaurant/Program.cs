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
        private List<ThreadedHandler> _queues;

        public void Start()
        {
            var orderHandler = new ConsolePrintingOrderHandler();
            var cashier = new Cashier(orderHandler);
            var assistant = new Assistantmanager(cashier);

            var rnd = new Random();
            startables = new List<ThreadedHandler>()
            {
                new ThreadedHandler(new Cook(assistant,  rnd.Next(0,1000) ), "cook1"),
                new ThreadedHandler(new Cook(assistant,  rnd.Next(0,1000) ), "cook2"),
                new ThreadedHandler(new Cook(assistant,  rnd.Next(0,1000) ), "cook3"),
                new ThreadedHandler(new Cook(assistant,  rnd.Next(0,1000) ), "cook4"),
                new ThreadedHandler(new Cook(assistant,  rnd.Next(0,1000) ), "cook5")
            };

            var dispatcher = new ThreadedHandler(new MoreFairDispatcher(startables), "fair dispatcher");
            dispatcher.Start();
          
            _queues = startables.ToList();
            _queues.Add(dispatcher);
           
            foreach (var startable in startables)
            {
                startable.Start();
            }

            //var waiter = new Waiter(cook);
            var waiter = new Waiter(dispatcher);

            var thread = new Thread(Monitor);
            thread.Start();

            var items = new[]
            {
                new Item {Name = "Soup", Qty = 2, Price = 3.50m},
                new Item {Name = "Goulash", Qty = 2, Price = 3.50m}
            };


            for (int i = 0; i < 1000; i++)
            {
                waiter.PlaceOrder(items);
            }


            foreach (var orderid in cashier.GetOutstandingOrders())
            {
                cashier.Pay(orderid);
            }

            Console.ReadLine();
        }

        public void Monitor()
        {
            while (true)
            {
                Thread.Sleep(2000);
                foreach (var startable in _queues)
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


    public class MoreFairDispatcher : IHandlerOrder
    {
        private readonly List<ThreadedHandler> _handlers;

        public MoreFairDispatcher(IEnumerable<ThreadedHandler> handlers)
        {
            _handlers = new List<ThreadedHandler>(handlers); ;
        }

        public void HandleAnOrder(Order order)
        {
            while (true)
            {

                foreach (var handle in _handlers)
                {
                    var queuecount = handle.GetQueueCount();
                    if (queuecount < 5)
                    {
                        handle.HandleAnOrder(order);
                        return;
                    }
                }

                Thread.Sleep(1);
            }
        }


    }


    public class ThreadedHandler : IHandlerOrder, IStartable, IMonitor
    {
        private readonly IHandlerOrder _handler;
        private readonly string _name;
        private readonly ConcurrentQueue<Order> _queue = new ConcurrentQueue<Order>();

        public ThreadedHandler(IHandlerOrder handler, string name)
        {
            _handler = handler;
            _name = name;
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
            return _name + " count in the queue : " + _queue.Count();
        }

        public int GetQueueCount()
        {
            return _queue.Count();
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
            // Console.WriteLine(JsonConvert.SerializeObject(order));
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
        private readonly int _cookTime;

        Dictionary<string, string[]> ingredients = new Dictionary<string, string[]>
        {
            {"Goulash", new[]{"meat", "onion"}},
            {"Soup", new[]{"tears", "elves"}},
            {"Pie", new[]{"beef", "broken dreams"}},
        };

        public Cook(IHandlerOrder nextHandler, int cookTime)
        {
            _nextHandler = nextHandler;
            _cookTime = cookTime;
        }

        public void HandleAnOrder(Order order)
        {
            foreach (var item in order.Items.ToList())
                foreach (var ingredient in ingredients[item.Name].ToList())
                {
                    order.AddIngredient(ingredient);
                }

            //    Console.WriteLine(JsonConvert.SerializeObject(order.Ingredients) + "added.");

            Thread.Sleep(_cookTime);

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
            order.SubTotal = order.Items.Sum(x => x.Price * x.Qty);
            order.Tax = order.SubTotal * 0.2m;
            order.Total = order.SubTotal + order.Tax;

            _nextHandler.HandleAnOrder(order);
        }
    }

    public class Cashier : IHandlerOrder
    {
        private IHandlerOrder _nextHandler;

        private Dictionary<string, Order> _orders = new Dictionary<string, Order>();

        public Cashier(IHandlerOrder nextHandler)
        {
            _nextHandler = nextHandler;
        }

        public void HandleAnOrder(Order order)
        {
            _orders.Add(order.OrderId, order);
            _nextHandler.HandleAnOrder(order);
        }

        public void Pay(string orderId)
        {
            _orders[orderId].Paid = true;
        }

        public IList<string> GetOutstandingOrders()
        {
            return _orders.Where(x => x.Value.Paid == false).Select(x => x.Key).ToList();


        }
    }

    public class Item
    {
        public string Name { get; set; }
        public int Qty { get; set; }
        public decimal Price { get; set; }
    }
}
