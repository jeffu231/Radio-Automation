using Catel.Collections;
using Microsoft.VisualBasic;
using Radio_Automation.Models;
using Radio_Automation.Utils;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;

namespace Radio_Automation.Extensions
{
	public static class CollectionExtensions
	{
		public static void Shuffle<T>(this FastObservableCollection<T> list)
		{
			var disposable = list.SuspendChangeNotifications(SuspensionMode.Mixed);
			int n = list.Count;
			while (n > 1)
			{
				n--;
				int k = ThreadSafeRandom.Instance.Next(n + 1);
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
			disposable.Dispose();
		}

		public static void Shuffle<T>(this ObservableCollection<T> list)
		{
			//var disposable = list.SuspendChangeNotifications(SuspensionMode.Mixed);
			int n = list.Count;
			while (n > 1)
			{
				n--;
				int k = ThreadSafeRandom.Instance.Next(n + 1);
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
			//disposable.Dispose();
		}

		public static void SortTrackArtist(this FastObservableCollection<Track> list)
		{
			using (list.SuspendChangeNotifications(SuspensionMode.None))
			{
				var sortedList = list.OrderBy(item => item.Artist).ToList();

				// Reorder the original collection using the Move() method to update indices efficiently
				for (int i = 0; i < sortedList.Count; ++i)
				{
					var actualItemIndex = list.IndexOf(sortedList[i]);
					if (actualItemIndex != i)
					{
						// Move the item to its sorted position. This fires a single Move notification internally.
						list.Move(actualItemIndex, i);
					}
				}
			}
		}

		public static void SortTrackName(this FastObservableCollection<Track> list)
		{
			using (list.SuspendChangeNotifications(SuspensionMode.None))
			{
				var sortedList = list.OrderBy(item => item.Name).ToList();

				// Reorder the original collection using the Move() method to update indices efficiently
				for (int i = 0; i < sortedList.Count; ++i)
				{
					var actualItemIndex = list.IndexOf(sortedList[i]);
					if (actualItemIndex != i)
					{
						// Move the item to its sorted position. This fires a single Move notification internally.
						list.Move(actualItemIndex, i);
					}
				}
			}
		}

	}
}
