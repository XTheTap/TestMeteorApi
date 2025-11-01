using Shared;
using Microsoft.EntityFrameworkCore;
using MeteorWorkerService;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient();
builder.Services.AddHostedService<Worker>();
builder.Services.Configure<MeteorSourceOptions>(builder.Configuration.GetSection("WorkerSettings"));
builder.Services.AddSingleton(provider =>
{
    var logger = provider.GetRequiredService<ILogger<Worker>>();
    return new MeteoriteServiceHelper(msg => logger.LogWarning(msg));
});


builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync(); 
}

host.Run();
