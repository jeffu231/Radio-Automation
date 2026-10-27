using System;
using NAudio.CoreAudioApi;

namespace NAudioWrapper
{
	[Serializable]
	public class Device: IEquatable<Device>
	{
		public Device()
		{
			
		}

		public Device(MMDevice device, bool isDefault)
			: this(device.FriendlyName, device.ID, isDefault)
		{
		}

		internal Device(string friendlyName, string id, bool isDefault)
		{
			FriendlyName = friendlyName;
			Id = id;
			IsDefault = isDefault;
		}

		public string FriendlyName { get; set; } = string.Empty;

		public string Id { get; set; } = string.Empty;

		public bool IsDefault { get; set; }


		#region Equality members

		/// <inheritdoc />
		public bool Equals(Device? other)
		{
			if (other is null) return false;
			if (ReferenceEquals(this, other)) return true;
			return Id == other.Id;
		}

		/// <inheritdoc />
		public override bool Equals(object? obj)
		{
			if (obj is null) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((Device) obj);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			return (Id != null ? Id.GetHashCode() : 0);
		}

		#endregion
	}
}
