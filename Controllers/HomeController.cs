using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using DoAnCNPM.Models;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
namespace DoAnCNPM.Controllers;
using System.Collections;
using System.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using IOF = System.IO.File;


public class HomeController : Controller
{
    
    private readonly ILogger<HomeController> _logger;
    private readonly ISession _session;

    public HomeController(ILogger<HomeController> logger, IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _session = httpContextAccessor.HttpContext.Session;
    }

    public IActionResult LayoutShare(){
        var taikhoan = _session.GetString("taikhoan");
        ViewData["TaiKhoan"] = taikhoan;

        DataModel db = new DataModel();
        ViewBag.listKB = db.get("EXEC getAllKhoaBenh");

        if (!string.IsNullOrEmpty(taikhoan))
        {
            var MaND = taikhoan;
            ViewBag.ListTB = db.get($"EXEC sp_GetThongBaoByMaND @MaND = {MaND}");
        }
        return View();
    }

    public IActionResult Index()
    {
        LayoutShare();
        DataModel db =new DataModel();
        ViewBag.ListBV = db.get("EXEC getAllBenhVien");
        ViewBag.ListBS5 = db.get("EXEC GetTop7Doctors");
                
        return View();
    }
    public IActionResult Article(string MaBV)
    {
        LayoutShare();

        DataModel db = new DataModel();

        // Sử dụng tham số hóa để tránh lỗi
        ViewBag.ListBV = db.get($"EXEC getBaiVietByIDMaKhoaBenh {MaBV}");

        return View();
    }
    public IActionResult ListArticleLoaiBV(string MaLBV)
    {
        LayoutShare();

        DataModel db = new DataModel();

        // Sử dụng tham số hóa để tránh lỗi
        ViewBag.ListLBV = db.get($"EXEC getBaiVietByIDMaLoai {MaLBV}");

        return View();
    }
    public IActionResult ArticleLoaiBV(string MaBV)
    {
        LayoutShare();

        DataModel db = new DataModel();

        // Sử dụng tham số hóa để tránh lỗi
        ViewBag.ListBV = db.get($"EXEC getBaiVietByID {MaBV}");

        return View();
    }

    
    // ---------- ĐĂNG NHẬP ------------//
    public IActionResult Login()
    {
        LayoutShare();
        return View();
    }
    [HttpPost]
    public IActionResult LoginProcess(string sdt, string password)
    {
        DataModel db = new DataModel();
        var list = db.get("EXEC CheckLogin '" + sdt + "','" + password + "'");
        if (list.Count > 0 && list[0] is ArrayList arrayList && arrayList.Count > 1)
        {
            // Lấy MaND từ kết quả
            var userInfo = arrayList[0]?.ToString() ?? "Unknown";
            // Kiểm tra xác thực email
            var emailVerified = db.get($"SELECT IsEmailVerified FROM NGUOIDUNG WHERE MaND = '{userInfo}'");
            if (emailVerified != null && emailVerified.Count > 0 && ((ArrayList)emailVerified[0])[0].ToString() == "0")
            {
                TempData["ErrorMessage"] = "Tài khoản chưa xác thực email. Vui lòng kiểm tra email để xác thực.";
                return RedirectToAction("VerifyEmail", "Home");
            }
            _session.SetString("taikhoan", userInfo);
            return RedirectToAction("Index", "Home");
        }
        else
        {
            return RedirectToAction("Login", "Home");
        }
    }

