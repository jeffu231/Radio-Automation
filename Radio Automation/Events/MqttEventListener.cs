using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet.Client;
using MQTTnet;
using Radio_Automation.Models;
using Catel.Logging;
using MQTTnet.Internal;
using System.Text;
using System.Windows.Media;

namespace Radio_Automation.Events
{
	public sealed class MqttEventListener
	{
		private static readonly ILog Log = LogManager.GetCurrentClassLogger();
		private Settings _settings;
		private IMqttClient _mqttClient;

		private readonly Dictionary<string, List<Event>> _events = new ();

		public async Task Connect(Settings settings)
		{
			if (settings == null) throw new ArgumentNullException(nameof(settings));
			_settings = settings;
			await InitBroker();
		}

		public async Task LoadSchedule(EventSchedule eventSchedule)
		{
			
			var mqttFactory = new MqttFactory();

			foreach (var e in _events.Keys)
			{
				var mqttSubscribeOptions = mqttFactory.CreateUnsubscribeOptionsBuilder()
					.WithTopicFilter(e)
					.Build();
				await _mqttClient.UnsubscribeAsync(mqttSubscribeOptions, CancellationToken.None);
			}

			_events.Clear();
			foreach (var e in eventSchedule.Events.Where(x => x.Enabled && x.Trigger == Trigger.Mqtt))
			{
				if (!_events.ContainsKey(e.MqttExpression.Topic))
				{
					Log.Info($"Subscribing to topic {e.MqttExpression.Topic}");
					var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
						.WithTopicFilter(
							f =>
							{
								f.WithTopic(e.MqttExpression.Topic);
								f.WithAtLeastOnceQoS();
							})
						.Build();
					await _mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);
				}
				
				if(_events.TryGetValue(e.MqttExpression.Topic, out var events))
				{
					events.Add(e);
				}
				else
				{
					var evts = new List<Event> { e };
					_events.Add(e.MqttExpression.Topic, evts);
				}

				
			}
		}

		private async Task InitBroker()
		{

			if (string.IsNullOrEmpty(_settings.MqttBroker))
			{
				return;
			}

			if (_mqttClient != null && _mqttClient.IsConnected)
			{
				var mqttClientDisconnectOptions = new MqttClientDisconnectOptionsBuilder().Build();
				await _mqttClient.DisconnectAsync(mqttClientDisconnectOptions, CancellationToken.None);
			}

			var mqttFactory = new MqttFactory();

			_mqttClient = mqttFactory.CreateMqttClient();
			
			var mqttClientOptions = new MqttClientOptionsBuilder()
				.WithTcpServer(_settings.MqttBroker, _settings.MqttBrokerPort).Build();

			// Setup message handling before connecting so that queued messages
			// are also handled properly. When there is no event handler attached all
			// received messages get lost.
			_mqttClient.ApplicationMessageReceivedAsync += _mqttClient_ApplicationMessageReceivedAsync;

			var result = await _mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

			if (result.ResultCode != MqttClientConnectResultCode.Success)
			{
				Log.Error($"MQTT connection failed with result code {result.ResultCode}");
			}
		}

		private Task _mqttClient_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
		{
			var topic = arg.ApplicationMessage.Topic;
			var message = arg.ApplicationMessage.ConvertPayloadToString();
			Log.Info($"MQTT {topic} - {message}");

			if (_events.TryGetValue(topic, out var e))
			{
				if (_events.TryGetValue(topic, out var events))
				{
					var eventsToTrigger = events.Where(x => x.MqttExpression.Message == message);
					foreach (var eventToTrigger in eventsToTrigger)
					{
						if (eventToTrigger.Enabled)
						{
							EventBus.OnEventTriggered(eventToTrigger);
						}
					}
				}
			}

			return Task.CompletedTask;
		}

		
	}
}
