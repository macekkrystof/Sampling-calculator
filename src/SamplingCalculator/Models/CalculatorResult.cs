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

    /// <summary>Summary sentence, e.g. "Your setup is likely oversampled for 2.0″ seeing."</summary>
    public string StatusMessage { get; set; } = string.Empty;

    /// <summary>Suggested binning level to move closer to the optimal range (null if already optimal).</summary>
    public int? RecommendedBinning { get; set; }

    /// <summary>Human-readable binning recommendation, e.g. "Consider 2×2 binning for ~1.94″/px".</summary>
    public string? BinningRecommendation { get; set; }

    /// <summary>Suggested reducer/barlow factor to approach optimal range (null if already optimal).</summary>
    public double? RecommendedCorrectorFactor { get; set; }

    /// <summary>Human-readable corrector recommendation.</summary>
    public string? CorrectorRecommendation { get; set; }

    /// <summary>Warning for extreme pixel scale values (null if within reasonable range).</summary>
    public string? ExtremeWarning { get; set; }
}

public enum SamplingStatus
{
    Undersampled,
    Optimal,
    Oversampled
}