     // -------- REGISTER ----------//
    public IActionResult Register()
    {
        LayoutShare();
        return View();
    }
    [HttpPost]
    public IActionResult RegisterProcess(string TenND, string Password, string sdt, string Email)
    {
        DataModel db = new DataModel();
        // Đăng ký user với IsEmailVerified = 0
        var list = db.get($"EXEC REGISTER N'{TenND}', '{Password}', '{sdt}', '{Email}', 0");
        // Lấy MaND mới nhất theo email và sdt
        var user = db.get($"SELECT TOP 1 MaND FROM NGUOIDUNG WHERE Email = '{Email}' AND SDT = '{sdt}' ORDER BY MaND DESC");
        if (user != null && user.Count > 0)
        {
            int maND = int.Parse(((ArrayList)user[0])[0].ToString());
            // Sinh OTP
            Random random = new Random();
            string otp = random.Next(100000, 999999).ToString();
            // Xóa OTP cũ nếu có
            db.get($"DELETE FROM RESET_TOKENS WHERE MaND = {maND}");
            // Lưu OTP mới
            db.get($"INSERT INTO RESET_TOKENS (MaND, Token, CreatedAt, ExpiresAt, IsUsed) VALUES ({maND}, '{otp}', GETDATE(), DATEADD(MINUTE, 15, GETDATE()), 0)");
            // Gửi OTP qua email
            string subject = "Mã xác thực đăng ký tài khoản";
            string body = $"Mã xác thực đăng ký tài khoản của bạn là: {otp}. Mã có hiệu lực trong 15 phút.";
            bool sent = DataModel.SendEmail(Email, subject, body);
            if (!sent)
            {
                TempData["ErrorMessage"] = "Không gửi được email xác thực. Vui lòng thử lại sau.";
                return RedirectToAction("Register", "Home");
            }
            TempData["SuccessMessage"] = $"Đã gửi mã xác thực đến email: {Email}. Vui lòng kiểm tra hộp thư.";
            TempData["Email"] = Email;
            return RedirectToAction("VerifyEmail", "Home", new { email = Email });
        }
        else
        {
            ViewBag.Error = "Đăng ký không thành công. Vui lòng thử lại.";
            return RedirectToAction("Register", "Home");
        }
    }
    // ------- Action đăng xuất------- //
    public IActionResult Logout()
    {
        // Xóa toàn bộ session
        HttpContext.Session.Clear();

        return RedirectToAction("Index", "Home");
    }
    public IActionResult FillterDoctor()
    {
        try
        {
            LayoutShare();
            DataModel db = new DataModel();
            
            // Test connection trước
            try
            {
                var testResult = db.get("SELECT 1 as test");
                if (testResult == null || testResult.Count == 0)
                {
                    throw new Exception("Không thể kết nối database");
                }
            }
            catch (Exception connEx)
            {
                TempData["ErrorMessage"] = $"Lỗi kết nối database: {connEx.Message}";
                return RedirectToAction("Index", "Home");
            }
            
            // Thử query đơn giản trước
            ViewBag.ListNameDoc = db.get("SELECT MaBS, nd.TenND FROM BACSI bs, NGUOIDUNG nd where bs.MaND = nd.MaND");
            
            // Thử stored procedure, nếu lỗi thì dùng query đơn giản
            try
            {
                ViewBag.ListDoc = db.get("Exec getAllBacSi");
            }
            catch (Exception spEx)
            {
                _logger.LogWarning(spEx, "Stored procedure getAllBacSi không hoạt động, sử dụng fallback query");
                // Fallback query nếu stored procedure không hoạt động
                ViewBag.ListDoc = db.get("SELECT bs.MaBS, nd.TenND, bs.HinhAnh, bv.TenBV, kb.TenKB, bs.PhiKham FROM BACSI bs INNER JOIN NGUOIDUNG nd ON bs.MaND = nd.MaND INNER JOIN BENHVIEN bv ON bs.MaBV = bv.MaBV INNER JOIN KHOABENH kb ON bs.MaKB = kb.MaKB");
            }
            
            return View();
        }
        catch (Exception ex)
        {
            // Log lỗi chi tiết
            _logger.LogError(ex, "Lỗi trong FillterDoctor action: {Message}", ex.Message);
            TempData["ErrorMessage"] = $"Lỗi database: {ex.Message}";
            return RedirectToAction("Index", "Home");
        }
    }
    public IActionResult FillterDoctorList(string khuvuc, string phikham, string khoabenh, string hocham)
    {
        LayoutShare();
        DataModel db = new DataModel();
        if(khuvuc != "Null"){
            khuvuc = "N'"+ khuvuc +"'";
        }
        if(hocham != "Null" ){
            hocham = "N'"+ hocham +"'";
        }
        ViewBag.ListDocFill = db.get($"EXEC FILTER_BACSI {khuvuc}, {phikham}, {khoabenh},  {hocham};");
        
        return View();
    }
    public IActionResult DetailDoctor(string MaBS)
    {
        LayoutShare();

        DataModel db = new DataModel();

   
        // Sử dụng tham số hóa để tránh lỗi
        ViewBag.ListDBS = db.get($"EXEC DetDETAILlBACSI {MaBS}");

        ViewBag.ListComment = db.get($"EXEC GetCommentBACSI {MaBS}");
        return View();
    }
    public IActionResult ListDoctor()
    {
        LayoutShare();
        
        return View();
    }

