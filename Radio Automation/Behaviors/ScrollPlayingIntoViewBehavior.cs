using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace Radio_Automation.Behaviors
{
	public class ScrollPlayingIntoViewBehavior: Behavior<ListView>
	{
		private ListView _lv;

		protected override void OnAttached()
		{
			if (AssociatedObject is ListView lv)
			{
				_lv = lv;
			}
		}

		private static void CurrentTrackIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if(d is ScrollPlayingIntoViewBehavior b)
			{
				Console.Out.WriteLine($"Index = {b.CurrentTrackIndex}");
				if (b._lv != null)
				{
					if (b.CurrentTrackIndex >= 0 && b.CurrentTrackIndex < b._lv.Items.Count)
					{
						var item = b._lv.Items[b.CurrentTrackIndex];
						if (item != null)
						{
							b._lv.ScrollIntoView(item);
						}
					}
				}
			}
		}

		protected override void OnDetaching()
		{
			
		}    

		public int CurrentTrackIndex
		{
			get { return (int)GetValue(CurrentTrackIndexProperty); }
			set { SetValue(CurrentTrackIndexProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Words. 
		//This enables animation, styling, binding, etc...
		public static  DependencyProperty CurrentTrackIndexProperty =
			DependencyProperty.Register("CurrentTrackIndex", typeof(int),
				typeof(ScrollPlayingIntoViewBehavior), new PropertyMetadata(default(int),new PropertyChangedCallback(CurrentTrackIndexChanged)));

		
	}
}
