using System.Threading.Tasks;
using Radio_Automation.Models;

namespace Radio_Automation.Services
{
	public interface IMqttPublisherService
	{
		Task ConnectAsync(Settings settings);
		Task PublishAsync(string topic, string payload);
	}
}
