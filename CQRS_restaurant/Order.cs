using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace CQRS_restaurant
{
    public class Order
    {
        private readonly JObject _doc;


        public Order(JObject doc)
        {
            _doc = doc;
        }

        public Order()
        {
            _doc = JObject.Parse("{items:[], ingredients:[]}");
        }

        private void Set(string name, string value)
        {
            var existing = _doc.Property(name);
            if (existing != null)
            {
                existing.Value = value;
            }
            _doc.Add(name, value);
        }

        private T Get<T>(string name)
        {
            var existing = _doc.Property(name);
            if (existing == null)
            {
                return default(T);
            }

            return existing.ToObject<T>();
        }

        private void Set(string name, int value)
        {
            var existing = _doc.Property(name);
            if (existing != null)
            {
                existing.Value = value;
            }
            _doc.Add(name, value);
        }

        private void Set(string name, bool value)
        {
            var existing = _doc.Property(name);
            if (existing != null)
            {
                existing.Value = value;
            }
            _doc.Add(name, value);
        }

        private void Set(string name, decimal value)
        {
            var existing = _doc.Property(name);
            if (existing != null)
            {
                existing.Value = value;
            }
            _doc.Add(name, value);
        }

        public string OrderId
        {
            get { return Get<string>("orderId"); }
            set { Set("orderId", value); }
        }

        public int TableNumber
        {
            get { return Get<int>("tableNumber"); }
            set { Set("tableNumber", value); }
        }
        public string ServerName
        {
            get { return Get<string>("serverName"); }
            set { Set("serverName", value); }
        }
        public string Timestamp
        {
            get { return Get<string>("timestamp"); }
            set { Set("timestamp", value); }
        }
        public string TimeToCook
        {
            get { return Get<string>("timeToCook"); }
            set { Set("timeToCook", value); }
        }
        public decimal SubTotal
        {
            get { return Get<decimal>("subTotal"); }
            set { Set("subTotal", value); }
        }
        public decimal Total
        {
            get { return Get<decimal>("total"); }
            set { Set("total", value); }
        }
        public decimal Tax
        {
            get { return Get<decimal>("tax"); }
            set { Set("tax", value); }
        }

        public bool Paid
        {
            get { return Get<bool>("paid"); }
            set { Set("paid", value); }
        }

        public IEnumerable<string> Ingredients
        {
            get
            {
                foreach (var ingredient in _doc.Value<JArray>("ingredients"))
                {
                    yield return ingredient.Value<string>();
                }
            }


        }

        public IEnumerable<Item> Items
        {
            get
            {
                foreach (var items in _doc.Value<JArray>("items"))
                {
                    var i = (JObject)items;
                    yield return new Item
                    {
                        Name = (string)i.Property("name").Value,
                        Qty = (int)i.Property("qty").Value,
                        Price = (decimal)i.Property("price").Value
                    };
                }
            }
        }

        public void AddItem(Item item)
        {
            var newItem = new JObject();
            newItem["name"] = item.Name;
            newItem["price"] = item.Price;
            newItem["qty"] = item.Qty;

            ((JArray)_doc.Property("items").Value).Add(newItem);
        }

        public void AddIngredient(string ingridient)
        {
            ((JArray)_doc.Property("ingredients").Value).Add(ingridient);
        }

    }
}