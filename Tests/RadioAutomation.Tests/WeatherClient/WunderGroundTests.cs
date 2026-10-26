using System.Threading.Tasks;
using Weather;
using Xunit;

namespace RadioAutomation.Tests.WeatherClient
{
	public sealed class WunderGroundTests
	{
		[Theory]
		[InlineData(null, "key")]
		[InlineData("", "key")]
		[InlineData("STATION", null)]
		[InlineData("STATION", "")]
		public async Task GetObservationAsync_MissingCredentials_ReturnsEmptyObservation(string stationId, string apiKey)
		{
			var observation = await new WunderGround().GetObservationAsync(stationId, apiKey);

			Assert.NotNull(observation);
			Assert.Equal(0, observation.Temp);
			Assert.Null(Assert.IsType<WunderGroundObservation>(observation).StationId);
		}
	}
}
