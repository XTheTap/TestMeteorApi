using System.Globalization;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MeteorWorkerService;

public class Worker : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<Worker> _logger;
    private readonly MeteorSourceOptions _options;

    public Worker(IServiceProvider services, IHttpClientFactory httpClientFactory,
     ILogger<Worker> logger, IOptions<MeteorSourceOptions> options)
    {
        _services = services;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SyncService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await MeteoriteSynchronizeAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during synchronization.");
            }
        }

        await Task.Delay(TimeSpan.FromMinutes(_options.SyncIntervalMinutes), stoppingToken);
    }

    private async Task MeteoriteSynchronizeAsync(CancellationToken stoppingToken)
    {
        var client = _httpClientFactory.CreateClient();
        _logger.LogInformation("Starting meteorite data synchronization...");

        var data = await client.GetFromJsonAsync<List<MeteoriteDto>>(_options.Url, stoppingToken);

        if (data is null)
        {
            _logger.LogWarning("Empty NASA response.");
            return;
        }

        _logger.LogInformation("Get {Count} objects", data.Count);

        using var scope = _services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        foreach (var item in data)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            var entity = MapToEntity(item);

            if (string.IsNullOrWhiteSpace(item.Id)) continue;

            var entityExisting = await dbContext.Meteorites
                .AsTracking()
                .FirstOrDefaultAsync(x => x.ExternalId == entity.ExternalId, stoppingToken);

            if (entityExisting is null)
            {
                dbContext.Meteorites.Add(entity);
                continue;
            }

            dbContext.Entry(entityExisting).CurrentValues.SetValues(entity);
            entityExisting.UpdatedAt = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync(stoppingToken);
        _logger.LogInformation("Meteorite data synchronization completed.");
    }
    
    private Meteorite MapToEntity(MeteoriteDto dto)
    {
        double? ParseDouble(string? s) =>
            double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : null;

        return new Meteorite
        {
            ExternalId = dto.Id,
            Name = dto.Name,
            Nametype = dto.Nametype,
            Recclass = dto.Recclass,
            Mass = ParseDouble(dto.Mass),
            Fall = dto.Fall,
            Year = dto.Year,
            Reclat = ParseDouble(dto.Reclat),
            Reclong = ParseDouble(dto.Reclong),
            RawJson = System.Text.Json.JsonSerializer.Serialize(dto)
        };
    }
}
