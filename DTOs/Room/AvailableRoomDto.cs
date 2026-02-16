namespace PeminjamanRuangan.DTOs.Room
{
    public class AvailableRoomDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string Code { get; set; } = default!;
        public string Building { get; set; } = default!;
        public int Capacity { get; set; }
    }
}
