using NAudioWrapper;
using RadioAutomation.Tests.TestUtilities;
using System.Text.Json;
using Xunit;

namespace RadioAutomation.Tests.Audio
{
	public sealed class DeviceTests
	{
		[Fact]
		public void Equals_SameId_IgnoresIsDefaultAndName()
		{
			var a = new Device { Id = "id-1", FriendlyName = "A", IsDefault = true };
			var b = new Device { Id = "id-1", FriendlyName = "B", IsDefault = false };

			Assert.True(a.Equals(b));
			Assert.True(a.Equals((object)b));
			Assert.Equal(a.GetHashCode(), b.GetHashCode());
		}

		[Fact]
		public void Equals_DifferentId_IsFalse()
		{
			var a = new Device { Id = "id-1" };
			var b = new Device { Id = "id-2" };

			Assert.False(a.Equals(b));
		}

		[Fact]
		public void Json_RoundTrip_PreservesIsDefault()
		{
			var device = new Device { Id = "id-1", FriendlyName = "Speakers", IsDefault = true };

			var json = JsonSerializer.Serialize(device, TestJsonOptions.Create());
			var clone = JsonSerializer.Deserialize<Device>(json, TestJsonOptions.Create());

			Assert.NotNull(clone);
			Assert.Equal("id-1", clone.Id);
			Assert.Equal("Speakers", clone.FriendlyName);
			Assert.True(clone.IsDefault);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void Constructor_SetsIsDefaultFromArgument(bool isDefault)
		{
			var device = new Device("Speakers", "id-1", isDefault);

			Assert.Equal("Speakers", device.FriendlyName);
			Assert.Equal("id-1", device.Id);
			Assert.Equal(isDefault, device.IsDefault);
		}

		[Fact]
		public void Json_LegacyPayloadWithoutIsDefault_DeserializesAsFalse()
		{
			var clone = JsonSerializer.Deserialize<Device>("{\"FriendlyName\":\"Speakers\",\"Id\":\"id-1\"}", TestJsonOptions.Create());

			Assert.NotNull(clone);
			Assert.False(clone.IsDefault);
		}
	}
}
