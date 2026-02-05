namespace SamplingCalculator.Models;

public class CalculatorInput : IEquatable<CalculatorInput>
{
    // Telescope / Optics
    public double BaseFocalLength { get; set; } = 800;
    public double? ApertureDiameter { get; set; } = 200;
    public double ReducerFactor { get; set; } = 1.0;
    public double BarlowFactor { get; set; } = 1.0;

    // Camera / Sensor
    public double PixelSize { get; set; } = 3.76;
    public int SensorWidthPx { get; set; } = 6248;
    public int SensorHeightPx { get; set; } = 4176;
    public int Binning { get; set; } = 1;
    public string? CameraName { get; set; }

    // Seeing / Target
    public double Seeing { get; set; } = 2.0;

    public double EffectiveFocalLength => BaseFocalLength * BarlowFactor / ReducerFactor;

    public CalculatorInput Clone()
    {
        return new CalculatorInput
        {
            BaseFocalLength = BaseFocalLength,
            ApertureDiameter = ApertureDiameter,
            ReducerFactor = ReducerFactor,
            BarlowFactor = BarlowFactor,
            PixelSize = PixelSize,
            SensorWidthPx = SensorWidthPx,
            SensorHeightPx = SensorHeightPx,
            Binning = Binning,
            CameraName = CameraName,
            Seeing = Seeing
        };
    }

    public void CopyFrom(CalculatorInput source)
    {
        BaseFocalLength = source.BaseFocalLength;
        ApertureDiameter = source.ApertureDiameter;
        ReducerFactor = source.ReducerFactor;
        BarlowFactor = source.BarlowFactor;
        PixelSize = source.PixelSize;
        SensorWidthPx = source.SensorWidthPx;
        SensorHeightPx = source.SensorHeightPx;
        Binning = source.Binning;
        CameraName = source.CameraName;
        Seeing = source.Seeing;
    }

    public override bool Equals(object? obj) => Equals(obj as CalculatorInput);

    public bool Equals(CalculatorInput? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        // Use approximate equality for doubles to handle floating point nuances safely
        const double tolerance = 0.000001;

        return Math.Abs(BaseFocalLength - other.BaseFocalLength) < tolerance &&
               Math.Abs((ApertureDiameter ?? 0) - (other.ApertureDiameter ?? 0)) < tolerance &&
               (ApertureDiameter.HasValue == other.ApertureDiameter.HasValue) &&
               Math.Abs(ReducerFactor - other.ReducerFactor) < tolerance &&
               Math.Abs(BarlowFactor - other.BarlowFactor) < tolerance &&
               Math.Abs(PixelSize - other.PixelSize) < tolerance &&
               SensorWidthPx == other.SensorWidthPx &&
               SensorHeightPx == other.SensorHeightPx &&
               Binning == other.Binning &&
               string.Equals(CameraName, other.CameraName, StringComparison.Ordinal) &&
               Math.Abs(Seeing - other.Seeing) < tolerance;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(BaseFocalLength);
        hash.Add(ApertureDiameter);
        hash.Add(ReducerFactor);
        hash.Add(BarlowFactor);
        hash.Add(PixelSize);
        hash.Add(SensorWidthPx);
        hash.Add(SensorHeightPx);
        hash.Add(Binning);
        hash.Add(CameraName);
        hash.Add(Seeing);
        return hash.ToHashCode();
    }
}
