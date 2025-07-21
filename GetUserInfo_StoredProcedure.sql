-- Stored Procedure để lấy thông tin user
CREATE PROCEDURE GetUserInfo
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

-- Nếu stored procedure đã tồn tại, sử dụng ALTER
-- ALTER PROCEDURE GetUserInfo
--     @MaND INT
-- AS
-- BEGIN
--     SELECT 
--         MaND,
--         TenND,
--         Password,
--         Email,
--         SDT,
--         DiaChi,
--         ISNULL(NamSinh, '1900-01-01') as NamSinh,
--         ISNULL(GioiTinh, N'Không xác định') as GioiTinh,
--         ISNULL(HinhAnh, 'ic_avatar.png') as HinhAnh,
--         ISNULL(SoDuTK, 0) as SoDuTK,
--         ISNULL(MaGioiThieu, '') as MaGioiThieu
--     FROM NGUOIDUNG 
--     WHERE MaND = @MaND
-- END
-- GO 