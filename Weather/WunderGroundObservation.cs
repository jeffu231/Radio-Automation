using System;
using Newtonsoft.Json;

namespace Weather
{
    public class WunderGroundObservation : IObservation
    {
	    public WunderGroundObservation()
	    {
		    Imperial = new Imperial();
	    }

	    [JsonProperty("imperial")]
		internal Imperial Imperial { get; set; }

	    [JsonProperty("stationID")]
	    public string StationId { get; set; }

	    [JsonProperty("obsTimeUtc")]
	    public DateTime ObservationTime { get; set; }

	    [JsonProperty("neighborhood")]
	    public string Name { get; set; }

	    [JsonProperty("country")]
	    public string Country { get; set; }

	    [JsonProperty("humidity")]
	    public double Humidity { get; set; }

	    [JsonProperty("winddir")]
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
	    [JsonProperty("temp")]
	    public double Temp { get; set; }

	    [JsonProperty("dewpt")]
	    public double DewPoint { get; set; }
	    
	    [JsonProperty("heatIndex")]
	    public double HeatIndex { get; set; }

	    [JsonProperty("windChill")]
	    public double WindChill { get; set; }

	    [JsonProperty("windSpeed")]
	    public double WindSpeed { get; set; }

	    [JsonProperty("windGust")]
	    public double WindGust { get; set; }

	    [JsonProperty("pressure")]
	    public double Pressure { get; set; }

	    [JsonProperty("precipRate")]
	    public double PrecipRate { get; set; }

	    [JsonProperty("precipTotal")]
	    public double PrecipTotal { get; set; }
    }
}
