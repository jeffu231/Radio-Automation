using System;

namespace Schedule.Common
{
    /// <summary>
    /// The month(s) in which a quarterly schedule recurs.
    /// </summary>
    [Flags]
    public enum MonthOfQuarterInterval
    {
        None = 0,
        First = 1,
        Second = 2,
        Third = 4
    }
}
