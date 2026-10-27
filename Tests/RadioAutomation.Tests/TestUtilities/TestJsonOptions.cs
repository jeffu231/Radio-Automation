using Radio_Automation;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace RadioAutomation.Tests.TestUtilities
{
	public static class TestJsonOptions
	{
		/// <summary>
		/// Mirrors the serializer options built in <c>App.OnStartup</c>. Must stay in sync with it.
		/// </summary>
		public static JsonSerializerOptions Create() => new JsonSerializerOptions
		{
			WriteIndented = true,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
			TypeInfoResolver = new DefaultJsonTypeInfoResolver
			{
				Modifiers = { App.IgnoreXmlIgnoredProperties }
			},
			Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
		};
	}
}
