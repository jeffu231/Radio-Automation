using ScheduleWidget.TemporalExpressions.Base;

namespace Schedule.Schedule.ConcreteFrequencyBuilders
{
    public class NullEventFrequencyBuilder : IEventFrequencyBuilder
    {
        public TemporalExpressionUnion Create(ISchedule schedule)
        {
            // no expressions
            return new TemporalExpressionUnion();
        }
    }
}
