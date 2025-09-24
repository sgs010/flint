using Azure.Messaging.ServiceBus;

namespace WebApp
{
	interface IEventBus
	{
		Task PublishAsync(string message);
	}

	class EventBus : IEventBus
	{
		private readonly ServiceBusSender _bus;
		public EventBus(ServiceBusSender bus)
		{
			_bus = bus;
		}

		public Task PublishAsync(string message)
		{
			var msg = new ServiceBusMessage(message);
			return _bus.SendMessageAsync(msg);
		}
	}
}
