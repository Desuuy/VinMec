-- =============================================
-- KIỂM TRA VÀ THÊM CHỨC NĂNG QUÊN MẬT KHẨU VÀO DATABASE
-- =============================================

-- Kiểm tra database hiện tại
SELECT DB_NAME() AS CurrentDatabase
GO

-- 1. Kiểm tra bảng RESET_TOKENS có tồn tại không
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RESET_TOKENS]') AND type in (N'U'))
BEGIN
    PRINT '✅ Bảng RESET_TOKENS đã tồn tại'
    
    -- Hiển thị thông tin bảng
    SELECT 
        COLUMN_NAME,
        DATA_TYPE,
        IS_NULLABLE,
        COLUMN_DEFAULT
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'RESET_TOKENS'
    ORDER BY ORDINAL_POSITION;
END
ELSE
BEGIN
    PRINT '❌ Bảng RESET_TOKENS chưa tồn tại - Đang tạo...'
    
    -- Tạo bảng RESET_TOKENS
    CREATE TABLE RESET_TOKENS (
        ID INT IDENTITY(1,1) PRIMARY KEY,
        MaND INT NOT NULL,
        Token NVARCHAR(10) NOT NULL,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        ExpiresAt DATETIME NOT NULL,
        IsUsed BIT NOT NULL DEFAULT 0,
        FOREIGN KEY (MaND) REFERENCES NGUOIDUNG(MaND)
    )
    
    PRINT '✅ Đã tạo bảng RESET_TOKENS thành công!'
END
GO

-- 2. Kiểm tra và tạo Stored Procedures
PRINT '--- KIỂM TRA STORED PROCEDURES ---'

-- CreateResetToken
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreateResetToken]') AND type in (N'P', N'PC'))
BEGIN
    PRINT '✅ Stored Procedure CreateResetToken đã tồn tại'
    DROP PROCEDURE [dbo].[CreateResetToken]
END
GO

CREATE PROCEDURE CreateResetToken
    @MaND INT,
    @Token NVARCHAR(10),
    @ExpiresInMinutes INT = 15
AS
BEGIN
    BEGIN TRY
        -- Xóa token cũ nếu có
        DELETE FROM RESET_TOKENS WHERE MaND = @MaND
        
        -- Tạo token mới
        INSERT INTO RESET_TOKENS (MaND, Token, CreatedAt, ExpiresAt, IsUsed)
        VALUES (@MaND, @Token, GETDATE(), DATEADD(MINUTE, @ExpiresInMinutes, GETDATE()), 0)
        
        SELECT @@ROWCOUNT AS RowsAffected
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        RAISERROR (@ErrorMessage, 16, 1)
    END CATCH
END
GO
PRINT '✅ Đã tạo Stored Procedure CreateResetToken'

-- ValidateResetToken
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ValidateResetToken]') AND type in (N'P', N'PC'))
BEGIN
    PRINT '✅ Stored Procedure ValidateResetToken đã tồn tại'
    DROP PROCEDURE [dbo].[ValidateResetToken]
END
GO

CREATE PROCEDURE ValidateResetToken
    @PhoneNumber NVARCHAR(20),
    @Token NVARCHAR(10)
AS
BEGIN
    SELECT 
        rt.MaND,
        rt.IsUsed,
        rt.ExpiresAt,
        nd.TenND
    FROM RESET_TOKENS rt
    INNER JOIN NGUOIDUNG nd ON rt.MaND = nd.MaND
    WHERE nd.SDT = @PhoneNumber 
        AND rt.Token = @Token 
        AND rt.IsUsed = 0 
        AND rt.ExpiresAt > GETDATE()
END
GO
PRINT '✅ Đã tạo Stored Procedure ValidateResetToken'

-- ResetUserPassword
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ResetUserPassword]') AND type in (N'P', N'PC'))
BEGIN
    PRINT '✅ Stored Procedure ResetUserPassword đã tồn tại'
    DROP PROCEDURE [dbo].[ResetUserPassword]
