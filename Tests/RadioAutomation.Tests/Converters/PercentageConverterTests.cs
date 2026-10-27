using Radio_Automation.Converters;
using RadioAutomation.Tests.TestUtilities;
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

		// The app targets en-US: these pin the strings a XAML binding can supply (ConverterParameter=.25).
		[Theory]
		[InlineData(200.0, ".25", 50)]
		[InlineData(200.0, "0.5", 100)]
		[InlineData("200", "0.5", 100)]
		[InlineData("1,000", ".5", 500)]
		[InlineData(400, ".12", 48)]
		public void Convert_EnUs_ParsesStringInputs(object value, object parameter, int expected)
		{
			using var _ = new CultureScope("en-US");

			var result = _converter.Convert(value, typeof(int), parameter, CultureInfo.CurrentCulture);

			Assert.Equal(expected, result);
		}

		// Not required (en-US is the target), but cheap to support: a XAML literal means the same in every culture.
		[Fact]
		public void Convert_DeDe_XamlLiteralParameterStillMeansQuarter()
		{
			using var _ = new CultureScope("de-DE");

			var result = _converter.Convert(200.0, typeof(int), ".25", CultureInfo.CurrentCulture);

			Assert.Equal(50, result);
		}

		[Theory]
		[InlineData(null, "0.5")]
		[InlineData(200.0, null)]
		public void Convert_EnUs_NullOperandCountsAsZero(object value, object parameter)
		{
			using var _ = new CultureScope("en-US");

			var result = _converter.Convert(value, typeof(int), parameter, CultureInfo.CurrentCulture);

			Assert.Equal(0, result);
		}

		[Fact]
		public void Convert_EnUs_NonNumericString_ThrowsFormatException()
		{
			using var _ = new CultureScope("en-US");

			Assert.Throws<FormatException>(() => _converter.Convert("abc", typeof(int), "0.5", CultureInfo.CurrentCulture));
			Assert.Throws<FormatException>(() => _converter.Convert(200.0, typeof(int), "abc", CultureInfo.CurrentCulture));
		}

		[Fact]
		public void ConvertBack_Always_ThrowsNotImplemented()
		{
			Assert.Throws<NotImplementedException>(() => _converter.ConvertBack(1, typeof(int), 1, CultureInfo.InvariantCulture));
		}
	}
}
