using System.Globalization;
using System.Web;
using Microsoft.AspNetCore.Components;
using SamplingCalculator.Models;

namespace SamplingCalculator.Services;

/// <summary>
/// Service for encoding/decoding calculator state to/from URL query strings.
/// </summary>
public static class UrlStateService
{
    // Short parameter keys for compact URLs
    private const string KeyFocalLength = "fl";
    private const string KeyAperture = "ap";
    private const string KeyReducer = "rd";
    private const string KeyBarlow = "bl";
    private const string KeyPixelSize = "px";
    private const string KeySensorWidth = "sw";
    private const string KeySensorHeight = "sh";
    private const string KeyBinning = "bin";
    private const string KeySeeing = "see";
    private const string KeyCompare = "cmp";

    // Setup B uses same keys with "b" prefix
    private const string PrefixB = "b";

    /// <summary>
    /// Encodes the calculator state to a query string.
    /// </summary>
    public static string EncodeState(CalculatorInput inputA, CalculatorInput? inputB = null, bool compareMode = false)
    {
        var parameters = new List<string>();
        var defaults = new CalculatorInput();

        // Only include non-default values for compact URLs
        AddIfNotDefault(parameters, KeyFocalLength, inputA.BaseFocalLength, defaults.BaseFocalLength);
        AddIfNotDefault(parameters, KeyAperture, inputA.ApertureDiameter, defaults.ApertureDiameter);
        AddIfNotDefault(parameters, KeyReducer, inputA.ReducerFactor, defaults.ReducerFactor);
        AddIfNotDefault(parameters, KeyBarlow, inputA.BarlowFactor, defaults.BarlowFactor);
        AddIfNotDefault(parameters, KeyPixelSize, inputA.PixelSize, defaults.PixelSize);
        AddIfNotDefault(parameters, KeySensorWidth, inputA.SensorWidthPx, defaults.SensorWidthPx);
        AddIfNotDefault(parameters, KeySensorHeight, inputA.SensorHeightPx, defaults.SensorHeightPx);
        AddIfNotDefault(parameters, KeyBinning, inputA.Binning, defaults.Binning);
        AddIfNotDefault(parameters, KeySeeing, inputA.Seeing, defaults.Seeing);

        if (compareMode)
        {
            parameters.Add($"{KeyCompare}=1");

            if (inputB != null)
            {
                AddIfNotDefault(parameters, PrefixB + KeyFocalLength, inputB.BaseFocalLength, defaults.BaseFocalLength);
                AddIfNotDefault(parameters, PrefixB + KeyAperture, inputB.ApertureDiameter, defaults.ApertureDiameter);
                AddIfNotDefault(parameters, PrefixB + KeyReducer, inputB.ReducerFactor, defaults.ReducerFactor);
                AddIfNotDefault(parameters, PrefixB + KeyBarlow, inputB.BarlowFactor, defaults.BarlowFactor);
                AddIfNotDefault(parameters, PrefixB + KeyPixelSize, inputB.PixelSize, defaults.PixelSize);
                AddIfNotDefault(parameters, PrefixB + KeySensorWidth, inputB.SensorWidthPx, defaults.SensorWidthPx);
                AddIfNotDefault(parameters, PrefixB + KeySensorHeight, inputB.SensorHeightPx, defaults.SensorHeightPx);
                AddIfNotDefault(parameters, PrefixB + KeyBinning, inputB.Binning, defaults.Binning);
                AddIfNotDefault(parameters, PrefixB + KeySeeing, inputB.Seeing, defaults.Seeing);
            }
        }

        return parameters.Count > 0 ? "?" + string.Join("&", parameters) : "";
    }

