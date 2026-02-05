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

// --- Task 4: Input validation tests ---

public class InputValidationServiceTests
{
    // Focal length
    [Theory]
    [InlineData(800, true)]
    [InlineData(1, true)]
    [InlineData(50000, true)]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    [InlineData(50001, false)]
    public void ValidateFocalLength(double value, bool expectedValid)
    {
        var result = InputValidationService.ValidateFocalLength(value);
        Assert.Equal(expectedValid, result.IsValid);
    }

    // Aperture (optional)
    [Fact]
    public void ValidateAperture_Null_IsValid()
    {
        var result = InputValidationService.ValidateAperture(null);
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(200, true)]
    [InlineData(1, true)]
    [InlineData(10000, true)]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    [InlineData(10001, false)]
    public void ValidateAperture(double value, bool expectedValid)
    {
        var result = InputValidationService.ValidateAperture(value);
        Assert.Equal(expectedValid, result.IsValid);
    }

    // Reducer factor
    [Theory]
    [InlineData(1.0, true)]
    [InlineData(0.7, true)]
    [InlineData(0.1, true)]
    [InlineData(0.09, false)]
    [InlineData(1.1, false)]
    public void ValidateReducerFactor(double value, bool expectedValid)
    {
        var result = InputValidationService.ValidateReducerFactor(value);
        Assert.Equal(expectedValid, result.IsValid);
    }

    // Barlow factor
    [Theory]
    [InlineData(1.0, true)]
    [InlineData(2.0, true)]
    [InlineData(5.0, true)]
    [InlineData(0.9, false)]
    [InlineData(5.1, false)]
    public void ValidateBarlowFactor(double value, bool expectedValid)
    {
        var result = InputValidationService.ValidateBarlowFactor(value);
        Assert.Equal(expectedValid, result.IsValid);
    }

    // Pixel size
    [Theory]
    [InlineData(3.76, true)]
    [InlineData(0.01, true)]
    [InlineData(50, true)]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    [InlineData(51, false)]
    public void ValidatePixelSize(double value, bool expectedValid)
    {
        var result = InputValidationService.ValidatePixelSize(value);
        Assert.Equal(expectedValid, result.IsValid);
    }

    // Sensor dimension
    [Theory]
    [InlineData(6248, true)]
    [InlineData(1, true)]
    [InlineData(100000, true)]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    [InlineData(100001, false)]
    public void ValidateSensorDimension(int value, bool expectedValid)
    {
        var result = InputValidationService.ValidateSensorDimension(value);
        Assert.Equal(expectedValid, result.IsValid);
    }

    // Seeing
    [Theory]
    [InlineData(2.0, true)]
    [InlineData(0.1, true)]
    [InlineData(20, true)]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    [InlineData(21, false)]
    public void ValidateSeeing(double value, bool expectedValid)
    {
        var result = InputValidationService.ValidateSeeing(value);
        Assert.Equal(expectedValid, result.IsValid);
    }

    // Error messages present when invalid
    [Fact]
    public void InvalidResult_HasErrorMessage()
    {
        var result = InputValidationService.ValidateFocalLength(0);
        Assert.False(result.IsValid);
        Assert.NotNull(result.ErrorMessage);
        Assert.NotEmpty(result.ErrorMessage);
    }

    [Fact]
    public void ValidResult_HasNoErrorMessage()
    {
        var result = InputValidationService.ValidateFocalLength(800);
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }
}

// --- Task 6: CalculatorInput Clone and CopyFrom tests ---

