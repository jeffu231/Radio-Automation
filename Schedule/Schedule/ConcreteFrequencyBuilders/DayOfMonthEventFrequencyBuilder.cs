using ScheduleWidget.TemporalExpressions;
using ScheduleWidget.TemporalExpressions.Base;

namespace Schedule.Schedule.ConcreteFrequencyBuilders
{
    public class DayOfMonthEventFrequencyBuilder : IEventFrequencyBuilder
    {
        public TemporalExpressionUnion Create(ISchedule schedule)
        {
            var union = new TemporalExpressionUnion();
            var dayOfMonth = new ScheduleDayOfMonth(schedule.DayOfMonth);
            union.Add(dayOfMonth);
            return union;
        }
    }
}
