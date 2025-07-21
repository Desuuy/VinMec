-- Stored Procedure để cập nhật thông tin user
CREATE PROCEDURE UpdateUserInfo
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

-- Nếu stored procedure đã tồn tại, sử dụng ALTER
-- ALTER PROCEDURE UpdateUserInfo
--     @MaND INT,
--     @TenND NVARCHAR(255),
--     @Email NVARCHAR(255),
--     @NamSinh DATE,
--     @GioiTinh NVARCHAR(50),
--     @DiaChi NVARCHAR(500),
--     @HinhAnh NVARCHAR(255)
-- AS
-- BEGIN
--     BEGIN TRY
--         IF NOT EXISTS (SELECT 1 FROM NGUOIDUNG WHERE MaND = @MaND)
--         BEGIN
--             RAISERROR ('User không tồn tại', 16, 1)
--             RETURN
--         END
--         
--         UPDATE NGUOIDUNG 
--         SET 
--             TenND = @TenND,
--             Email = @Email,
--             NamSinh = @NamSinh,
--             GioiTinh = @GioiTinh,
--             DiaChi = @DiaChi,
--             HinhAnh = @HinhAnh
--         WHERE MaND = @MaND
--         
--         SELECT 
--             MaND,
--             TenND,
--             Email,
--             NamSinh,
--             GioiTinh,
--             DiaChi,
--             HinhAnh
--         FROM NGUOIDUNG 
--         WHERE MaND = @MaND
--         
--     END TRY
--     BEGIN CATCH
--         DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
--         DECLARE @ErrorSeverity INT = ERROR_SEVERITY()
--         DECLARE @ErrorState INT = ERROR_STATE()
--         
--         RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState)
--     END CATCH
-- END
-- GO 