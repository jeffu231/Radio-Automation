using System.Collections.Generic;

namespace Weather
{
	public class WundergroundObservations
	{
		public WundergroundObservations()
		{
			Observations = new List<WunderGroundObservation>();
		}
		public List<WunderGroundObservation> Observations { get; set; }
	}
}
