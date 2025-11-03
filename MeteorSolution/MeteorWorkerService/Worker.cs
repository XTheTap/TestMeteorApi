using Shared;
using System.Text.Json;
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
            _logger.LogInformation("Starting meteorite data synchronization... \n Using URL: {Url}", _options.Url);

            using var response = await client.GetAsync(_options.Url, HttpCompletionOption.ResponseHeadersRead, stoppingToken);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(stoppingToken);
            await ProcessStreamAsync(stream, stoppingToken);
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

    private async Task ProcessStreamAsync(Stream jsonStream, CancellationToken stoppingToken)
    {
        using var scope = _services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var recclassCache = await dbContext.MeteorRecclasses
            .ToDictionaryAsync(x => x.Name, StringComparer.OrdinalIgnoreCase, stoppingToken);

        var buffer = new List<Meteorite>(capacity: 500);
        var incomingIds = new HashSet<string>();

        var existingIds = await dbContext.Meteorites
            .Select(x => x.ExternalId)
            .ToHashSetAsync(stoppingToken);

        await foreach (var dto in JsonSerializer.DeserializeAsyncEnumerable<MeteoriteDto>(
            jsonStream,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
            stoppingToken))
        {
            if (dto is null || !_helper.ValidateDto(dto)) continue;

            MeteorRecclass? recclass = null;
            if (!string.IsNullOrWhiteSpace(dto.Recclass))
            {
                var name = dto.Recclass.Trim();

                if (!recclassCache.TryGetValue(name, out recclass))
                {
                    recclass = new MeteorRecclass { Name = name };
                    dbContext.MeteorRecclasses.Add(recclass);
                    recclassCache[name] = recclass;
                }
            }

            var entity = _helper.MapToEntity(dto, recclass);
            buffer.Add(entity);
            incomingIds.Add(entity.ExternalId);

            if (buffer.Count >= 500)
            {
                await UpsertBatchAsync(dbContext, buffer, existingIds, stoppingToken);
                buffer.Clear();
            }
        }

        if (buffer.Count > 0)
            await UpsertBatchAsync(dbContext, buffer, existingIds, stoppingToken);

        var toDelete = await dbContext.Meteorites
            .Where(m => !incomingIds.Contains(m.ExternalId))
            .ToListAsync(stoppingToken);

        _logger.LogInformation("Meteorite data synchronization completed.");
    }

    private static async Task UpsertBatchAsync(AppDbContext dbContext, List<Meteorite> batch, HashSet<string> existingIds, CancellationToken token)
    {
        foreach (var entity in batch)
        {
            if (existingIds.Contains(entity.ExternalId))
            {
                dbContext.Meteorites.Update(entity);
            }
            else
            {
                dbContext.Meteorites.Add(entity);
                existingIds.Add(entity.ExternalId);
            }
        }

        await dbContext.SaveChangesAsync(token);
    }
}