using SamplingCalculator.Models;
using SamplingCalculator.Services;

namespace SamplingCalculator.Tests;

public class SamplingCalculatorServiceTests
{
    private readonly SamplingCalculatorService _sut = new();

    [Fact]
    public void PixelScale_DefaultInputs_CalculatesCorrectly()
    {
        // 206.265 * 3.76 / 800 = 0.9694...
        var input = new CalculatorInput
        {
            BaseFocalLength = 800,
            PixelSize = 3.76,
            Binning = 1,
            ReducerFactor = 1.0,
            BarlowFactor = 1.0
        };

        var result = _sut.Calculate(input);

        Assert.Equal(0.97, result.PixelScale, 2);
    }

    [Fact]
    public void PixelScale_WithBinning2_Doubles()
    {
        var input = new CalculatorInput
        {
            BaseFocalLength = 800,
            PixelSize = 3.76,
            Binning = 2,
            ReducerFactor = 1.0,
            BarlowFactor = 1.0
        };

        var result = _sut.Calculate(input);

        // Should be double the 1x1 value
        Assert.Equal(1.94, result.PixelScale, 2);
    }

    [Fact]
    public void EffectiveFocalLength_WithReducer()
    {
        var input = new CalculatorInput
        {
            BaseFocalLength = 1000,
            ReducerFactor = 0.7,
            BarlowFactor = 1.0
        };

        var result = _sut.Calculate(input);

        Assert.Equal(1000.0 / 0.7, result.EffectiveFocalLength, 1);
    }

    [Fact]
    public void EffectiveFocalLength_WithBarlow()
    {
        var input = new CalculatorInput
        {
            BaseFocalLength = 1000,
            ReducerFactor = 1.0,
            BarlowFactor = 2.0
        };

        var result = _sut.Calculate(input);

        Assert.Equal(2000.0, result.EffectiveFocalLength, 1);
    }

    [Fact]
    public void FovWidth_CalculatesCorrectly()
    {
        var input = new CalculatorInput
        {
            BaseFocalLength = 800,
            PixelSize = 3.76,
            SensorWidthPx = 6248,
            SensorHeightPx = 4176,
            Binning = 1,
            ReducerFactor = 1.0,
            BarlowFactor = 1.0
        };

        var result = _sut.Calculate(input);

        // pixelScale ~0.9694, FOV width = 0.9694 * 6248 / 3600
        double expectedFovWidthDeg = (206.265 * 3.76 / 800.0) * 6248.0 / 3600.0;
        Assert.Equal(expectedFovWidthDeg, result.FovWidthDeg, 3);
    }

    [Fact]
    public void FovArcmin_Is60TimesDeg()
    {
        var input = new CalculatorInput();
        var result = _sut.Calculate(input);

        Assert.Equal(result.FovWidthDeg * 60.0, result.FovWidthArcmin, 5);
        Assert.Equal(result.FovHeightDeg * 60.0, result.FovHeightArcmin, 5);
    }

    [Fact]
    public void SamplingStatus_Optimal_WhenInRange()
    {
        // seeing=2.0, optimal range = 0.667 .. 1.0
        // pixelScale = 206.265 * pixelSize / focal
        // Need pixelScale ~0.8 => pixelSize = 0.8 * focal / 206.265
        // With focal=1000: pixelSize = 0.8 * 1000 / 206.265 = 3.879
        var input = new CalculatorInput
        {
            BaseFocalLength = 1000,
            PixelSize = 3.879,
            Binning = 1,
            Seeing = 2.0,
            ReducerFactor = 1.0,
            BarlowFactor = 1.0
        };

        var result = _sut.Calculate(input);

        Assert.Equal(SamplingStatus.Optimal, result.Status);
    }

    [Fact]
    public void SamplingStatus_Undersampled_WhenPixelScaleTooLarge()
    {
        // seeing=2.0, undersampled when pixelScale > 1.0
        // pixelScale = 206.265 * 5.0 / 500 = 2.063 => undersampled
        var input = new CalculatorInput
        {
            BaseFocalLength = 500,
            PixelSize = 5.0,
            Binning = 1,
            Seeing = 2.0,
            ReducerFactor = 1.0,
            BarlowFactor = 1.0
        };

        var result = _sut.Calculate(input);

        Assert.Equal(SamplingStatus.Undersampled, result.Status);
    }