    // ----- PERSONAL PAGE ------//
     public IActionResult PersonalPage()
    {
        LayoutShare();
        DataModel db = new DataModel();
        var taikhoan = HttpContext.Session.GetString("taikhoan");
        ViewData["TaiKhoan"] = taikhoan;

        if (!string.IsNullOrEmpty(taikhoan))
        {
            try
            {
                DataModel dbPersonal = new DataModel();
                var parameters = new Dictionary<string, object>
                {
                    { "@MaND", taikhoan }
                };
                var result = dbPersonal.ExecuteStoredProcedure("GetUserInfo", parameters);
                if (result != null && result.Count > 0)
                {
                    ViewBag.UserInfo = result[0];
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin user: {Message}", ex.Message);
                // Fallback query đơn giản - chỉ lấy các cột cơ bản
                try
                {
                    DataModel dbFallback = new DataModel();
                    var result = dbFallback.get("SELECT MaND, TenND, Password, Email, SDT, DiaChi, SoDuTK from NGUOIDUNG where MaND=" + taikhoan);
                    if (result != null && result.Count > 0)
                    {
                        ViewBag.UserInfo = result[0];
                    }
                }
                catch (Exception fallbackEx)
                {
                    _logger.LogError(fallbackEx, "Lỗi fallback khi lấy thông tin user: {Message}", fallbackEx.Message);
                }
            }
        }
        return View();
    }

    [HttpPost]
    public IActionResult UpdateUserInfo(string MaND, string TenND, string Email, 
                                        string NamSinh, string GioiTinh, 
                                        string DiaChi, IFormFile Hinhcanhan) 
    {
        try
        {
            DataModel dbUpdate = new DataModel();
            int manD = int.Parse(MaND);
            DateTime parsedDate;
            string formattedDate;
            
            if (DateTime.TryParse(NamSinh, out parsedDate))
            {
                formattedDate = parsedDate.ToString("yyyy-MM-dd");
            }
            else
            {
                formattedDate = "1900-01-01"; // Giá trị mặc định
            }

            // Lấy thông tin hình ảnh hiện tại
            var currentUser = dbUpdate.get($"SELECT HinhAnh FROM NGUOIDUNG WHERE MaND = {manD}");
            string currentImage = "NULL";
            if (currentUser != null && currentUser.Count > 0)
            {
                currentImage = ((ArrayList)currentUser[0])[0]?.ToString() ?? "NULL";
            }

            string nameFile = currentImage; // Giữ nguyên hình cũ nếu không upload mới
            
            if (Hinhcanhan != null && Hinhcanhan.Length > 0)
            {
                // Validate file type
                var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif" };
                if (!allowedTypes.Contains(Hinhcanhan.ContentType.ToLower()))
                {
                    TempData["ErrorMessage"] = "Chỉ chấp nhận file hình ảnh (JPG, PNG, GIF)!";
                    return RedirectToAction("PersonalPage", "Home");
                }

                // Validate file size (max 5MB)
                if (Hinhcanhan.Length > 5 * 1024 * 1024)
                {
                    TempData["ErrorMessage"] = "File quá lớn! Vui lòng chọn file nhỏ hơn 5MB.";
                    return RedirectToAction("PersonalPage", "Home");
                }

                // Tạo tên file unique
                string extension = Path.GetExtension(Hinhcanhan.FileName);
                nameFile = $"avatar_{manD}_{DateTime.Now:yyyyMMddHHmmss}{extension}";

                // Đường dẫn để lưu tệp
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads");
                Directory.CreateDirectory(uploadsFolder); // Tạo thư mục nếu chưa tồn tại
                string filePath = Path.Combine(uploadsFolder, nameFile);
                
                // Lưu tệp
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    Hinhcanhan.CopyTo(stream);
                }

                // Xóa file cũ nếu có và khác NULL
                if (currentImage != "NULL" && !string.IsNullOrEmpty(currentImage))
                {
                    string oldFilePath = Path.Combine(uploadsFolder, currentImage);
                    if (IOF.Exists(oldFilePath))
                    {
                        try
                        {
                            IOF.Delete(oldFilePath);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Không thể xóa file cũ: {FilePath}", oldFilePath);
                        }
                    }
                }
            }
 
            // Cập nhật thông tin user
            var updateParameters = new Dictionary<string, object>
            {
                { "@MaND", manD },
                { "@TenND", TenND },
                { "@Email", Email },
                { "@NgaySinh", formattedDate == "1900-01-01" ? DBNull.Value : (object)formattedDate },
                { "@GioiTinh", GioiTinh },
                { "@DiaChi", DiaChi },
                { "@HinhAnh", nameFile }
            };
            
            var result = dbUpdate.ExecuteStoredProcedure("UpdateUserInfo", updateParameters);
            
            if (result != null && result.Count > 0)
            {
                TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật thông tin!";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật thông tin user: {Message}", ex.Message);
            TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
        }

        return RedirectToAction("PersonalPage", "Home");
    }

