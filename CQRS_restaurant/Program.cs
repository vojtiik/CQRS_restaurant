using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CQRS_restaurant
{
    public class WireUp
    {
        List<ThreadedHandler<OrderPlaced>> cooks;
        private List<IMonitor> monitorQueues;

        public void Start()
        {
            var pubsub = new Pubsub();

            var orderHandler = new ConsolePrintingHandler<OrderPaid>();
            var cashier = new ThreadedHandler<OrderPriced>(new Cashier(pubsub), "cashier");
            var assistant = new ThreadedHandler<OrderCooked>(new Assistantmanager(pubsub), "assistant");

            var rnd = new Random();
            cooks = new List<ThreadedHandler<OrderPlaced>>()
            {
                new ThreadedHandler<OrderPlaced>(new Cook(pubsub,  rnd.Next(0,1000) ), "cook1"),
                new ThreadedHandler<OrderPlaced>(new Cook(pubsub,  rnd.Next(0,1000) ), "cook2"),
                new ThreadedHandler<OrderPlaced>(new Cook(pubsub,  rnd.Next(0,1000) ), "cook3"),
                new ThreadedHandler<OrderPlaced>(new Cook(pubsub,  rnd.Next(0,1000) ), "cook4"),
                new ThreadedHandler<OrderPlaced>(new Cook(pubsub,  rnd.Next(0,1000) ), "cook5"),

            };

            var kitchen = new ThreadedHandler<OrderPlaced>(new TimeToLiveHandler<OrderPlaced>(new MoreFairDispatcher<OrderPlaced>(cooks)), "disp");

            pubsub.Subscribe(kitchen);
            pubsub.Subscribe(assistant);
            pubsub.Subscribe(cashier);
            pubsub.Subscribe(orderHandler);

            kitchen.Start();
            assistant.Start();
            cashier.Start();

            monitorQueues = new List<IMonitor>();

            foreach (var threadCook in cooks)
            {
                monitorQueues.Add((IMonitor)threadCook);
                threadCook.Start();
            }


            monitorQueues.Add(kitchen);
            monitorQueues.Add(cashier);
            monitorQueues.Add(assistant);

            var waiter = new Waiter(pubsub);

            var thread = new Thread(Monitor);
            thread.Start();

            var items = new[]
            {
                new Item {Name = "Soup", Qty = 2, Price = 3.50m},
                new Item {Name = "Goulash", Qty = 2, Price = 3.50m}
            };

            for (int i = 0; i < 100; i++)
            {
                waiter.PlaceOrder(items);
            }

            Console.ReadLine();
        }

        public void Monitor()
        {
            while (true)
            {
                Thread.Sleep(1000);
                foreach (var startable in monitorQueues)
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

    public interface IPublisher
    {
        void Publish<T>(string topic, T message) where T : IMessage;
        void Publish<T>(T message) where T : IMessage;
    }

    public class Pubsub : IPublisher
    {
        private Dictionary<string, Multiplexer<IMessage>> mutiplexers = new Dictionary<string, Multiplexer<IMessage>>();

        public void Subscribe<T>(IHandler<T> handler) where T : IMessage
        {
            Multiplexer<IMessage> multi;
            if (mutiplexers.TryGetValue(typeof(T).FullName, out multi))
            {
                multi.AddHandler(new Widener<IMessage, T>(handler));
            }
            else
            {
                mutiplexers.Add(typeof(T).FullName,
                    new Multiplexer<IMessage>(new List<IHandler<IMessage>> { new Widener<IMessage, T>(handler) }));
            }
        }

        public void Publish<T>(string topic, T message) where T : IMessage
        {
            Multiplexer<IMessage> multi;
            if (mutiplexers.TryGetValue(typeof(T).FullName, out multi))
            {
                multi.Handle(message);
            }
            else
            {
                Console.WriteLine("no matching multiplexer." + topic);
            }
        }

        public void Publish<T>(T message) where T : IMessage
        {
            Publish(typeof(T).FullName, message);
        }

    }

    public class Widener<TIn, TOut> : IHandler<TIn>
        where TOut : TIn
        where TIn : IMessage
    {
        private readonly IHandler<TOut> _handler;

        public Widener(IHandler<TOut> handler)
        {
            _handler = handler;
        }

        public void Handle(TIn message)
        {
            TOut mess;
            try
            {
                mess = (TOut)message;
            }
            catch
            {
                return;
            }
            _handler.Handle(mess);
        }
    }


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

            if (foo != null && foo.LiveUntil > DateTime.Now)
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


    public class MoreFairDispatcher<T> : IHandler<T> where T : IMessage
    {
        private readonly List<ThreadedHandler<T>> _handlers;

        public MoreFairDispatcher(IEnumerable<ThreadedHandler<T>> handlers)
        {
            _handlers = new List<ThreadedHandler<T>>(handlers); ;
        }

        public void Handle(T message)
        {
            while (true)
            {

                foreach (var handle in _handlers)
                {
                    var queuecount = handle.GetQueueCount();
                    if (queuecount < 5)
                    {
                        handle.Handle(message);
                        return;
                    }
                }

                Thread.Sleep(1);
            }
        }
    }


    public class ThreadedHandler<T> : IStartable, IMonitor, IHandler<T> where T : IMessage
    {
        private readonly IHandler<T> _handler;
        private readonly string _name;
        private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();

        public ThreadedHandler(IHandler<T> handler, string name)
        {
            _handler = handler;
            _name = name;
        }

        public void Handle(T message)
        {
            _queue.Enqueue(message);
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
                T message;
                if (!_queue.TryDequeue(out message))
                {
                    Thread.Sleep(17);

                }
                else
                {
                    try
                    {
                        _handler.Handle(message);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Handler failed placing the order back to the queue" + e.Message);
                        _queue.Enqueue(message);
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

    public class RoundRobin<T> : IHandler<T> where T : IMessage
    {
        private readonly Queue<IHandler<T>> _roundRobinHandlers;

        public void Handle(T order)
        {
            var handle = _roundRobinHandlers.Dequeue();
            try
            {
                handle.Handle(order);
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

        public RoundRobin(IEnumerable<IHandler<T>> handlers)
        {
            _roundRobinHandlers = new Queue<IHandler<T>>(handlers);
        }
    }

    public class Multiplexer<T> : IHandler<T> where T : IMessage
    {
        private readonly List<IHandler<T>> _handlers;

        public Multiplexer(IEnumerable<IHandler<T>> handlers)
        {
            _handlers = handlers.ToList();
        }

        public void Handle(T order)
        {
            foreach (var handler in _handlers)
            {
                handler.Handle(order);
            }
        }

        public void AddHandler(IHandler<T> handler)
        {
            _handlers.Add(handler);
        }
    }

    public interface IHandler<T> where T : IMessage
    {
        void Handle(T message);
    }

    public class ConsolePrintingHandler<T> : IHandler<T> where T : IMessage
    {
        public void Handle(T message)
        {
            Console.WriteLine("order completed" + message.ToString());
            // Console.WriteLine(JsonConvert.SerializeObject(order));
        }
    }

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

            _publisher.Publish(new OrderPlaced()
            {
                Order = order,
                LiveUntil = DateTime.Now.AddSeconds(10)
            });

            return order.OrderId;
        }
    }

    public class Cook : IHandler<OrderPlaced>
    {
        private readonly IPublisher _publisher;

        private readonly int _cookTime;

        Dictionary<string, string[]> ingredients = new Dictionary<string, string[]>
        {
            {"Goulash", new[]{"meat", "onion"}},
            {"Soup", new[]{"tears", "elves"}},
            {"Pie", new[]{"beef", "broken dreams"}},
        };

        public Cook(IPublisher publisher, int cookTime)
        {
            _publisher = publisher;
            _cookTime = cookTime;
        }

        public void Handle(OrderPlaced message)
        {
            foreach (var item in (message).Order.Items.ToList())
                foreach (var ingredient in ingredients[item.Name].ToList())
                {
                    message.Order.AddIngredient(ingredient);
                }

            //    Console.WriteLine(JsonConvert.SerializeObject(order.Ingredients) + "added.");

            Thread.Sleep(_cookTime);

            _publisher.Publish( new OrderCooked() { Order = message.Order});
        }
    }

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

    public class Cashier : IHandler<OrderPriced>
    {
        private readonly IPublisher _publisher;

        private Dictionary<string, Order> _orders = new Dictionary<string, Order>();

        public Cashier(IPublisher publisher)
        {
            _publisher = publisher;
        }

        public void Handle(OrderPriced message)
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

    public class Item
    {
        public string Name { get; set; }
        public int Qty { get; set; }
        public decimal Price { get; set; }
    }
}
