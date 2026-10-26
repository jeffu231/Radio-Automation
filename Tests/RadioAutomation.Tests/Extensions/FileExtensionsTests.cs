using Radio_Automation.Extensions;
using RadioAutomation.Tests.TestUtilities;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RadioAutomation.Tests.Extensions
{
	public sealed class FileExtensionsTests
	{
		private static readonly string[] ThreeLines = { "one", "two", "three" };
		private static readonly string[] AccentedLines = { "héllo", "wörld" };

		[Fact]
		public void GetEncoding_Utf8Bom_ReturnsUtf8()
		{
			using var dir = new TempDirectory();
			File.WriteAllBytes(dir.File("f.txt"), new byte[] { 0xEF, 0xBB, 0xBF, 0x41, 0x42 });

			Assert.Equal(Encoding.UTF8, FileExtensions.GetEncoding(dir.File("f.txt")));
		}

		[Fact]
		public void GetEncoding_Utf16LeBom_ReturnsUnicode()
		{
			using var dir = new TempDirectory();
			File.WriteAllBytes(dir.File("f.txt"), new byte[] { 0xFF, 0xFE, 0x41, 0x00 });

			Assert.Equal(Encoding.Unicode, FileExtensions.GetEncoding(dir.File("f.txt")));
		}

		[Fact]
		public void GetEncoding_Utf16BeBom_ReturnsBigEndianUnicode()
		{
			using var dir = new TempDirectory();
			File.WriteAllBytes(dir.File("f.txt"), new byte[] { 0xFE, 0xFF, 0x00, 0x41 });

			Assert.Equal(Encoding.BigEndianUnicode, FileExtensions.GetEncoding(dir.File("f.txt")));
		}

		[Fact]
		public void GetEncoding_NoBom_ReturnsDefault()
		{
			using var dir = new TempDirectory();
			File.WriteAllBytes(dir.File("f.txt"), new byte[] { 0x41, 0x42, 0x43, 0x44 });

			Assert.Equal(Encoding.Default, FileExtensions.GetEncoding(dir.File("f.txt")));
		}

		[Fact]
		public async Task ReadAllLinesAsync_ReturnsAllLines()
		{
			using var dir = new TempDirectory();
			File.WriteAllText(dir.File("f.txt"), "one\r\ntwo\nthree");

			var lines = await FileExtensions.ReadAllLinesAsync(dir.File("f.txt"));

			Assert.Equal(ThreeLines, lines);
		}

		[Fact]
		public async Task ReadAllLinesAsync_Utf16File_DecodesCorrectly()
		{
			using var dir = new TempDirectory();
			File.WriteAllText(dir.File("f.txt"), "héllo\nwörld", Encoding.Unicode);

			var lines = await FileExtensions.ReadAllLinesAsync(dir.File("f.txt"));

			Assert.Equal(AccentedLines, lines);
		}

		[Fact]
		public async Task ReadAllLinesAsync_MissingFile_Throws()
		{
			using var dir = new TempDirectory();

			await Assert.ThrowsAsync<FileNotFoundException>(() => FileExtensions.ReadAllLinesAsync(dir.File("missing.txt")));
		}

		[Fact]
		public void GetEncoding_FileShorterThanFourBytes_ReturnsDefault()
		{
			using var dir = new TempDirectory();
			File.WriteAllBytes(dir.File("f.txt"), new byte[] { 0x41, 0x42 });

			Assert.Equal(Encoding.Default, FileExtensions.GetEncoding(dir.File("f.txt")));
		}

		[Fact]
		public void GetEncoding_EmptyFile_ReturnsDefault()
		{
			using var dir = new TempDirectory();
			File.WriteAllBytes(dir.File("f.txt"), System.Array.Empty<byte>());

			Assert.Equal(Encoding.Default, FileExtensions.GetEncoding(dir.File("f.txt")));
		}

		[Fact]
		public void GetEncoding_TwoByteUtf16LeBomOnly_ReturnsUnicode()
		{
			using var dir = new TempDirectory();
			File.WriteAllBytes(dir.File("f.txt"), new byte[] { 0xFF, 0xFE });

			Assert.Equal(Encoding.Unicode, FileExtensions.GetEncoding(dir.File("f.txt")));
		}

		[Fact]
		public void GetEncoding_Utf32LeBom_ReturnsUtf32()
		{
			using var dir = new TempDirectory();
			File.WriteAllBytes(dir.File("f.txt"), new byte[] { 0xFF, 0xFE, 0x00, 0x00, 0x41, 0x00, 0x00, 0x00 });

			Assert.Equal(Encoding.UTF32, FileExtensions.GetEncoding(dir.File("f.txt")));
		}

		[Fact]
		public void GetEncoding_Utf32BeBom_ReturnsBigEndianUtf32()
		{
			using var dir = new TempDirectory();
			File.WriteAllBytes(dir.File("f.txt"), new byte[] { 0x00, 0x00, 0xFE, 0xFF, 0x00, 0x00, 0x00, 0x41 });

			var encoding = FileExtensions.GetEncoding(dir.File("f.txt"));

			Assert.Equal(12001, encoding.CodePage);
		}
	}
}
