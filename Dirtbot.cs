//using DirtBot.Database;
//using DirtBot.Services;
//using DirtBot.Translation;
//using Discord;
//using Discord.Commands;
//using Discord.WebSocket;
//using Microsoft.Data.Sqlite;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using StackExchange.Redis;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Net.Http;
//using System.Reflection;
//using System.Threading.Tasks;
//using Color = System.Drawing.Color;

//namespace DirtBot
//{
//    // Named like this to solve naming problems
//    public class Dirtbot
//    {
//        //public static DiscordSocketClient Client { get; private set; }
//        //public static ConnectionMultiplexer Redis { get; private set; }
//        //public static IServiceProvider Services { get; private set; }
//        //private static IConfiguration Configuration { get; set; }

//        public Dirtbot(IConfiguration config)
//        {
//            //Configuration = config;
//        }

//        public async Task StartAsync()
//        {
//            using (var s = ConfigureServices())
//            {
//                //Services = s;
//                //Client = Services.GetRequiredService<DiscordSocketClient>();

//                //// Log used for different operations in this method
//                //Logger log;

//                //log = Logger.GetLogger<TranslationManager>();
//                //log.Info("Loading translations");
//                //await TranslationManager.LoadTranslations();
//                //log.Info("Loaded all translations");

//                //// Login
//                //log = Logger.GetLogger(this);
//                //try
//                //{
//                //    log.Info("Logging onto Discord");
//                //    await Client.LoginAsync(TokenType.Bot, Configuration["Token"]);
//                //}
//                //catch (Discord.Net.HttpException)
//                //{
//                //    // Token is invalid
//                //    log.Critical("Invalid token. Terminating");
//                //    Environment.Exit(-1);
//                //}
//                //catch (Exception e)
//                //{
//                //    log.Critical($"Failed to connect to Discord ({e.Message})", e);
//                //    Environment.Exit(-1);
//                //}

//                //// Start the client
//                //await Client.StartAsync();

//                // Making sure we won't exit the application so that the bot can stay online
//                await Task.Delay(-1);
//            }
//        }

//        private ServiceProvider ConfigureServices()
//        {
//            var services = new ServiceCollection();
//                //// Discord.Net stuff
//                //.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
//                //{
//                //    ExclusiveBulkDelete = true
//                //}))
//                //.AddSingleton(new CommandService(new CommandServiceConfig
//                //{
//                //    DefaultRunMode = RunMode.Async,

//                //}))
//                //.AddSingleton<HttpClient>()
//                //// Services
//                //.AddSingleton<CommandHandlerService>()
//                //.AddSingleton<PrefixManagerService>()
//                //.AddSingleton<CategoryManagerService>()
//                //.AddSingleton<HelpProviderService>()
//                //// Database and Redis
//                //.AddDbContextPool<DatabaseContext>(options => options.UseSqlite(Configuration.GetConnectionString("Sqlite")))
//                //.AddSingleton(ConnectRedis(Configuration.GetConnectionString("Redis")));

//            //// Easy to add new services
//            //foreach (var kvp in ServiceTypes)
//            //    services.AddSingleton(kvp.Key);
//            return services.BuildServiceProvider();
//        }

//        private ConnectionMultiplexer ConnectRedis(string redisUrl)
//        {
//            return null;
//            //if (Redis != null)
//            //    return Redis;

//            //var log = Logger.GetLogger("Redis");
//            //if (String.IsNullOrEmpty(redisUrl.TrimEnd()))
//            //{
//            //    log.Critical("Redis connection string not provided. Exiting");
//            //    Environment.Exit(-1);
//            //}

//            //log.Info("Connecting to Redis...");
//            //try
//            //{
//            //    Redis = ConnectionMultiplexer.Connect(redisUrl);
//            //}
//            //catch (RedisConnectionException e)
//            //{
//            //    log.Critical("Failed to connect to Redis. Check the Redis address and that the server is online and try again.", e);
//            //    Environment.Exit(-1);
//            //}
//            //log.Info("Successfully connected to Redis.");
//            //return Redis;
//        }
//    }
//}