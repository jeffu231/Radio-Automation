using Xunit;

namespace RadioAutomation.Tests.TestUtilities
{
	[CollectionDefinition(Name, DisableParallelization = true)]
	public sealed class NonParallelCollection
	{
		public const string Name = "NonParallel";
	}
}