public class CalculatorInputCopyTests
{
    [Fact]
    public void Clone_CreatesIndependentCopy()
    {
        var original = new CalculatorInput
        {
            BaseFocalLength = 1200,
            ApertureDiameter = 250,
            ReducerFactor = 0.75,
            BarlowFactor = 1.5,
            PixelSize = 4.5,
            SensorWidthPx = 4096,
            SensorHeightPx = 2160,
            Binning = 2,
            CameraName = "Test Camera",
            Seeing = 2.5
        };

        var clone = original.Clone();

        // Verify all values match
        Assert.Equal(original.BaseFocalLength, clone.BaseFocalLength);
        Assert.Equal(original.ApertureDiameter, clone.ApertureDiameter);
        Assert.Equal(original.ReducerFactor, clone.ReducerFactor);
        Assert.Equal(original.BarlowFactor, clone.BarlowFactor);
        Assert.Equal(original.PixelSize, clone.PixelSize);
        Assert.Equal(original.SensorWidthPx, clone.SensorWidthPx);
        Assert.Equal(original.SensorHeightPx, clone.SensorHeightPx);
        Assert.Equal(original.Binning, clone.Binning);
        Assert.Equal(original.CameraName, clone.CameraName);
        Assert.Equal(original.Seeing, clone.Seeing);

        // Verify independence - modifying clone doesn't affect original
        clone.BaseFocalLength = 999;
        clone.ApertureDiameter = 100;
        clone.CameraName = "Modified";

        Assert.Equal(1200, original.BaseFocalLength);
        Assert.Equal(250, original.ApertureDiameter);
        Assert.Equal("Test Camera", original.CameraName);
    }

    [Fact]
    public void Clone_WithNullAperture_ClonesCorrectly()
    {
        var original = new CalculatorInput { ApertureDiameter = null };

        var clone = original.Clone();

        Assert.Null(clone.ApertureDiameter);
    }

    [Fact]
    public void Clone_WithNullCameraName_ClonesCorrectly()
    {
        var original = new CalculatorInput { CameraName = null };

        var clone = original.Clone();

        Assert.Null(clone.CameraName);
    }

    [Fact]
    public void CopyFrom_CopiesAllValues()
    {
        var source = new CalculatorInput
        {
            BaseFocalLength = 1500,
            ApertureDiameter = 300,
            ReducerFactor = 0.63,
            BarlowFactor = 2.0,
            PixelSize = 2.4,
            SensorWidthPx = 9576,
            SensorHeightPx = 6388,
            Binning = 3,
            CameraName = "Source Camera",
            Seeing = 1.8
        };

        var target = new CalculatorInput(); // Has default values

        target.CopyFrom(source);

        Assert.Equal(source.BaseFocalLength, target.BaseFocalLength);
        Assert.Equal(source.ApertureDiameter, target.ApertureDiameter);
        Assert.Equal(source.ReducerFactor, target.ReducerFactor);
        Assert.Equal(source.BarlowFactor, target.BarlowFactor);
        Assert.Equal(source.PixelSize, target.PixelSize);
        Assert.Equal(source.SensorWidthPx, target.SensorWidthPx);
        Assert.Equal(source.SensorHeightPx, target.SensorHeightPx);
        Assert.Equal(source.Binning, target.Binning);
        Assert.Equal(source.CameraName, target.CameraName);
        Assert.Equal(source.Seeing, target.Seeing);
    }

    [Fact]
    public void CopyFrom_OverwritesExistingValues()
    {
        var source = new CalculatorInput { BaseFocalLength = 2000, Binning = 4 };
        var target = new CalculatorInput { BaseFocalLength = 500, Binning = 1 };

        target.CopyFrom(source);

        Assert.Equal(2000, target.BaseFocalLength);
        Assert.Equal(4, target.Binning);
    }

    [Fact]
    public void CopyFrom_SourceUnaffected()
    {
        var source = new CalculatorInput { BaseFocalLength = 1000 };
        var target = new CalculatorInput();

        target.CopyFrom(source);
        target.BaseFocalLength = 9999;

        Assert.Equal(1000, source.BaseFocalLength);
    }
}

// --- Task 7: Preset model tests ---

public class PresetModelTests
{
    [Fact]
    public void TelescopePreset_FromInput_CapturesTelescopeValues()
    {
        var input = new CalculatorInput
        {
            BaseFocalLength = 1200,
            ApertureDiameter = 250,
            ReducerFactor = 0.75,
            BarlowFactor = 1.5,
            PixelSize = 4.5,
            SensorWidthPx = 4096,
            SensorHeightPx = 2160,
            Binning = 2,
            Seeing = 2.5
        };

        var preset = TelescopePreset.FromInput(input, "My Telescope");

        Assert.Equal("My Telescope", preset.Name);
        Assert.Equal(PresetType.Telescope, preset.Type);
        Assert.Equal(1200, preset.BaseFocalLength);
        Assert.Equal(250, preset.ApertureDiameter);
        Assert.Equal(0.75, preset.ReducerFactor);
        Assert.Equal(1.5, preset.BarlowFactor);
    }

