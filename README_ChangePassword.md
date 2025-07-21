# Hướng dẫn sử dụng chức năng Đổi mật khẩu

## Tổng quan
Chức năng đổi mật khẩu đã được thêm vào ứng dụng website đặt lịch khám bệnh. Người dùng có thể thay đổi mật khẩu của mình một cách an toàn và dễ dàng.

## Các tính năng

### 1. Giao diện người dùng
- **Trang đổi mật khẩu**: Giao diện hiện đại với thiết kế responsive
- **Hiển thị độ mạnh mật khẩu**: Thanh progress bar hiển thị độ mạnh của mật khẩu mới
- **Hiển thị/ẩn mật khẩu**: Nút toggle để hiển thị hoặc ẩn mật khẩu
- **Validation real-time**: Kiểm tra mật khẩu xác nhận ngay khi nhập

### 2. Bảo mật
- **Kiểm tra mật khẩu hiện tại**: Xác minh mật khẩu hiện tại trước khi cho phép đổi
- **Validation mật khẩu**: Kiểm tra độ dài tối thiểu (6 ký tự)
- **Xác nhận mật khẩu**: Yêu cầu nhập lại mật khẩu mới để tránh lỗi
- **Stored Procedures**: Sử dụng stored procedures để bảo mật database

### 3. Trải nghiệm người dùng
- **Thông báo rõ ràng**: Hiển thị thông báo thành công hoặc lỗi
- **Navigation dễ dàng**: Nút quay lại trang cá nhân
- **Responsive design**: Hoạt động tốt trên mọi thiết bị

## Cách sử dụng

### Bước 1: Truy cập trang đổi mật khẩu
1. Đăng nhập vào hệ thống
2. Vào trang "Trang cá nhân"
3. Nhấn nút "Đổi mật khẩu"

### Bước 2: Nhập thông tin
1. **Mật khẩu hiện tại**: Nhập mật khẩu đang sử dụng
2. **Mật khẩu mới**: Nhập mật khẩu mới (tối thiểu 6 ký tự)
3. **Xác nhận mật khẩu**: Nhập lại mật khẩu mới

### Bước 3: Hoàn tất
1. Kiểm tra độ mạnh mật khẩu trên thanh progress
2. Nhấn nút "Đổi mật khẩu"
3. Nhận thông báo thành công

## Cài đặt Database

### Tạo Stored Procedures và Sửa lỗi Database
**QUAN TRỌNG**: Chạy file `All_StoredProcedures.sql` trong SQL Server Management Studio hoặc công cụ quản lý database để:
1. Tạo tất cả stored procedures cần thiết
2. Thêm các cột còn thiếu vào bảng NGUOIDUNG
3. Sửa lỗi "Invalid column name"

```sql
-- File All_StoredProcedures.sql sẽ tự động:
-- 1. Tạo tất cả stored procedures cần thiết
-- 2. Thêm các cột còn thiếu vào bảng NGUOIDUNG:
--    - NamSinh (DATE)
--    - GioiTinh (NVARCHAR(50))
--    - HinhAnh (NVARCHAR(255))
--    - MaGioiThieu (NVARCHAR(50))
-- 3. Kiểm tra và hiển thị cấu trúc bảng hiện tại
```

## Cấu trúc code

### Controller (HomeController.cs)
- `ChangePassword()`: Hiển thị trang đổi mật khẩu
- `ChangePasswordProcess()`: Xử lý logic đổi mật khẩu

### View (ChangePassword.cshtml)
- Giao diện người dùng với form đổi mật khẩu
- JavaScript validation và hiển thị độ mạnh mật khẩu

### Model (DataModel.cs)
- `ExecuteStoredProcedure()`: Thực thi stored procedures với tham số
- `ExecuteNonQuery()`: Thực thi stored procedures không trả về dữ liệu

## Lưu ý bảo mật

1. **Mật khẩu được lưu dưới dạng plain text**: Trong phiên bản hiện tại, mật khẩu được lưu dưới dạng text thường. Để bảo mật hơn, nên mã hóa mật khẩu bằng bcrypt hoặc các thuật toán hash khác.

2. **Validation phía client**: Mặc dù có validation JavaScript, vẫn cần validation phía server để đảm bảo an toàn.

3. **Session management**: Đảm bảo session được quản lý đúng cách để tránh tấn công session hijacking.

## Cải tiến có thể thực hiện

1. **Mã hóa mật khẩu**: Sử dụng bcrypt hoặc Argon2 để mã hóa mật khẩu
2. **Two-factor authentication**: Thêm xác thực 2 yếu tố
3. **Password history**: Lưu lịch sử mật khẩu để tránh sử dụng lại
4. **Email confirmation**: Gửi email xác nhận khi đổi mật khẩu
5. **Rate limiting**: Giới hạn số lần thử đổi mật khẩu

## Troubleshooting

### Lỗi thường gặp

1. **"Mật khẩu hiện tại không đúng"**
   - Kiểm tra lại mật khẩu hiện tại
   - Đảm bảo không có khoảng trắng thừa

2. **"Mật khẩu xác nhận không khớp"**
   - Nhập lại chính xác mật khẩu mới trong trường xác nhận

3. **"Mật khẩu phải có ít nhất 6 ký tự"**
   - Tăng độ dài mật khẩu mới

4. **Lỗi database**
   - Kiểm tra kết nối database
   - Đảm bảo stored procedures đã được tạo
   - Chạy file `All_StoredProcedures.sql` để thêm các cột còn thiếu

5. **Lỗi "Invalid column name"**
   - Chạy file `All_StoredProcedures.sql` để tự động thêm các cột còn thiếu
   - Các cột sẽ được thêm: NamSinh, GioiTinh, HinhAnh, MaGioiThieu

### Kiểm tra logs
- Xem logs trong Visual Studio Output window
- Kiểm tra SQL Server logs nếu có lỗi database 