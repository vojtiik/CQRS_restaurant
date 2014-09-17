using CQRS_restaurant.Handlers;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.Linq;

namespace CQRS_restaurant.Tests
{
    [TestFixture]
    public class Class1
    {

        [SetUp]
        public void SetUp()
        {
        }

        [TestCase]
        public void CanWriteToEventStore()
        {
            
            var queue = new QueuedHandler<IMessage>(null, "NAME");
            queue.Enqueue(new PlaceOrder());
        }

        [TestCase]
        public void ReadProperty()
        {
            var doc = JObject.Parse(@"{ orderId : '1', items : [{ name : 'pratryuwn', qty : 5, price: 123 }, { name : 'prawn', qty : 3, price: 1233 }] }");
            var wrapper = new Order(doc);
            wrapper.OrderId.Should().Be("1");

            var items = wrapper.Items.ToList();

            items[0].Name.Should().Be("pratryuwn");
            items[0].Qty.Should().Be(5);
            items[0].Price.Should().Be(123);
        }



        [TestCase]
        public void WriteProperty()
        {
            var doc = JObject.Parse(@"{ orderId : '1', items : [{ name : 'pratryuwn', qty : 5, price: 123 }, { name : 'prawn', qty : 3, price: 1233 }] }");
            var wrapper = new Order(doc);
            wrapper.OrderId = "566";

            ((string)doc.Property("orderId").Value).Should().Be("566");
        }

        [TestCase]
        public void WriteProperty_WhilstPreservingUnknownState()
        {
            var doc = JObject.Parse(@"{ aaa:123, orderId : '1', items : [{ name : 'pratryuwn', qty : 5, price: 123 }, { name : 'prawn', qty : 3, price: 1233 }] }");
            var wrapper = new Order(doc);
            wrapper.OrderId = "566";

            ((string)doc.Property("orderId").Value).Should().Be("566");
            ((int)doc.Property("aaa").Value).Should().Be(123);
        }


        [TestCase]
        public void Additem_ShouldAdd()
        {
            var doc = JObject.Parse(@"{ aaa:123, orderId : '1', items : [{ name : 'pratryuwn', qty : 5, price: 123 }, { name : 'prawn', qty : 3, price: 1233 }] }");
            var wrapper = new Order(doc);

            var item = new Item {Name = "vojtech", Price = 150000000, Qty = 6};

            wrapper.AddItem(item);

            var lastItem = wrapper.Items.Last();

            lastItem.Name.Should().Be(item.Name);
            lastItem.Qty.Should().Be(item.Qty);
            lastItem.Price.Should().Be(item.Price);
        }
    }
}
