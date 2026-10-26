using System;
using System.Globalization;
using System.Windows.Data;

namespace Radio_Automation.Converters
{
	public class PercentageConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			// The operands are machine values (ActualWidth and a XAML literal such as ".25"), so parse them
			// culture-independently; the result is identical under en-US.
			return (int)(System.Convert.ToDouble(value, CultureInfo.InvariantCulture) * System.Convert.ToDouble(parameter, CultureInfo.InvariantCulture));
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
