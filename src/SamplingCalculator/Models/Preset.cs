namespace SamplingCalculator.Models;

public enum PresetType
{
    Telescope,
    Camera,
    FullRig
}

public class PresetBase
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public PresetType Type { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class TelescopePreset : PresetBase
{
    public TelescopePreset()
    {
        Type = PresetType.Telescope;
    }

    public double BaseFocalLength { get; set; }
    public double? ApertureDiameter { get; set; }
    public double ReducerFactor { get; set; } = 1.0;
    public double BarlowFactor { get; set; } = 1.0;

    public void ApplyTo(CalculatorInput input)
    {
        input.BaseFocalLength = BaseFocalLength;
        input.ApertureDiameter = ApertureDiameter;
        input.ReducerFactor = ReducerFactor;
        input.BarlowFactor = BarlowFactor;
    }

    public static TelescopePreset FromInput(CalculatorInput input, string name)
    {
        return new TelescopePreset
        {
            Name = name,
            BaseFocalLength = input.BaseFocalLength,
            ApertureDiameter = input.ApertureDiameter,
            ReducerFactor = input.ReducerFactor,
            BarlowFactor = input.BarlowFactor
        };
    }
}

public class CameraPreset : PresetBase
{
    public CameraPreset()
    {
        Type = PresetType.Camera;
    }

    public double PixelSize { get; set; }
    public int SensorWidthPx { get; set; }
    public int SensorHeightPx { get; set; }
    public int Binning { get; set; } = 1;

    public void ApplyTo(CalculatorInput input)
    {
        input.PixelSize = PixelSize;
        input.SensorWidthPx = SensorWidthPx;
        input.SensorHeightPx = SensorHeightPx;
        input.Binning = Binning;
    }

    public static CameraPreset FromInput(CalculatorInput input, string name)
    {
        return new CameraPreset
        {
            Name = name,
            PixelSize = input.PixelSize,
            SensorWidthPx = input.SensorWidthPx,
            SensorHeightPx = input.SensorHeightPx,
            Binning = input.Binning
        };
    }
}

public class FullRigPreset : PresetBase
{
    public FullRigPreset()
    {
        Type = PresetType.FullRig;
    }

    // Telescope
    public double BaseFocalLength { get; set; }
    public double? ApertureDiameter { get; set; }
    public double ReducerFactor { get; set; } = 1.0;
    public double BarlowFactor { get; set; } = 1.0;

    // Camera
    public double PixelSize { get; set; }
    public int SensorWidthPx { get; set; }
    public int SensorHeightPx { get; set; }
    public int Binning { get; set; } = 1;

    // Seeing
    public double Seeing { get; set; } = 2.0;

    public void ApplyTo(CalculatorInput input)
    {
        input.BaseFocalLength = BaseFocalLength;
        input.ApertureDiameter = ApertureDiameter;
        input.ReducerFactor = ReducerFactor;
        input.BarlowFactor = BarlowFactor;
        input.PixelSize = PixelSize;
        input.SensorWidthPx = SensorWidthPx;
        input.SensorHeightPx = SensorHeightPx;
        input.Binning = Binning;
        input.Seeing = Seeing;
    }

    public static FullRigPreset FromInput(CalculatorInput input, string name)
    {
        return new FullRigPreset
        {
            Name = name,
            BaseFocalLength = input.BaseFocalLength,
            ApertureDiameter = input.ApertureDiameter,
            ReducerFactor = input.ReducerFactor,
            BarlowFactor = input.BarlowFactor,
            PixelSize = input.PixelSize,
            SensorWidthPx = input.SensorWidthPx,
            SensorHeightPx = input.SensorHeightPx,
            Binning = input.Binning,
            Seeing = input.Seeing
        };
    }
}

public class PresetCollection
{
    public int Version { get; set; } = 1;
    public List<TelescopePreset> Telescopes { get; set; } = new();
    public List<CameraPreset> Cameras { get; set; } = new();
    public List<FullRigPreset> FullRigs { get; set; } = new();
}
