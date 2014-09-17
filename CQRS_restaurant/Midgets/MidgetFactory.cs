namespace CQRS_restaurant.Midgets
{
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
}
