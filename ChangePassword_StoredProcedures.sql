-- Stored Procedure để kiểm tra mật khẩu hiện tại
CREATE PROCEDURE CheckCurrentPassword
    @MaND INT,
    @CurrentPassword NVARCHAR(255)
AS
BEGIN
    SELECT MaND, TenND, Password
    FROM NGUOIDUNG
    WHERE MaND = @MaND AND Password = @CurrentPassword
END
GO

-- Stored Procedure để cập nhật mật khẩu mới
CREATE PROCEDURE UpdatePassword
    @MaND INT,
    @NewPassword NVARCHAR(255)
AS
BEGIN
    UPDATE NGUOIDUNG
    SET Password = @NewPassword
    WHERE MaND = @MaND
    
    -- Trả về số dòng bị ảnh hưởng để kiểm tra xem có cập nhật thành công không
    SELECT @@ROWCOUNT AS RowsAffected
END
GO

-- Nếu các stored procedures đã tồn tại, sử dụng ALTER thay vì CREATE
-- ALTER PROCEDURE CheckCurrentPassword
--     @MaND INT,
--     @CurrentPassword NVARCHAR(255)
-- AS
-- BEGIN
--     SELECT MaND, TenND, Password
--     FROM NGUOIDUNG
--     WHERE MaND = @MaND AND Password = @CurrentPassword
-- END
-- GO

-- ALTER PROCEDURE UpdatePassword
--     @MaND INT,
--     @NewPassword NVARCHAR(255)
-- AS
-- BEGIN
--     UPDATE NGUOIDUNG
--     SET Password = @NewPassword
--     WHERE MaND = @MaND
    
--     SELECT @@ROWCOUNT AS RowsAffected
-- END
-- GO 