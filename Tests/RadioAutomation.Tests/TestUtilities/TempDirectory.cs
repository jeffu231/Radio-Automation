using System;
using System.IO;

namespace RadioAutomation.Tests.TestUtilities
{
	/// <summary>
	/// A uniquely named directory under the system temp path that is deleted on dispose.
	/// </summary>
	public sealed class TempDirectory : IDisposable
	{
		public TempDirectory()
		{
			Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "RadioAutomation.Tests", Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(Path);
		}

		public string Path { get; }

		public string File(string name)
		{
			return System.IO.Path.Combine(Path, name);
		}

		public void Dispose()
		{
			try
			{
				Directory.Delete(Path, true);
			}
			catch (IOException)
			{
				// file handles may still be closing
			}
		}
	}
}
