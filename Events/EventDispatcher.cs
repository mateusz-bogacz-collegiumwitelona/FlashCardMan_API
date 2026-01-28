using Events.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Events
{
    public class EventDispatcher : IEventDispatcher
    {
        private readonly IServiceProvider _serviceProvider;

        public EventDispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task PublishAsync<T>(T @event) where T : IEvent
        {
            using var scope = _serviceProvider.CreateScope();
            var handlers = scope.ServiceProvider.GetServices<IEventHandler<T>>();

            foreach (var handler in handlers)
            {
                try
                {
                    await handler.HandleAsync(@event);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error handling event {typeof(T).Name} with handler {handler.GetType().Name}: {ex.Message}");
                }
            }

        }
    }
}
