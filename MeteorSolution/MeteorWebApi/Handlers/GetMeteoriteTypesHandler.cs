using MediatR;
using Microsoft.EntityFrameworkCore;

public record GetMeteoriteTypesQuery : IRequest<IEnumerable<MeteorRecclassDto>>;

public class GetMeteoriteTypesHandler : IRequestHandler<GetMeteoriteTypesQuery, IEnumerable<MeteorRecclassDto>>
{
    private readonly AppDbContext _db;

    public GetMeteoriteTypesHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<MeteorRecclassDto>> Handle(GetMeteoriteTypesQuery request, CancellationToken cancellationToken)
    {
        return await _db.MeteorRecclasses
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => new MeteorRecclassDto { Id = r.Id, Name = r.Name })
            .ToListAsync(cancellationToken);
    }
}
