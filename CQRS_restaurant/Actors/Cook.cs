using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CQRS_restaurant.Handlers;

namespace CQRS_restaurant.Actors
{
    public class Cook : IHandler<CookFood>
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

        public void Handle(CookFood message)
        {
            foreach (var item in (message).Order.Items.ToList())
                foreach (var ingredient in ingredients[item.Name].ToList())
                {
                    message.Order.AddIngredient(ingredient);
                }

            //    Console.WriteLine(JsonConvert.SerializeObject(order.Ingredients) + "added.");

            Thread.Sleep(_cookTime);

            _publisher.Publish(new FoodCooked()
            {
                Order = message.Order,
                CorrelationId = message.CorrelationId,
                CausationId = message.EventId
            });
        }
    }
}