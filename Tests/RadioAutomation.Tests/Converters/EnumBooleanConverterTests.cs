using Radio_Automation.Converters;
using Radio_Automation.Models;
using System;
using System.Globalization;
using DependencyProperty = System.Windows.DependencyProperty;
using Xunit;

namespace RadioAutomation.Tests.Converters
{
	public sealed class EnumBooleanConverterTests
	{
		private readonly EnumBooleanConverter _converter = new EnumBooleanConverter();

		[Fact]
		public void Convert_ValueMatchesParameter_ReturnsTrue()
		{
			var result = _converter.Convert(Trigger.Mqtt, typeof(bool), "Mqtt", CultureInfo.InvariantCulture);

			Assert.Equal(true, result);
		}

		[Fact]
		public void Convert_ValueDiffersFromParameter_ReturnsFalse()
		{
			var result = _converter.Convert(Trigger.Cron, typeof(bool), "Mqtt", CultureInfo.InvariantCulture);

			Assert.Equal(false, result);
		}

		[Fact]
		public void Convert_NullParameter_ReturnsUnsetValue()
		{
			var result = _converter.Convert(Trigger.Cron, typeof(bool), null, CultureInfo.InvariantCulture);

			Assert.Same(DependencyProperty.UnsetValue, result);
		}

		[Fact]
		public void Convert_UndefinedEnumValue_ReturnsUnsetValue()
		{
			var result = _converter.Convert((Trigger)99, typeof(bool), "Cron", CultureInfo.InvariantCulture);

			Assert.Same(DependencyProperty.UnsetValue, result);
		}

		[Fact]
		public void Convert_ParameterNotAnEnumName_ThrowsArgumentException()
		{
			Assert.Throws<ArgumentException>(() => _converter.Convert(Trigger.Cron, typeof(bool), "Bogus", CultureInfo.InvariantCulture));
		}

		[Fact]
		public void ConvertBack_ValidParameter_ReturnsParsedEnum()
		{
			var result = _converter.ConvertBack(true, typeof(Trigger), "Mqtt", CultureInfo.InvariantCulture);

			Assert.Equal(Trigger.Mqtt, result);
		}

		[Fact]
		public void ConvertBack_NullParameter_ReturnsUnsetValue()
		{
			var result = _converter.ConvertBack(true, typeof(Trigger), null, CultureInfo.InvariantCulture);

			Assert.Same(DependencyProperty.UnsetValue, result);
		}
	}
}
