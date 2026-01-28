namespace Events.Interfaces
{
    public interface IEventHandler<T> where T : IEvent
    {
        Task HandleAsync(T @event);
    }
}
