namespace Api.Contracts.Admin;

public record CompanyWorkingHoursDto(Guid Id, TimeSpan WorkingStart, TimeSpan WorkingEnd);

