using ScheduleWidget.TemporalExpressions.Base;

namespace Schedule.Schedule
{
    public interface IEventFrequencyBuilder
    {
        TemporalExpressionUnion Create(ISchedule schedule);
    }
}