END
GO

CREATE PROCEDURE ResetUserPassword
    @MaND INT,
    @NewPassword NVARCHAR(255),
    @Token NVARCHAR(10)
AS
BEGIN
    BEGIN TRY
        BEGIN TRANSACTION
        
        -- Cập nhật mật khẩu mới
        UPDATE NGUOIDUNG 
        SET Password = @NewPassword 
        WHERE MaND = @MaND
        
        -- Đánh dấu token đã sử dụng
        UPDATE RESET_TOKENS 
        SET IsUsed = 1 
        WHERE MaND = @MaND AND Token = @Token
        
        COMMIT TRANSACTION
        
        SELECT @@ROWCOUNT AS RowsAffected
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE()
        RAISERROR (@ErrorMessage, 16, 1)
    END CATCH
END
GO
PRINT '✅ Đã tạo Stored Procedure ResetUserPassword'

-- 3. Tạo Indexes để tối ưu hiệu suất
PRINT '--- TẠO INDEXES ---'

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RESET_TOKENS_MaND')
BEGIN
    CREATE INDEX IX_RESET_TOKENS_MaND ON RESET_TOKENS(MaND)
    PRINT '✅ Đã tạo Index IX_RESET_TOKENS_MaND'
END
ELSE
BEGIN
    PRINT '✅ Index IX_RESET_TOKENS_MaND đã tồn tại'
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RESET_TOKENS_Token')
BEGIN
    CREATE INDEX IX_RESET_TOKENS_Token ON RESET_TOKENS(Token)
    PRINT '✅ Đã tạo Index IX_RESET_TOKENS_Token'
END
ELSE
BEGIN
    PRINT '✅ Index IX_RESET_TOKENS_Token đã tồn tại'
END

-- 4. Kiểm tra dữ liệu mẫu
PRINT '--- KIỂM TRA DỮ LIỆU ---'

-- Kiểm tra bảng NGUOIDUNG
DECLARE @TotalUsers INT
SELECT @TotalUsers = COUNT(*) FROM NGUOIDUNG
PRINT '📊 Tổng số người dùng trong hệ thống: ' + CAST(@TotalUsers AS NVARCHAR(10))

-- Kiểm tra bảng RESET_TOKENS
DECLARE @TotalTokens INT
SELECT @TotalTokens = COUNT(*) FROM RESET_TOKENS
PRINT '🔑 Tổng số token reset: ' + CAST(@TotalTokens AS NVARCHAR(10))

-- 5. Test Stored Procedures
PRINT '--- TEST STORED PROCEDURES ---'

-- Test tạo token (thay MaND = 1 nếu có user)
DECLARE @TestMaND INT = 1
DECLARE @TestToken NVARCHAR(10) = '123456'

-- Kiểm tra user có tồn tại không
IF EXISTS (SELECT 1 FROM NGUOIDUNG WHERE MaND = @TestMaND)
BEGIN
    EXEC CreateResetToken @MaND = @TestMaND, @Token = @TestToken
    PRINT '✅ Test tạo token thành công cho MaND = ' + CAST(@TestMaND AS NVARCHAR(10))
    
    -- Xóa token test
    DELETE FROM RESET_TOKENS WHERE MaND = @TestMaND AND Token = @TestToken
    PRINT '🧹 Đã xóa token test'
END
ELSE
BEGIN
    PRINT '⚠️ Không có user với MaND = ' + CAST(@TestMaND AS NVARCHAR(10)) + ' để test'
END

PRINT '============================================='
PRINT '🎉 HOÀN TẤT CÀI ĐẶT CHỨC NĂNG QUÊN MẬT KHẨU!'
PRINT '============================================='
PRINT '✅ Bảng RESET_TOKENS đã sẵn sàng'
PRINT '✅ 3 Stored Procedures đã được tạo'
PRINT '✅ Indexes đã được tối ưu'
PRINT '🚀 Bạn có thể test chức năng quên mật khẩu ngay!' 