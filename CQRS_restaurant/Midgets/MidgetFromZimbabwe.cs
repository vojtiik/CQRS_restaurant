﻿using System;

namespace CQRS_restaurant.Midgets
{
    public class MidgetFromZimbabwe : IMidget
    {
        private readonly IPublisher _publisher;
        private bool _cooked;
        private int _retryCount;

        public MidgetFromZimbabwe(IPublisher publisher)
        {
            _publisher = publisher;
        }


        public void Handle(OrderPlaced message)
        {
            if (message == null) return;

            _publisher.Publish(new PriceOrder()
            {
                CorrelationId = message.CorrelationId,
                CausationId = message.EventId,
                Order = message.Order
            });
        }


        public void Handle(FoodCooked message)
        {
            if (message == null) return;
            _publisher.Publish(new CQRS_restaurant.SpikeOrder()
            {
                CorrelationId = message.CorrelationId,
                CausationId = message.EventId,
                Order = message.Order
            });

            _publisher.UnSubscribe(message.CorrelationId);
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
            _publisher.Publish(new CookFood()
            {
                CorrelationId = message.CorrelationId,
                CausationId = message.EventId,
                Order = message.Order
            });
        }



        public void Handle(CookFoodTimedout message)
        {

            if (message == null) return;



            if (!_cooked)
            {
                if (_retryCount > 3)
                {
                    _publisher.UnSubscribe(message.CorrelationId);
                    Console.WriteLine("We giving up !!!");
                    return;
                }


                _publisher.Publish(new CookFood()
                {
                    CorrelationId = message.CorrelationId,
                    CausationId = message.EventId,
                    Order = message.Order
                });

                _publisher.Publish(new SendToMeInX(10)
                {
                    CorrelationId = message.CorrelationId,
                    CausationId = message.EventId,
                    Order = message.Order,
                    Message = new CookFoodTimedout()
                    {
                        CorrelationId = message.CorrelationId,
                        CausationId = message.EventId,
                        Order = message.Order,
                    }
                });

                _retryCount++;

                Console.WriteLine("recoooooking !!!");
            }
            else
            {
                //Floor let me introduce you to my friend message.

            }
        }
    }
}