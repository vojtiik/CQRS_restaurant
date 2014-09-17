using System.Collections.Generic;
using CQRS_restaurant.Handlers;

namespace CQRS_restaurant.Midgets
{
    public class MidgetHouse : IHandler<IMessage>
    {
        private readonly IPublisher _publisher;
        readonly Dictionary<string, IMidget> _midgets = new Dictionary<string, IMidget>();
        private readonly MidgetFactory factory;

        public MidgetHouse(IPublisher publisher)
        {
            _publisher = publisher;
            factory = new MidgetFactory(_publisher);
        }

        public void Handle(IMessage message)
        {
            IMidget midget;
            if (message is OrderPlaced)
            {
                midget = factory.GetMidget();
                _midgets.Add(message.CorrelationId, midget);
            }
            else
            {
                midget = _midgets[message.CorrelationId];
            }

            midget.Handle(message as OrderPlaced);
            midget.Handle(message as FoodCooked);
            midget.Handle(message as OrderPriced);
            midget.Handle(message as OrderPaid);
            midget.Handle(message as CookFoodTimedout);
          
        }
    }
}