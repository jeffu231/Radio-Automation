using System;
using System.Threading;
using System.Threading.Tasks;
using Catel.Logging;
using MQTTnet;
using Radio_Automation.Models;

namespace Radio_Automation.Services
{
	public class MqttPublisherService : IMqttPublisherService
	{
		private static readonly ILog Log = LogManager.GetCurrentClassLogger();
		private IMqttClient _mqttClient;

		public async Task ConnectAsync(Settings settings)
		{
			if (string.IsNullOrEmpty(settings.MqttBroker)) return;

			if (_mqttClient != null && _mqttClient.IsConnected)
			{
				var disconnectOptions = new MqttClientDisconnectOptionsBuilder().Build();
				await _mqttClient.DisconnectAsync(disconnectOptions, CancellationToken.None);
			}

			var factory = new MqttClientFactory();
			_mqttClient = factory.CreateMqttClient();

			var options = new MqttClientOptionsBuilder()
				.WithTcpServer(settings.MqttBroker, settings.MqttBrokerPort)
				.Build();

			try
			{
				var result = await _mqttClient.ConnectAsync(options, CancellationToken.None);
				if (result.ResultCode != MqttClientConnectResultCode.Success)
					Log.Error($"MqttPublisherService connection failed: {result.ResultCode}");
			}
			catch (Exception e)
			{
				Log.Error(e, "MqttPublisherService failed to connect to broker.");
			}
		}

		public async Task PublishAsync(string topic, string payload)
		{
			if (_mqttClient == null || !_mqttClient.IsConnected) return;

			try
			{
				var message = new MqttApplicationMessageBuilder()
					.WithTopic(topic)
					.WithPayload(payload)
					.WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
					.WithRetainFlag(true)
					.Build();

				await _mqttClient.PublishAsync(message, CancellationToken.None);
			}
			catch (Exception e)
			{
				Log.Error(e, $"MqttPublisherService failed to publish to topic '{topic}'.");
			}
		}
	}
}
