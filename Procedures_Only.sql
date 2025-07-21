-- =============================================
-- STORED PROCEDURES CHO CHỨC NĂNG QUÊN MẬT KHẨU
-- =============================================

-- 1. Tạo bảng RESET_TOKENS (nếu chưa có)
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
GO

-- 2. Stored Procedure: Tạo token reset password
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreateResetToken]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[CreateResetToken]
GO

CREATE PROCEDURE CreateResetToken
    @MaND INT,
    @Token NVARCHAR(10),
    @ExpiresInMinutes INT = 15
AS
BEGIN
    -- Xóa token cũ nếu có
    DELETE FROM RESET_TOKENS WHERE MaND = @MaND
    
    -- Tạo token mới
    INSERT INTO RESET_TOKENS (MaND, Token, CreatedAt, ExpiresAt, IsUsed)
    VALUES (@MaND, @Token, GETDATE(), DATEADD(MINUTE, @ExpiresInMinutes, GETDATE()), 0)
    
    SELECT @@ROWCOUNT AS RowsAffected
END
GO
PRINT '✅ Đã tạo CreateResetToken'

-- 3. Stored Procedure: Kiểm tra token hợp lệ
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ValidateResetToken]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[ValidateResetToken]
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
PRINT '✅ Đã tạo ValidateResetToken'

-- 4. Stored Procedure: Reset password
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
END
GO
PRINT '✅ Đã tạo ResetUserPassword'

-- 5. Tạo Indexes để tối ưu hiệu suất
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RESET_TOKENS_MaND')
    CREATE INDEX IX_RESET_TOKENS_MaND ON RESET_TOKENS(MaND)
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RESET_TOKENS_Token')
    CREATE INDEX IX_RESET_TOKENS_Token ON RESET_TOKENS(Token)
GO

PRINT '✅ Đã tạo Indexes'

-- 6. Test procedures
PRINT '--- TEST PROCEDURES ---'

-- Test tạo token
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
PRINT '🎉 HOÀN TẤT! 3 STORED PROCEDURES ĐÃ SẴN SÀNG!'
PRINT '============================================='
PRINT '1. CreateResetToken - Tạo token reset'
PRINT '2. ValidateResetToken - Kiểm tra token'
PRINT '3. ResetUserPassword - Đặt lại mật khẩu'
PRINT '🚀 Bạn có thể test chức năng quên mật khẩu ngay!' 