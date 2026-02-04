namespace SamplingCalculator.Models;

public class CalculatorResult
{
    public double PixelScale { get; set; }
    public double FovWidthDeg { get; set; }
    public double FovHeightDeg { get; set; }
    public double FovWidthArcmin { get; set; }
    public double FovHeightArcmin { get; set; }
    public double EffectiveFocalLength { get; set; }
    public double? FRatio { get; set; }
    public double? DawesLimitArcsec { get; set; }
    public SamplingStatus Status { get; set; }
    public double OptimalRangeMin { get; set; }
    public double OptimalRangeMax { get; set; }
}

public enum SamplingStatus
{
    Undersampled,
    Optimal,
    Oversampled
}
