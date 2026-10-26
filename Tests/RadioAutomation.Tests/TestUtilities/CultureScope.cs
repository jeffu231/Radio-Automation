using System;
using System.Globalization;

namespace RadioAutomation.Tests.TestUtilities
{
	/// <summary>
	/// Sets the current thread's culture for the lifetime of the scope and restores it on dispose.
	/// The app targets en-US, so tests that depend on culture pin it explicitly.
	/// </summary>
	public sealed class CultureScope : IDisposable
	{
		private readonly CultureInfo _previousCulture = CultureInfo.CurrentCulture;
		private readonly CultureInfo _previousUiCulture = CultureInfo.CurrentUICulture;

		public CultureScope(string name)
		{
			var culture = CultureInfo.GetCultureInfo(name);
			CultureInfo.CurrentCulture = culture;
			CultureInfo.CurrentUICulture = culture;
		}

		public void Dispose()
		{
			CultureInfo.CurrentCulture = _previousCulture;
			CultureInfo.CurrentUICulture = _previousUiCulture;
		}
	}
}
