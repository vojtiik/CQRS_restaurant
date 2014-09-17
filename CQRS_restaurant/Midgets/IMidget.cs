using CQRS_restaurant.Handlers;

namespace CQRS_restaurant.Midgets
{
    public interface IMidget : IHandler<OrderPlaced>, IHandler<FoodCooked>, IHandler<OrderPriced>, IHandler<OrderPaid>, IHandler<CookFoodTimedout>
    {

    }
}