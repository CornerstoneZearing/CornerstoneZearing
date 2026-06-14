namespace CornerstoneZearing
{
    public enum MonthlyYearlyPattern
    {
        SpecificDate = 0,
        DayOfWeekPosition = 1
    }

    public enum PackageType
    {
        Style,
        Script
    }

    public enum PageStatus
    {
        Draft = 0,
        Published = 1,
        Withdrawn = 2
    }

    public enum RecurrenceType
    {
        None = 0,
        Daily = 1,
        Weekly = 2,
        Monthly = 3,
        Yearly = 4
    }
}