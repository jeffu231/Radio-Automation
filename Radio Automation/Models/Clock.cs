using System;
using System.Timers;
using Catel.Data;

namespace Radio_Automation.Models
{
	public class Clock:ModelBase
	{
		private readonly Timer _clockTimer;

		public Clock()
		{
			UpdateTime();
			_clockTimer = new Timer(1000);
			_clockTimer.Elapsed += _clockTimer_Elapsed;
			_clockTimer.Start();
		}

		private void _clockTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			UpdateTime();
		}

		#region CurrentDateTime property

		/// <summary>
		/// Gets or sets the DateTime value.
		/// </summary>
		public DateTime CurrentDateTime
		{
			get { return GetValue<DateTime>(CurrentDateTimeProperty); }
			set { SetValue(CurrentDateTimeProperty, value); }
		}

		/// <summary>
		/// DateTime property data.
		/// </summary>
		public static readonly PropertyData CurrentDateTimeProperty = RegisterProperty("CurrentDateTime", typeof(DateTime));

		#endregion

		private void UpdateTime()
		{
			CurrentDateTime = DateTime.Now;
		}

	}
}
