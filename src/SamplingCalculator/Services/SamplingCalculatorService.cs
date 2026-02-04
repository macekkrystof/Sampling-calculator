using SamplingCalculator.Models;

namespace SamplingCalculator.Services;

public class SamplingCalculatorService
{
    public CalculatorResult Calculate(CalculatorInput input)
    {
        var effectiveFocal = input.EffectiveFocalLength;
        var effectivePixelSize = input.PixelSize * input.Binning;

        var pixelScale = 206.265 * effectivePixelSize / effectiveFocal;

        var fovWidthDeg = (pixelScale * input.SensorWidthPx / input.Binning) / 3600.0;
        var fovHeightDeg = (pixelScale * input.SensorHeightPx / input.Binning) / 3600.0;

        var optimalMin = input.Seeing / 3.0;
        var optimalMax = input.Seeing / 2.0;

        SamplingStatus status;
        if (pixelScale > optimalMax)
            status = SamplingStatus.Undersampled;
        else if (pixelScale < optimalMin)
            status = SamplingStatus.Oversampled;
        else
            status = SamplingStatus.Optimal;

        double? fRatio = input.ApertureDiameter.HasValue && input.ApertureDiameter.Value > 0
            ? effectiveFocal / input.ApertureDiameter.Value
            : null;

        double? dawes = input.ApertureDiameter.HasValue && input.ApertureDiameter.Value > 0
            ? 116.0 / input.ApertureDiameter.Value
            : null;

        return new CalculatorResult
        {
            PixelScale = pixelScale,
            FovWidthDeg = fovWidthDeg,
            FovHeightDeg = fovHeightDeg,
            FovWidthArcmin = fovWidthDeg * 60.0,
            FovHeightArcmin = fovHeightDeg * 60.0,
            EffectiveFocalLength = effectiveFocal,
            FRatio = fRatio,
            DawesLimitArcsec = dawes,
            Status = status,
            OptimalRangeMin = optimalMin,
            OptimalRangeMax = optimalMax
        };
    }
}
