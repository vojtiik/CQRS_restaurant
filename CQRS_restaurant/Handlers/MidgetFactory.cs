using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CQRS_restaurant.Handlers
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

        }
    }

    public class MidgetFactory
    {
        private readonly IPublisher _publisher;

        public MidgetFactory(IPublisher publisher)
        {
            _publisher = publisher;
        }

        public IMidget GetMidget()
        {
            return new Midget(_publisher);
        }

    }

    public interface IMidget : IHandler<OrderPlaced>, IHandler<FoodCooked>, IHandler<OrderPriced>, IHandler<OrderPaid>
    {

    }

    public class Midget : IMidget
    {
        private readonly IPublisher _publisher;

        public Midget(IPublisher publisher)
        {
            _publisher = publisher;
        }


        public void Handle(OrderPlaced message)
        {
            if (message == null) return;

            _publisher.Publish(new CookFood()
            {
                CorrelationId = message.CorrelationId,
                CausationId = message.EventId,
                Order = message.Order
            });
        }


        public void Handle(FoodCooked message)
        {
            if (message == null) return;
            _publisher.Publish(new PriceOrder()
            {
                CorrelationId = message.CorrelationId,
                CausationId = message.EventId,
                Order = message.Order
            });
        }

        public void Handle(OrderPriced message)
        {
            if (message == null) return;
            _publisher.Publish(new TakePayment()
            {
                CorrelationId = message.CorrelationId,
                CausationId = message.EventId,
                Order = message.Order
            });
        }

        public void Handle(OrderPaid message)
        {
            if (message == null) return;
            _publisher.Publish(new PrintOrder()
            {
                CorrelationId = message.CorrelationId,
                CausationId = message.EventId,
                Order = message.Order
            });
        }


    }

}
