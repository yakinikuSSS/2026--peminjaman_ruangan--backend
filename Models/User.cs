public enum UserRole
{
    Admin,
    Customer
}

public class User
{
    public int Id { get; set; }

    public string Name { get; set; } // required
    public string Email { get; set; } // required & unique
    public string PasswordHash { get; set; }

    public string? Phone { get; set; }
    public string? Address { get; set; }

    public bool IsActive { get; set; } = true; // status active/inactive

    public UserRole Role { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }

    public ICollection<RoomBooking> Bookings { get; set; }
}
