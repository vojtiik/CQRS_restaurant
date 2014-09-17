using System;

namespace CQRS_restaurant
{
    public class BaseEvent : IMessage, IWontWaitForEver
    {
        public BaseEvent()
        {
            EventId = Guid.NewGuid().ToString();
        }

        public string CorrelationId { get; set; }
        public string CausationId { get; set; }

        public string EventId { get; private set; }

        public DateTime LiveUntil { get; set; }
    }

    public class OrderPlaced : BaseEvent
    {
        public Order Order { get; set; }
    }

    public class OrderPriced : BaseEvent
    {
        public Order Order { get; set; }
    }

    public class FoodCooked : BaseEvent
    {
        public Order Order { get; set; }
    }

    public class OrderPaid : BaseEvent
    {
        public Order Order { get; set; }

    }

    public interface IMessage
    {

        string CorrelationId { get; set; }
        string CausationId { get; set; }
        string EventId { get; }
    }

    public class CookFood : BaseEvent
    {
        public Order Order { get; set; }
    }

    public class PlaceOrder : BaseEvent
    {
        public Order Order { get; set; }
    }

    public class TakePayment : BaseEvent
    {
        public Order Order { get; set; }
    }

    public class PriceOrder : BaseEvent
    {
        public Order Order { get; set; }
    }


    public class SpikeOrder : BaseEvent
    {
        public Order Order { get; set; }
    }

    public class SpikeIt : BaseEvent
    {
        public Order Order { get; set; }
    }

}

