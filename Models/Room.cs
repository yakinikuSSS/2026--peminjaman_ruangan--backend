public class Room
{
    public int Id { get; set; }

    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string Building { get; set; } = default!;
    public int Capacity { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<RoomBooking> Bookings { get; set; } = new List<RoomBooking>();
}
