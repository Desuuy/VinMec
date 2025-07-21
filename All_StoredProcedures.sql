-- =============================================
-- TẤT CẢ STORED PROCEDURES CHO CHỨC NĂNG ĐỔI MẬT KHẨU VÀ CẬP NHẬT THÔNG TIN
-- =============================================

-- 1. Stored Procedure để lấy thông tin user
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetUserInfo]') AND type in (N'P', N'PC'))
BEGIN
    EXEC('CREATE PROCEDURE [dbo].[GetUserInfo] AS BEGIN SELECT 1 END')
END
GO

ALTER PROCEDURE GetUserInfo
    @MaND INT
AS
BEGIN
    -- Lấy thông tin cơ bản của user
    SELECT 
        MaND,
        TenND,
        Password,
        Email,
        SDT,
        DiaChi,
        ISNULL(NamSinh, '1900-01-01') as NamSinh,
        ISNULL(GioiTinh, N'Không xác định') as GioiTinh,
        ISNULL(HinhAnh, 'ic_avatar.png') as HinhAnh,
        ISNULL(SoDuTK, 0) as SoDuTK,
        ISNULL(MaGioiThieu, '') as MaGioiThieu
    FROM NGUOIDUNG 
    WHERE MaND = @MaND
END
GO

-- 2. Stored Procedure để kiểm tra mật khẩu hiện tại
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CheckCurrentPassword]') AND type in (N'P', N'PC'))
BEGIN
    EXEC('CREATE PROCEDURE [dbo].[CheckCurrentPassword] AS BEGIN SELECT 1 END')
END
GO

ALTER PROCEDURE CheckCurrentPassword
    @MaND INT,
    @CurrentPassword NVARCHAR(255)
AS
BEGIN
    SELECT MaND, TenND, Password
    FROM NGUOIDUNG
    WHERE MaND = @MaND AND Password = @CurrentPassword
END
GO

-- 3. Stored Procedure để cập nhật mật khẩu mới
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdatePassword]') AND type in (N'P', N'PC'))
BEGIN
    EXEC('CREATE PROCEDURE [dbo].[UpdatePassword] AS BEGIN SELECT 1 END')
END
GO

ALTER PROCEDURE UpdatePassword
    @MaND INT,
    @NewPassword NVARCHAR(255)
AS
BEGIN
    UPDATE NGUOIDUNG
    SET Password = @NewPassword
    WHERE MaND = @MaND
    
    SELECT @@ROWCOUNT AS RowsAffected
END
GO

-- 4. Stored Procedure để cập nhật thông tin user
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateUserInfo]') AND type in (N'P', N'PC'))
BEGIN
    EXEC('CREATE PROCEDURE [dbo].[UpdateUserInfo] AS BEGIN SELECT 1 END')
END
GO

ALTER PROCEDURE UpdateUserInfo
    @MaND INT,
    @TenND NVARCHAR(255),
    @Email NVARCHAR(255),
    @NamSinh DATE,
    @GioiTinh NVARCHAR(50),
    @DiaChi NVARCHAR(500),
    @HinhAnh NVARCHAR(255)
AS
BEGIN
    BEGIN TRY
        -- Kiểm tra xem user có tồn tại không
        IF NOT EXISTS (SELECT 1 FROM NGUOIDUNG WHERE MaND = @MaND)
        BEGIN
            RAISERROR ('User không tồn tại', 16, 1)
            RETURN
        END
        
        -- Cập nhật thông tin user
        UPDATE NGUOIDUNG 
        SET 
            TenND = @TenND,
            Email = @Email,
            NamSinh = @NamSinh,
            GioiTinh = @GioiTinh,
            DiaChi = @DiaChi,
            HinhAnh = @HinhAnh
        WHERE MaND = @MaND
        
        -- Trả về thông tin đã cập nhật
        SELECT 
            MaND,
            TenND,
            Email,
            NamSinh,
            GioiTinh,
            DiaChi,
            HinhAnh
        FROM NGUOIDUNG 
        WHERE MaND = @MaND
        
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY()
        DECLARE @ErrorState INT = ERROR_STATE()
        
        RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState)
    END CATCH
END
GO

-- =============================================
-- KIỂM TRA CẤU TRÚC BẢNG NGUOIDUNG
-- =============================================

-- Kiểm tra xem các cột cần thiết có tồn tại không
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'NGUOIDUNG'
ORDER BY ORDINAL_POSITION

-- =============================================
-- THÊM CÁC CỘT NẾU CHƯA TỒN TẠI
-- =============================================

-- Thêm cột NamSinh nếu chưa có
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'NGUOIDUNG' AND COLUMN_NAME = 'NamSinh')
BEGIN
    ALTER TABLE NGUOIDUNG ADD NamSinh DATE NULL
    PRINT 'Đã thêm cột NamSinh'
END

-- Thêm cột GioiTinh nếu chưa có
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'NGUOIDUNG' AND COLUMN_NAME = 'GioiTinh')
BEGIN
    ALTER TABLE NGUOIDUNG ADD GioiTinh NVARCHAR(50) NULL
    PRINT 'Đã thêm cột GioiTinh'
END

-- Thêm cột HinhAnh nếu chưa có
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'NGUOIDUNG' AND COLUMN_NAME = 'HinhAnh')
BEGIN
    ALTER TABLE NGUOIDUNG ADD HinhAnh NVARCHAR(255) NULL
    PRINT 'Đã thêm cột HinhAnh'
END

-- Thêm cột MaGioiThieu nếu chưa có
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'NGUOIDUNG' AND COLUMN_NAME = 'MaGioiThieu')
BEGIN
    ALTER TABLE NGUOIDUNG ADD MaGioiThieu NVARCHAR(50) NULL
    PRINT 'Đã thêm cột MaGioiThieu'
END

PRINT 'Hoàn tất cài đặt tất cả stored procedures và cấu trúc bảng!' 