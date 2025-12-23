namespace WALMS.API.Common
{
    public class LastActivity
    {
        public static string GetLastActiveDisplay(DateTime lastLoginTime, DateTime currentDateTime)
        {
            var timeDifference = currentDateTime - lastLoginTime;

            if (timeDifference.TotalMinutes < 1)
                return " (just now)";
            if (timeDifference.TotalMinutes < 60)
                return $" ({Math.Floor(timeDifference.TotalMinutes)} min ago)";
            if (timeDifference.TotalHours < 24)
                return $" ({Math.Floor(timeDifference.TotalHours)} hour ago)";
            return $" ({Math.Floor(timeDifference.TotalDays)} day ago)";
        }

    }
}
