using Microsoft.EntityFrameworkCore;
using PeminjamanRuangan.Services;
using PeminjamanRuangan.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=room_booking.db"));
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                    .AllowAnyHeader()
                    .AllowAnyMethod();
        });
});

var app = builder.Build();
app.UseCors("AllowFrontend");

var rooms = new List<Room>();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    context.Database.Migrate();

    if (!context.Rooms.Any())
    {
    var buildingConfig = new Dictionary<string, int>
    {
        { "SAW", 11 },
        { "Pasca Sarjana", 11 },
        { "Gedung D4", 3 },
        { "Gedung D3", 2 }
    };

    int roomsPerFloor = 10;

    foreach (var building in buildingConfig)
    {
        string buildingName = building.Key;
        int totalFloors = building.Value;

        string buildingCode = buildingName switch
        {
            "SAW" => "SAW",
            "Pasca Sarjana" => "PAS",
            "Gedung D4" => "D4",
            "Gedung D3" => "D3",
            _ => "UNK"
        };

        for (int floor = 1; floor <= totalFloors; floor++)
        {
            for (int roomNumber = 1; roomNumber <= roomsPerFloor; roomNumber++)
            {
                rooms.Add(new Room
                {
                    Name = $"Ruang {buildingName} Lantai {floor} - {roomNumber}",
                    Code = $"{buildingCode}-{floor}-{roomNumber:D2}",
                    Building = buildingName,
                    Capacity = 20 + (roomNumber * 5),
                    IsActive = true
                });
            }
        }
    }
    // Ruangan khusus

    rooms.AddRange(new List<Room>
    {
        new Room
        {
            Name = "Lapangan Basket",
            Code = "D4-0-01",
            Building = "Gedung D4",
            Capacity = 200,
            IsActive = true
        },
        new Room
        {
            Name = "Lapangan Merah",
            Code = "D3-0-01",
            Building = "Gedung D3",
            Capacity = 150,
            IsActive = true
        },
        new Room
        {
            Name = "Aula Gedung D4",
            Code = "D4-1-99",
            Building = "Gedung D4",
            Capacity = 300,
            IsActive = true
        },
        new Room
        {
            Name = "Auditorium Pasca Sarjana",
            Code = "PAS-6-99",
            Building = "Pasca Sarjana",
            Capacity = 400,
            IsActive = true
        },
        new Room
        {
            Name = "Teater D3",
            Code = "D3-1-99",
            Building = "Gedung D3",
            Capacity = 120,
            IsActive = true
        },
        new Room
        {
            Name = "Mini Teater Pasca Sarjana",
            Code = "PAS-2-98",
            Building = "Pasca Sarjana",
            Capacity = 80,
            IsActive = true
        }
    });
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
