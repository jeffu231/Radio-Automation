﻿using System;
using Schedule.Common;
using Schedule.TemporalExpressions;
using ScheduleWidget.TemporalExpressions.Base;

namespace Schedule.Schedule.ConcreteFrequencyBuilders
{
    public class MonthlyDayOfMonthEventFrequencyBuilder : IEventFrequencyBuilder
    {
        public TemporalExpressionUnion Create(ISchedule schedule)
        {
            if (schedule.WeekInterval == WeekInterval.None)
                throw new ArgumentException("WeekInterval must be set for schedules with a monthly frequency.");

            if (schedule.DayInterval == DayInterval.None)
                throw new ArgumentException("DayInterval must be set for schedules with a monthly frequency.");

            var union = new TemporalExpressionUnion();
            var weeklyIntervals = union.GetFlags(schedule.WeekInterval);
            foreach (var weeklyInterval in weeklyIntervals)
            {
                var daysOfWeek = union.GetFlags(schedule.DayInterval);
                foreach (var dayOfWeek in daysOfWeek)
                {
                    var dayInMonth = new ScheduleDayInMonth((DayInterval)dayOfWeek, (WeekInterval)weeklyInterval);
                    union.Add(dayInMonth);
                }
            }

            return union;
        }
    }
}
