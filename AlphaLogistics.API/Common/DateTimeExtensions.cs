namespace Logistics
{
    public static class DateTimeExtensions
    {
        public static DateTime UTCToIST(this DateTime date)
        {
#if DEBUG
            return date;
#else

            return TimeZoneInfo.ConvertTimeFromUtc(date, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
#endif
        }
    }
}
