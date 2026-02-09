public class Room
{
    public int Id { get; set; }

    public string Code { get; set; }
    public string Name { get; set; }
    public string Building { get; set; }

    public int Capacity { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<RoomBooking> Bookings { get; set; }
}
