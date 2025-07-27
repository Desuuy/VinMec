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
using Newtonsoft.Json;
using System.Text;
using Websiste_Healthcare_Booking_VoVBacSi_main.Models;




public class HomeController : Controller
{

    private readonly ILogger<HomeController> _logger;
    private readonly ISession _session;
    private readonly HttpClient _httpClient;
    private readonly string _apiUrl = "http://localhost:8000/predict";


    public HomeController(ILogger<HomeController> logger, IHttpContextAccessor httpContextAccessor, HttpClient httpClient)
    {
        _logger = logger;
        _session = httpContextAccessor.HttpContext.Session;
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(30); // Set timeout
    }

    public IActionResult LayoutShare()
    {
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
        DataModel db = new DataModel();
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
            if (emailVerified != null && emailVerified.Count > 0 && emailVerified[0] is ArrayList emailArray && emailArray.Count > 0 && emailArray[0].ToString() == "0")
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
        // Kiểm tra số điện thoại đã tồn tại chưa
        var phoneExists = db.get($"SELECT 1 FROM NGUOIDUNG WHERE SDT = '{sdt}'");
        if (phoneExists != null && phoneExists.Count > 0)
        {
            TempData["ErrorMessage"] = "Vui lòng nhập lại";
            return RedirectToAction("Register", "Home");
        }
        // Kiểm tra email đã tồn tại chưa
        var emailExists = db.get($"SELECT 1 FROM NGUOIDUNG WHERE Email = '{Email}'");
        if (emailExists != null && emailExists.Count > 0)
        {
            TempData["ErrorMessage"] = "Vui lòng nhập lại";
            return RedirectToAction("Register", "Home");
        }
        // Sinh OTP và lưu vào TempData
        Random random = new Random();
        string otp = random.Next(100000, 999999).ToString();
        TempData["OTP"] = otp;
        TempData["TenND"] = TenND;
        TempData["Password"] = Password;
        TempData["sdt"] = sdt;
        TempData["Email"] = Email;
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
        return RedirectToAction("VerifyEmail", "Home", new { email = Email });
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
        LayoutShare();
        DataModel db = new DataModel();
        ViewBag.ListNameDoc = db.get("SELECT MaBS, nd.TenND FROM BACSI bs, NGUOIDUNG nd where bs.MaND = nd.MaND");
        ViewBag.ListDoc = db.get("Exec getAllBacSi");

        return View();
    }
    public IActionResult FillterDoctorList(string khuvuc, string phikham, string khoabenh, string hocham)
    {
        LayoutShare();
        DataModel db = new DataModel();
        if (khuvuc != "Null")
        {
            khuvuc = "N'" + khuvuc + "'";
        }
        if (hocham != "Null")
        {
            hocham = "N'" + hocham + "'";
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
            var result = db.get("SELECT * from NGUOIDUNG where manD=" + taikhoan);
            if (result != null && result.Count > 0)
            {
                ViewBag.UserInfo = result[0]; // Lấy dòng đầu tiên của kết quả
            }
        }
        return View();
    }