    /// <summary>
    /// Decodes a query string into calculator state. Invalid parameters fall back to defaults.
    /// </summary>
    public static (CalculatorInput inputA, CalculatorInput inputB, bool compareMode) DecodeState(string queryString)
    {
        var inputA = new CalculatorInput();
        var inputB = new CalculatorInput();
        var compareMode = false;

        if (string.IsNullOrWhiteSpace(queryString))
        {
            return (inputA, inputB, compareMode);
        }

        var query = queryString.TrimStart('?');
        var parameters = HttpUtility.ParseQueryString(query);

        // Parse Setup A
        inputA.BaseFocalLength = ParseDouble(parameters[KeyFocalLength], inputA.BaseFocalLength, 1, 100000);
        inputA.ApertureDiameter = ParseNullableDouble(parameters[KeyAperture], inputA.ApertureDiameter, 1, 10000);
        inputA.ReducerFactor = ParseDouble(parameters[KeyReducer], inputA.ReducerFactor, 0.1, 10);
        inputA.BarlowFactor = ParseDouble(parameters[KeyBarlow], inputA.BarlowFactor, 0.1, 10);
        inputA.PixelSize = ParseDouble(parameters[KeyPixelSize], inputA.PixelSize, 0.1, 100);
        inputA.SensorWidthPx = ParseInt(parameters[KeySensorWidth], inputA.SensorWidthPx, 1, 100000);
        inputA.SensorHeightPx = ParseInt(parameters[KeySensorHeight], inputA.SensorHeightPx, 1, 100000);
        inputA.Binning = ParseInt(parameters[KeyBinning], inputA.Binning, 1, 4);
        inputA.Seeing = ParseDouble(parameters[KeySeeing], inputA.Seeing, 0.1, 20);

        // Parse compare mode
        compareMode = parameters[KeyCompare] == "1";

        if (compareMode)
        {
            // Parse Setup B (start with defaults, not copy of A)
            inputB.BaseFocalLength = ParseDouble(parameters[PrefixB + KeyFocalLength], inputB.BaseFocalLength, 1, 100000);
            inputB.ApertureDiameter = ParseNullableDouble(parameters[PrefixB + KeyAperture], inputB.ApertureDiameter, 1, 10000);
            inputB.ReducerFactor = ParseDouble(parameters[PrefixB + KeyReducer], inputB.ReducerFactor, 0.1, 10);
            inputB.BarlowFactor = ParseDouble(parameters[PrefixB + KeyBarlow], inputB.BarlowFactor, 0.1, 10);
            inputB.PixelSize = ParseDouble(parameters[PrefixB + KeyPixelSize], inputB.PixelSize, 0.1, 100);
            inputB.SensorWidthPx = ParseInt(parameters[PrefixB + KeySensorWidth], inputB.SensorWidthPx, 1, 100000);
            inputB.SensorHeightPx = ParseInt(parameters[PrefixB + KeySensorHeight], inputB.SensorHeightPx, 1, 100000);
            inputB.Binning = ParseInt(parameters[PrefixB + KeyBinning], inputB.Binning, 1, 4);
            inputB.Seeing = ParseDouble(parameters[PrefixB + KeySeeing], inputB.Seeing, 0.1, 20);
        }

        return (inputA, inputB, compareMode);
    }

    /// <summary>
    /// Builds a full shareable URL from the current location and state.
    /// </summary>
    public static string BuildShareableUrl(string baseUrl, CalculatorInput inputA, CalculatorInput? inputB = null, bool compareMode = false)
    {
        var uri = new Uri(baseUrl);
        var basePath = uri.GetLeftPart(UriPartial.Path);
        return basePath + EncodeState(inputA, inputB, compareMode);
    }

    private static void AddIfNotDefault(List<string> parameters, string key, double value, double defaultValue)
    {
        if (Math.Abs(value - defaultValue) > 0.0001)
        {
            parameters.Add($"{key}={value.ToString(CultureInfo.InvariantCulture)}");
        }
    }

    private static void AddIfNotDefault(List<string> parameters, string key, double? value, double? defaultValue)
    {
        if (value != defaultValue)
        {
            if (value.HasValue)
            {
                parameters.Add($"{key}={value.Value.ToString(CultureInfo.InvariantCulture)}");
            }
            else
            {
                parameters.Add($"{key}=");
            }
        }
    }

    private static void AddIfNotDefault(List<string> parameters, string key, int value, int defaultValue)
    {
        if (value != defaultValue)
        {
            parameters.Add($"{key}={value.ToString(CultureInfo.InvariantCulture)}");
        }
    }

    private static double ParseDouble(string? value, double defaultValue, double min, double max)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
        {
            if (result >= min && result <= max)
            {
                return result;
            }
        }

        return defaultValue;
    }

    private static double? ParseNullableDouble(string? value, double? defaultValue, double min, double max)
    {
        if (value == null)
        {
            return defaultValue;
        }

        if (value == "")
        {
            return null;
        }

        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
        {
            if (result >= min && result <= max)
            {
                return result;
            }
        }

        return defaultValue;
    }

    private static int ParseInt(string? value, int defaultValue, int min, int max)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
        {
            if (result >= min && result <= max)
            {
                return result;
            }
        }

        return defaultValue;
    }
}
