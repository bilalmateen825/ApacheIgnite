using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Cache.Query;
using Apache.Ignite.Core.Cache.Query.Continuous;
using Apache.Ignite.Core.Communication.Tcp;
using Apache.Ignite.Core.Discovery.Tcp.Static;
using Apache.Ignite.Core.Discovery.Tcp;
using Apache.Ignite.Core.Messaging;
using System;
using Apache.Ignite.Core.Cache.Event;
using IgniteAPI;
using System.Reflection;

namespace IgniteServer
{
    class IgniteServer
    {
        private const string CacheName = "myCache";
        static void Main(string[] args)
        {
            var endpoints = new List<string>();
            int startPort = 47500;
            //int endPort = 47509;
            int endPort = 47501;

            for (int port = startPort; port <= endPort; port++)
            {
                string endpoint = $"127.0.0.1:{port}";
                endpoints.Add(endpoint);
            }
            string directoryPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/ssd/storage";
            IgniteConfiguration cfg = new IgniteConfiguration
            {
                DiscoverySpi = new TcpDiscoverySpi
                {
                    IpFinder = new TcpDiscoveryStaticIpFinder
                    {
                        //Endpoints = new[] { "127.0.0.1:47500..47509" }
                        Endpoints = endpoints.ToArray()
                        //Endpoints = new[] { "127.0.0.1:10800" }
                    }
                },

                CommunicationSpi = new TcpCommunicationSpi { IdleConnectionTimeout = TimeSpan.FromSeconds(5) },

                //https://ignite.apache.org/docs/latest/persistence/native-persistence
                DataStorageConfiguration = new Apache.Ignite.Core.Configuration.DataStorageConfiguration()
                {
                    StoragePath =Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)+ "\\ssd\\storage",
                    //StoragePath = "/ssd/storage",
                    DefaultDataRegionConfiguration = new Apache.Ignite.Core.Configuration.DataRegionConfiguration()
                    {
                        Name = "Default_Region",
                        PersistenceEnabled = true,
                    },
                    CheckpointFrequency = TimeSpan.FromMinutes(15),
                    WalMode = Apache.Ignite.Core.Configuration.WalMode.Fsync, //WalMode.LogOnly,
                    WalSegmentSize = 64 * 1024 * 1024,
                    MetricsEnabled = true
                }
            };

            using (var ignite = Ignition.Start(cfg))
            {
                ignite.GetCluster().SetActive(true); // Activate the cluster

                //ignite.GetCluster().DisableWal(CacheName);
                var cache = ignite.GetOrCreateCache<int, Person>(new CacheConfiguration
                {
                    Name = CacheName,
                    QueryEntities = new[] { new QueryEntity(typeof(int), typeof(Person)) }
                });

                var a = cache[1];
                var qry = new ContinuousQuery<int, Person>(new LocalListener(), new RemoteFilter());

                cache.QueryContinuous(qry);
                //ignite.GetCluster().EnableWal(CacheName);
                Console.WriteLine("Server started. Press any key to exit...");
                Console.ReadKey();
            }
        }

        public class LocalListener : ICacheEntryEventListener<int, Person>
        {
            private bool isUpdatingCache = false;

            public void OnEvent(IEnumerable<ICacheEntryEvent<int, Person>> evts)
            {
                foreach (var evt in evts)
                {
                    if (!isUpdatingCache)
                    {
                        Console.WriteLine($"Intercepted request from client: {evt.EventType} [Key={evt.Key}, Value={evt.Value}]");
                        var cache = Ignition.GetIgnite().GetOrCreateCache<int, Person>(CacheName);
                        Person p = evt.Value;

                        p.Name = "BBBBBBBBBBBBB";
                        isUpdatingCache = true;
                        cache.Put(evt.Key, p); // Update the cache entry with the modified value
                        isUpdatingCache = false;
                    }

                    // Perform some operations here before the data is added to the cache and database
                }
            }
        }

        public class RemoteFilter : ICacheEntryEventFilter<int, Person>
        {
            public bool Evaluate(ICacheEntryEvent<int, Person> evt)
            {
                return true;
            }
        }
    } 

    //// Data class for Person table
    //class Person
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }
    //    public int DepartmentId { get; set; }

    //    public Person(int id, string name, int departmentId)
    //    {
    //        Id = id;
    //        Name = name;
    //        DepartmentId = departmentId;
    //    }

    //    public override string ToString()
    //    {
    //        return $"Person [Id={Id}, Name={Name}, DepartmentId={DepartmentId}]";
    //    }
    //}

    //// Data class for Department table
    //class Department
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }

    //    public Department(int id, string name)
    //    {
    //        Id = id;
    //        Name = name;
    //    }

    //    public override string ToString()
    //    {
    //        return $"Department [Id={Id}, Name={Name}]";
    //    }
    //}
}
