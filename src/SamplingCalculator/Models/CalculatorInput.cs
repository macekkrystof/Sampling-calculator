namespace SamplingCalculator.Models;

public class CalculatorInput
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
}