    public IActionResult BookExamine(string MaBS)
    {
        LayoutShare();

        DataModel db = new DataModel();
        var taikhoan = HttpContext.Session.GetString("taikhoan");
        ViewData["TaiKhoan"] = taikhoan;

        if (!string.IsNullOrEmpty(taikhoan))
        {
            var result = db.get("SELECT * from NGUOIDUNG where manD=" + taikhoan);
            if (result != null && result.Count > 0)
            {
                ViewBag.UserInfo = result[0]; // Lấy dòng đầu tiên của kết quả
            }
        }

        ViewBag.ListDBS = db.get($"EXEC DetDETAILlBACSI1 {MaBS}");

        return View();
    }
   [HttpPost]
    public IActionResult BookExamineProcess(string MaBS, string MoTaBenh, List<IFormFile> HinhAnhBenhs)
    {
        try
        {
            DataModel db = new DataModel();

            var taikhoan = HttpContext.Session.GetString("taikhoan");
            ViewData["TaiKhoan"] = taikhoan;
            
            if (string.IsNullOrEmpty(taikhoan))
            {
                return RedirectToAction("Login", "Home");
            }
            
            var MaND = taikhoan;

            // Lưu hồ sơ bệnh
            var result = db.get($"DECLARE @MaHS INT; EXEC SAVEHOSO {MaND}, N'{MoTaBenh}', @MaHS = @MaHS OUTPUT; SELECT @MaHS;");
            if (result == null || result.Count == 0)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi lưu hồ sơ bệnh";
                return RedirectToAction("BookExamine", "Home", new { MaBS = MaBS });
            }
            
            var MaHS = int.Parse(((ArrayList)result[0])[0].ToString());
            
            // Lưu hình ảnh bệnh
            if (HinhAnhBenhs != null && HinhAnhBenhs.Count > 0)
            {
                foreach (var file in HinhAnhBenhs)
                {
                    if (file.Length > 0)
                    {
                        // lấy tên tệp
                        string nameFile = Path.GetFileName(file.FileName);

                        // Đường dẫn để lưu tệp
                        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads");
                        Directory.CreateDirectory(uploadsFolder); // Tạo thư mục nếu chưa tồn tại
                        string filePath = Path.Combine(uploadsFolder, nameFile);
                        // Lưu tệp
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }
                        
                        db.get($"EXEC SAVEHINHANHBENH {MaHS}, '{nameFile}'");
                    }
                }
            }