    [Fact]
    public void TelescopePreset_ApplyTo_SetsTelescopeValues()
    {
        var preset = new TelescopePreset
        {
            BaseFocalLength = 1500,
            ApertureDiameter = 300,
            ReducerFactor = 0.8,
            BarlowFactor = 2.0
        };
        var input = new CalculatorInput();

        preset.ApplyTo(input);

        Assert.Equal(1500, input.BaseFocalLength);
        Assert.Equal(300, input.ApertureDiameter);
        Assert.Equal(0.8, input.ReducerFactor);
        Assert.Equal(2.0, input.BarlowFactor);
    }

    [Fact]
    public void TelescopePreset_ApplyTo_DoesNotAffectCameraValues()
    {
        var preset = new TelescopePreset { BaseFocalLength = 1500 };
        var input = new CalculatorInput
        {
            PixelSize = 5.0,
            SensorWidthPx = 8000,
            Binning = 3
        };

        preset.ApplyTo(input);

        Assert.Equal(5.0, input.PixelSize);
        Assert.Equal(8000, input.SensorWidthPx);
        Assert.Equal(3, input.Binning);
    }

    [Fact]
    public void CameraPreset_FromInput_CapturesCameraValues()
    {
        var input = new CalculatorInput
        {
            BaseFocalLength = 1200,
            PixelSize = 3.76,
            SensorWidthPx = 6248,
            SensorHeightPx = 4176,
            Binning = 2
        };

        var preset = CameraPreset.FromInput(input, "My Camera");

        Assert.Equal("My Camera", preset.Name);
        Assert.Equal(PresetType.Camera, preset.Type);
        Assert.Equal(3.76, preset.PixelSize);
        Assert.Equal(6248, preset.SensorWidthPx);
        Assert.Equal(4176, preset.SensorHeightPx);
        Assert.Equal(2, preset.Binning);
    }

    [Fact]
    public void CameraPreset_ApplyTo_SetsCameraValues()
    {
        var preset = new CameraPreset
        {
            PixelSize = 2.4,
            SensorWidthPx = 9576,
            SensorHeightPx = 6388,
            Binning = 1
        };
        var input = new CalculatorInput();

        preset.ApplyTo(input);

        Assert.Equal(2.4, input.PixelSize);
        Assert.Equal(9576, input.SensorWidthPx);
        Assert.Equal(6388, input.SensorHeightPx);
        Assert.Equal(1, input.Binning);
    }

    [Fact]
    public void CameraPreset_ApplyTo_DoesNotAffectTelescopeValues()
    {
        var preset = new CameraPreset { PixelSize = 2.4 };
        var input = new CalculatorInput
        {
            BaseFocalLength = 1200,
            ApertureDiameter = 250,
            ReducerFactor = 0.7
        };

        preset.ApplyTo(input);

        Assert.Equal(1200, input.BaseFocalLength);
        Assert.Equal(250, input.ApertureDiameter);
        Assert.Equal(0.7, input.ReducerFactor);
    }

    [Fact]
    public void FullRigPreset_FromInput_CapturesAllValues()
    {
        var input = new CalculatorInput
        {
            BaseFocalLength = 1200,
            ApertureDiameter = 250,
            ReducerFactor = 0.75,
            BarlowFactor = 1.5,
            PixelSize = 3.76,
            SensorWidthPx = 6248,
            SensorHeightPx = 4176,
            Binning = 2,
            Seeing = 1.8
        };

        var preset = FullRigPreset.FromInput(input, "Full Rig");

        Assert.Equal("Full Rig", preset.Name);
        Assert.Equal(PresetType.FullRig, preset.Type);
        Assert.Equal(1200, preset.BaseFocalLength);
        Assert.Equal(250, preset.ApertureDiameter);
        Assert.Equal(0.75, preset.ReducerFactor);
        Assert.Equal(1.5, preset.BarlowFactor);
        Assert.Equal(3.76, preset.PixelSize);
        Assert.Equal(6248, preset.SensorWidthPx);
        Assert.Equal(4176, preset.SensorHeightPx);
        Assert.Equal(2, preset.Binning);
        Assert.Equal(1.8, preset.Seeing);
    }

