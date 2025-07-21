-- =============================================
-- STORED PROCEDURE UPDATEUSERINFO ĐÃ SỬA
-- =============================================

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateUserInfo]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[UpdateUserInfo]
END
GO

CREATE PROCEDURE UpdateUserInfo
    @MaND INT,
    @TenND NVARCHAR(255),
    @Email NVARCHAR(255),
    @NamSinh DATE = NULL,
    @GioiTinh NVARCHAR(50) = NULL,
    @DiaChi NVARCHAR(500) = NULL,
    @HinhAnh NVARCHAR(255) = NULL
AS
BEGIN
    BEGIN TRY
        -- Kiểm tra xem user có tồn tại không
        IF NOT EXISTS (SELECT 1 FROM NGUOIDUNG WHERE MaND = @MaND)
        BEGIN
            RAISERROR ('User không tồn tại', 16, 1)
            RETURN
        END
        
        -- Kiểm tra xem các cột có tồn tại không
        DECLARE @HasNamSinh BIT = 0
        DECLARE @HasGioiTinh BIT = 0
        DECLARE @HasHinhAnh BIT = 0
        
        IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'NGUOIDUNG' AND COLUMN_NAME = 'NamSinh')
            SET @HasNamSinh = 1
        
        IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'NGUOIDUNG' AND COLUMN_NAME = 'GioiTinh')
            SET @HasGioiTinh = 1
        
        IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'NGUOIDUNG' AND COLUMN_NAME = 'HinhAnh')
            SET @HasHinhAnh = 1
        
        -- Tạo câu lệnh UPDATE động
        DECLARE @SQL NVARCHAR(MAX) = 'UPDATE NGUOIDUNG SET '
        
        SET @SQL = @SQL + 'TenND = @TenND, '
        SET @SQL = @SQL + 'Email = @Email, '
        
        IF @HasNamSinh = 1
            SET @SQL = @SQL + 'NamSinh = @NamSinh, '
        
        IF @HasGioiTinh = 1
            SET @SQL = @SQL + 'GioiTinh = @GioiTinh, '
        
        SET @SQL = @SQL + 'DiaChi = @DiaChi, '
        
        IF @HasHinhAnh = 1
            SET @SQL = @SQL + 'HinhAnh = @HinhAnh '
        ELSE
            SET @SQL = @SQL + 'HinhAnh = @HinhAnh '
        
        SET @SQL = @SQL + 'WHERE MaND = @MaND'
        
        -- Thực thi câu lệnh UPDATE
        EXEC sp_executesql @SQL, 
            N'@MaND INT, @TenND NVARCHAR(255), @Email NVARCHAR(255), @NamSinh DATE, @GioiTinh NVARCHAR(50), @DiaChi NVARCHAR(500), @HinhAnh NVARCHAR(255)',
            @MaND, @TenND, @Email, @NamSinh, @GioiTinh, @DiaChi, @HinhAnh
        
        -- Trả về thông tin đã cập nhật
        EXEC GetUserInfo @MaND
        
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY()
        DECLARE @ErrorState INT = ERROR_STATE()
        
        RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState)
    END CATCH
END
GO

PRINT 'Đã cập nhật stored procedure UpdateUserInfo!' 