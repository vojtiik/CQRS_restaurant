using System;
using System.CodeDom;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;
using CQRS_restaurant.Actors;
using CQRS_restaurant.Handlers;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CQRS_restaurant
{
    public class WireUp
    {
        List<QueuedHandler<CookFood>> cooks;
        private List<IMonitor> monitorQueues;

        public void Start()
        {
            var pubsub = new TopicBasedPubsub();

            var orderHandler = new ConsolePrintingHandler();
            var cashier = new QueuedHandler<TakePayment>(new Cashier(pubsub), "cashier");
            var assistant = new QueuedHandler<PriceOrder>(new Assistantmanager(pubsub), "assistant");

            var rnd = new Random();
            cooks = new List<QueuedHandler<CookFood>>()
            {
                new QueuedHandler<CookFood>(new Cook(pubsub,  rnd.Next(0,1000) ), "cook1"),
                new QueuedHandler<CookFood>(new Cook(pubsub,  rnd.Next(0,1000) ), "cook2"),
                new QueuedHandler<CookFood>(new Cook(pubsub,  rnd.Next(0,1000) ), "cook3"),
                new QueuedHandler<CookFood>(new Cook(pubsub,  rnd.Next(0,1000) ), "cook4"),
                new QueuedHandler<CookFood>(new Cook(pubsub,  rnd.Next(0,1000) ), "cook5"),

            };

            var kitchen = new QueuedHandler<CookFood>(
                new TimeToLiveHandler<CookFood>(
                    new MoreFairDispatcherHandler<CookFood>(cooks)
                    ), "kitchen");

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
                monitorQueues.Add(threadCook);
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

            for (int i = 0; i < 1000; i++)
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


    public class Item
    {
        public string Name { get; set; }
        public int Qty { get; set; }
        public decimal Price { get; set; }
    }
}
