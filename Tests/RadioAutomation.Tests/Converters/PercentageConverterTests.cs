using Radio_Automation.Converters;
using System;
using System.Globalization;
using Xunit;

namespace RadioAutomation.Tests.Converters
{
	public sealed class PercentageConverterTests
	{
		private readonly PercentageConverter _converter = new PercentageConverter();

		[Theory]
		[InlineData(200.0, 0.5, 100)]
		[InlineData(10.0, 0.999, 9)]
		[InlineData(0.0, 0.75, 0)]
		[InlineData(100, 1.5, 150)]
		public void Convert_MultipliesAndTruncates(object value, object parameter, int expected)
		{
			var result = _converter.Convert(value, typeof(int), parameter, CultureInfo.InvariantCulture);

			Assert.IsType<int>(result);
			Assert.Equal(expected, result);
		}

		[Fact]
		public void ConvertBack_Always_ThrowsNotImplemented()
		{
			Assert.Throws<NotImplementedException>(() => _converter.ConvertBack(1, typeof(int), 1, CultureInfo.InvariantCulture));
		}
	}
}
