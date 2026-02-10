public class RoomBooking
{
    public int Id { get; set; }

    public int RoomId { get; set; }
    public Room Room { get; set; } = default!;
    public string BorrowerName { get; set; } = default!;
    public string BorrowerPhone { get; set; } = default!;

    public string Purpose { get; set; } = default!;

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? DeletedAt { get; set; }
}
