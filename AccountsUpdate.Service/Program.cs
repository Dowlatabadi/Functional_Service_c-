// See https://aka.ms/new-console-template for more information
using AccountsUpdate.Application.AccountUpdate.Commands;
using AccountsUpdate.Application.Services;
using AccountsUpdate.Application.Pipelines;
using MediatR;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using System.Collections.Specialized;
using AccountsUpdate.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
//using MassTransit;
using AccountsUpdate.Application.Consumer;
using AccountsUpdate.Infrastructure;
using System.Net;
using Microsoft.Extensions.Configuration;
//using static MassTransit.Logging.DiagnosticHeaders.Messaging;
ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

var builder = WebApplication.CreateBuilder(args);

var val = builder.Configuration.GetValue<string>("mykeys:key1");
Console.WriteLine(val);

builder.Services.AddSingleton(builder.Configuration);
var _config = builder.Configuration;

builder.Services.AddScoped<IUpdateService, UpdateService>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();


builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining(typeof(Program));
    cfg.RegisterServicesFromAssemblyContaining(typeof(UpdateService));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));

});

var elasticSearchUrl = builder.Configuration["ElasticSearch:Url"];
var elasticSearchUsername = builder.Configuration["ElasticSearch:UserName"];
var elasticSearchPassword = builder.Configuration["ElasticSearch:Password"];
var elasticSearchPasswordHash = builder.Configuration["ElasticSearch:PasswordHash"];

Log.Logger = new LoggerConfiguration()
           .MinimumLevel.Information()
           .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
           .Enrich.FromLogContext()
            .Enrich.WithCorrelationId()
           .WriteTo.Console()
           .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticSearchUrl))
           {
               ModifyConnectionSettings =
                      x => x.BasicAuthentication(elasticSearchUsername, elasticSearchPassword).GlobalHeaders(new NameValueCollection
                      {
                          {"Authorization", $"Bearer {elasticSearchPasswordHash}"},
                          {"Content-Type", "application/json"}
                      }).ServerCertificateValidationCallback(
                          (o, certificate, arg3, arg4) => { return true; }),
               IndexFormat = builder.Configuration["ElasticSearch:IndexFormat"]
           })
           .CreateLogger();

builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.ClearProviders();
    loggingBuilder.AddSerilog();
});

ILoggerFactory loggerFactory = new LoggerFactory()
            .AddSerilog(Log.Logger);

Microsoft.Extensions.Logging.ILogger logger = loggerFactory.CreateLogger<Program>();


builder.Services.AddSingleton(logger);

builder.Services.Configure<DBOptions>(_config.GetSection("DBOptions"));

builder.Services.AddDbContext<AccountsDbContext>(options =>
{
    if (_config.GetSection("DBOptions").GetValue<bool>("usesql"))
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    else
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQLConnection"));
    }
});

//builder.Services.AddMassTransit(cfg =>
//{
//    cfg.AddConsumer<Consumer>();

//    cfg.UsingRabbitMq((context, cfg) =>
//    {
//        //cfg.Host(new Uri(_config["emailrabbit:host"]), h =>
//        cfg.Host(new Uri("amqp://prod-rabbitmq-acc.mofid.dc:32001/Account"), h =>
//        {            
//            //h.Username(_config["emailrabbit:username"]);
//            h.Username("account-rabbit-mo");
//            //h.Password(_config["emailrabbit:password"]);
//            h.Password("4ZamKCspcvikG9");
//        });
//        cfg.ReceiveEndpoint("AccountChangeQueue_P", e =>
//        {
//        e.ConfigureConsumeTopology = false;
//            e.ClearSerialization();
//            e.UseJsonSerializer();
//            e.PublishFaults = false;
//            e.Durable = false;
//            e.AutoDelete = false;
//            e.SetQueueArgument("durable", "false");
//            e.ConfigureConsumer<Consumer>(context);
//        });
//    });
//});

builder.Services.AddSingleton<IConsumerService, RabbitMQConsumerService>();


var app = builder.Build();


var rabbit = app.Services.GetRequiredService<IConsumerService>();
rabbit.StartConsuming();
//var mediator = app.Services.GetRequiredService<MediatR.IMediator>();

//var t1=Task.Run(()=> mediator.Send(new UpdateAccountRequest("1=8e286045-6f7f-455c-b92f-b4ca6de65a90|2=638286636430410046|3=2|4=1|13=1360561481|16=11291360561481|17=1|11=09037891788|18=ChangeByAdminPanel")));
//var t2=Task.Run(()=> mediator.Send(new UpdateAccountRequest("1=8e286045-6f7f-455c-b92f-b4ca6de65a90|2=638286636430410046|3=2|4=1|13=1360561481|16=11291360561481|17=1|18=ChangeByAdminPanel")));
//await Task.WhenAll(t1,t2);
app.Run();
