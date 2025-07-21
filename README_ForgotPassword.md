# 🔐 HƯỚNG DẪN CHỨC NĂNG QUÊN MẬT KHẨU

## 📋 **Tổng quan**
Chức năng quên mật khẩu cho phép người dùng đặt lại mật khẩu khi quên mật khẩu cũ. Hệ thống sẽ tạo mã xác thực 6 số và hiển thị trên màn hình.

## 🚀 **Cách sử dụng**

### **Bước 1: Truy cập chức năng quên mật khẩu**
1. Vào trang **Đăng nhập** (`/Home/Login`)
2. Click vào link **"Quên mật khẩu?"** dưới trường mật khẩu

### **Bước 2: Nhập số điện thoại**
1. Nhập số điện thoại đã đăng ký trong hệ thống
2. Click **"Gửi mã xác thực"**
3. Hệ thống sẽ hiển thị mã xác thực 6 số (có hiệu lực 15 phút)

### **Bước 3: Đặt lại mật khẩu**
1. Nhập mã xác thực 6 số
2. Nhập mật khẩu mới (tối thiểu 6 ký tự)
3. Xác nhận mật khẩu mới
4. Click **"Đặt lại mật khẩu"**

### **Bước 4: Đăng nhập**
1. Quay lại trang đăng nhập
2. Đăng nhập với mật khẩu mới

## 🛠️ **Cài đặt Database**

### **Chạy SQL Script**
1. Mở **SQL Server Management Studio**
2. Kết nối đến database **VOV**
3. Mở file `ForgotPassword_Database.sql`
4. Chạy toàn bộ script

### **Kiểm tra kết quả**
Script sẽ tạo:
- ✅ Bảng `RESET_TOKENS`
- ✅ Stored Procedure `CreateResetToken`
- ✅ Stored Procedure `ValidateResetToken`
- ✅ Stored Procedure `ResetUserPassword`
- ✅ Indexes để tối ưu hiệu suất

## 📁 **Files đã thêm/sửa**

### **Controllers**
- `Controllers/HomeController.cs` - Thêm 4 actions:
  - `ForgotPassword()` - Hiển thị trang quên mật khẩu
  - `ForgotPasswordProcess()` - Xử lý yêu cầu quên mật khẩu
  - `ResetPassword()` - Hiển thị trang đặt lại mật khẩu
  - `ResetPasswordProcess()` - Xử lý đặt lại mật khẩu

### **Views**
- `Views/Home/ForgotPassword.cshtml` - Trang nhập số điện thoại
- `Views/Home/ResetPassword.cshtml` - Trang đặt lại mật khẩu
- `Views/Home/Login.cshtml` - Thêm link "Quên mật khẩu?"

### **CSS**
- `wwwroot/css/Dangky.css` - Thêm styles cho link quên mật khẩu

### **Database**
- `ForgotPassword_Database.sql` - Script tạo bảng và stored procedures

## 🔒 **Bảo mật**

### **Token Security**
- ✅ Token 6 số ngẫu nhiên
- ✅ Thời hạn 15 phút
- ✅ Chỉ sử dụng 1 lần
- ✅ Tự động xóa token cũ

### **Validation**
- ✅ Kiểm tra số điện thoại tồn tại
- ✅ Validate mật khẩu mới (tối thiểu 6 ký tự)
- ✅ Xác nhận mật khẩu khớp
- ✅ Kiểm tra token hợp lệ và chưa hết hạn

## 🎨 **Giao diện**

### **Features**
- ✅ Responsive design
- ✅ Real-time validation
- ✅ Thông báo lỗi/thành công đẹp mắt
- ✅ Auto-hide alerts sau 5 giây
- ✅ Smooth scrolling
- ✅ Bootstrap styling

### **UX Improvements**
- ✅ Chỉ cho phép nhập số cho token
- ✅ Hiển thị thời hạn token
- ✅ Link quay lại dễ dàng
- ✅ Loading states (có thể thêm)

## 🐛 **Troubleshooting**

### **Lỗi thường gặp**

**1. "Số điện thoại không tồn tại"**
- Kiểm tra số điện thoại đã đăng ký chưa
- Đảm bảo format số điện thoại đúng

**2. "Mã xác thực không hợp lệ"**
- Kiểm tra mã 6 số đã nhập đúng chưa
- Token có thể đã hết hạn (15 phút)
- Token đã được sử dụng

**3. "Mật khẩu xác nhận không khớp"**
- Đảm bảo nhập lại mật khẩu mới chính xác

**4. Lỗi database**
- Chạy lại script `ForgotPassword_Database.sql`
- Kiểm tra kết nối database

### **Debug**
- Kiểm tra Console browser để xem lỗi JavaScript
- Kiểm tra SQL Server logs
- Xem TempData messages trong controller

## 🔄 **Maintenance**

### **Dọn dẹp token cũ**
```sql
-- Xóa token hết hạn hoặc đã sử dụng
DELETE FROM RESET_TOKENS 
WHERE ExpiresAt < GETDATE() OR IsUsed = 1
```

### **Tạo SQL Agent Job (tùy chọn)**
```sql
-- Chạy hàng ngày để dọn dẹp
CREATE PROCEDURE CleanupExpiredTokens
AS
BEGIN
    DELETE FROM RESET_TOKENS 
    WHERE ExpiresAt < GETDATE() OR IsUsed = 1
END
```

## 📞 **Support**
Nếu gặp vấn đề, vui lòng:
1. Kiểm tra logs trong Console
2. Xem thông báo lỗi chi tiết
3. Chạy lại script database
4. Restart ứng dụng

---
**Phiên bản:** 1.0  
**Ngày tạo:** $(Get-Date -Format "dd/MM/yyyy")  
**Tác giả:** AI Assistant 