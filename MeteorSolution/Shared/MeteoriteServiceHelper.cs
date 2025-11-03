using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Shared;

public class MeteoriteServiceHelper
{
    private readonly Action<string>? _log;

    public MeteoriteServiceHelper(Action<string>? log = null)
    {
        _log = log;
    }

    public bool ValidateDto(MeteoriteDto dto)
    {
        var context = new ValidationContext(dto);
        var results = new List<ValidationResult>();
        bool isValid = Validator.TryValidateObject(dto, context, results, validateAllProperties: true);

        if (!isValid && _log is not null)
        {
            _log($"Skipping invalid record ID {dto.Id ?? "NULL"}: " +
                 $"{string.Join("; ", results.Select(r => r.ErrorMessage))}");
        }

        return isValid;
    }

    public Meteorite MapToEntity(MeteoriteDto dto, MeteorRecclass? recclass)
    {
        double? ParseDouble(string? s) =>
            double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : null;
        
        DateTime? ToUtc(DateTime? dt) => dt.HasValue ? DateTime.SpecifyKind(dt.Value, DateTimeKind.Utc) : null;

        return new Meteorite
        {
            ExternalId = dto.Id,
            Name = dto.Name,
            Nametype = dto.Nametype,
            Recclass = recclass,
            Mass = ParseDouble(dto.Mass),
            Fall = dto.Fall,
            Year = ToUtc(dto.Year),
            Reclat = ParseDouble(dto.Reclat),
            Reclong = ParseDouble(dto.Reclong),
            RawJson = System.Text.Json.JsonSerializer.Serialize(dto.Geolocation)
        };
    }
}
