namespace CQRS_restaurant
{
    public class BaseEvent : IMessage
    {
        public string EventId { get; set; }
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

    public interface IMessage
    {
    }
}