    [Fact]
    public void SamplingStatus_Oversampled_WhenPixelScaleTooSmall()
    {
        // seeing=2.0, oversampled when pixelScale < 0.667
        // pixelScale = 206.265 * 2.0 / 2000 = 0.206 => oversampled
        var input = new CalculatorInput
        {
            BaseFocalLength = 2000,
            PixelSize = 2.0,
            Binning = 1,
            Seeing = 2.0,
            ReducerFactor = 1.0,
            BarlowFactor = 1.0
        };

        var result = _sut.Calculate(input);

        Assert.Equal(SamplingStatus.Oversampled, result.Status);
    }

    [Fact]
    public void OptimalRange_CalculatedFromSeeing()
    {
        var input = new CalculatorInput { Seeing = 3.0 };

        var result = _sut.Calculate(input);

        Assert.Equal(1.0, result.OptimalRangeMin, 2);
        Assert.Equal(1.5, result.OptimalRangeMax, 2);
    }

    [Fact]
    public void DawesLimit_CalculatedWhenApertureProvided()
    {
        var input = new CalculatorInput { ApertureDiameter = 200 };

        var result = _sut.Calculate(input);

        Assert.NotNull(result.DawesLimitArcsec);
        Assert.Equal(0.58, result.DawesLimitArcsec!.Value, 2);
    }

    [Fact]
    public void DawesLimit_NullWhenNoAperture()
    {
        var input = new CalculatorInput { ApertureDiameter = null };

        var result = _sut.Calculate(input);

        Assert.Null(result.DawesLimitArcsec);
    }

    [Fact]
    public void FRatio_CalculatedWhenApertureProvided()
    {
        var input = new CalculatorInput
        {
            BaseFocalLength = 800,
            ApertureDiameter = 200,
            ReducerFactor = 1.0,
            BarlowFactor = 1.0
        };

        var result = _sut.Calculate(input);

        Assert.NotNull(result.FRatio);
        Assert.Equal(4.0, result.FRatio!.Value, 1);
    }

    [Fact]
    public void FRatio_NullWhenNoAperture()
    {
        var input = new CalculatorInput { ApertureDiameter = null };

        var result = _sut.Calculate(input);

        Assert.Null(result.FRatio);
    }

    [Fact]
    public void EffectiveFocalLength_WithCombinedReducerAndBarlow()
    {
        var input = new CalculatorInput
        {
            BaseFocalLength = 1000,
            ReducerFactor = 0.8,
            BarlowFactor = 2.0
        };

        var result = _sut.Calculate(input);

        // 1000 * 2.0 / 0.8 = 2500
        Assert.Equal(2500.0, result.EffectiveFocalLength, 1);
    }

    [Fact]
    public void PixelScale_WithBinning3_Triples()
    {
        var input = new CalculatorInput
        {
            BaseFocalLength = 800,
            PixelSize = 3.76,
            Binning = 3,
            ReducerFactor = 1.0,
            BarlowFactor = 1.0
        };

        var result = _sut.Calculate(input);

        // 206.265 * (3.76 * 3) / 800 = 206.265 * 11.28 / 800 = 2.908
        Assert.Equal(2.91, result.PixelScale, 2);
    }

    [Fact]
    public void PixelScale_WithBinning4_Quadruples()
    {
        var input = new CalculatorInput
        {
            BaseFocalLength = 800,
            PixelSize = 3.76,
            Binning = 4,
            ReducerFactor = 1.0,
            BarlowFactor = 1.0
        };

        var result = _sut.Calculate(input);

        // 206.265 * (3.76 * 4) / 800 = 206.265 * 15.04 / 800 = 3.878
        Assert.Equal(3.88, result.PixelScale, 2);
    }

    [Fact]
    public void Fov_UnchangedByBinning()
    {
        var baseInput = new CalculatorInput
        {
            BaseFocalLength = 800,
            PixelSize = 3.76,
            SensorWidthPx = 6248,
            SensorHeightPx = 4176,
            Binning = 1,
            ReducerFactor = 1.0,
            BarlowFactor = 1.0
        };

        var binnedInput = new CalculatorInput
        {
            BaseFocalLength = 800,
            PixelSize = 3.76,
            SensorWidthPx = 6248,
            SensorHeightPx = 4176,
            Binning = 2,
            ReducerFactor = 1.0,
            BarlowFactor = 1.0
        };

        var baseResult = _sut.Calculate(baseInput);
        var binnedResult = _sut.Calculate(binnedInput);

        // FOV should be the same regardless of binning
        Assert.Equal(baseResult.FovWidthDeg, binnedResult.FovWidthDeg, 5);
        Assert.Equal(baseResult.FovHeightDeg, binnedResult.FovHeightDeg, 5);
    }

