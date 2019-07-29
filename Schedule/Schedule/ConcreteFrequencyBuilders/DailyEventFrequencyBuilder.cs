using System;
using Schedule.Common;
using Schedule.TemporalExpressions;
using ScheduleWidget.TemporalExpressions.Base;

namespace Schedule.Schedule.ConcreteFrequencyBuilders
{
    public class DailyEventFrequencyBuilder : IEventFrequencyBuilder
    {
        public TemporalExpressionUnion Create(ISchedule schedule)
        {
            var union = new TemporalExpressionUnion();
            foreach (DayInterval day in Enum.GetValues(typeof(DayInterval)))
            {
                union.Add(new ScheduleDayOfWeek(day));
            }

            return union;
        }
    }
}
