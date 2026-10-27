using Radio_Automation.ViewModels;
using RadioAutomation.Tests.TestUtilities;
using Xunit;

namespace RadioAutomation.Tests.ViewModels
{
	public sealed class TemperatureFileNameTests
	{
		// The names must match the bumper files on disk (TMP072.mp3, TMPN-005.mp3), so the app targets en-US here.
		[Theory]
		[InlineData(72, "TMP072.mp3")]
		[InlineData(5, "TMP005.mp3")]
		[InlineData(0, "TMP000.mp3")]
		[InlineData(100, "TMP100.mp3")]
		[InlineData(-5, "TMPN-005.mp3")]
		[InlineData(-25, "TMPN-025.mp3")]
		public void GetTemperatureFileName_EnUs_MatchesBumperFileNames(int temperature, string expected)
		{
			using var _ = new CultureScope("en-US");

			Assert.Equal(expected, MainWindowViewModel.GetTemperatureFileName(temperature));
		}

		// Not required (en-US is the target), but cheap: sv-SE writes the minus sign as U+2212.
		[Fact]
		public void GetTemperatureFileName_SvSe_StillUsesAsciiMinus()
		{
			using var _ = new CultureScope("sv-SE");

			Assert.Equal("TMPN-005.mp3", MainWindowViewModel.GetTemperatureFileName(-5));
		}
	}
}