    [HttpPost]
    public IActionResult UpdateUserInfo(string MaND, string TenND, string Email,
                                        string NamSinh, string GioiTinh,
                                        string DiaChi, IFormFile Hinhcanhan)
    {
        // Log để debug
        _logger.LogInformation($"UpdateUserInfo called with: MaND={MaND}, TenND={TenND}, Email={Email}, NamSinh='{NamSinh}', GioiTinh={GioiTinh}, DiaChi={DiaChi}");
        
        DataModel db = new DataModel();
        
        // Xử lý an toàn cho MaND
        if (string.IsNullOrEmpty(MaND))
        {
            TempData["ErrorMessage"] = "Mã người dùng không hợp lệ";
            return RedirectToAction("PersonalPage", "Home");
        }
        
        int manD;
        if (!int.TryParse(MaND, out manD))
        {
            TempData["ErrorMessage"] = "Mã người dùng không hợp lệ";
            return RedirectToAction("PersonalPage", "Home");
        }
        
        // Xử lý an toàn cho các tham số
        TenND = string.IsNullOrEmpty(TenND) ? "" : TenND.Trim();
        Email = string.IsNullOrEmpty(Email) ? "" : Email.Trim();
        GioiTinh = string.IsNullOrEmpty(GioiTinh) ? "" : GioiTinh.Trim();
        DiaChi = string.IsNullOrEmpty(DiaChi) ? "" : DiaChi.Trim();
        
        // Xử lý trường hợp NamSinh null hoặc rỗng
        string formattedDate = "NULL";
        if (!string.IsNullOrEmpty(NamSinh) && NamSinh.Trim() != "")
        {
            DateTime parsedDate;
            if (DateTime.TryParse(NamSinh, out parsedDate))
            {
                formattedDate = parsedDate.ToString("yyyy-MM-dd");
            }
            else
            {
                TempData["ErrorMessage"] = "Định dạng ngày sinh không hợp lệ. Vui lòng nhập theo định dạng mm/dd/yyyy";
                return RedirectToAction("PersonalPage", "Home");
            }
        }

        string nameFile = "NULL";
        if (Hinhcanhan != null)
        {
            // lấy tên tệp
            nameFile = Path.GetFileName(Hinhcanhan.FileName);

            // Đường dẫn để lưu tệp
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads");
            Directory.CreateDirectory(uploadsFolder); // Tạo thư mục nếu chưa tồn tại
            string filePath = Path.Combine(uploadsFolder, nameFile);
            // Lưu tệp
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                Hinhcanhan.CopyTo(stream);
            }
        }

        // Xử lý SQL query với trường hợp NamSinh null
        string sqlQuery;
        if (formattedDate == "NULL")
        {
            sqlQuery = $"EXEC UpdateUserInfo {manD}, N'{TenND.Replace("'", "''")}', '{Email.Replace("'", "''")}', NULL, N'{GioiTinh.Replace("'", "''")}', N'{DiaChi.Replace("'", "''")}', '{nameFile.Replace("'", "''")}'";
        }
        else
        {
            sqlQuery = $"EXEC UpdateUserInfo {manD}, N'{TenND.Replace("'", "''")}', '{Email.Replace("'", "''")}', '{formattedDate}', N'{GioiTinh.Replace("'", "''")}', N'{DiaChi.Replace("'", "''")}', '{nameFile.Replace("'", "''")}'";
        }
        
        // Log SQL query để debug
        _logger.LogInformation($"SQL Query: {sqlQuery}");
        
        try
        {
            db.get(sqlQuery);
            TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in UpdateUserInfo: {ex.Message}");
            TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật thông tin: " + ex.Message;
        }

