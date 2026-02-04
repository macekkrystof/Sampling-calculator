using System.Globalization;
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

        // Status message (use invariant culture for consistent decimal formatting)
        var inv = CultureInfo.InvariantCulture;
        var statusMessage = status switch
        {
            SamplingStatus.Undersampled => string.Format(inv, "Your setup is likely undersampled for {0:0.0}″ seeing.", input.Seeing),
            SamplingStatus.Oversampled => string.Format(inv, "Your setup is likely oversampled for {0:0.0}″ seeing.", input.Seeing),
            SamplingStatus.Optimal => string.Format(inv, "Your setup is well-matched for {0:0.0}″ seeing.", input.Seeing),
            _ => string.Empty
        };

        // Binning recommendation (only when undersampled — higher binning increases pixel scale toward optimal)
        int? recommendedBinning = null;
        string? binningRecommendation = null;
        if (status == SamplingStatus.Undersampled)
        {
            // Pixel scale is too large; binning would make it even larger — no binning help here.
            // Actually undersampled means pixels are too big, so no binning recommendation.
        }
        else if (status == SamplingStatus.Oversampled && input.Binning < 4)
        {
            // Try increasing binning to bring pixel scale into the optimal range
            var targetScale = (optimalMin + optimalMax) / 2.0;
            var neededBinning = (int)Math.Ceiling(targetScale / (pixelScale / input.Binning));
            neededBinning = Math.Clamp(neededBinning, input.Binning + 1, 4);
            var newScale = 206.265 * (input.PixelSize * neededBinning) / effectiveFocal;
            recommendedBinning = neededBinning;
            binningRecommendation = string.Format(inv, "Consider {0}×{0} binning for ~{1:0.00}″/px.", neededBinning, newScale);
        }

        // Corrector (reducer/barlow) recommendation
        double? recommendedCorrectorFactor = null;
        string? correctorRecommendation = null;
        if (status == SamplingStatus.Oversampled)
        {
            // Oversampled = pixelScale too small = effectiveFocal too large.
            // With the PRD formula (effectiveFocal = baseFocal * barlow / reducer),
            // a physical reducer (0.7×) actually INCREASES effectiveFocal. So to reduce
            // effective focal length we'd need reducer > 1, which isn't a real product.
            // Instead, suggest using a shorter focal length telescope or rely on binning.
            // We express the recommendation as a "target effective focal length" ratio.
            var targetScale = (optimalMin + optimalMax) / 2.0;
            var neededFocalRatio = pixelScale / targetScale; // < 1 means we need shorter focal
            if (neededFocalRatio >= 0.5 && neededFocalRatio < 1.0)
            {
                var neededFocal = effectiveFocal * neededFocalRatio;
                recommendedCorrectorFactor = Math.Round(neededFocalRatio, 2);
                var newScale = targetScale;
                correctorRecommendation = string.Format(inv,
                    "With a {0:0.0#}× reducer you would get ~{1:0.00}″/px (effective focal ~{2:0}mm).",
                    recommendedCorrectorFactor, newScale, neededFocal);
            }
        }
        else if (status == SamplingStatus.Undersampled)
        {
            // Need to decrease pixel scale → increase focal length → use a barlow
            var targetScale = (optimalMin + optimalMax) / 2.0;
            var newEffectiveFocal = 206.265 * effectivePixelSize / targetScale;
            var suggestedBarlow = newEffectiveFocal * input.ReducerFactor / input.BaseFocalLength;
            if (suggestedBarlow >= 1.5 && suggestedBarlow <= 5.0)
            {
                recommendedCorrectorFactor = Math.Round(suggestedBarlow, 1);
                var newScale = targetScale;
                correctorRecommendation = string.Format(inv, "With a {0:0.0}× barlow you would get ~{1:0.00}″/px.", recommendedCorrectorFactor, newScale);
            }
        }

        // Extreme warnings
        string? extremeWarning = null;
        if (pixelScale > 4.0)
            extremeWarning = "Pixel scale is very large (>4″/px). Image resolution will be very low.";
        else if (pixelScale < 0.2)
            extremeWarning = "Pixel scale is very small (<0.2″/px). Guiding demands will be extreme and SNR will suffer.";

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
            OptimalRangeMax = optimalMax,
            StatusMessage = statusMessage,
            RecommendedBinning = recommendedBinning,
            BinningRecommendation = binningRecommendation,
            CorrectorRecommendation = correctorRecommendation,
            RecommendedCorrectorFactor = recommendedCorrectorFactor,
            ExtremeWarning = extremeWarning
        };
    }
}
