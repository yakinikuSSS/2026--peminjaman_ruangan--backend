public class CreateRoomDto
{
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string? Building { get; set; }
    public int Capacity { get; set; }
}