        return RedirectToAction("PersonalPage", "Home");
    }

    public IActionResult BookExamine(string MaBS)
    {
        LayoutShare();

        DataModel db = new DataModel();
        var taikhoan = HttpContext.Session.GetString("taikhoan");
        ViewData["TaiKhoan"] = taikhoan;

        // Nếu chưa đăng nhập thì chuyển hướng về trang Login
        if (string.IsNullOrEmpty(taikhoan))
        {
            TempData["ErrorMessage"] = "Vui lòng đăng nhập để đặt lịch khám.";
            return RedirectToAction("Login", "Home");
        }

        var result = db.get("SELECT * from NGUOIDUNG where manD=" + taikhoan);
        if (result != null && result.Count > 0)
        {
            ViewBag.UserInfo = result[0]; // Lấy dòng đầu tiên của kết quả
        }
        else
        {
            TempData["ErrorMessage"] = "Không tìm thấy thông tin người dùng.";
            return RedirectToAction("Login", "Home");
        }

        var listDBS = db.get($"EXEC DetDETAILlBACSI1 {MaBS}");
        if (listDBS == null || listDBS.Count == 0)
        {
            TempData["ErrorMessage"] = "Không tìm thấy thông tin bác sĩ.";
            return RedirectToAction("FillterDoctor", "Home");
        }
        ViewBag.ListDBS = listDBS;

        return View();
    }
    [HttpPost]
    public IActionResult BookExamineProcess(string MaBS, string MoTaBenh, List<IFormFile> HinhAnhBenhs)
    {
        DataModel db = new DataModel();

        var taikhoan = HttpContext.Session.GetString("taikhoan");
        ViewData["TaiKhoan"] = taikhoan;
        var MaND = taikhoan;

        var result = db.get($"DECLARE @MaHS INT; EXEC SAVEHOSO {MaND}, N'{MoTaBenh}', @MaHS = @MaHS OUTPUT; SELECT @MaHS;");
        var MaHS = 0;
        if (result != null && result.Count > 0)
        {
            ViewBag.MaHS = result[0]; // Lấy dòng đầu tiên của kết quả
            if (ViewBag.MaHS != null)
            {
                int.TryParse(ViewBag.MaHS.ToString(), out MaHS);
            }
        }

        foreach (var file in HinhAnhBenhs)
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

        db.get($"EXEC SAVECUOCHENKHAM {MaND}, {MaBS}, {MaHS}, null");

        return RedirectToAction("Index", "Home");
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


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
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
            int maND = 0;
            if (userInfo != null && userInfo.Count > 0 && int.TryParse(userInfo[0].ToString(), out int userId))
            {
                maND = userId;
            }
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
            int maND = 0;
            if (tokenInfo != null && tokenInfo.Count > 0 && int.TryParse(tokenInfo[0].ToString(), out int userId))
            {
                maND = userId;
            }
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
        string sentOtp = TempData["OTP"] as string;
        string TenND = TempData["TenND"] as string;
        string Password = TempData["Password"] as string;
        string sdt = TempData["sdt"] as string;
        string Email = TempData["Email"] as string;
        if (sentOtp == otp && email == Email)
        {
            DataModel db = new DataModel();
            // Đăng ký user với IsEmailVerified = 1
            var list = db.get($"EXEC REGISTER N'{TenND}', '{Password}', '{sdt}', '{Email}', 1");
            TempData["SuccessMessage"] = "Đăng ký thành công. Bạn có thể đăng nhập.";
            return RedirectToAction("Login", "Home");
        }
        else
        {
            TempData["ErrorMessage"] = "Mã xác thực không đúng hoặc đã hết hạn. Vui lòng thử lại.";
            return RedirectToAction("VerifyEmail", "Home", new { email = email });
        }
    }

    public IActionResult DiseasePredict()
    {
        LayoutShare();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> DiseasePredict(string description)
    {
        LayoutShare();
        ViewBag.Description = description;
        if (string.IsNullOrWhiteSpace(description))
        {
            ViewBag.Error = "Vui lòng nhập mô tả triệu chứng";
            return View();
        }

        try
        {
            var requestBody = new { text = description.Trim() };
            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var resultString = await response.Content.ReadAsStringAsync();
                var predictResult = JsonConvert.DeserializeObject<PredictionResponse>(resultString);
                ViewBag.PredictResult = predictResult;
            }
            else
            {
                ViewBag.Error = $"API trả về lỗi: {response.StatusCode}";
            }
        }
        catch (HttpRequestException)
        {
            ViewBag.Error = "Không thể kết nối đến API dự đoán. Vui lòng kiểm tra server Python có đang chạy không.";
        }
        catch (TaskCanceledException)
        {
            ViewBag.Error = "Yêu cầu bị timeout. Vui lòng thử lại.";
        }
        catch (Exception)
        {
            ViewBag.Error = "Có lỗi xảy ra khi xử lý yêu cầu.";
        }
        return View();
    }
}

