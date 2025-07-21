-- =============================================
-- SỬA LỖI CỘT NAMSINH VÀ CẬP NHẬT STORED PROCEDURE
-- =============================================

-- 1. Kiểm tra cấu trúc bảng hiện tại
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'NGUOIDUNG'
ORDER BY ORDINAL_POSITION

-- 2. Thêm cột NamSinh nếu chưa có
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'NGUOIDUNG' AND COLUMN_NAME = 'NamSinh')
BEGIN
    ALTER TABLE NGUOIDUNG ADD NamSinh DATE NULL
    PRINT 'Đã thêm cột NamSinh'
END
ELSE
BEGIN
    PRINT 'Cột NamSinh đã tồn tại'
END

-- 3. Thêm cột GioiTinh nếu chưa có
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'NGUOIDUNG' AND COLUMN_NAME = 'GioiTinh')
BEGIN
    ALTER TABLE NGUOIDUNG ADD GioiTinh NVARCHAR(50) NULL
    PRINT 'Đã thêm cột GioiTinh'
END
ELSE
BEGIN
    PRINT 'Cột GioiTinh đã tồn tại'
END

-- 4. Thêm cột HinhAnh nếu chưa có
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'NGUOIDUNG' AND COLUMN_NAME = 'HinhAnh')
BEGIN
    ALTER TABLE NGUOIDUNG ADD HinhAnh NVARCHAR(255) NULL
    PRINT 'Đã thêm cột HinhAnh'
END
ELSE
BEGIN
    PRINT 'Cột HinhAnh đã tồn tại'
END

-- 5. Thêm cột MaGioiThieu nếu chưa có
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'NGUOIDUNG' AND COLUMN_NAME = 'MaGioiThieu')
BEGIN
    ALTER TABLE NGUOIDUNG ADD MaGioiThieu NVARCHAR(50) NULL
    PRINT 'Đã thêm cột MaGioiThieu'
END
ELSE
BEGIN
    PRINT 'Cột MaGioiThieu đã tồn tại'
END

-- 6. Cập nhật stored procedure GetUserInfo để xử lý trường hợp cột chưa tồn tại
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetUserInfo]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[GetUserInfo]
END
GO

CREATE PROCEDURE GetUserInfo
    @MaND INT
AS
BEGIN
    -- Kiểm tra xem các cột có tồn tại không
    DECLARE @HasNamSinh BIT = 0
    DECLARE @HasGioiTinh BIT = 0
    DECLARE @HasHinhAnh BIT = 0
    DECLARE @HasMaGioiThieu BIT = 0
    
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'NGUOIDUNG' AND COLUMN_NAME = 'NamSinh')
        SET @HasNamSinh = 1
    
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'NGUOIDUNG' AND COLUMN_NAME = 'GioiTinh')
        SET @HasGioiTinh = 1
    
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'NGUOIDUNG' AND COLUMN_NAME = 'HinhAnh')
        SET @HasHinhAnh = 1
    
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'NGUOIDUNG' AND COLUMN_NAME = 'MaGioiThieu')
        SET @HasMaGioiThieu = 1
    
    -- Tạo câu lệnh SQL động
    DECLARE @SQL NVARCHAR(MAX) = '
    SELECT 
        MaND,
        TenND,
        Password,
        Email,
        SDT,
        DiaChi,'
    
    IF @HasNamSinh = 1
        SET @SQL = @SQL + '
        ISNULL(NamSinh, NULL) as NamSinh,'
    ELSE
        SET @SQL = @SQL + '
        NULL as NamSinh,'
    
    IF @HasGioiTinh = 1
        SET @SQL = @SQL + '
        ISNULL(GioiTinh, N''Không xác định'') as GioiTinh,'
    ELSE
        SET @SQL = @SQL + '
        N''Không xác định'' as GioiTinh,'
    
    IF @HasHinhAnh = 1
        SET @SQL = @SQL + '
        ISNULL(HinhAnh, ''ic_avatar.png'') as HinhAnh,'
    ELSE
        SET @SQL = @SQL + '
        ''ic_avatar.png'' as HinhAnh,'
    
    SET @SQL = @SQL + '
        ISNULL(SoDuTK, 0) as SoDuTK,'
    
    IF @HasMaGioiThieu = 1
        SET @SQL = @SQL + '
        ISNULL(MaGioiThieu, '''') as MaGioiThieu'
    ELSE
        SET @SQL = @SQL + '
        '''' as MaGioiThieu'
    
    SET @SQL = @SQL + '
    FROM NGUOIDUNG 
    WHERE MaND = @MaND'
    
    -- Thực thi câu lệnh SQL động
    EXEC sp_executesql @SQL, N'@MaND INT', @MaND
END
GO

-- 7. Test stored procedure
PRINT 'Testing GetUserInfo stored procedure...'
EXEC GetUserInfo 1

PRINT 'Hoàn tất sửa lỗi NamSinh!' 