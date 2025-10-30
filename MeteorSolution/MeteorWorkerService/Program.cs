using Microsoft.EntityFrameworkCore;
using MeteorWorkerService;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient();
builder.Services.AddHostedService<Worker>();
builder.Services.Configure<MeteorSourceOptions>(builder.Configuration.GetSection("WorkerSettings"));

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

host.Run();