            // Lưu cuộc hẹn khám
            db.get($"EXEC SAVECUOCHENKHAM {MaND}, {MaBS}, {MaHS}, null");

            TempData["SuccessMessage"] = "Đặt lịch khám thành công! Vui lòng chờ bác sĩ xác nhận.";
            return RedirectToAction("ExamineHistory", "Home");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
            return RedirectToAction("BookExamine", "Home", new { MaBS = MaBS });
        }
    }


    public IActionResult ExamineHistory()
    {
        LayoutShare();
        
        var taikhoan = HttpContext.Session.GetString("taikhoan");
        ViewData["TaiKhoan"] = taikhoan;
        var MaND = taikhoan;

        DataModel db = new DataModel();
        ViewBag.ListLichKham = db.get($"EXEC GetLichKhamInfoByMaND {MaND}");


        return View();
    }
    public IActionResult Cancel(string maCHK)
    {
        DataModel db = new DataModel();
        ViewBag.List = db.get("EXEC DeleteCHKbyID " + maCHK);
        
        return RedirectToAction("ExamineHistory", "Home");
    }

    public IActionResult AccountBank()
    {
        LayoutShare();
        DataModel db = new DataModel();
        var taikhoan = HttpContext.Session.GetString("taikhoan");
        ViewData["TaiKhoan"] = taikhoan;

        if (!string.IsNullOrEmpty(taikhoan))
        {
            var result = db.get("SELECT * from NGUOIDUNG where manD=" + taikhoan);
            if (result != null && result.Count > 0)
            {
                ViewBag.UserInfo = result[0]; // Lấy dòng đầu tiên của kết quả
            }
        }
        return View();
    }
    [HttpPost]
    public IActionResult AccountBankProcess(string SoDuTK)
    {
        var taikhoan = HttpContext.Session.GetString("taikhoan");
        ViewData["TaiKhoan"] = taikhoan;
        var MaND = taikhoan;

        DataModel db = new DataModel();
        db.get($"EXEC NapTien {MaND}, {SoDuTK}");

        return RedirectToAction("AccountBank", "Home");
    }
    
    
    // ---------- CHỨC NĂNG ĐỔI MẬT KHẨU ------------//
    public IActionResult ChangePassword()
    {
        LayoutShare();
        var taikhoan = HttpContext.Session.GetString("taikhoan");
        
        if (string.IsNullOrEmpty(taikhoan))
        {
            return RedirectToAction("Login", "Home");
        }
        
        ViewData["TaiKhoan"] = taikhoan;
        return View();
    }
    
    [HttpPost]
    public IActionResult ChangePasswordProcess(string currentPassword, string newPassword, string confirmPassword)
    {
        var taikhoan = HttpContext.Session.GetString("taikhoan");
        
        if (string.IsNullOrEmpty(taikhoan))
        {
            TempData["ErrorMessage"] = "Vui lòng đăng nhập để thực hiện chức năng này.";
            return RedirectToAction("Login", "Home");
        }
        
        // Kiểm tra mật khẩu mới và xác nhận mật khẩu
        if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 6)
        {
            TempData["ErrorMessage"] = "Mật khẩu mới phải có ít nhất 6 ký tự.";
            return RedirectToAction("ChangePassword", "Home");
        }
        
        if (newPassword != confirmPassword)
        {
            TempData["ErrorMessage"] = "Mật khẩu xác nhận không khớp.";
            return RedirectToAction("ChangePassword", "Home");
        }
        
        try
        {
            DataModel db = new DataModel();
            
            // Kiểm tra mật khẩu hiện tại
            var parameters = new Dictionary<string, object>
            {
                { "@MaND", taikhoan },
                { "@CurrentPassword", currentPassword }
            };
            var checkCurrentPassword = db.ExecuteStoredProcedure("CheckCurrentPassword", parameters);
            
            if (checkCurrentPassword == null || checkCurrentPassword.Count == 0)
            {
                TempData["ErrorMessage"] = "Mật khẩu hiện tại không đúng.";
                return RedirectToAction("ChangePassword", "Home");
            }
            
            // Cập nhật mật khẩu mới
            var updateParameters = new Dictionary<string, object>
            {
                { "@MaND", taikhoan },
                { "@NewPassword", newPassword }
            };
            int rowsAffected = db.ExecuteNonQuery("UpdatePassword", updateParameters);
            
            if (rowsAffected > 0)
            {
                TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
                return RedirectToAction("PersonalPage", "Home");
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể cập nhật mật khẩu. Vui lòng thử lại.";
                return RedirectToAction("ChangePassword", "Home");
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
            return RedirectToAction("ChangePassword", "Home");
        }
    }
    
    // ---------- CHỨC NĂNG QUÊN MẬT KHẨU ------------//
    public IActionResult ForgotPassword()
    {
        LayoutShare();
        return View();
    }
    
    [HttpPost]
    public IActionResult ForgotPasswordProcess(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            TempData["ErrorMessage"] = "Vui lòng nhập email.";
            return RedirectToAction("ForgotPassword", "Home");
        }
        try
        {
            DataModel db = new DataModel();
            // Kiểm tra email có tồn tại không
            var user = db.get($"SELECT MaND, TenND FROM NGUOIDUNG WHERE Email = '{email}'");
            if (user == null || user.Count == 0)
            {
                TempData["ErrorMessage"] = "Email không tồn tại trong hệ thống.";
                return RedirectToAction("ForgotPassword", "Home");
            }
            // Tạo token reset password (6 số ngẫu nhiên)
            Random random = new Random();
            string resetToken = random.Next(100000, 999999).ToString();
            // Lưu token vào database
            var userInfo = user[0] as ArrayList;
            int maND = Convert.ToInt32(userInfo[0]);
            db.get($"DELETE FROM RESET_TOKENS WHERE MaND = {maND}");
            db.get($"INSERT INTO RESET_TOKENS (MaND, Token, CreatedAt, ExpiresAt, IsUsed) VALUES ({maND}, '{resetToken}', GETDATE(), DATEADD(MINUTE, 15, GETDATE()), 0)");
            // Gửi email OTP
            string subject = "Mã xác thực đặt lại mật khẩu";
            string body = $"Mã xác thực đặt lại mật khẩu của bạn là: {resetToken}. Mã có hiệu lực trong 15 phút.";
            bool sent = DataModel.SendEmail(email, subject, body);
            if (!sent)
            {
                TempData["ErrorMessage"] = "Không gửi được email. Vui lòng thử lại sau.";
                return RedirectToAction("ForgotPassword", "Home");
            }
            TempData["SuccessMessage"] = $"Đã gửi mã xác thực đến email: {email}. Vui lòng kiểm tra hộp thư.";
            TempData["Email"] = email;
            return RedirectToAction("ResetPassword", "Home");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
            return RedirectToAction("ForgotPassword", "Home");
        }
    }
    
    public IActionResult ResetPassword()
    {
        LayoutShare();
        return View();
    }
    
    [HttpPost]
    public IActionResult ResetPasswordProcess(string email, string token, string newPassword, string confirmPassword)
    {
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
        {
            TempData["ErrorMessage"] = "Vui lòng nhập đầy đủ thông tin.";
            return RedirectToAction("ResetPassword", "Home");
        }
        if (newPassword != confirmPassword)
        {
            TempData["ErrorMessage"] = "Mật khẩu xác nhận không khớp.";
            return RedirectToAction("ResetPassword", "Home");
        }
        if (newPassword.Length < 6)
        {
            TempData["ErrorMessage"] = "Mật khẩu phải có ít nhất 6 ký tự.";
            return RedirectToAction("ResetPassword", "Home");
        }
        try
        {
            DataModel db = new DataModel();
            // Kiểm tra token có hợp lệ không (theo email)
            var tokenCheck = db.get($"SELECT rt.MaND, rt.IsUsed, rt.ExpiresAt FROM RESET_TOKENS rt INNER JOIN NGUOIDUNG nd ON rt.MaND = nd.MaND WHERE nd.Email = '{email}' AND rt.Token = '{token}' AND rt.IsUsed = 0 AND rt.ExpiresAt > GETDATE()");
            if (tokenCheck == null || tokenCheck.Count == 0)
            {
                TempData["ErrorMessage"] = "Mã xác thực không hợp lệ hoặc đã hết hạn.";
                return RedirectToAction("ResetPassword", "Home");
            }
            var tokenInfo = tokenCheck[0] as ArrayList;
            int maND = Convert.ToInt32(tokenInfo[0]);
            // Cập nhật mật khẩu mới
            db.get($"UPDATE NGUOIDUNG SET Password = '{newPassword}' WHERE MaND = {maND}");
            // Đánh dấu token đã sử dụng
            db.get($"UPDATE RESET_TOKENS SET IsUsed = 1 WHERE MaND = {maND} AND Token = '{token}'");
            TempData["SuccessMessage"] = "Đặt lại mật khẩu thành công! Vui lòng đăng nhập với mật khẩu mới.";
            return RedirectToAction("Login", "Home");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
            return RedirectToAction("ResetPassword", "Home");
        }
    }
    
    public IActionResult VerifyEmail(string email)
    {
        LayoutShare();
        ViewBag.Email = email;
        return View();
    }

    [HttpPost]
    public IActionResult VerifyEmailProcess(string email, string otp)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(otp))
        {
            TempData["ErrorMessage"] = "Vui lòng nhập đầy đủ thông tin.";
            return RedirectToAction("VerifyEmail", new { email });
        }
        try
        {
            DataModel db = new DataModel();
            // Lấy MaND mới nhất theo email
            var user = db.get($"SELECT TOP 1 MaND FROM NGUOIDUNG WHERE Email = '{email}' ORDER BY MaND DESC");
            if (user == null || user.Count == 0)
            {
                TempData["ErrorMessage"] = "Email không tồn tại.";
                return RedirectToAction("VerifyEmail", new { email });
            }
            int maND = int.Parse(((ArrayList)user[0])[0].ToString());
            // Kiểm tra OTP đúng user, chưa dùng, chưa hết hạn
            var tokenCheck = db.get($"SELECT 1 FROM RESET_TOKENS WHERE MaND = {maND} AND Token = '{otp}' AND IsUsed = 0 AND ExpiresAt > GETDATE()");
            if (tokenCheck == null || tokenCheck.Count == 0)
            {
                TempData["ErrorMessage"] = "Mã xác thực không hợp lệ hoặc đã hết hạn.";
                return RedirectToAction("VerifyEmail", new { email });
            }
            // Cập nhật xác thực email
            db.get($"UPDATE NGUOIDUNG SET IsEmailVerified = 1 WHERE MaND = {maND}");
            db.get($"UPDATE RESET_TOKENS SET IsUsed = 1 WHERE MaND = {maND} AND Token = '{otp}'");
            TempData["SuccessMessage"] = "Xác thực email thành công! Bạn có thể đăng nhập.";
            return RedirectToAction("Login", "Home");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
            return RedirectToAction("VerifyEmail", new { email });
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    // Thêm action để xem database
    public IActionResult ViewDatabase()
    {
        try
        {
            DataModel db = new DataModel();
            
            // Lấy danh sách các bảng
            ViewBag.Tables = db.get("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'");
            
            // Lấy dữ liệu từ một số bảng quan trọng
            ViewBag.Users = db.get("SELECT TOP 10 * FROM NGUOIDUNG");
            ViewBag.Doctors = db.get("SELECT TOP 10 * FROM BACSI");
            ViewBag.Hospitals = db.get("SELECT TOP 10 * FROM BENHVIEN");
            ViewBag.Articles = db.get("SELECT TOP 10 * FROM BAIVIET");
            
            return View();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Lỗi kết nối database: " + ex.Message;
            return RedirectToAction("Index", "Home");
        }
    }
}