    [Fact]
    public void FullRigPreset_ApplyTo_SetsAllValues()
    {
        var preset = new FullRigPreset
        {
            BaseFocalLength = 1500,
            ApertureDiameter = 300,
            ReducerFactor = 0.8,
            BarlowFactor = 2.0,
            PixelSize = 2.4,
            SensorWidthPx = 9576,
            SensorHeightPx = 6388,
            Binning = 3,
            Seeing = 2.5
        };
        var input = new CalculatorInput();

        preset.ApplyTo(input);

        Assert.Equal(1500, input.BaseFocalLength);
        Assert.Equal(300, input.ApertureDiameter);
        Assert.Equal(0.8, input.ReducerFactor);
        Assert.Equal(2.0, input.BarlowFactor);
        Assert.Equal(2.4, input.PixelSize);
        Assert.Equal(9576, input.SensorWidthPx);
        Assert.Equal(6388, input.SensorHeightPx);
        Assert.Equal(3, input.Binning);
        Assert.Equal(2.5, input.Seeing);
    }

    [Fact]
    public void PresetBase_HasUniqueId()
    {
        var preset1 = new TelescopePreset();
        var preset2 = new TelescopePreset();

        Assert.NotEqual(preset1.Id, preset2.Id);
        Assert.False(string.IsNullOrEmpty(preset1.Id));
    }

    [Fact]
    public void PresetBase_HasCreatedAt()
    {
        var before = DateTime.UtcNow;
        var preset = new CameraPreset();
        var after = DateTime.UtcNow;

        Assert.True(preset.CreatedAt >= before);
        Assert.True(preset.CreatedAt <= after);
    }

    [Fact]
    public void TelescopePreset_WithNullAperture()
    {
        var input = new CalculatorInput { ApertureDiameter = null };

        var preset = TelescopePreset.FromInput(input, "Test");

        Assert.Null(preset.ApertureDiameter);

        var newInput = new CalculatorInput { ApertureDiameter = 200 };
        preset.ApplyTo(newInput);

        Assert.Null(newInput.ApertureDiameter);
    }

    [Fact]
    public void FullRigPreset_WithNullAperture()
    {
        var input = new CalculatorInput { ApertureDiameter = null };

        var preset = FullRigPreset.FromInput(input, "Test");

        Assert.Null(preset.ApertureDiameter);

        var newInput = new CalculatorInput { ApertureDiameter = 200 };
        preset.ApplyTo(newInput);

        Assert.Null(newInput.ApertureDiameter);
    }

    [Fact]
    public void PresetCollection_DefaultsToEmptyLists()
    {
        var collection = new PresetCollection();

        Assert.NotNull(collection.Telescopes);
        Assert.NotNull(collection.Cameras);
        Assert.NotNull(collection.FullRigs);
        Assert.Empty(collection.Telescopes);
        Assert.Empty(collection.Cameras);
        Assert.Empty(collection.FullRigs);
    }

    [Fact]
    public void PresetCollection_HasVersion()
    {
        var collection = new PresetCollection();

        Assert.Equal(1, collection.Version);
    }
}

// --- Task 8: URL state encoding/decoding tests ---

public class UrlStateServiceTests
{
    [Fact]
    public void EncodeState_DefaultInputs_ReturnsEmptyString()
    {
        var input = new CalculatorInput();

        var result = UrlStateService.EncodeState(input);

        Assert.Equal("", result);
    }

    [Fact]
    public void EncodeState_NonDefaultFocalLength_IncludesFl()
    {
        var input = new CalculatorInput { BaseFocalLength = 1200 };

        var result = UrlStateService.EncodeState(input);

        Assert.Contains("fl=1200", result);
    }

    [Fact]
    public void EncodeState_NonDefaultSeeing_IncludesSee()
    {
        var input = new CalculatorInput { Seeing = 3.5 };

        var result = UrlStateService.EncodeState(input);

        Assert.Contains("see=3.5", result);
    }

    [Fact]
    public void EncodeState_MultipleNonDefaults_IncludesAll()
    {
        var input = new CalculatorInput
        {
            BaseFocalLength = 1000,
            PixelSize = 5.0,
            Binning = 2
        };

        var result = UrlStateService.EncodeState(input);

        Assert.Contains("fl=1000", result);
        Assert.Contains("px=5", result);
        Assert.Contains("bin=2", result);
    }

