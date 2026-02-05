using SamplingCalculator.Models;
using Xunit;

namespace SamplingCalculator.Tests;

public class CalculatorInputTests
{
    [Fact]
    public void Equals_SameValues_ReturnsTrue()
    {
        var input1 = new CalculatorInput
        {
            BaseFocalLength = 800,
            PixelSize = 3.76
        };
        var input2 = new CalculatorInput
        {
            BaseFocalLength = 800,
            PixelSize = 3.76
        };

        Assert.True(input1.Equals(input2));
        Assert.True(input1.Equals((object)input2));
        Assert.Equal(input1.GetHashCode(), input2.GetHashCode());
    }

    [Fact]
    public void Equals_DifferentValues_ReturnsFalse()
    {
        var input1 = new CalculatorInput { BaseFocalLength = 800 };
        var input2 = new CalculatorInput { BaseFocalLength = 801 };

        Assert.False(input1.Equals(input2));
    }

    [Fact]
    public void Equals_DifferentValues_PixelSize_ReturnsFalse()
    {
        var input1 = new CalculatorInput { PixelSize = 3.76 };
        var input2 = new CalculatorInput { PixelSize = 3.77 };

        Assert.False(input1.Equals(input2));
    }

    [Fact]
    public void Equals_DifferentValues_Binning_ReturnsFalse()
    {
        var input1 = new CalculatorInput { Binning = 1 };
        var input2 = new CalculatorInput { Binning = 2 };

        Assert.False(input1.Equals(input2));
    }

    [Fact]
    public void Clone_CreatesEqualObject()
    {
        var input = new CalculatorInput
        {
            BaseFocalLength = 1200,
            PixelSize = 2.4,
            Seeing = 1.5,
            Binning = 2,
            CameraName = "Test Camera"
        };

        var clone = input.Clone();

        Assert.True(input.Equals(clone));
        Assert.NotSame(input, clone); // Ensure it's a new instance
    }
}
