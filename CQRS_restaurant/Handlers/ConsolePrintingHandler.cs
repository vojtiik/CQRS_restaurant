using System;

namespace CQRS_restaurant.Handlers
{
    public class ConsolePrintingHandler :  IHandler<CookFood>, IHandler<PlaceOrder>, IHandler<TakePayment> 
    {
        
       public void Handle(CookFood message)
        {
            Console.WriteLine(string.Format("Id: {0} Corr: {1} Cause: {2}", message.EventId, message.CorrelationId, message.CausationId));
        }

        public void Handle(PriceOrder message)
        {
            Console.WriteLine(string.Format("Id: {0} Corr: {1} Cause: {2}", message.EventId, message.CorrelationId, message.CausationId));
        
        }

        public void Handle(TakePayment message)
        {
            Console.WriteLine(string.Format("Id: {0} Corr: {1} Cause: {2}", message.EventId, message.CorrelationId, message.CausationId));
        
        }

        public void Handle(PlaceOrder message)
        {
            Console.WriteLine(string.Format("Id: {0} Corr: {1} Cause: {2}", message.EventId, message.CorrelationId, message.CausationId));
      
        }
    }
}