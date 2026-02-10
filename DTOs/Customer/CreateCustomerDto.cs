public class CreateCustomerDto
{
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? Phone { get; set; }
    public string? Address { get; set; }
}
