using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Weather
{
	public class WunderGround
	{
		private static readonly HttpClient _client = new HttpClient();
		private static readonly string _url = "https://api.weather.com/v2/pws/observations/current";

		static WunderGround()
		{
			_client.DefaultRequestHeaders.Accept.Clear();
			_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		}

		public async Task<IObservation> GetObservationAsync(string stationId, string apiKey, bool highPrecision = false)
		{
			if (!string.IsNullOrEmpty(stationId) && !string.IsNullOrEmpty(apiKey))
			{
				WundergroundObservations obs = null;
				var precision = highPrecision ? "&numericPrecision=decimal" : String.Empty;
				var request = $"{_url}?stationId={stationId}&format=json&units=e&apiKey={apiKey}{precision}";
				HttpResponseMessage response = await _client.GetAsync(request);
				if (response.IsSuccessStatusCode)
				{
					obs = await response.Content.ReadAsAsync<WundergroundObservations>();
				}

				if (obs != null && obs.Observations.Any())
				{
					return obs.Observations.First();
				}
			}

			return new WunderGroundObservation();
		}
	}
}
