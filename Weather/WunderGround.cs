using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Weather
{
	public class WunderGround
	{
		private readonly string _apiKey;
		private static HttpClient _client = new HttpClient();
		private static string _url = "https://api.weather.com/v2/pws/observations/current";

		static WunderGround()
		{
			_client.DefaultRequestHeaders.Accept.Clear();
			_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		}

		public WunderGround(string apiKey)
		{
			if (string.IsNullOrEmpty(apiKey))
			{
				throw new ArgumentNullException(nameof(apiKey));
			}
			_apiKey = apiKey;
		}

		public async Task<IObservation> GetObservationAsync(string stationId, bool highPrecision = false)
		{
			WundergroundObservations obs = null;
			var precision = highPrecision ? "&numericPrecision=decimal" : String.Empty;
			var request = $"{_url}?stationId={stationId}&format=json&units=e&apiKey={_apiKey}{precision}";
			HttpResponseMessage response = await _client.GetAsync(request);
			if (response.IsSuccessStatusCode)
			{
				obs = await response.Content.ReadAsAsync<WundergroundObservations>();
			}

			if (obs != null && obs.Observations.Any())
			{
				return obs.Observations.First();
			}

			return new WunderGroundObservation();
		}
	}
}
