namespace SamplingCalculator.Services;

public class ValidationResult
{
    public bool IsValid { get; set; } = true;
    public string? ErrorMessage { get; set; }

    public static ValidationResult Valid() => new() { IsValid = true };
    public static ValidationResult Error(string message) => new() { IsValid = false, ErrorMessage = message };
}

public static class InputValidationService
{
    public static ValidationResult ValidateFocalLength(double value)
    {
        if (value <= 0)
            return ValidationResult.Error("Must be greater than 0.");
        if (value > 50000)
            return ValidationResult.Error("Value seems unreasonably large (max 50 000 mm).");
        return ValidationResult.Valid();
    }

    public static ValidationResult ValidateAperture(double? value)
    {
        if (!value.HasValue)
            return ValidationResult.Valid(); // optional field
        if (value.Value <= 0)
            return ValidationResult.Error("Must be greater than 0.");
        if (value.Value > 10000)
            return ValidationResult.Error("Value seems unreasonably large (max 10 000 mm).");
        return ValidationResult.Valid();
    }

    public static ValidationResult ValidateReducerFactor(double value)
    {
        if (value < 0.1)
            return ValidationResult.Error("Minimum is 0.1×.");
        if (value > 1.0)
            return ValidationResult.Error("Reducer factor must be ≤ 1.0× (use Barlow for magnification).");
        return ValidationResult.Valid();
    }

    public static ValidationResult ValidateBarlowFactor(double value)
    {
        if (value < 1.0)
            return ValidationResult.Error("Barlow factor must be ≥ 1.0× (use Reducer for reduction).");
        if (value > 5.0)
            return ValidationResult.Error("Maximum is 5.0×.");
        return ValidationResult.Valid();
    }

    public static ValidationResult ValidatePixelSize(double value)
    {
        if (value <= 0)
            return ValidationResult.Error("Must be greater than 0.");
        if (value > 50)
            return ValidationResult.Error("Value seems unreasonably large (max 50 µm).");
        return ValidationResult.Valid();
    }

    public static ValidationResult ValidateSensorDimension(int value)
    {
        if (value <= 0)
            return ValidationResult.Error("Must be greater than 0.");
        if (value > 100000)
            return ValidationResult.Error("Value seems unreasonably large.");
        return ValidationResult.Valid();
    }

    public static ValidationResult ValidateSeeing(double value)
    {
        if (value <= 0)
            return ValidationResult.Error("Must be greater than 0.");
        if (value > 20)
            return ValidationResult.Error("Value seems unreasonably large (max 20″).");
        return ValidationResult.Valid();
    }
}
