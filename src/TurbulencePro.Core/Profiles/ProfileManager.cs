using System.Text.Json;

namespace TurbulencePro.Core.Profiles;

public sealed class ProfileManager
{
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true, PropertyNameCaseInsensitive = true };
    private readonly Dictionary<string, AircraftProfile> _profiles = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<AircraftProfile> All => _profiles.Values.ToArray();

    public void Add(AircraftProfile profile) => _profiles[profile.Id] = profile;

    public AircraftProfile Resolve(string? title)
    {
        if (!string.IsNullOrWhiteSpace(title))
        {
            var hit = _profiles.Values.FirstOrDefault(p =>
                !string.IsNullOrWhiteSpace(p.MatchTitleContains) &&
                title.Contains(p.MatchTitleContains, StringComparison.OrdinalIgnoreCase));
            if (hit is not null)
                return hit;
        }
        return _profiles.Values.FirstOrDefault(p => p.Id == "generic-airliner")
            ?? _profiles.Values.FirstOrDefault()
            ?? new AircraftProfile();
    }

    public void SaveAll(string directory)
    {
        Directory.CreateDirectory(directory);
        foreach (var profile in _profiles.Values)
        {
            var path = Path.Combine(directory, profile.Id + ".json");
            File.WriteAllText(path, JsonSerializer.Serialize(profile, Json));
        }
    }

    public void LoadDirectory(string directory)
    {
        if (!Directory.Exists(directory))
            return;
        foreach (var file in Directory.EnumerateFiles(directory, "*.json"))
        {
            try
            {
                var profile = JsonSerializer.Deserialize<AircraftProfile>(File.ReadAllText(file), Json);
                if (profile is not null && !string.IsNullOrWhiteSpace(profile.Id))
                    _profiles[profile.Id] = profile;
            }
            catch (JsonException)
            {
                // Corrupt profile is skipped. The caller can inspect the file.
            }
        }
    }

    public static ProfileManager CreateWithStarters()
    {
        var manager = new ProfileManager();
        manager.Add(new AircraftProfile { Id = "fenix-a320", Aircraft = "Fenix A320", MatchTitleContains = "A320", ReferenceMassKg = 65000, VerticalResponse = 1.0, LateralResponse = 0.8, LongitudinalResponse = 0.7, PitchResponse = 0.8, RollResponse = 0.75, YawResponse = 0.5, StructuralVibration = 1.0 });
        manager.Add(new AircraftProfile { Id = "pmdg-737", Aircraft = "PMDG 737", MatchTitleContains = "737", ReferenceMassKg = 65000, VerticalResponse = 1.05, LateralResponse = 0.85, LongitudinalResponse = 0.72, PitchResponse = 0.82, RollResponse = 0.8, YawResponse = 0.52, StructuralVibration = 1.05 });
        manager.Add(new AircraftProfile { Id = "pmdg-777", Aircraft = "PMDG 777", MatchTitleContains = "777", ReferenceMassKg = 220000, VerticalResponse = 0.62, LateralResponse = 0.5, LongitudinalResponse = 0.48, PitchResponse = 0.5, RollResponse = 0.42, YawResponse = 0.35, StructuralVibration = 0.7 });
        manager.Add(new AircraftProfile { Id = "asobo-787", Aircraft = "Asobo 787", MatchTitleContains = "787", ReferenceMassKg = 180000, VerticalResponse = 0.7, LateralResponse = 0.55, LongitudinalResponse = 0.5, PitchResponse = 0.55, RollResponse = 0.48, YawResponse = 0.38, StructuralVibration = 0.75 });
        manager.Add(new AircraftProfile { Id = "generic-airliner", Aircraft = "Generic Airliner", MatchTitleContains = "", ReferenceMassKg = 70000, VerticalResponse = 1, LateralResponse = 0.8, LongitudinalResponse = 0.7, PitchResponse = 0.8, RollResponse = 0.75, YawResponse = 0.5, StructuralVibration = 1 });
        manager.Add(new AircraftProfile { Id = "generic-ga", Aircraft = "Generic GA", MatchTitleContains = "Cessna", ReferenceMassKg = 1100, VerticalResponse = 1.6, LateralResponse = 1.4, LongitudinalResponse = 1.2, PitchResponse = 1.5, RollResponse = 1.6, YawResponse = 1.1, StructuralVibration = 1.3 });
        return manager;
    }
}
