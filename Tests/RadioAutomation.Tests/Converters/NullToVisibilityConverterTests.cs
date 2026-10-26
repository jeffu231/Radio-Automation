using Radio_Automation.Converters;
using System;
using System.Globalization;
using System.Windows;
using Xunit;

namespace RadioAutomation.Tests.Converters
{
	public sealed class NullToVisibilityConverterTests
	{
		private readonly NullToVisibilityConverter _converter = new NullToVisibilityConverter();

		[Fact]
		public void Convert_NullValue_ReturnsVisible()
		{
			var result = _converter.Convert(null, typeof(Visibility), null, CultureInfo.InvariantCulture);

			Assert.Equal(Visibility.Visible, result);
		}

		[Fact]
		public void Convert_NonNullValue_ReturnsCollapsed()
		{
			var result = _converter.Convert(new object(), typeof(Visibility), null, CultureInfo.InvariantCulture);

			Assert.Equal(Visibility.Collapsed, result);
		}

		[Fact]
		public void ConvertBack_Always_ThrowsNotImplemented()
		{
			Assert.Throws<NotImplementedException>(() => _converter.ConvertBack(Visibility.Visible, typeof(object), null, CultureInfo.InvariantCulture));
		}
	}
}
