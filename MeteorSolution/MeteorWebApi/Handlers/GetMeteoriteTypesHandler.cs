using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

public record GetMeteoriteTypesQuery : IRequest<IEnumerable<MeteorRecclassDto>>;

public class GetMeteoriteTypesHandler : IRequestHandler<GetMeteoriteTypesQuery, IEnumerable<MeteorRecclassDto>>
{
    private readonly AppDbContext _db;
    private readonly IMemoryCache _cache;

    public GetMeteoriteTypesHandler(AppDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<IEnumerable<MeteorRecclassDto>> Handle(GetMeteoriteTypesQuery request, CancellationToken cancellationToken)
    {
        var key = "meteoriteTypes";
        if (_cache.TryGetValue(key, out IEnumerable<MeteorRecclassDto>? cached))
        {
            return cached!;
        }

        var recclassList = await _db.MeteorRecclasses
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => new MeteorRecclassDto { Id = r.Id, Name = r.Name })
            .ToListAsync(cancellationToken);

        _cache.Set(key, recclassList, TimeSpan.FromMinutes(30));

        return recclassList;
    }
}
