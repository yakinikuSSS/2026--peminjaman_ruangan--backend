using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Tambahkan Controller support
builder.Services.AddControllers();

// 🔹 Tambahkan SQLite DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=room_booking.db"));

// 🔹 Tambahkan Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    context.Database.EnsureCreated();

    if (!context.Rooms.Any())
    {
        var rooms = new List<Room>
        {
            new Room
            {
                Name = "Lab AI",
                Code = "AI-01",
                Building = "Gedung A",
                Capacity = 40,
                IsActive = true
            },
            new Room
            {
                Name = "Ruang Seminar",
                Code = "SEM-01",
                Building = "Gedung B",
                Capacity = 100,
                IsActive = true
            },
            new Room
            {
                Name = "Ruang Rapat",
                Code = "RPT-01",
                Building = "Gedung C",
                Capacity = 25,
                IsActive = true
            },
            new Room
            {
                Name = "Aula Utama",
                Code = "AULA-01",
                Building = "Gedung Utama",
                Capacity = 250,
                IsActive = true
            }
        };

        context.Rooms.AddRange(rooms);
        context.SaveChanges();
    }
}

// 🔹 Aktifkan Swagger hanya saat Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 🔹 Aktifkan Controller routing
app.MapControllers();

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm",
    "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast(
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();

    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
