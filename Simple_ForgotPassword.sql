-- =============================================
-- SCRIPT ĐƠN GIẢN TẠO CHỨC NĂNG QUÊN MẬT KHẨU
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
        IsUsed BIT NOT NULL DEFAULT 0
    )
    PRINT '✅ Đã tạo bảng RESET_TOKENS'
END
ELSE
BEGIN
    PRINT '✅ Bảng RESET_TOKENS đã tồn tại'
END
GO

-- 2. Tạo Stored Procedure CreateResetToken
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreateResetToken]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[CreateResetToken]
GO

CREATE PROCEDURE CreateResetToken
    @MaND INT,
    @Token NVARCHAR(10),
    @ExpiresInMinutes INT = 15
AS
BEGIN
    DELETE FROM RESET_TOKENS WHERE MaND = @MaND
    INSERT INTO RESET_TOKENS (MaND, Token, CreatedAt, ExpiresAt, IsUsed)
    VALUES (@MaND, @Token, GETDATE(), DATEADD(MINUTE, @ExpiresInMinutes, GETDATE()), 0)
    SELECT @@ROWCOUNT AS RowsAffected
END
GO
PRINT '✅ Đã tạo CreateResetToken'

-- 3. Tạo Stored Procedure ValidateResetToken
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ValidateResetToken]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[ValidateResetToken]
GO

CREATE PROCEDURE ValidateResetToken
    @PhoneNumber NVARCHAR(20),
    @Token NVARCHAR(10)
AS
BEGIN
    SELECT rt.MaND, rt.IsUsed, rt.ExpiresAt, nd.TenND
    FROM RESET_TOKENS rt
    INNER JOIN NGUOIDUNG nd ON rt.MaND = nd.MaND
    WHERE nd.SDT = @PhoneNumber 
        AND rt.Token = @Token 
        AND rt.IsUsed = 0 
        AND rt.ExpiresAt > GETDATE()
END
GO
PRINT '✅ Đã tạo ValidateResetToken'

-- 4. Tạo Stored Procedure ResetUserPassword
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ResetUserPassword]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[ResetUserPassword]
GO

CREATE PROCEDURE ResetUserPassword
    @MaND INT,
    @NewPassword NVARCHAR(255),
    @Token NVARCHAR(10)
AS
BEGIN
    BEGIN TRANSACTION
    UPDATE NGUOIDUNG SET Password = @NewPassword WHERE MaND = @MaND
    UPDATE RESET_TOKENS SET IsUsed = 1 WHERE MaND = @MaND AND Token = @Token
    COMMIT TRANSACTION
    SELECT @@ROWCOUNT AS RowsAffected
END
GO
PRINT '✅ Đã tạo ResetUserPassword'

-- 5. Tạo Indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RESET_TOKENS_MaND')
    CREATE INDEX IX_RESET_TOKENS_MaND ON RESET_TOKENS(MaND)
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RESET_TOKENS_Token')
    CREATE INDEX IX_RESET_TOKENS_Token ON RESET_TOKENS(Token)
GO

PRINT '✅ Đã tạo Indexes'

-- 6. Kiểm tra kết quả
SELECT 'RESET_TOKENS' AS TableName, COUNT(*) AS RecordCount FROM RESET_TOKENS
UNION ALL
SELECT 'NGUOIDUNG' AS TableName, COUNT(*) AS RecordCount FROM NGUOIDUNG

PRINT '🎉 HOÀN TẤT! Chức năng quên mật khẩu đã sẵn sàng!' 