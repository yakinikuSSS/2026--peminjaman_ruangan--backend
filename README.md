# Sistem Peminjaman Ruangan – Backend

Backend API dikembangkan menggunakan **ASP.NET Core Web API**, **Entity Framework Core**, dan **SQLite** sebagai database.

## Tech Stack

- ASP.NET Core Web API
- Entity Framework Core
- SQLite
- Swagger (OpenAPI)

## Requirements

Pastikan sudah terinstall:

- **.NET SDK** (disarankan .NET 8 atau sesuai target framework project)

Download .NET SDK:
https://dotnet.microsoft.com/en-us/download

Cek versi .NET:

```bash
dotnet --version
```

## Installation

Masuk ke folder backend:

```bash
cd 2026--peminjaman_ruangan--frontend
```

Restore dependencies:

```bash
dotnet restore
```

## 🗄 Database Configuration

Database menggunakan **SQLite**.

Lakukan perintah dibawah:

```bash
dotnet ef database update
```

Perintah ini akan membuat file database SQLite secara otomatis berdasarkan migration.


## Running the Application

Jalankan backend:

```bash
dotnet run
```

Backend akan berjalan pada:

```
http://localhost:5023
```


## Swagger API Documentation

Setelah backend berjalan, buka:

```
http://localhost:5023/swagger
```

Swagger digunakan untuk:

- Melihat seluruh endpoint API
- Testing request dan response
- Validasi fitur booking, history, dan dashboard


## 📌 Core Features

### Booking
- Create Booking
- Update Booking
- Soft Delete
- Change Status:
  - Pending → Approved
  - Pending → Rejected
  - Approved → Completed
  - Approved → Cancelled
- Validasi konflik waktu (hanya status **Approved** yang mengunci ruangan)
- Auto update status berdasarkan waktu


### History
- Menampilkan booking dengan status:
  - Rejected
  - Completed
  - Cancelled
- Search berdasarkan:
  - Nama peminjam
  - Nama ruangan
  - Kode ruangan
  - Tujuan
  - Status
- Filter berdasarkan:
  - Status
  - Rentang tanggal
- Sorting berdasarkan:
  - StartTime
  - CreatedAt
  - Status


### Dashboard
- Total Active Bookings (Pending + Approved)
- Total Pending
- Total Approved
- Total Rejected
- Recent Bookings (5 terbaru)


### Room
- Melihat seluruh ruangan yang ada 
- Filter berdasarkan gedung


## Environment Configuration (Optional)

Template konfigurasi tersedia pada:

```
.env.example
```


## Folder Structure (Simplified)

```
BACKEND/
│
├── Controllers/
├── Services/
├── DTOs/
├── Models/
├── Migrations/
├── Data/
├── Enums/
├── appsettings.json
└── Program.cs
```

---

## Troubleshooting

Jika terjadi error saat menjalankan:

1. Pastikan .NET SDK sudah terinstall
2. Jalankan `dotnet restore`
3. Pastikan migration sudah dijalankan (`dotnet ef database update`)
4. Pastikan port 5023 tidak digunakan aplikasi lain

---

Backend siap digunakan.
