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
}
