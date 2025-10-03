namespace Samples
{
	interface IEventService
	{
		Task FirePostAdded(int postId);
	}

	class EventService : IEventService
	{
		private readonly IEventBus _bus;

		public EventService(IEventBus bus)
		{
			_bus = bus;
		}

		public async Task FirePostAdded(int postId)
		{
			await _bus.PublishAsync("new post " + postId);
		}
	}
}
