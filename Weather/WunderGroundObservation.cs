using System;
using System.Text.Json.Serialization;

namespace Weather
{
    public class WunderGroundObservation : IObservation
    {
	    public WunderGroundObservation()
	    {
		    Imperial = new Imperial();
	    }

	    [JsonInclude]
		[JsonPropertyName("imperial")]
		internal Imperial Imperial { get; set; }

	    [JsonPropertyName("stationID")]
	    public string StationId { get; set; }

	    [JsonPropertyName("obsTimeUtc")]
	    public DateTime ObservationTime { get; set; }

	    [JsonPropertyName("neighborhood")]
	    public string Name { get; set; }

	    [JsonPropertyName("country")]
	    public string Country { get; set; }

	    [JsonPropertyName("humidity")]
	    public double Humidity { get; set; }

	    [JsonPropertyName("winddir")]
	    public uint WindDirection { get; set; }

	    /// <inheritdoc />
	    public double Temp => Imperial.Temp;

	    /// <inheritdoc />
	    public double DewPoint => Imperial.DewPoint;

	    /// <inheritdoc />
	    public double HeatIndex => Imperial.HeatIndex;

	    /// <inheritdoc />
	    public double WindChill => Imperial.WindChill;

	    /// <inheritdoc />
	    public double WindSpeed => Imperial.WindSpeed;

	    /// <inheritdoc />
	    public double WindGust => Imperial.WindGust;

	    /// <inheritdoc />
	    public double Pressure => Imperial.Pressure;

	    /// <inheritdoc />
	    public double PrecipRate => Imperial.PrecipRate;

	    /// <inheritdoc />
	    public double PrecipTotal => Imperial.PrecipTotal;
    }

    class Imperial
    {
	    [JsonPropertyName("temp")]
	    public double Temp { get; set; }

	    [JsonPropertyName("dewpt")]
	    public double DewPoint { get; set; }
	    
	    [JsonPropertyName("heatIndex")]
	    public double HeatIndex { get; set; }

	    [JsonPropertyName("windChill")]
	    public double WindChill { get; set; }

	    [JsonPropertyName("windSpeed")]
	    public double WindSpeed { get; set; }

	    [JsonPropertyName("windGust")]
	    public double WindGust { get; set; }

	    [JsonPropertyName("pressure")]
	    public double Pressure { get; set; }

	    [JsonPropertyName("precipRate")]
	    public double PrecipRate { get; set; }

	    [JsonPropertyName("precipTotal")]
	    public double PrecipTotal { get; set; }
    }
}
