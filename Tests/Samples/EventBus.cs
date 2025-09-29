using Azure.Messaging.ServiceBus;

namespace Samples
{
	public interface IEventBus
	{
		Task PublishAsync(string message);
	}

	public class EventBus : IEventBus
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
