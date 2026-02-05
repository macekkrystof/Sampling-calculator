using System.Text.Json;
using Microsoft.JSInterop;
using SamplingCalculator.Models;

namespace SamplingCalculator.Services;

public class PresetService
{
    private readonly IJSRuntime _jsRuntime;
    private const string StorageKey = "sampling-calculator-presets";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private PresetCollection? _cache;

    public PresetService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<PresetCollection> GetAllPresetsAsync()
    {
        if (_cache != null)
            return _cache;

        try
        {
            var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", StorageKey);
            if (string.IsNullOrEmpty(json))
            {
                _cache = new PresetCollection();
                return _cache;
            }

            _cache = JsonSerializer.Deserialize<PresetCollection>(json, JsonOptions);
            if (_cache == null)
            {
                _cache = new PresetCollection();
            }
        }
        catch (JsonException)
        {
            // Invalid JSON - reset to empty collection
            _cache = new PresetCollection();
            await SaveCollectionAsync();
        }

        return _cache;
    }

    private async Task SaveCollectionAsync()
    {
        if (_cache == null) return;

        var json = JsonSerializer.Serialize(_cache, JsonOptions);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
    }

    // Telescope presets
    public async Task<List<TelescopePreset>> GetTelescopePresetsAsync()
    {
        var collection = await GetAllPresetsAsync();
        return collection.Telescopes;
    }

    public async Task SaveTelescopePresetAsync(TelescopePreset preset)
    {
        var collection = await GetAllPresetsAsync();
        var existing = collection.Telescopes.FindIndex(p => p.Id == preset.Id);
        if (existing >= 0)
        {
            preset.UpdatedAt = DateTime.UtcNow;
            collection.Telescopes[existing] = preset;
        }
        else
        {
            collection.Telescopes.Add(preset);
        }
        await SaveCollectionAsync();
    }

    public async Task DeleteTelescopePresetAsync(string id)
    {
        var collection = await GetAllPresetsAsync();
        collection.Telescopes.RemoveAll(p => p.Id == id);
        await SaveCollectionAsync();
    }

    // Camera presets
    public async Task<List<CameraPreset>> GetCameraPresetsAsync()
    {
        var collection = await GetAllPresetsAsync();
        return collection.Cameras;
    }

    public async Task SaveCameraPresetAsync(CameraPreset preset)
    {
        var collection = await GetAllPresetsAsync();
        var existing = collection.Cameras.FindIndex(p => p.Id == preset.Id);
        if (existing >= 0)
        {
            preset.UpdatedAt = DateTime.UtcNow;
            collection.Cameras[existing] = preset;
        }
        else
        {
            collection.Cameras.Add(preset);
        }
        await SaveCollectionAsync();
    }

    public async Task DeleteCameraPresetAsync(string id)
    {
        var collection = await GetAllPresetsAsync();
        collection.Cameras.RemoveAll(p => p.Id == id);
        await SaveCollectionAsync();
    }

    // Full rig presets
    public async Task<List<FullRigPreset>> GetFullRigPresetsAsync()
    {
        var collection = await GetAllPresetsAsync();
        return collection.FullRigs;
    }

    public async Task SaveFullRigPresetAsync(FullRigPreset preset)
    {
        var collection = await GetAllPresetsAsync();
        var existing = collection.FullRigs.FindIndex(p => p.Id == preset.Id);
        if (existing >= 0)
        {
            preset.UpdatedAt = DateTime.UtcNow;
            collection.FullRigs[existing] = preset;
        }
        else
        {
            collection.FullRigs.Add(preset);
        }
        await SaveCollectionAsync();
    }

    public async Task DeleteFullRigPresetAsync(string id)
    {
        var collection = await GetAllPresetsAsync();
        collection.FullRigs.RemoveAll(p => p.Id == id);
        await SaveCollectionAsync();
    }

    // Utility for clearing cache (e.g., after external changes)
    public void ClearCache()
    {
        _cache = null;
    }

    // Clear all presets
    public async Task ClearAllPresetsAsync()
    {
        _cache = new PresetCollection();
        await SaveCollectionAsync();
    }
}
