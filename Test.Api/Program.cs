using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Test.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateHostBuilder(string[] args)
        {
            string env = null;
            return WebHost.CreateDefaultBuilder(args).ConfigureAppConfiguration((context, builder) =>
                {
                    builder.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json");
                    env = context.HostingEnvironment.EnvironmentName;
                })
                .UseKestrel(opts =>
                {
                    // Если дебаг - просто слушаем 5000
                    if (env == "Debug")
                    {
                        opts.ListenAnyIP(5000);
                    }
                    // Иначе 5001 и по https
                    else
                    {
                        opts.ListenLocalhost(5001, opt => opt.UseHttps());
                    }
                })
                .UseStartup<Startup>();
        }
    }
}