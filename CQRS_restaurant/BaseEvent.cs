using System;

namespace CQRS_restaurant
{
    public class BaseEvent : IMessage, IWontWaitForEver
    {
        public BaseEvent()
        {
            EventId = Guid.NewGuid().ToString();
        }

        public string EventId { get; private set; }

        public DateTime LiveUntil
        { get; set; }
    }

    public class OrderPlaced : BaseEvent
    {
        public Order Order { get; set; }
    }

    public class OrderPriced : BaseEvent
    {
        public Order Order { get; set; }
    }

    public class OrderCooked : BaseEvent
    {
        public Order Order { get; set; }
    }

    public class OrderPaid : BaseEvent
    {
        public Order Order { get; set; }
    }

    public interface IMessage
    {

    }
}
