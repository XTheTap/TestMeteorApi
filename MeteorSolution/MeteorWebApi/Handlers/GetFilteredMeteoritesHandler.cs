using MediatR;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Caching.Memory;

public record GetFilteredMeteoritesQuery(MeteoriteFilterDto Filter)
    : IStreamRequest<MeteoriteGroupDto>;

public class GetFilteredMeteoritesHandler : IStreamRequestHandler<GetFilteredMeteoritesQuery, MeteoriteGroupDto>
{
    private readonly AppDbContext _db;
    private readonly IMemoryCache _cache;

    public GetFilteredMeteoritesHandler(AppDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async IAsyncEnumerable<MeteoriteGroupDto> Handle(GetFilteredMeteoritesQuery request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var key = $"meteorites:{JsonSerializer.Serialize(request.Filter)}";
        if (_cache.TryGetValue(key, out IEnumerable<MeteoriteGroupDto>? cached))
        {
            foreach (var meteor in cached!)
            {
                yield return meteor;
            }
            yield break;
        }

        var filter = request.Filter;
        var query = _db.Meteorites.AsNoTracking().AsQueryable();

        if (filter.YearFrom.HasValue)
            query = query.Where(m => m.Year >= new DateTime(filter.YearFrom.Value, 1, 1));

        if (filter.YearTo.HasValue)
            query = query.Where(m => m.Year <= new DateTime(filter.YearTo.Value, 12, 31));

        if (filter.Recclass is not null)
            query = query.Where(m => m.Recclass!.Id == filter.Recclass);

        if (!string.IsNullOrWhiteSpace(filter.NameContains))
            query = query.Where(m => m.Name.ToLower().Contains(filter.NameContains.ToLower()));

        var groups = new Dictionary<int, MeteoriteGroupDto>();

        await foreach (var m in query.AsAsyncEnumerable().WithCancellation(cancellationToken))
        {
            if (!m.Year.HasValue) continue;
            var year = m.Year.Value.Year;

            if (!groups.TryGetValue(year, out var dto))
            {
                dto = new MeteoriteGroupDto { Year = year };
                groups[year] = dto;
            }

            dto.Count++;
            dto.TotalMass += m.Mass ?? 0;
        }

        var result = groups.Values.AsEnumerable();

        result = request.Filter.SortBy switch
        {
            "count" => request.Filter.Desc ? result.OrderByDescending(x => x.Count) : result.OrderBy(x => x.Count),
            "mass" => request.Filter.Desc ? result.OrderByDescending(x => x.TotalMass) : result.OrderBy(x => x.TotalMass),
            _ => request.Filter.Desc ? result.OrderByDescending(x => x.Year) : result.OrderBy(x => x.Year)
        };

        var list = result.ToList();
        _cache.Set(key, list, TimeSpan.FromMinutes(30));

        foreach (var dto in list)
        {
            yield return dto;
        }
    }
}