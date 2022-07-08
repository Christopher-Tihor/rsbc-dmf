﻿namespace Pssg.SharedUtils
{
    public class DateUtility
    {
        public static DateTime? FormatDatePacific(DateTimeOffset? inputDate)
        {
            TimeZoneInfo hwZone;
            try
            {
                hwZone = TimeZoneInfo.FindSystemTimeZoneById("America/Vancouver");
            }
            catch (TimeZoneNotFoundException)
            {
                hwZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            }

            DateTime? result = inputDate.HasValue
                ? TimeZoneInfo.ConvertTimeFromUtc(inputDate.Value.DateTime, hwZone)
                : (DateTime?)null;
            return result;
        }
    }
}
