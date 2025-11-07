namespace Api.Contracts.Rooms;

public class RoomFilterDto
{
    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public int? MinCapacity { get; set; }
    public int? MaxCapacity { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
}

