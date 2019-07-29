using System;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using Catel.Collections;
using Radio_Automation.Utils;

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
	}
}
