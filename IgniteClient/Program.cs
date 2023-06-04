using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Client.Cache;
using Apache.Ignite.Core.Client;
using Apache.Ignite.Core.Messaging;
using System;
using System.Text;
using Apache.Ignite.Core.Discovery.Tcp.Static;
using Apache.Ignite.Core.Discovery.Tcp;
using IgniteAPI;

namespace IgniteClient
{
    class IgniteClient
    {
        private const string CacheName = "myCache";
        static void Main(string[] args)
        {
            // Start Ignite client node
            //var cfg = new IgniteClientConfiguration { Endpoints = new[] { "127.0.0.1:47500..47501" } };

            Thread.Sleep(10000); // Waiting for Server to Up
            //try
            //{
               //    using (var client = Ignition.StartClient(cfg))
               //{
               //    var cache = client.GetOrCreateCache<int, Person>(new CacheClientConfiguration
               //    {
               //        Name = CacheName,
               //        QueryEntities = new[] { new QueryEntity(typeof(int), typeof(Person)) }
               //    });

            //    ////Read
            //    // var a = cache[1];

            //    // Create
            //    cache[0] = new Person { Name = "Person 1", Age = 20 };
            //    cache[2] = new Person { Name = "Person 2", Age = 22 };

            //    Console.WriteLine("Data added to cache. Press any key to exit...");
            //    Console.ReadKey();
            //}
            //}
            //catch (IgniteClientException ex)
            //{
            //    Console.WriteLine($"Client connection failed: {ex.InnerException?.Message}");
            //}

            //foreach (IClientConnection connection in client.GetConnections())
            //{
            //    Console.WriteLine(connection.RemoteEndPoint);
            //}

            var client = Ignition.Start(new IgniteConfiguration
            {
                DiscoverySpi = new TcpDiscoverySpi
                {
                    IpFinder = new TcpDiscoveryStaticIpFinder
                    {
                        Endpoints = new[] { "127.0.0.1:47500..47501" }
                    },
                    SocketTimeout = TimeSpan.FromSeconds(0.3)
                },
                ClientMode = true // Setting client mode
            });

            var stName = client.GetCacheNames();

            var cache = client.GetOrCreateCache<int, Person>(new CacheConfiguration
            {
                Name = CacheName,
                //QueryEntities = new[] { new QueryEntity(typeof(int), typeof(Person)) }
            });

            stName = client.GetCacheNames();
            cache.Put(3, new Person(1, "Person 99", 4));

            Console.Read();
        }
    }
    //public class Person
    //{
    //    [QuerySqlField]
    //    public string Name { get; set; }

    //    [QuerySqlField]
    //    public int Age { get; set; }
    //}
}