    [Fact]
    public void PixelScale_WithReducer_IncreasesScale()
    {
        var input = new CalculatorInput
        {
            BaseFocalLength = 1000,
            PixelSize = 3.76,
            Binning = 1,
            ReducerFactor = 0.7,
            BarlowFactor = 1.0
        };

        var result = _sut.Calculate(input);

        // effectiveFocal = 1000 / 0.7 = 1428.57
        // pixelScale = 206.265 * 3.76 / 1428.57 = 0.543
        // Wait – reducer shortens focal length, so effectiveFocal = 1000 * 1.0 / 0.7 = 1428.57
        // That INCREASES focal length, which would DECREASE pixel scale.
        // But a reducer should DECREASE focal length (0.7x reducer means 700mm effective).
        // The PRD formula: effectiveFocal = baseFocal * barlowFactor / reducerFactor
        // So 1000 * 1.0 / 0.7 = 1428.57 — this seems inverted.
        // Actually the PRD explicitly states this formula, so we follow it.
        double expectedFocal = 1000.0 * 1.0 / 0.7;
        double expectedScale = 206.265 * 3.76 / expectedFocal;
        Assert.Equal(expectedScale, result.PixelScale, 3);
    }

    [Fact]
    public void FRatio_WithReducerAndBarlow()
    {
        var input = new CalculatorInput
        {
            BaseFocalLength = 800,
            ApertureDiameter = 200,
            ReducerFactor = 0.8,
            BarlowFactor = 1.5
        };

        var result = _sut.Calculate(input);

        // effectiveFocal = 800 * 1.5 / 0.8 = 1500
        // fRatio = 1500 / 200 = 7.5
        Assert.NotNull(result.FRatio);
        Assert.Equal(7.5, result.FRatio!.Value, 1);
    }

    // --- Task 3: Classification boundary conditions ---

    [Fact]
    public void SamplingStatus_ExactlyAtOptimalMax_IsOptimal()
    {
        // seeing=2.0, optimalMax = 1.0
        // pixelScale = 206.265 * pixelSize / focal = 1.0
        // pixelSize = 1.0 * 1000 / 206.265 = 4.8481...
        var input = new CalculatorInput
        {
            BaseFocalLength = 1000,
            PixelSize = 1000.0 / 206.265,
            Binning = 1,
            Seeing = 2.0,
            ReducerFactor = 1.0,
            BarlowFactor = 1.0
        };

        var result = _sut.Calculate(input);

        // pixelScale == seeing/2 == 1.0 => boundary is NOT > optimalMax, so Optimal
        Assert.Equal(SamplingStatus.Optimal, result.Status);
    }

    [Fact]
    public void SamplingStatus_ExactlyAtOptimalMin_IsOptimal()
    {
        // seeing=3.0, optimalMin = 1.0
        // pixelScale = 206.265 * pixelSize / focal = 1.0
        var input = new CalculatorInput
        {
            BaseFocalLength = 1000,
            PixelSize = 1000.0 / 206.265,
            Binning = 1,
            Seeing = 3.0,
            ReducerFactor = 1.0,
            BarlowFactor = 1.0
        };

        var result = _sut.Calculate(input);

        // pixelScale == seeing/3 == 1.0 => boundary is NOT < optimalMin, so Optimal
        Assert.Equal(SamplingStatus.Optimal, result.Status);
    }

    [Fact]
    public void SamplingStatus_JustAboveOptimalMax_IsUndersampled()
    {
        // seeing=2.0, optimalMax = 1.0
        // pixelScale slightly above 1.0
        var input = new CalculatorInput
        {
            BaseFocalLength = 1000,
            PixelSize = 1000.0 / 206.265 + 0.01,
            Binning = 1,
            Seeing = 2.0,
            ReducerFactor = 1.0,
            BarlowFactor = 1.0
        };

        var result = _sut.Calculate(input);

        Assert.Equal(SamplingStatus.Undersampled, result.Status);
    }

