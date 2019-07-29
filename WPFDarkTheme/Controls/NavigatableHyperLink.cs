using System.Diagnostics;
using System.Windows.Documents;

namespace WPFDarkTheme.Controls
{
	public class NavigatableHyperLink: Hyperlink
	{
		protected override void OnClick()
		{
			base.OnClick();

			Process p = new Process()
			{
				StartInfo = new ProcessStartInfo()
				{
					FileName = this.NavigateUri.AbsoluteUri
				}
			};
			p.Start();
		}
	}
}
