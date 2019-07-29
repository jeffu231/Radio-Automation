using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Radio_Automation.Extensions
{
	public static class FileExtensions
	{
		/// <summary>
		/// This is the same default buffer size as
		/// <see cref="StreamReader"/> and <see cref="FileStream"/>.
		/// </summary>
		private const int DefaultBufferSize = 4096;

		/// <summary>
		/// Indicates that
		/// 1. The file is to be used for asynchronous reading.
		/// 2. The file is to be accessed sequentially from beginning to end.
		/// </summary>
		private const FileOptions DefaultOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;

		public static Task<string[]> ReadAllLinesAsync(string path)
		{
			var encoding = GetEncoding(path);
			return ReadAllLinesAsync(path, encoding);
		}

		public static async Task<string[]> ReadAllLinesAsync(string path, Encoding encoding)
		{
			var lines = new List<string>();

			// Open the FileStream with the same FileMode, FileAccess
			// and FileShare as a call to File.OpenText would've done.
			using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultBufferSize, DefaultOptions))
			using (var reader = new StreamReader(stream, encoding))
			{
				string line;
				while ((line = await reader.ReadLineAsync()) != null)
				{
					lines.Add(line);
				}
			}

			return lines.ToArray();
		}

		/// <summary>
		/// Determines a text file's encoding by analyzing its byte order mark (BOM).
		/// Defaults to ASCII when detection of the text file's endianness fails.
		/// </summary>
		/// <param name="filename">The text file to analyze.</param>
		/// <returns>The detected encoding.</returns>
		public static Encoding GetEncoding(string filename)
		{
			// Read the BOM
			var bom = new byte[4];
			using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read))
			{
				file.Read(bom, 0, 4);
			}

			// Analyze the BOM
			if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.UTF7;
			if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
			if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; //UTF-16LE
			if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE
			if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return Encoding.UTF32;
			return Encoding.Default;
		}
	}
}
