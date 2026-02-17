# Changelog

Semua perubahan penting pada backend akan didokumentasikan di file ini.

Project ini menggunakan Semantic Versioning.

---

## [1.0.0] - 2026-02-XX

### Added
- Implementasi Booking CRUD API
- Implementasi Room
- Implementasi Dashboard Summary API
- Endpoint ketersediaan ruangan
- Endpoint history booking
- Search, filter, dan sorting pada history
- Auto update status booking (Approved → Completed, Pending → Cancelled)
- Validasi konflik waktu (hanya Approved yang mengunci ruangan)
- DTO projection untuk mencegah circular reference
- Pagination response structure
- Swagger documentation

---

[1.0.0]: Initial stable release
