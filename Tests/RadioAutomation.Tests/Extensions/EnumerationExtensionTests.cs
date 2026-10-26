using Radio_Automation.Extensions;
using System;
using System.ComponentModel;
using Xunit;

namespace RadioAutomation.Tests.Extensions
{
	public sealed class EnumerationExtensionTests
	{
		private enum Sample
		{
			[Description("First Item")] First,
			Second
		}

		[Fact]
		public void Ctor_NullType_ThrowsArgumentNullException()
		{
			var ex = Assert.Throws<ArgumentNullException>(() => new EnumerationExtension(null));

			Assert.Equal("enumType", ex.ParamName);
		}

		[Fact]
		public void Ctor_NonEnumType_ThrowsArgumentException()
		{
			Assert.Throws<ArgumentException>(() => new EnumerationExtension(typeof(string)));
		}

		[Fact]
		public void Ctor_NullableEnumType_IsAccepted()
		{
			var extension = new EnumerationExtension(typeof(Sample?));

			Assert.Equal(typeof(Sample?), extension.EnumType);
		}

		[Fact]
		public void ProvideValue_ReturnsOneMemberPerEnumValue()
		{
			var members = (EnumerationExtension.EnumerationMember[])new EnumerationExtension(typeof(Sample)).ProvideValue(null);

			Assert.Equal(2, members.Length);
			Assert.Equal(Sample.First, members[0].Value);
			Assert.Equal(Sample.Second, members[1].Value);
		}

		[Fact]
		public void ProvideValue_UsesDescriptionAttributeWhenPresent()
		{
			var members = (EnumerationExtension.EnumerationMember[])new EnumerationExtension(typeof(Sample)).ProvideValue(null);

			Assert.Equal("First Item", members[0].Description);
		}

		[Fact]
		public void ProvideValue_FallsBackToNameWithoutDescription()
		{
			var members = (EnumerationExtension.EnumerationMember[])new EnumerationExtension(typeof(Sample)).ProvideValue(null);

			Assert.Equal("Second", members[1].Description);
		}

		[Fact]
		public void ProvideValue_NullableEnum_ReturnsUnderlyingMembers()
		{
			var members = (EnumerationExtension.EnumerationMember[])new EnumerationExtension(typeof(Sample?)).ProvideValue(null);

			Assert.Equal(2, members.Length);
			Assert.Equal("First Item", members[0].Description);
			Assert.Equal("Second", members[1].Description);
		}
	}
}
