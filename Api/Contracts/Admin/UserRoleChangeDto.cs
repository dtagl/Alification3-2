using Api.Data.Entities;

namespace Api.Contracts.Admin;

public record UserRoleChangeDto(Guid Id, Role Role);

