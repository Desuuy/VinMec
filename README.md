# 🏥 VinMec Healthcare Booking System

[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8.0-green.svg)](https://dotnet.microsoft.com/apps/aspnet)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2019+-orange.svg)](https://www.microsoft.com/en-us/sql-server)
[![License](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

## 📋 Mô tả

VinMec Healthcare Booking System là một ứng dụng web đặt lịch khám bệnh trực tuyến, được phát triển bằng ASP.NET Core 8.0. Hệ thống cho phép bệnh nhân đặt lịch khám với bác sĩ, quản lý hồ sơ bệnh án, và cung cấp các tính năng quản trị cho admin và bác sĩ.

## ✨ Tính năng chính

### 👥 Cho Bệnh nhân
- **Đăng ký/Đăng nhập**: Hệ thống xác thực an toàn
- **Đặt lịch khám**: Chọn bác sĩ, chuyên khoa, thời gian khám
- **Quản lý hồ sơ**: Xem lịch sử khám bệnh, hồ sơ cá nhân
- **Tìm kiếm bác sĩ**: Lọc theo chuyên khoa, bệnh viện
- **Đánh giá và phản hồi**: Gửi đánh giá sau khi khám
- **Dự đoán bệnh**: Tính năng AI dự đoán bệnh dựa trên triệu chứng
- **Quên mật khẩu**: Khôi phục mật khẩu qua email

### 👨‍⚕️ Cho Bác sĩ
- **Quản lý lịch hẹn**: Xem, xác nhận, hủy lịch hẹn
- **Hồ sơ bệnh nhân**: Xem thông tin chi tiết bệnh nhân
- **Thống kê doanh thu**: Theo dõi thu nhập
- **Thông báo**: Nhận thông báo về lịch hẹn mới
- **Quản lý cá nhân**: Cập nhật thông tin cá nhân

### 👨‍💼 Cho Admin
- **Quản lý người dùng**: Bệnh nhân, bác sĩ, admin
- **Quản lý bệnh viện**: Thêm, sửa, xóa bệnh viện
- **Quản lý chuyên khoa**: Quản lý các chuyên khoa y tế
- **Quản lý bài viết**: Đăng bài viết y tế
- **Thống kê tổng quan**: Báo cáo doanh thu, số lượng khám
- **Quản lý thanh toán**: Theo dõi các giao dịch

## 🛠️ Công nghệ sử dụng

### Backend
- **ASP.NET Core 8.0**: Framework web hiện đại
- **C#**: Ngôn ngữ lập trình chính
- **Entity Framework**: ORM cho database
- **SignalR**: Real-time communication
- **Session Management**: Quản lý phiên đăng nhập

### Frontend
- **HTML5/CSS3**: Giao diện người dùng
- **JavaScript/jQuery**: Tương tác client-side
- **Bootstrap**: Framework CSS responsive
- **Razor Views**: Template engine

### Database
- **SQL Server**: Hệ quản trị cơ sở dữ liệu
- **Stored Procedures**: Tối ưu hiệu suất truy vấn

### Khác
- **Microsoft.Data.SqlClient**: Kết nối database
- **Microsoft.AspNetCore.Session**: Quản lý session
- **Microsoft.AspNetCore.SignalR**: Real-time features

## 🚀 Cài đặt và chạy

### Yêu cầu hệ thống
- .NET 8.0 SDK
- SQL Server 2019 hoặc cao hơn
- Visual Studio 2022 hoặc VS Code

### Bước 1: Clone repository
```bash
git clone https://github.com/Desuuy/VinMec.git
cd VinMec
```

### Bước 2: Cấu hình database
1. Tạo database mới trong SQL Server
2. Chạy script `All_StoredProcedures.sql` để tạo stored procedures
3. Cập nhật connection string trong `appsettings.json`

### Bước 3: Cài đặt dependencies
```bash
dotnet restore
```

### Bước 4: Chạy ứng dụng
```bash
dotnet run
```

Ứng dụng sẽ chạy tại: `https://localhost:5001` hoặc `http://localhost:5000`

## 📁 Cấu trúc dự án

```
VinMec/
├── Controllers/          # Controllers xử lý logic
│   ├── HomeController.cs
│   ├── AdminController.cs
│   ├── BacsisController.cs
│   └── KhachHangAPI.cs
├── Models/              # Data models
│   ├── DataModel.cs
│   ├── DiseasePrediction.cs
│   └── PredictionResponse.cs
├── Views/               # Razor views
│   ├── Home/           # Views cho bệnh nhân
│   ├── Admin/          # Views cho admin
│   ├── Bacsis/         # Views cho bác sĩ
│   └── Shared/         # Layout và partial views
├── wwwroot/            # Static files
│   ├── css/
│   ├── js/
│   ├── images/
│   └── Uploads/
├── SQL Scripts/        # Database scripts
└── Program.cs          # Entry point
```

## 🔧 Cấu hình

### Connection String
Cập nhật connection string trong `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your_server;Database=VinMecDB;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

### Session Configuration
Session được cấu hình với timeout 30 phút trong `Program.cs`.

## 📊 Database Schema

Hệ thống sử dụng các bảng chính:
- **Users**: Thông tin người dùng
- **Doctors**: Thông tin bác sĩ
- **Patients**: Thông tin bệnh nhân
- **Appointments**: Lịch hẹn khám
- **Hospitals**: Thông tin bệnh viện
- **Specialties**: Chuyên khoa
- **Articles**: Bài viết y tế

## 🔐 Bảo mật

- **Session Management**: Quản lý phiên đăng nhập an toàn
- **Password Hashing**: Mã hóa mật khẩu
- **Input Validation**: Kiểm tra dữ liệu đầu vào
- **HTTPS**: Sử dụng HTTPS trong production

## 🤝 Đóng góp

1. Fork dự án
2. Tạo feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit thay đổi (`git commit -m 'Add some AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Mở Pull Request

## 📝 License

Dự án này được phân phối dưới giấy phép MIT. Xem file `LICENSE` để biết thêm chi tiết.

## 👨‍💻 Tác giả

**Desuuy** - [GitHub](https://github.com/Desuuy)

## 📞 Liên hệ

- GitHub: [https://github.com/Desuuy]
- Project Link: [https://github.com/Desuuy/VinMec]

## 🙏 Cảm ơn

Cảm ơn bạn đã quan tâm đến dự án VinMec Healthcare Booking System!

---

⭐ Nếu dự án này hữu ích, hãy cho chúng tôi một star trên GitHub!
