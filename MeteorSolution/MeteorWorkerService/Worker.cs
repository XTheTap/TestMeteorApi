using Shared;
using System.Text.Json;
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
    private readonly MeteoriteServiceHelper _helper;

    public Worker(IServiceProvider services, IHttpClientFactory httpClientFactory,
     ILogger<Worker> logger, IOptions<MeteorSourceOptions> options,
     MeteoriteServiceHelper meteoriteServiceHelper)
    {
        _services = services;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _options = options.Value;
        _helper = meteoriteServiceHelper;
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

            await Task.Delay(TimeSpan.FromMinutes(_options.SyncIntervalMinutes), stoppingToken);
        }
    }

    private async Task MeteoriteSynchronizeAsync(CancellationToken stoppingToken)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            _logger.LogInformation("Starting meteorite data synchronization...");

            var data = await client.GetFromJsonAsync<List<MeteoriteDto>>(_options.Url, stoppingToken);

            if (data is null || data.Count == 0)
            {
                _logger.LogWarning("Empty NASA response.");
                return;
            }

            _logger.LogInformation("Get {Count} objects", data.Count);

            await ProcessDataAsync(data, stoppingToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network or HTTP error while fetching NASA data.");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error parsing NASA JSON payload.");
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database update failed during synchronization.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected synchronization error.");
        }
    }
    
    private async Task ProcessDataAsync(IEnumerable<MeteoriteDto> data, CancellationToken stoppingToken)
    {
        using var scope = _services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var existing = await dbContext.Meteorites.AsNoTracking().ToListAsync(stoppingToken);
        var existingDict = existing.ToDictionary(x => x.ExternalId, x => x);

        var incomingIds = data.Select(d => d.Id).ToHashSet();

        foreach (var item in data)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                return;
            }

            if (_helper.ValidateDto(item))
            {
                continue;
            }

            var entity = _helper.MapToEntity(item);

            if (!existingDict.TryGetValue(item.Id, out var entitys))
            {
                dbContext.Meteorites.Add(entity);
                continue;
            }

            dbContext.Entry(entitys).CurrentValues.SetValues(entity);
            entitys.UpdatedAt = DateTime.UtcNow;
        }
        
        var toDelete = existing
        .Where(e => !incomingIds
        .Contains(e.ExternalId))
        .ToList();

        if (toDelete.Any())
        {
            dbContext.Meteorites.RemoveRange(toDelete);
            _logger.LogInformation("Removed {Count} old values", toDelete.Count);
        }

        await dbContext.SaveChangesAsync(stoppingToken);
        _logger.LogInformation("Meteorite data synchronization completed.");
    }
}