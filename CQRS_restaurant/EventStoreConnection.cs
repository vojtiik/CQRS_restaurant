using System.Net;
using EventStore.ClientAPI;

namespace CQRS_restaurant
{
    public class EventStoreConnection
    {

        public static IEventStoreConnection _connection;

        static EventStoreConnection()
        {
            _connection = EventStore.ClientAPI.EventStoreConnection.Create(new IPEndPoint(IPAddress.Loopback, 1113));
            _connection.ConnectAsync().Wait();
        }
    }
}