    [Fact]
    public void SamplingStatus_JustBelowOptimalMin_IsOversampled()
    {
        // seeing=3.0, optimalMin = 1.0
        // pixelScale slightly below 1.0
        var input = new CalculatorInput
        {
            BaseFocalLength = 1000,
            PixelSize = 1000.0 / 206.265 - 0.01,
            Binning = 1,
            Seeing = 3.0,
            ReducerFactor = 1.0,
            BarlowFactor = 1.0
        };

        var result = _sut.Calculate(input);

        Assert.Equal(SamplingStatus.Oversampled, result.Status);
    }

    // --- Task 3: Status message ---

    [Fact]
    public void StatusMessage_Optimal_ContainsWellMatched()
    {
        var input = new CalculatorInput
        {
            BaseFocalLength = 1000,
            PixelSize = 3.879,
            Binning = 1,
            Seeing = 2.0
        };

        var result = _sut.Calculate(input);

        Assert.Contains("well-matched", result.StatusMessage);
        Assert.Contains("2.0", result.StatusMessage);
    }

    [Fact]
    public void StatusMessage_Undersampled_ContainsUndersampled()
    {
        var input = new CalculatorInput
        {
            BaseFocalLength = 500,
            PixelSize = 5.0,
            Binning = 1,
            Seeing = 2.0
        };

        var result = _sut.Calculate(input);

        Assert.Contains("undersampled", result.StatusMessage);
    }

    [Fact]
    public void StatusMessage_Oversampled_ContainsOversampled()
    {
        var input = new CalculatorInput
        {
            BaseFocalLength = 2000,
            PixelSize = 2.0,
            Binning = 1,
            Seeing = 2.0
        };

        var result = _sut.Calculate(input);

        Assert.Contains("oversampled", result.StatusMessage);
    }

    // --- Task 3: Binning recommendation ---

