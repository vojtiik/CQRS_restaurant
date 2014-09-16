namespace CQRS_restaurant.Handlers
{
    public interface IHandler<T> where T : IMessage
    {
        void Handle(T message);
    }
}