public class RoomBooking
{
    public int Id{ get; set; }
    public int UserId{ get; set; }
    public User User { get; set; }
    public int RoomId { get; set; }
    public Room Room { get; set; }
    public string Purpose{ get; set; }
    public DateTime StartTime{ get; set; }
    public DateTime EndTime{ get; set; }
    public BookingStatus Status{ get; set; } = BookingStatus.Pending;
    public DateTime CreatedAt{ get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt{ get; set; }
}
