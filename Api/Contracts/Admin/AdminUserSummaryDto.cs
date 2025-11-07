using Api.Data.Entities;

namespace Api.Contracts.Admin;

public record AdminUserSummaryDto(Guid Id, string UserName, Role Role);

