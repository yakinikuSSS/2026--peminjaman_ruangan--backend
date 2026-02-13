public class RoomResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string? Building { get; set; }
    public int Capacity { get; set; }
    public bool IsActive { get; set; }
}
