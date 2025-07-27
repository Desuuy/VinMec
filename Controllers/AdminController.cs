using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DoAnCNPM.Models;
using Microsoft.AspNetCore.SignalR.Protocol;
using System.Collections;
using Newtonsoft.Json;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Web;

namespace DAPMBSVOV.Controllers
{
    
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private ISession _session => _httpContextAccessor.HttpContext.Session;

        public AdminController(ILogger<AdminController> logger, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpContextAccessor = httpContextAccessor;
        }

        private bool IsAdminLoggedIn()
        {
            return _session.GetString("AdminLoggedIn") == "true";
        }

        private IActionResult RedirectIfNotLoggedIn()
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("AdminLogin");
            return null;
        }

        public IActionResult Index(int? year)
        {
            var redirect = RedirectIfNotLoggedIn();
            if (redirect != null) return redirect;
            DataModel db = new DataModel();
            int selectedYear = year ?? DateTime.Now.Year;

            // Tổng số người dùng
            ArrayList slnd_sql = db.get($"SELECT COUNT(*) FROM NGUOIDUNG where MaPQ = 2");
            ArrayList row = (ArrayList)slnd_sql[0];
            var users = int.Parse(row[0].ToString());
            ViewBag.TotalUsers = users;

            // Tổng số bác sĩ
            ArrayList slbs_sql = db.get($"SELECT COUNT(*) FROM BACSI");
            ArrayList rowbs = (ArrayList)slbs_sql[0];
            var doctors = int.Parse(rowbs[0].ToString());
            ViewBag.TotalDoctors = doctors;

            // Tổng doanh thu
            ArrayList sldt_sql = db.get($"SELECT ISNULL(SUM(SoTienTT),0) FROM CUOCHENKHAM WHERE MaTTCH = 4");
            ArrayList rowdt = (ArrayList)sldt_sql[0];
            var totalRevenue = int.Parse(rowdt[0].ToString());
            ViewBag.TotalRevenue = totalRevenue;

            // Tổng thu nhập (30% của tổng doanh thu)
            var totalIncome = (int)(totalRevenue * 0.3);
            ViewBag.TotalIncome = totalIncome;

            // Tổng lịch hẹn đã hoàn thành
            ArrayList slht_sql = db.get($"SELECT COUNT(*) FROM CUOCHENKHAM WHERE MaTTCH = 4");
            ArrayList rowht = (ArrayList)slht_sql[0];
            var totalCompleted = int.Parse(rowht[0].ToString());
            ViewBag.TotalCompleted = totalCompleted;

            // Biểu đồ doanh thu và lịch hẹn hoàn thành theo tháng trong năm
            var chartData = db.get($@"
                SELECT MONTH(ThoiGianHen) as Thang, 
                       COUNT(*) as SoLichHoanThanh, 
                       ISNULL(SUM(SoTienTT),0) as DoanhThu 
                FROM CUOCHENKHAM 
                WHERE MaTTCH = 4 AND YEAR(ThoiGianHen) = {selectedYear}
                GROUP BY MONTH(ThoiGianHen)
                ORDER BY Thang");
            ViewBag.ChartData = Newtonsoft.Json.JsonConvert.SerializeObject(chartData);
            ViewBag.SelectedYear = selectedYear;

            // Danh sách năm có dữ liệu
            var years = db.get("SELECT DISTINCT YEAR(ThoiGianHen) FROM CUOCHENKHAM WHERE MaTTCH = 4 ORDER BY YEAR(ThoiGianHen) DESC");
            ViewBag.Years = years;

            // Top bác sĩ có số lịch hoàn thành nhiều nhất
            var topDoctors = db.get($@"
                SELECT TOP 10 bs.MaBS, nd.TenND, COUNT(*) as SoLichHoanThanh
                FROM CUOCHENKHAM ck
                JOIN BACSI bs ON ck.MaBS = bs.MaBS
                JOIN NGUOIDUNG nd ON bs.MaND = nd.MaND
                WHERE ck.MaTTCH = 4
                GROUP BY bs.MaBS, nd.TenND
                ORDER BY SoLichHoanThanh DESC");
            ViewBag.TopDoctors = topDoctors;

            return View();
        }

        public IActionResult AdminLogin()
        {
            return View();
        }

        [HttpPost]
        public IActionResult XuLyAdminLogin(string sdt, string password)
        {
            DataModel db = new DataModel();
            ViewBag.list = db.get("EXEC CheckAdminLogin '" + sdt + "', '" + password + "';");
            if (ViewBag.list != null && ViewBag.list.Count > 0)
            {
                // Đăng nhập thành công
                _session.SetString("AdminLoggedIn", "true");
                return RedirectToAction("Index", "Admin");
            }
            else
            {
                TempData["Error"] = "Số điện thoại hoặc mật khẩu không chính xác!";
                return RedirectToAction("AdminLogin", "Admin");
            }
        }

        public IActionResult Logout()
        {
            _session.Remove("AdminLoggedIn");
            return RedirectToAction("AdminLogin", "Admin");
        }

        // Bài viết
        public IActionResult DMBaiViet()
        {
            var redirect = RedirectIfNotLoggedIn();
            if (redirect != null) return redirect;
            DataModel db = new DataModel();
            ViewBag.listLBV = db.get("SELECT * from LOAIBAIVIET");
            ViewBag.listBV = db.get("SELECT * from BAIVIET");
            ViewBag.listKB = db.get("SELECT * from KHOABENH");
            return View();
        }
        [HttpPost]
    public IActionResult ThemBaiViet(string tieudebv, string noidungbv, string makb, 
        string linkytb, string malbv)
    {
        try
        {
            // Validation đầu vào cơ bản
            if (string.IsNullOrEmpty(tieudebv) || string.IsNullOrEmpty(noidungbv) 
                || string.IsNullOrEmpty(malbv))
            {
                TempData["Error"] = "Vui lòng điền đầy đủ thông tin bắt buộc";
                return RedirectToAction("DMBaiViet");
            }

            DataModel db = new DataModel();
            DateTime ngaydang = DateTime.Now;
            
            // Xử lý giá trị null cho makb và linkytb
            string maKhoaBenhParam = string.IsNullOrEmpty(makb) || makb == "NULL" ? "NULL" : makb;
            string linkYoutubeParam = string.IsNullOrEmpty(linkytb) ? "NULL" : $"'{linkytb}'";

            // Escape các ký tự đặc biệt trong nội dung và tiêu đề
            tieudebv = tieudebv.Replace("'", "''");
            noidungbv = noidungbv.Replace("'", "''");

            string query = $@"EXEC AddBAIVIET 
                @TieuDeBV = N'{tieudebv}',
                @NoiDung = N'{noidungbv}',
                @MaKB = {maKhoaBenhParam},
                @LinkYtb = {linkYoutubeParam},
                @NgayDang = '{ngaydang:yyyy-MM-dd}',
                @MaLBV = {malbv},
                @LuotXem = 0";

            ViewBag.list = db.get(query);

            return RedirectToAction("DMBaiViet", "Admin");
        }
        catch (Exception ex)
        {
            // Xử lý lỗi
            return RedirectToAction("DMBaiViet", "Admin");
        }
    }

        [HttpPost]
        public IActionResult XoaBaiViet(string id)
        {
            DataModel db = new DataModel();
            ViewBag.list = db.get("EXEC DeleteBAIVIET " + id + ";");
            return RedirectToAction("DMBaiViet", "Admin"); 
        }

        public IActionResult TimBaiViet(string mabv)
        {
            try
            {
                DataModel db = new DataModel();
                // Lấy thông tin bài viết cần sửa
                ViewBag.list = db.get($"SELECT * FROM BAIVIET WHERE MaBV = {mabv}");
                // Lấy danh sách khoa bệnh và loại bài viết để populate dropdown
                ViewBag.listKB = db.get("SELECT * FROM KHOABENH");
                ViewBag.listLBV = db.get("SELECT * FROM LOAIBAIVIET");
                
                return View(); // Trả về view sửa bài viết
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi tìm bài viết";
                return RedirectToAction("DMBaiViet");
            }
        }

        public IActionResult ChiTietBaiViet(string mabv)
        {
            DataModel db = new DataModel();
            ViewBag.list = db.get("EXEC FindBAIVIET_ID " +mabv+ ";");      
            ViewBag.listLBV = db.get("SELECT * from LOAIBAIVIET");
            ViewBag.listKB = db.get("SELECT * from KHOABENH");      
            return View(); 
        }

        [HttpPost]
        public IActionResult SuaBaiViet(string mabv, string tieudebv, string noidungbv,
            string makb, string linkytb, string malbv, string luotxem)
        {
            try
            {
                // Validation đầu vào
                if (string.IsNullOrEmpty(mabv) || string.IsNullOrEmpty(tieudebv) 
                    || string.IsNullOrEmpty(noidungbv) || string.IsNullOrEmpty(malbv))
                {
                    TempData["Error"] = "Vui lòng điền đầy đủ thông tin bắt buộc";
                    return RedirectToAction("TimBaiViet", new { mabv = mabv });
                }

                DataModel db = new DataModel();
                DateTime ngaydang = DateTime.Now;

                // Xử lý các giá trị null và escape ký tự đặc biệt
                string maKhoaBenhParam = string.IsNullOrEmpty(makb) ? "NULL" : makb;
                string linkYoutubeParam = string.IsNullOrEmpty(linkytb) ? "NULL" : $"'{linkytb.Replace("'", "''")}'";
                tieudebv = tieudebv.Replace("'", "''");
                noidungbv = noidungbv.Replace("'", "''");

                string query = $@"EXEC UpdateBAIVIET 
                    @MaBV = {mabv},
                    @TieuDeBV = N'{tieudebv}',
                    @NoiDung = N'{noidungbv}',
                    @MaKB = {maKhoaBenhParam},
                    @LinkYtb = {linkYoutubeParam},
                    @NgayDang = '{ngaydang:yyyy-MM-dd}',
                    @MaLBV = {malbv},
                    @LuotXem = {luotxem}";

                ViewBag.list = db.get(query);
                
                TempData["Success"] = "Cập nhật bài viết thành công";
                return RedirectToAction("DMBaiViet");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật bài viết";
                return RedirectToAction("TimBaiViet", new { mabv = mabv });
            }
        }
        // End Bài viết

        // Chuyên ngành
        public IActionResult DMChuyenNganh()
        {
            var redirect = RedirectIfNotLoggedIn();
            if (redirect != null) return redirect;
            DataModel db = new DataModel();
            ViewBag.listCN = db.get("SELECT * from CHUYENNGANH");
            return View(); 
        }

        [HttpPost]
        public IActionResult ThemChuyenNganh(string tencn, IFormFile hinhanh)
        {
            if (hinhanh == null || hinhanh.Length == 0)
            {
                TempData["Error"] = "Vui lòng tải lên hình ảnh hợp lệ!";
                return RedirectToAction("DMChuyenNganh", "Admin");
            }

            try
            {
                DataModel db = new DataModel();

                // Lấy tên tệp
                string namefile = Path.GetFileName(hinhanh.FileName);

                // Đường dẫn để lưu tệp
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads");
                Directory.CreateDirectory(uploadsFolder); // Tạo thư mục nếu chưa tồn tại
                string filePath = Path.Combine(uploadsFolder, namefile);

                // Lưu tệp
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    hinhanh.CopyTo(stream);
                }
            db.get($"EXEC sp_ThemChuyenNganh N'{tencn}','{namefile}';");
            TempData["Message"] = "Thêm chuyên ngành thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Có lỗi xảy ra: {ex.Message}";
            }

            return RedirectToAction("DMChuyenNganh", "Admin");
        }

        public IActionResult TimChuyenNganh(string macn) 
        {
            var redirect = RedirectIfNotLoggedIn();
            if (redirect != null) return redirect;
            DataModel db = new DataModel();
            ViewBag.list = db.get("EXEC sp_TimChuyenNganhTheoMa " + macn + ";");
            return View();
        }

        [HttpPost]
        public IActionResult SuaChuyenNganh(string macn, string tencn, IFormFile hinhanh)
        {
            if (hinhanh == null || hinhanh.Length == 0)
            {
                TempData["Error"] = "Vui lòng tải lên hình ảnh hợp lệ!";
                return RedirectToAction("DMChuyenNganh", "Admin");
            }

            try
            {
                DataModel db = new DataModel();

                // Lấy tên tệp
                string namefile = Path.GetFileName(hinhanh.FileName);

                // Đường dẫn để lưu tệp
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads");
                Directory.CreateDirectory(uploadsFolder); // Tạo thư mục nếu chưa tồn tại
                string filePath = Path.Combine(uploadsFolder, namefile);

                // Lưu tệp
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    hinhanh.CopyTo(stream);
                }
            db.get($"EXEC sp_SuaChuyenNganh {macn}, N'{tencn}', N'{namefile}';");
            TempData["Message"] = "Sửa chuyên ngành thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Có lỗi xảy ra: {ex.Message}";
            }

            return RedirectToAction("DMChuyenNganh", "Admin");
        }

        [HttpPost]
        public IActionResult XoaChuyenNganh(string macn) 
        {
            var redirect = RedirectIfNotLoggedIn();
            if (redirect != null) return redirect;
            DataModel db = new DataModel();
            try
            {
                // Gọi stored procedure để xóa chuyên ngành
                db.get("EXEC sp_XoaChuyenNganh " + macn + ";");
                TempData["Message"] = "Xóa chuyên ngành thành công!";
            }
            catch (Exception)
            {
                // Nếu xảy ra lỗi, thông báo lỗi liên quan đến khóa ngoại
                TempData["Error"] = "Chuyên ngành được chọn có liên kết với bác sĩ. Không xóa thành công !!!!";
            }
            return RedirectToAction("DMChuyenNganh", "Admin");
        }
        // End Chuyên ngành

        // Khoa bệnh
        public IActionResult DMKhoaBenh()
        {
            var redirect = RedirectIfNotLoggedIn();
            if (redirect != null) return redirect;
            DataModel db = new DataModel();
            ViewBag.listKB = db.get("SELECT * from KHOABENH");
            return View(); 
        }

        [HttpPost]
        public IActionResult ThemKhoaBenh(string tenkb, string mota)
        {
            var redirect = RedirectIfNotLoggedIn();
            if (redirect != null) return redirect;
            DataModel db = new DataModel();
            ViewBag.list = db.get("EXEC sp_ThemKhoaBenh N'" + tenkb + "', N'" + mota + "';");
            return RedirectToAction("DMKhoaBenh", "Admin");
        }

        [HttpPost]
        public IActionResult XoaKhoaBenh(string id)
        {
            var redirect = RedirectIfNotLoggedIn();
            if (redirect != null) return redirect;
            DataModel db = new DataModel();
            try
            {
                // Gọi stored procedure để xóa khoa bệnh
                db.get("EXEC sp_XoaKhoaBenh " + id + ";");
                TempData["Message"] = "Xóa khoa bệnh thành công!";
            }
            catch (Exception)
            {
                // Nếu xảy ra lỗi, thông báo lỗi liên quan đến khóa ngoại
                TempData["Error"] = "Khoa bệnh được chọn có liên kết dữ liệu. Không xóa thành công !!!!";
            }
            return RedirectToAction("DMKhoaBenh", "Admin");
        }

        public IActionResult TimKhoaBenh(string makb) 
        {
            var redirect = RedirectIfNotLoggedIn();
            if (redirect != null) return redirect;
            DataModel db = new DataModel();
            ViewBag.list = db.get("EXEC sp_TimKhoaBenhTheoMa " + makb + ";");
            return View();
        }

        [HttpPost]
        public IActionResult SuaKhoaBenh(string makb, string tenkb, string mota)
        {
            var redirect = RedirectIfNotLoggedIn();
            if (redirect != null) return redirect;
            DataModel db = new DataModel();
            ViewBag.list = db.get("EXEC sp_SuaKhoaBenh " + makb + ",N'" + tenkb + "', N'" + mota + "';");
            return RedirectToAction("DMKhoaBenh", "Admin");
        }
        // End Khoa bệnh

        // Bệnh viện
        public IActionResult DMBenhVien()
        {
            var redirect = RedirectIfNotLoggedIn();
            if (redirect != null) return redirect;
            DataModel db = new DataModel();
            ViewBag.list = db.get("SELECT * from BENHVIEN");
            return View();
        }

        [HttpPost] 
        public IActionResult ThemBenhVien(string tenbv, IFormFile hinhanh)
        {
            if (hinhanh == null || hinhanh.Length == 0)
            {
                TempData["Error"] = "Vui lòng tải lên hình ảnh hợp lệ!";
                return RedirectToAction("DMBenhVien", "Admin");
            }
            try
            {
                DataModel db = new DataModel();

                // Lấy tên tệp
                string namefile = Path.GetFileName(hinhanh.FileName);

                // Đường dẫn để lưu tệp
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads");
                Directory.CreateDirectory(uploadsFolder); // Tạo thư mục nếu chưa tồn tại
                string filePath = Path.Combine(uploadsFolder, namefile);

                // Lưu tệp
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    hinhanh.CopyTo(stream);
                }
            db.get($"EXEC AddBenhVien N'{tenbv}','{namefile}';");
            TempData["Message"] = "Thêm bệnh viện thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Có lỗi xảy ra: {ex.Message}";
            }

            return RedirectToAction("DMBenhVien", "Admin");
        }

        public IActionResult TimBenhVien(string mabv) 
        {
            var redirect = RedirectIfNotLoggedIn();
            if (redirect != null) return redirect;
            DataModel db = new DataModel();
            ViewBag.list = db.get("EXEC sp_TimBenhVienTheoMa " + mabv + ";");
            return View();
        }

        [HttpPost] 
        public IActionResult SuaBenhVien(string mabv, string tenbv, IFormFile hinhanh)
        {
            if (hinhanh == null || hinhanh.Length == 0)
            {
                TempData["Error"] = "Vui lòng tải lên hình ảnh hợp lệ!";
                return RedirectToAction("DMBenhVien", "Admin");
            }

            try
            {
                DataModel db = new DataModel();

                // Lấy tên tệp
                string namefile = Path.GetFileName(hinhanh.FileName);

                // Đường dẫn để lưu tệp
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads");
                Directory.CreateDirectory(uploadsFolder); // Tạo thư mục nếu chưa tồn tại
                string filePath = Path.Combine(uploadsFolder, namefile);

                // Lưu tệp
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    hinhanh.CopyTo(stream);
                }
            db.get($"EXEC sp_SuaBenhVien {mabv}, N'{tenbv}', N'{namefile}';");
            TempData["Message"] = "Sửa bệnh viện thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Có lỗi xảy ra: {ex.Message}";
            }

            return RedirectToAction("DMBenhVien", "Admin");
        }

        [HttpPost]
        public IActionResult XoaBenhVien(string mabv) 
        {
            var redirect = RedirectIfNotLoggedIn();
            if (redirect != null) return redirect;
            DataModel db = new DataModel();
            try
            {
                // Gọi stored procedure để xóa khoa bệnh
                db.get("EXEC sp_XoaBenhVien " + mabv + ";");
                TempData["Message"] = "Xóa bệnh viện thành công!";
            }
            catch (Exception)
            {
                // Nếu xảy ra lỗi, thông báo lỗi liên quan đến khóa ngoại
                TempData["Error"] = "Bệnh viện được chọn không xóa được!!!!";
            }
            return RedirectToAction("DMBenhVien", "Admin");
        }
        // End Bệnh viện

        // Bệnh nhân
        public IActionResult HoSoBenhNhan(string q = null)
        {
            var redirect = RedirectIfNotLoggedIn();
            if (redirect != null) return redirect;
            DataModel db = new DataModel();
            ArrayList list;
            if (!string.IsNullOrEmpty(q))
            {
                // Tìm kiếm tên gần đúng, không phân biệt hoa thường
                list = db.get($"SELECT * FROM NGUOIDUNG WHERE MaPQ = 2 AND TenND LIKE N'%{q.Replace("'", "''")}%'");
            }
            else
            {
                list = db.get("SELECT * FROM NGUOIDUNG WHERE MaPQ = 2");
            }
            ViewBag.list = list;
            ViewBag.TotalPatients = list.Count;
            ViewBag.SearchQuery = q;
            return View();
        }

        public IActionResult ChiTietBenhNhan(string id)
        {
            var redirect = RedirectIfNotLoggedIn();
            if (redirect != null) return redirect;
            DataModel db = new DataModel();
            ViewBag.list = db.get($"SELECT * FROM NGUOIDUNG WHERE MaND = {id}");
            return View();
        }

        public IActionResult SreachBenhNhan(string tenbn)
        {
            var redirect = RedirectIfNotLoggedIn();
            if (redirect != null) return redirect;
            DataModel db = new DataModel();
            ViewBag.list = db.get("EXEC SearchPatient N'" +tenbn+ "';" );
            return View();
        }


        [HttpPost]
        public IActionResult XoaBenhNhan(string mabn) 
        {
            var redirect = RedirectIfNotLoggedIn();
            if (redirect != null) return redirect;
            DataModel db = new DataModel();
            try
            {
                // Gọi stored procedure để xóa khoa bệnh
                db.get("EXEC DeletePatientById " + mabn + ";");
                TempData["Message"] = "Xóa bệnh nhân thành công!";
            }
            catch (Exception)
            {
                // Nếu xảy ra lỗi, thông báo lỗi liên quan đến khóa ngoại
                TempData["Error"] = "Bệnh nhân được chọn không xóa được!!!!";
            }
            return RedirectToAction("HoSoBenhNhan", "Admin");
        }

        // End Bệnh nhân


        // Bác sĩ
        public IActionResult HoSoBacSi(string q = null)
        {
            var redirect = RedirectIfNotLoggedIn();
            if (redirect != null) return redirect;
            DataModel db = new DataModel();
            ArrayList list;
            if (!string.IsNullOrEmpty(q))
            {
                // Tìm kiếm tên gần đúng, không phân biệt hoa thường
                list = db.get($@"SELECT bs.MaBS, nd.TenND, bs.ChuyenKhoa, bs.ChuyenMon, nd.GioiTinh, bs.QuaTrinhCongTac, bs.MaND FROM BACSI bs JOIN NGUOIDUNG nd ON bs.MaND = nd.MaND WHERE nd.TenND LIKE N'%{q.Replace("'", "''")}%'");
            }
            else
            {
                list = db.get("SELECT bs.MaBS, nd.TenND, bs.ChuyenKhoa, bs.ChuyenMon, nd.GioiTinh, bs.QuaTrinhCongTac, bs.MaND FROM BACSI bs JOIN NGUOIDUNG nd ON bs.MaND = nd.MaND");
            }
            ViewBag.list = list;
            ViewBag.TotalDoctors = list.Count;
            ViewBag.SearchQuery = q;
            return View();
        }

        public IActionResult ChiTietBacSi(string id)
        {
            var redirect = RedirectIfNotLoggedIn();
            if (redirect != null) return redirect;
            DataModel db = new DataModel();
            ViewBag.list = db.get("EXEC GetDoctorDetailsByUser_Id " +id+ ";" );
            return View();
        }

        public IActionResult SreachBacSi(string tenbs)
        {
            var redirect = RedirectIfNotLoggedIn();
            if (redirect != null) return redirect;
            DataModel db = new DataModel();
            ViewBag.list = db.get("EXEC SearchDoctorByName N'" +tenbs+ "';" );
            return View();
        }

        [HttpPost]
        public IActionResult XoaBacSi(string mabs) 
        {
            var redirect = RedirectIfNotLoggedIn();
            if (redirect != null) return redirect;
            DataModel db = new DataModel();
            try
            {
                // Gọi stored procedure để xóa khoa bệnh
                db.get("EXEC DeleteDoctor " + mabs + ";");
                TempData["Message"] = "Xóa bác sĩ thành công!";
            }
            catch (Exception)
            {
                // Nếu xảy ra lỗi, thông báo lỗi liên quan đến khóa ngoại
                TempData["Error"] = "Bác sĩ được chọn không xóa được!!!!";
            }
            return RedirectToAction("HoSoBenhNhan", "Admin");
        }

        public IActionResult DonDangKyBacSi()
        {
            var redirect = RedirectIfNotLoggedIn();
            if (redirect != null) return redirect;
            DataModel db = new DataModel();
            ViewBag.listDDK = db.get("EXEC GetPatientsByDoctor");
            return View();
        }

        public IActionResult ChiTietDonDangKy(string mabs) 
        {
            var redirect = RedirectIfNotLoggedIn();
            if (redirect != null) return redirect;
            DataModel db = new DataModel();
            ViewBag.list = db.get("EXEC GetDoctorDetails " + mabs + ";");
            return View();
        }

        [HttpPost]
        public IActionResult DuyetBacSi(string mabs, string sotienkham) {
            var redirect = RedirectIfNotLoggedIn();
            if (redirect != null) return redirect;
            DataModel db = new DataModel();
            ViewBag.list = db.get("EXEC ConfirmDoctor " + mabs + "," + sotienkham + ";");
            return RedirectToAction("DonDangKyBacSi", "Admin");
        }

        [HttpPost]
        public IActionResult HuyDonDangKy(string mabs)
        {
            var redirect = RedirectIfNotLoggedIn();
            if (redirect != null) return redirect;
            DataModel db = new DataModel();
            ViewBag.list = db.get("EXEC DeleteDoctorRegistration " + mabs + ";");
            return RedirectToAction("DonDangKyBacSi", "Admin");
        }

        // End Bác sĩ

        //Lịch khám
        public IActionResult LichKhamBenh()
        {
            var redirect = RedirectIfNotLoggedIn();
            if (redirect != null) return redirect;
            DataModel db = new DataModel();
            ViewBag.listLH = db.get("EXEC GetAppointmentDetails");
            return View();
        }

        //End Lịch khám

        //Đánh giá
        public IActionResult DanhGiaPhanHoi() 
        {
            var redirect = RedirectIfNotLoggedIn();
            if (redirect != null) return redirect;
            DataModel db = new DataModel();
            ViewBag.listDG = db.get("EXEC GetAllRatings ");
            return View();
        }

        //End Đánh giá

        //Thống kê
        public IActionResult ThongKe() 
        {
            var redirect = RedirectIfNotLoggedIn();
            if (redirect != null) return redirect;
            DataModel db = new DataModel();

            ViewBag.listTKLK = JsonConvert.SerializeObject(db.get("EXEC ThongKeLuotKhamTheoThang"));
            ViewBag.listTKDT = JsonConvert.SerializeObject(db.get("EXEC ThongKeDoanhThuKhachTheoThang"));

            return View();
        }

        //End Thống kê

        //Thông báo
        public IActionResult ThongBaoBV()
        {
            var redirect = RedirectIfNotLoggedIn();
            if (redirect != null) return redirect;
            DataModel db = new DataModel();
            ViewBag.listTB = db.get("EXEC LayDanhSachThongBao ");
            ViewBag.listND = db.get("SELECT * from NGUOIDUNG ");
            return View();
        }

        [HttpPost]
        public IActionResult GuiThongBao(string tieudetb, string noidungtb, string thoigiantb, string mand)
        {
            try
            {
                // Chuyển đổi thời gian từ chuỗi thành kiểu DateTime
                DateTime parsedTime = DateTime.Parse(thoigiantb);
                
                // Thực thi câu lệnh SQL để lưu thông báo
                DataModel db = new DataModel();
                db.get($"EXEC GuiThongBaoChoNguoiDung N'{tieudetb}', N'{noidungtb}', '{parsedTime}', '{mand}'");
                
                TempData["Message"] = "Thông báo đã được gửi thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi gửi thông báo: " + ex.Message;
            }

            return RedirectToAction("ThongBaoBV", "Admin");
        }


        [HttpPost]
        public IActionResult XoaThongBao(string matb)
        {
            var redirect = RedirectIfNotLoggedIn();
            if (redirect != null) return redirect;
            DataModel db = new DataModel();
            try
            {
                // Gọi stored procedure để xóa khoa bệnh
                db.get("EXEC XoaThongBao " + matb + ";");
                TempData["Message"] = "Xóa thông báo thành công!";
            }
            catch (Exception)
            {
                
                TempData["Error"] = "Thông báo được chọn không xóa được!!!!";
            }
            return RedirectToAction("ThongBaoBV", "Admin");
        }

        //End Thông báo
        

        //Thanh toán
        public IActionResult DSThanhToan()
        {
            var redirect = RedirectIfNotLoggedIn();
            if (redirect != null) return redirect;
            DataModel db = new DataModel();
            ViewBag.litsTT = db.get("EXEC LayDanhSachThanhToan");
            return View();
        }

        [HttpPost]       
        public IActionResult HoanPhiKham(string mathanhtoan, string refundReason)
        {
            var redirect = RedirectIfNotLoggedIn();
            if (redirect != null) return redirect;
            DataModel db = new DataModel();
            try
            {
                // Gọi stored procedure mới với lý do hoàn phí
                var parameters = new Dictionary<string, object>
                {
                    { "@MaTT", mathanhtoan },
                    { "@RefundReason", refundReason ?? "" }
                };
                db.ExecuteStoredProcedure("HoanPhiKham", parameters);
                TempData["Message"] = "Hoàn phí thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi hoàn phí: " + ex.Message;
            }
            return RedirectToAction("DSThanhToan", "Admin");
        }

        //End Thanh toán

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(); // Trả về view lỗi mặc định 'Error.cshtml'
        }
    }
}