namespace Api.Contracts.Admin;

public class ChangeWorkingHoursDto
{
    public TimeSpan WorkingStart { get; set; }
    public TimeSpan WorkingEnd { get; set; }
}