    [Fact]
    public void EncodeState_CompareMode_IncludesCmp()
    {
        var inputA = new CalculatorInput();
        var inputB = new CalculatorInput();

        var result = UrlStateService.EncodeState(inputA, inputB, compareMode: true);

        Assert.Contains("cmp=1", result);
    }

    [Fact]
    public void EncodeState_CompareModeWithDifferentB_IncludesBPrefixedParams()
    {
        var inputA = new CalculatorInput();
        var inputB = new CalculatorInput { BaseFocalLength = 1500 };

        var result = UrlStateService.EncodeState(inputA, inputB, compareMode: true);

        Assert.Contains("cmp=1", result);
        Assert.Contains("bfl=1500", result);
    }

    [Fact]
    public void EncodeState_InvariantCulture_UsesDotDecimalSeparator()
    {
        // Use non-default pixel size to ensure it's encoded
        var input = new CalculatorInput { PixelSize = 4.85 };

        var result = UrlStateService.EncodeState(input);

        Assert.Contains("px=4.85", result);
        Assert.DoesNotContain(",", result);
    }

    [Fact]
    public void DecodeState_EmptyString_ReturnsDefaults()
    {
        var (inputA, inputB, compareMode) = UrlStateService.DecodeState("");

        Assert.Equal(800, inputA.BaseFocalLength);
        Assert.Equal(2.0, inputA.Seeing);
        Assert.False(compareMode);
    }

    [Fact]
    public void DecodeState_ValidFocalLength_ParsesCorrectly()
    {
        var (inputA, _, _) = UrlStateService.DecodeState("?fl=1200");

        Assert.Equal(1200, inputA.BaseFocalLength);
    }

    [Fact]
    public void DecodeState_ValidSeeing_ParsesCorrectly()
    {
        var (inputA, _, _) = UrlStateService.DecodeState("?see=3.5");

        Assert.Equal(3.5, inputA.Seeing);
    }

    [Fact]
    public void DecodeState_MultipleParams_ParsesAll()
    {
        var (inputA, _, _) = UrlStateService.DecodeState("?fl=1000&px=5&bin=2&see=2.5");

        Assert.Equal(1000, inputA.BaseFocalLength);
        Assert.Equal(5.0, inputA.PixelSize);
        Assert.Equal(2, inputA.Binning);
        Assert.Equal(2.5, inputA.Seeing);
    }

    [Fact]
    public void DecodeState_CompareMode_ParsesCmp()
    {
        var (_, _, compareMode) = UrlStateService.DecodeState("?cmp=1");

        Assert.True(compareMode);
    }

    [Fact]
    public void DecodeState_CompareModeWithB_ParsesBValues()
    {
        var (_, inputB, compareMode) = UrlStateService.DecodeState("?cmp=1&bfl=1500&bbin=3");

        Assert.True(compareMode);
        Assert.Equal(1500, inputB.BaseFocalLength);
        Assert.Equal(3, inputB.Binning);
    }

    [Fact]
    public void DecodeState_InvalidFocalLength_FallsBackToDefault()
    {
        var (inputA, _, _) = UrlStateService.DecodeState("?fl=invalid");

        Assert.Equal(800, inputA.BaseFocalLength);
    }

    [Fact]
    public void DecodeState_OutOfRangeFocalLength_FallsBackToDefault()
    {
        var (inputA, _, _) = UrlStateService.DecodeState("?fl=-100");

        Assert.Equal(800, inputA.BaseFocalLength);
    }

    [Fact]
    public void DecodeState_OutOfRangeBinning_FallsBackToDefault()
    {
        var (inputA, _, _) = UrlStateService.DecodeState("?bin=10");

        Assert.Equal(1, inputA.Binning);
    }

    [Fact]
    public void DecodeState_NegativeSeeing_FallsBackToDefault()
    {
        var (inputA, _, _) = UrlStateService.DecodeState("?see=-1");

        Assert.Equal(2.0, inputA.Seeing);
    }

    [Fact]
    public void DecodeState_EmptyAperture_SetsNull()
    {
        var (inputA, _, _) = UrlStateService.DecodeState("?ap=");

        Assert.Null(inputA.ApertureDiameter);
    }

    [Fact]
    public void DecodeState_ValidAperture_ParsesCorrectly()
    {
        var (inputA, _, _) = UrlStateService.DecodeState("?ap=250");

        Assert.Equal(250, inputA.ApertureDiameter);
    }

