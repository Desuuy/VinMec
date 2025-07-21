-- =============================================
-- TẠO BẢNG VÀ STORED PROCEDURES CHO CHỨC NĂNG QUÊN MẬT KHẨU
-- =============================================

-- 1. Tạo bảng RESET_TOKENS
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RESET_TOKENS]') AND type in (N'U'))
BEGIN
    CREATE TABLE RESET_TOKENS (
        ID INT IDENTITY(1,1) PRIMARY KEY,
        MaND INT NOT NULL,
        Token NVARCHAR(10) NOT NULL,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        ExpiresAt DATETIME NOT NULL,
        IsUsed BIT NOT NULL DEFAULT 0,
        FOREIGN KEY (MaND) REFERENCES NGUOIDUNG(MaND)
    )
    PRINT 'Đã tạo bảng RESET_TOKENS'
END
ELSE
BEGIN
    PRINT 'Bảng RESET_TOKENS đã tồn tại'
END
GO

-- 2. Tạo stored procedure để tạo token reset
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreateResetToken]') AND type in (N'P', N'PC'))
BEGIN
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

-- 3. Tạo stored procedure để kiểm tra token
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ValidateResetToken]') AND type in (N'P', N'PC'))
BEGIN
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

-- 4. Tạo stored procedure để reset password
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ResetUserPassword]') AND type in (N'P', N'PC'))
BEGIN
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

-- 5. Tạo index để tối ưu hiệu suất
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RESET_TOKENS_MaND')
BEGIN
    CREATE INDEX IX_RESET_TOKENS_MaND ON RESET_TOKENS(MaND)
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RESET_TOKENS_Token')
BEGIN
    CREATE INDEX IX_RESET_TOKENS_Token ON RESET_TOKENS(Token)
END

-- 6. Tạo job để xóa token hết hạn (tùy chọn)
-- Có thể tạo SQL Agent Job để chạy hàng ngày:
-- DELETE FROM RESET_TOKENS WHERE ExpiresAt < GETDATE() OR IsUsed = 1

PRINT 'Hoàn tất tạo database cho chức năng quên mật khẩu!'
PRINT 'Các stored procedures đã được tạo:'
PRINT '- CreateResetToken'
PRINT '- ValidateResetToken' 
PRINT '- ResetUserPassword' 