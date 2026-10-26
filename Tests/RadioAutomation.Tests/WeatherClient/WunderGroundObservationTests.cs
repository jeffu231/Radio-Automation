using System;
using System.Text.Json;
using Weather;
using Xunit;

namespace RadioAutomation.Tests.WeatherClient
{
	public sealed class WunderGroundObservationTests
	{
		private const string SamplePayload = "{\"observations\":[{\"stationID\":\"KTEST123\",\"obsTimeUtc\":\"2026-10-26T14:00:00Z\",\"neighborhood\":\"Test Hill\",\"country\":\"US\",\"humidity\":65,\"winddir\":270,\"imperial\":{\"temp\":72.5,\"dewpt\":60.1,\"heatIndex\":73.0,\"windChill\":72.0,\"windSpeed\":5.0,\"windGust\":8.0,\"pressure\":30.01,\"precipRate\":0.0,\"precipTotal\":0.12}}]}";

		private static readonly JsonSerializerOptions WebOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

		private static WundergroundObservations Deserialize(string json)
		{
			return JsonSerializer.Deserialize<WundergroundObservations>(json, WebOptions);
		}

		[Fact]
		public void Deserialize_SamplePayload_MapsTopLevelFields()
		{
			var observation = Assert.Single(Deserialize(SamplePayload).Observations);

			Assert.Equal("KTEST123", observation.StationId);
			Assert.Equal("Test Hill", observation.Name);
			Assert.Equal("US", observation.Country);
			Assert.Equal(65, observation.Humidity);
			Assert.Equal(270u, observation.WindDirection);
			Assert.Equal(new DateTime(2026, 10, 26, 14, 0, 0, DateTimeKind.Utc), observation.ObservationTime.ToUniversalTime());
		}

		[Fact]
		public void Deserialize_SamplePayload_MapsImperialFields()
		{
			var observation = Assert.Single(Deserialize(SamplePayload).Observations);

			Assert.Equal(72.5, observation.Temp, precision: 3);
			Assert.Equal(60.1, observation.DewPoint, precision: 3);
			Assert.Equal(73.0, observation.HeatIndex, precision: 3);
			Assert.Equal(5.0, observation.WindSpeed, precision: 3);
			Assert.Equal(8.0, observation.WindGust, precision: 3);
			Assert.Equal(30.01, observation.Pressure, precision: 3);
			Assert.Equal(0.12, observation.PrecipTotal, precision: 3);
		}

		[Fact]
		public void Ctor_Default_ImperialValuesAreZero()
		{
			Assert.Equal(0, new WunderGroundObservation().Temp);
		}

		[Fact]
		public void Deserialize_EmptyObservations_YieldsEmptyList()
		{
			Assert.Empty(Deserialize("{\"observations\":[]}").Observations);
		}
	}
}
