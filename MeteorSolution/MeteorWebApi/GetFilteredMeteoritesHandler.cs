using MediatR;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

public record GetFilteredMeteoritesQuery(MeteoriteFilterDto Filter)
    : IRequest<IEnumerable<MeteoriteGroupDto>>;

public class GetFilteredMeteoritesHandler : IRequestHandler<GetFilteredMeteoritesQuery, IEnumerable<MeteoriteGroupDto>>
{
    private readonly AppDbContext _db;
    private readonly IMemoryCache _cache;

    public GetFilteredMeteoritesHandler(AppDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<IEnumerable<MeteoriteGroupDto>> Handle(GetFilteredMeteoritesQuery request, CancellationToken ct)
    {
        var key = $"meteorites:{JsonSerializer.Serialize(request.Filter)}";
        if (_cache.TryGetValue(key, out IEnumerable<MeteoriteGroupDto>? cached))
            return cached!;

        var f = request.Filter;
        var query = _db.Meteorites.AsNoTracking().AsQueryable();

        if (f.YearFrom.HasValue)
            query = query.Where(m => m.Year >= new DateTime(f.YearFrom.Value, 1, 1));

        if (f.YearTo.HasValue)
            query = query.Where(m => m.Year <= new DateTime(f.YearTo.Value, 12, 31));

        if (!string.IsNullOrWhiteSpace(f.Recclass))
            query = query.Where(m => m.Recclass!.ToLower() == f.Recclass.ToLower());

        if (!string.IsNullOrWhiteSpace(f.NameContains))
            query = query.Where(m => m.Name.ToLower().Contains(f.NameContains.ToLower()));

        var grouped = await query
            .Where(m => m.Year.HasValue)
            .GroupBy(m => m.Year!.Value.Year)
            .Select(g => new MeteoriteGroupDto
            {
                Year = g.Key,
                Count = g.Count(),
                TotalMass = g.Sum(x => x.Mass ?? 0)
            })
            .ToListAsync(ct);

        grouped = f.SortBy switch
        {
            "count" => f.Desc ? grouped.OrderByDescending(x => x.Count).ToList() : grouped.OrderBy(x => x.Count).ToList(),
            "mass" => f.Desc ? grouped.OrderByDescending(x => x.TotalMass).ToList() : grouped.OrderBy(x => x.TotalMass).ToList(),
            _ => f.Desc ? grouped.OrderByDescending(x => x.Year).ToList() : grouped.OrderBy(x => x.Year).ToList()
        };

        _cache.Set(key, grouped, TimeSpan.FromMinutes(1));

        return grouped;
    }
}