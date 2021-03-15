using System.Text.Json.Serialization;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Test.Bl.MessageHandlers;
using Test.Bl.RequestHandlers;
using Test.Data;

namespace Test.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<TestDbContext>(options =>
            {
                options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddControllers();
            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo {Title = "Test.Api", Version = "v1"}); });

            services.AddScoped<ITasksRequestsHandler, TasksRequestsHandler>();
            services.AddScoped<TaskCreatedMessageHandler>();
            services.AddScoped<TaskStartedMessageHandler>();
            
            services.AddMassTransit(x =>
            {
                x.AddRabbitMqMessageScheduler();
                x.AddConsumer<TaskCreatedMessageHandler>();
                x.AddConsumer<TaskStartedMessageHandler>();

                x.UsingRabbitMq((context, cfg) => 
                {
                    cfg.Host("localhost", "/", h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });

                    cfg.UseDelayedExchangeMessageScheduler();

                    cfg.ReceiveEndpoint("task-created", e =>
                    {
                        e.ConfigureConsumer<TaskCreatedMessageHandler>(context);
                    });
                    cfg.ReceiveEndpoint("task-started", e =>
                    {
                        e.ConfigureConsumer<TaskStartedMessageHandler>(context);
                    });
                });
            });

            services.AddMassTransitHostedService();
            
            services.AddMvc()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsEnvironment("Debug"))
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Test.Api v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

            // При запуске сразу накатываем миграции на бд
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>()?.CreateScope())
            {
                var context = serviceScope?.ServiceProvider.GetRequiredService<TestDbContext>();
                context?.Database.Migrate();
            }
        }
    }
}