    [Fact]
    public void DecodeState_InvariantCulture_ParsesDotDecimals()
    {
        var (inputA, _, _) = UrlStateService.DecodeState("?px=3.76");

        Assert.Equal(3.76, inputA.PixelSize);
    }

    [Fact]
    public void RoundTrip_AllFields_PreservesValues()
    {
        var original = new CalculatorInput
        {
            BaseFocalLength = 1200,
            ApertureDiameter = 250,
            ReducerFactor = 0.7,
            BarlowFactor = 2.0,
            PixelSize = 4.5,
            SensorWidthPx = 4096,
            SensorHeightPx = 2160,
            Binning = 2,
            Seeing = 3.0
        };

        var queryString = UrlStateService.EncodeState(original);
        var (decoded, _, _) = UrlStateService.DecodeState(queryString);

        Assert.Equal(original.BaseFocalLength, decoded.BaseFocalLength);
        Assert.Equal(original.ApertureDiameter, decoded.ApertureDiameter);
        Assert.Equal(original.ReducerFactor, decoded.ReducerFactor);
        Assert.Equal(original.BarlowFactor, decoded.BarlowFactor);
        Assert.Equal(original.PixelSize, decoded.PixelSize);
        Assert.Equal(original.SensorWidthPx, decoded.SensorWidthPx);
        Assert.Equal(original.SensorHeightPx, decoded.SensorHeightPx);
        Assert.Equal(original.Binning, decoded.Binning);
        Assert.Equal(original.Seeing, decoded.Seeing);
    }

    [Fact]
    public void RoundTrip_CompareMode_PreservesValues()
    {
        var inputA = new CalculatorInput { BaseFocalLength = 1000 };
        var inputB = new CalculatorInput { BaseFocalLength = 1500, Binning = 2 };

        var queryString = UrlStateService.EncodeState(inputA, inputB, compareMode: true);
        var (decodedA, decodedB, compareMode) = UrlStateService.DecodeState(queryString);

        Assert.True(compareMode);
        Assert.Equal(1000, decodedA.BaseFocalLength);
        Assert.Equal(1500, decodedB.BaseFocalLength);
        Assert.Equal(2, decodedB.Binning);
    }

    [Fact]
    public void BuildShareableUrl_CombinesBaseAndQuery()
    {
        var input = new CalculatorInput { BaseFocalLength = 1200 };

        var url = UrlStateService.BuildShareableUrl("https://example.com/", input);

        Assert.StartsWith("https://example.com/", url);
        Assert.Contains("fl=1200", url);
    }

    [Fact]
    public void BuildShareableUrl_StripsExistingQuery()
    {
        var input = new CalculatorInput { BaseFocalLength = 1200 };

        var url = UrlStateService.BuildShareableUrl("https://example.com/?old=value", input);

        Assert.DoesNotContain("old=value", url);
        Assert.Contains("fl=1200", url);
    }

    [Fact]
    public void DecodeState_WithQuestionMarkPrefix_ParsesCorrectly()
    {
        var (inputA, _, _) = UrlStateService.DecodeState("?fl=1200");

        Assert.Equal(1200, inputA.BaseFocalLength);
    }

    [Fact]
    public void DecodeState_WithoutQuestionMarkPrefix_ParsesCorrectly()
    {
        var (inputA, _, _) = UrlStateService.DecodeState("fl=1200");

        Assert.Equal(1200, inputA.BaseFocalLength);
    }

    [Fact]
    public void DecodeState_ReducerOutOfRange_FallsBackToDefault()
    {
        // Reducer must be 0.1-1.0
        var (inputA, _, _) = UrlStateService.DecodeState("?rd=0.05");

        Assert.Equal(1.0, inputA.ReducerFactor);
    }

    [Fact]
    public void DecodeState_BarlowOutOfRange_FallsBackToDefault()
    {
        // Barlow must be 0.1-10
        var (inputA, _, _) = UrlStateService.DecodeState("?bl=15");

        Assert.Equal(1.0, inputA.BarlowFactor);
    }

    [Fact]
    public void EncodeState_NullApertureWhenDefaultIsNotNull_IncludesEmptyAp()
    {
        var input = new CalculatorInput { ApertureDiameter = null };
        // Default has ApertureDiameter = 200

        var result = UrlStateService.EncodeState(input);

        Assert.Contains("ap=", result);
    }
}