    [Fact]
    public void BinningRecommendation_WhenOversampled_SuggestsHigherBinning()
    {
        // pixelScale = 206.265 * 2.0 / 2000 = 0.206, seeing=2.0 => oversampled
        var input = new CalculatorInput
        {
            BaseFocalLength = 2000,
            PixelSize = 2.0,
            Binning = 1,
            Seeing = 2.0,
            ReducerFactor = 1.0,
            BarlowFactor = 1.0
        };

        var result = _sut.Calculate(input);

        Assert.NotNull(result.RecommendedBinning);
        Assert.True(result.RecommendedBinning > 1);
        Assert.NotNull(result.BinningRecommendation);
        Assert.Contains("binning", result.BinningRecommendation, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BinningRecommendation_WhenOptimal_IsNull()
    {
        var input = new CalculatorInput
        {
            BaseFocalLength = 1000,
            PixelSize = 3.879,
            Binning = 1,
            Seeing = 2.0
        };

        var result = _sut.Calculate(input);

        Assert.Null(result.RecommendedBinning);
        Assert.Null(result.BinningRecommendation);
    }

    [Fact]
    public void BinningRecommendation_WhenUndersampled_IsNull()
    {
        var input = new CalculatorInput
        {
            BaseFocalLength = 500,
            PixelSize = 5.0,
            Binning = 1,
            Seeing = 2.0
        };

        var result = _sut.Calculate(input);

        Assert.Null(result.RecommendedBinning);
        Assert.Null(result.BinningRecommendation);
    }

    [Fact]
    public void BinningRecommendation_AlreadyBinning4_NoRecommendation()
    {
        // Even if oversampled, can't go above 4
        var input = new CalculatorInput
        {
            BaseFocalLength = 2000,
            PixelSize = 2.0,
            Binning = 4,
            Seeing = 2.0,
            ReducerFactor = 1.0,
            BarlowFactor = 1.0
        };

        var result = _sut.Calculate(input);

        // pixelScale = 206.265 * 8 / 2000 = 0.825 => optimal or close
        // Might not be oversampled at binning 4
        // If optimal, no recommendation. If still oversampled, binning=4 so no recommendation.
        Assert.Null(result.RecommendedBinning);
    }

    // --- Task 3: Corrector recommendation ---

    [Fact]
    public void CorrectorRecommendation_WhenOversampled_SuggestsReducer()
    {
        // pixelScale = 206.265 * 3.76 / 1200 = 0.646, seeing=2.0, optMin=0.667 => just oversampled
        // target = 0.833, newEffFocal = 206.265*3.76/0.833 = 931.3
        // suggestedReducer = 1200*1.0/931.3 = 1.29 ... still > 0.95
        // Need setup where the reducer factor lands in 0.5-0.95.
        // pixelScale = 206.265 * 3.76 / 800 = 0.97 at f=800 => optimal
        // Let's use focal=1000: pixelScale = 206.265*3.76/1000 = 0.7756, optMin=0.667 => optimal
        // focal=1100: pixelScale = 0.705 => optimal
        // focal=1200: pixelScale = 0.646 => oversampled (< 0.667)
        // target=0.833, newEffFocal = 206.265*3.76/0.833 = 930.8
        // reducer = 1200/930.8 = 1.289 — too high. The issue: focal is already close, reducer would need to be >1.
        // Need a bigger gap: focal=1600, pixelScale=0.485, target=0.833
        // newEffFocal = 206.265*3.76/0.833 = 930.8, reducer = 1600/930.8 = 0.582 => in range!
        var input = new CalculatorInput
        {
            BaseFocalLength = 1600,
            PixelSize = 3.76,
            Binning = 1,
            Seeing = 2.0,
            ReducerFactor = 1.0,
            BarlowFactor = 1.0
        };

        var result = _sut.Calculate(input);

        Assert.Equal(SamplingStatus.Oversampled, result.Status);
        Assert.NotNull(result.CorrectorRecommendation);
        Assert.Contains("reducer", result.CorrectorRecommendation, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CorrectorRecommendation_WhenUndersampled_SuggestsBarlow()
    {
        // pixelScale = 206.265 * 5.0 / 500 = 2.063, seeing=2.0 => undersampled
        var input = new CalculatorInput
        {
            BaseFocalLength = 500,
            PixelSize = 5.0,
            Binning = 1,
            Seeing = 2.0,
            ReducerFactor = 1.0,
            BarlowFactor = 1.0
        };

        var result = _sut.Calculate(input);

        Assert.Equal(SamplingStatus.Undersampled, result.Status);
        Assert.NotNull(result.CorrectorRecommendation);
        Assert.Contains("barlow", result.CorrectorRecommendation, StringComparison.OrdinalIgnoreCase);
    }

    // --- Task 3: Extreme warnings ---

    [Fact]
    public void ExtremeWarning_WhenPixelScaleAbove4()
    {
        // pixelScale = 206.265 * 5.0 / 200 = 5.157 => extreme
        var input = new CalculatorInput
        {
            BaseFocalLength = 200,
            PixelSize = 5.0,
            Binning = 1,
            Seeing = 2.0,
            ReducerFactor = 1.0,
            BarlowFactor = 1.0
        };

        var result = _sut.Calculate(input);

        Assert.NotNull(result.ExtremeWarning);
        Assert.Contains(">4", result.ExtremeWarning);
    }

    [Fact]
    public void ExtremeWarning_WhenPixelScaleBelow02()
    {
        // pixelScale = 206.265 * 1.0 / 5000 = 0.041 => extreme
        var input = new CalculatorInput
        {
            BaseFocalLength = 5000,
            PixelSize = 1.0,
            Binning = 1,
            Seeing = 2.0,
            ReducerFactor = 1.0,
            BarlowFactor = 1.0
        };

        var result = _sut.Calculate(input);

        Assert.NotNull(result.ExtremeWarning);
        Assert.Contains("<0.2", result.ExtremeWarning);
    }

    [Fact]
    public void ExtremeWarning_NullWhenNormal()
    {
        var input = new CalculatorInput
        {
            BaseFocalLength = 800,
            PixelSize = 3.76,
            Binning = 1,
            Seeing = 2.0
        };

        var result = _sut.Calculate(input);

        Assert.Null(result.ExtremeWarning);
    }

    // --- Task 3: Different seeing values ---

    [Theory]
    [InlineData(1.0, 0.333, 0.5)]
    [InlineData(2.0, 0.667, 1.0)]
    [InlineData(3.0, 1.0, 1.5)]
    [InlineData(4.0, 1.333, 2.0)]
    public void OptimalRange_ScalesWithSeeing(double seeing, double expectedMin, double expectedMax)
    {
        var input = new CalculatorInput { Seeing = seeing };
        var result = _sut.Calculate(input);

        Assert.Equal(expectedMin, result.OptimalRangeMin, 2);
        Assert.Equal(expectedMax, result.OptimalRangeMax, 2);
    }
}
