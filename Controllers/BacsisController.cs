
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
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;

using Microsoft.Extensions.Logging;

public class BacsisController : Controller
{
    private readonly ILogger<BacsisController> _logger;
    private readonly ISession _session;
    public BacsisController(ILogger<BacsisController> logger,IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _session = httpContextAccessor.HttpContext.Session;
    }
    
    DataModel db = new DataModel();
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public IActionResult KTBS(string TenDn, string password)
    {
        DataModel db = new DataModel();
        var list = db.get("EXEC CheckLoginBSs '" + TenDn + "','" + password + "'");

        if (list.Count > 0 && list[0] is ArrayList arrayList && arrayList.Count > 1)
        {
            var userId = arrayList[0]?.ToString() ?? "Unknown"; // Giả sử ID là phần tử thứ 2 trong ArrayList
            var tennd = arrayList[1]?.ToString() ?? "Unknown";
            
            // Lưu thông tin vào session
            _session.SetString("tennd", tennd);
            _session.SetString("userId", userId); // Lưu ID vào session

            return RedirectToAction("HomeBs", "Bacsis"); // Chuyển hướng đến trang chủ của bác sĩ
        }
        else
        {
            // Nếu đăng nhập thất bại, tạo thông báo lỗi và trả về cho view
            TempData["ErrorMessage"] = "Đăng nhập không thành công. Vui lòng kiểm tra lại số điện thoại hoặc mật khẩu.";
            return RedirectToAction("Index", "Bacsis"); // Quay lại trang đăng nhập với thông báo lỗi
        }
    }



    public IActionResult DangKyBs()
    {
        ViewBag.kb = db.get("select * from KHOABENH");
        ViewBag.bv = db.get("select * from BENHVIEN");
        ViewBag.cn = db.get("select * from CHUYENNGANH");
        return View();
    }

    [HttpPost]
    public IActionResult ThucHienDKBs(
        string username,
        string password,
        string email,
        string phone,
        string diaChi,
        DateTime birthyear,
        string gender,
        IFormFile hinhanh,
        string cmtnumber,
        DateTime issuedate,
        string issueplace,
        IFormFile cmtimagefront,
        IFormFile cmtimageback,
        string chonChuyenKhoa,
        int namKinhNghiem,
        string noiCongTac,
        string khoa,
        string soChungChi,
        DateTime ngayThangCap,
        IFormFile anhChungChi1,
        IFormFile anhChungChi2,
        string hocVi,
        string hocHam,
        string chuyenMon,
        string thontinbangcap,
        string gioithieu,
        string cacBenhDieuTri,
        string quaTrinhhoc,
        string quaTrinhCongTac,
        string nghienCuuKhoaHoc,
        string giangDay,
        string hoiVienCongTac)
    {
        try
        {
            DataModel db = new DataModel();
            // Kiểm tra tệp hình ảnh và lưu vào thư mục "Hinh"
            if (hinhanh != null && hinhanh.Length > 0)
            {
                string filename = Path.GetFileName(hinhanh.FileName); // Lấy tên tệp
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", filename); // Đường dẫn lưu tệp
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    hinhanh.CopyTo(stream); // Lưu tệp
                }
            }

            // Tương tự cho các tệp khác như cmtimagefront, cmtimageback, anhChungChi1, anhChungChi2
            // Lưu tên tệp vào cơ sở dữ liệu
            if (cmtimagefront != null && cmtimagefront.Length > 0)
            {
                string cmtFrontFileName = Path.GetFileName(cmtimagefront.FileName);
                string cmtFrontPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", cmtFrontFileName);
                using (var stream = new FileStream(cmtFrontPath, FileMode.Create))
                {
                    cmtimagefront.CopyTo(stream);
                }
            }

            if (cmtimageback != null && cmtimageback.Length > 0)
            {
                string cmtBackFileName = Path.GetFileName(cmtimageback.FileName);
                string cmtBackPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", cmtBackFileName);
                using (var stream = new FileStream(cmtBackPath, FileMode.Create))
                {
                    cmtimageback.CopyTo(stream);
                }
            }

            int randomValue = new Random().Next(1, 10000);

            // Sử dụng stored procedure để thực hiện thêm thông tin bác sĩ vào cơ sở dữ liệu
            string result = "EXEC DangKyBacSi N'" + username + "','"
                + password + "', '" 
                + email + "', '" 
                + phone + "', N'"
                + diaChi + "', '" 
                + birthyear.ToString("yyyy-MM-dd") + "', N'" 
                + gender + "', N'"
                + hinhanh.FileName + "', 0 , "
                + randomValue +", NULL, 2 ,"
                + cmtnumber + ", '"
                + issuedate.ToString("yyyy-MM-dd") + "', N'"
                + issueplace + "', N'"         
                + cmtimagefront.FileName + "', '" 
                + cmtimageback.FileName + "', '" 
                + chonChuyenKhoa + "', " 
                + namKinhNghiem + ", "
                + noiCongTac + ", " 
                + khoa + ", '"
                + soChungChi + "', '" 
                + ngayThangCap.ToString("yyyy-MM-dd") + "', N'"
                + anhChungChi1.FileName + "', N'" 
                + anhChungChi2.FileName + "', N'" 
                + hocVi + "', N'" 
                + hocHam + "', N'"
                + chuyenMon + "', N'"
                + thontinbangcap + "', N'" 
                + gioithieu + "', N'"
                + cacBenhDieuTri + "', N'"
                + quaTrinhhoc + "', N'" 
                + quaTrinhCongTac + "', N'"
                + nghienCuuKhoaHoc + "', N'"
                + giangDay + "', N'"
                + hoiVienCongTac + "', 0 ,2 ;";

            db.get(result);      
            

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Có lỗi xảy ra khi thực hiện đăng ký bác sĩ.");
            // Nếu có lỗi, giữ lại trang DangKyBs và thông báo lỗi
            return RedirectToAction("DangKyBs", "Bacsis");
        }   
        return RedirectToAction("DKTC", "Bacsis");
  
    }


    public IActionResult HomeBs()
    {
        var userId = _session.GetString("userId");
        ArrayList idBS_sql = db.get("EXEC GetMaBSByUserId @UserId = " + userId);

        // Ép kiểu từng phần tử
        ArrayList row = (ArrayList)idBS_sql[0]; // Lấy dòng đầu tiên
        int idBS = int.Parse(row[0].ToString());
        ViewBag.luong = db.get("exec sp_XemThongTinNguoiDung " + userId);
        ViewBag.daXacNhan = db.get($"EXEC Xemtatcalichhendaxacnhan {idBS}");
        ViewBag.choXacNhan = db.get($"EXEC Xemtatcalichhenchuaxacnhan {idBS}");
        return View();  
    }
    public IActionResult ThongKe(string nam, string loaiBieuDo = "line")
    {
        var userId = _session.GetString("userId");
        ViewBag.luong = db.get("exec sp_XemThongTinNguoiDung " + userId);
        ViewBag.list = db.get("EXEC ThongKeCuocHenTheoThang '" + userId + "','" + nam + "'");
        ViewBag.loaiBieuDo = loaiBieuDo;
        ViewBag.nam = nam;
        // 1. Lấy ID Bác sĩ từ UserId
        ArrayList idBS_sql = db.get($"EXEC GetMaBSByUserId @UserId = '{userId}'");
        ArrayList row = (ArrayList)idBS_sql[0];
        int idBS = int.Parse(row[0].ToString());

        // 3. Lấy thống kê trong năm
        var listHT = db.get("SELECT COUNT(*) FROM CUOCHENKHAM WHERE MaBS = '"+ idBS +"' AND MaTTCH = 4 AND YEAR(ThoiGianHen) = '"+ nam +"'");
        var listHuy = db.get("SELECT COUNT(*) FROM CUOCHENKHAM WHERE MaBS = '"+ idBS +"' AND MaTTCH = 3 AND YEAR(ThoiGianHen) = '"+ nam +"'");
        var tongTienList = db.get("SELECT ISNULL(SUM(SoTienTT), 0) FROM CUOCHENKHAM WHERE MaBS = '"+idBS+"' AND YEAR(ThoiGianHen) = '"+ nam +"'");

        int soHoanThanh = 0, soHuy = 0, tongTienValue = 0;
        if (listHT != null && listHT.Count > 0 && listHT[0] is System.Collections.IList rowHT)
            int.TryParse(rowHT[0]?.ToString(), out soHoanThanh);
        if (listHuy != null && listHuy.Count > 0 && listHuy[0] is System.Collections.IList rowHuy)
            int.TryParse(rowHuy[0]?.ToString(), out soHuy);
        if (tongTienList != null && tongTienList.Count > 0 && tongTienList[0] is System.Collections.IList rowTien)
            int.TryParse(rowTien[0]?.ToString(), out tongTienValue);

        ViewBag.SoHoanThanh = soHoanThanh;
        ViewBag.SoHuy = soHuy;
        ViewBag.TongTien = tongTienValue;
        return View("ThongKe");
    }

    public IActionResult DKTC(){
        return View(); 
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View("Error!");
    }
    public IActionResult HoSoBN()
    {
        
        var userId = _session.GetString("userId");
        ViewBag.list = db.get("EXEC sp_LayDanhSachHoSoTheoMaBacSi "+ userId);
        return View();
    }

    [HttpPost]
    public IActionResult ThemHs(string patientPhone, string patientDescription)
    {

        ViewBag.list = db.get("exec AddPatientRecordByPhone '"+ patientPhone +"',N'" + patientDescription + "'");
        return RedirectToAction("HoSoBN", "Bacsis");
    }
    public IActionResult XoaHs(string id){
     
        ViewBag.list=db.get("EXEC DeletePatientRecord "+ id);
        return RedirectToAction("HoSoBN", "Bacsis");
    }

    [HttpPost]
    public IActionResult Suahoso(string patientId,string patientDescription){
      
        ViewBag.list=db.get("EXEC UpdatePatientRecord "+ patientId +",N'"+ patientDescription + "'");
        return RedirectToAction("HoSoBN", "Bacsis");
    }

    public ActionResult LichHenKham()
    {
        var userId = _session.GetString("userId");
        ViewBag.list = db.get("EXEC GetAllCuocHenByMaND " + userId);
        return View();
    }

     public ActionResult DaXacNhan()
    {
        var userId = _session.GetString("userId");
        ViewBag.list = db.get("EXEC GetAllCuocHenByMaNDDaXN " + userId);
        return View();
    }
     public ActionResult DaHoanThanh()
    {
        var userId = _session.GetString("userId");
        ViewBag.luong = db.get("exec sp_XemThongTinNguoiDung " + userId);
        ViewBag.list = db.get("EXEC GetAllCuocHenByMaNDDaHT "  + userId);
        ViewBag.SoLuongHoanThanh = ViewBag.list != null ? ((System.Collections.ICollection)ViewBag.list).Count : 0;
        return View();
    }

     public ActionResult DaBiHuy()
    {
         var userId = _session.GetString("userId");
        ViewBag.list = db.get("EXEC GetAllCuocHenByMaNDDaHuy "  + userId);
        ViewBag.SoLuongHuy = ViewBag.list != null ? ((System.Collections.ICollection)ViewBag.list).Count : 0;
        return View();
    }

     [HttpPost]
    public ActionResult Updatecuochen(string id, string matt)
    {
         // Lấy thời gian hiện tại
        db.get("Exec UpdateMaTTCH "+ id +"," + matt);
        return RedirectToAction("HomeBs","Bacsis");
    }

    [HttpPost]
    public ActionResult UpdatecuochenTT(string id,string matt)
    {
        var userId = _session.GetString("userId");
        db.get("Exec UpdateMaTTCH "+ id +"," + matt);
        db.get("Exec UpdateSoDuTKForUserAndDoctor "+ userId +"," + id);
        return RedirectToAction("DaHoanThanh","Bacsis");
    }
     public ActionResult DoanhThu()
    {
        return View();
    }
  
     public ActionResult ThongBao()
    {
         var userId = _session.GetString("userId");
        ViewBag.ThongBaos = db.get("EXEC sp_GetThongBaoByMaND " + userId );
        return View();
    }

    // API để lấy tất cả lịch hẹn chưa xác nhận
    [HttpGet]
    public JsonResult GetAllUnconfirmedAppointments()
    {
        var userId = _session.GetString("userId");
        var list = db.get($"EXEC Xemtatcalichhenchuaxacnhan {userId}");
        return Json(list);
    }

    // API để lấy tất cả lịch hẹn đã xác nhận
    [HttpGet]
    public JsonResult GetAllConfirmedAppointments()
    {
        var userId = _session.GetString("userId");
        var list = db.get($"EXEC Xemtatcalichhendaxacnhan {userId}");
        return Json(list);
    }

    // API để lấy tất cả lịch hẹn đã hoàn thành
    [HttpGet]
    public JsonResult GetAllCompletedAppointments()
    {
        var userId = _session.GetString("userId");
        var list = db.get($"EXEC Xemtatcalichhendahoanthanh {userId}");
        return Json(list);
    }

    // API để lấy tất cả lịch hẹn đã bị hủy
    [HttpGet]
    public JsonResult GetAllCancelledAppointments()
    {
        var userId = _session.GetString("userId");
        var list = db.get($"EXEC Xemtatcalichhendahuy {userId}");
        return Json(list);
    }

    // API cập nhật trạng thái kết thúc của một lịch hẹn
    [HttpPost]
    public JsonResult UpdateAppointmentStatus(int id, string status)
    {
        db.get($"EXEC UpdateMaTTCH {id}, '{status}'");
        return Json(new { success = true });
    }

    // API để lấy thông báo của bác sĩ
    [HttpGet]
    public JsonResult GetNotifications()
    {
        var userId = _session.GetString("userId");
        var notifications = db.get($"EXEC sp_GetThongBaoByMaND {userId}");
        return Json(notifications);
    }

    public IActionResult ChiTietHoSoBN(int id)
    {
        // Lấy thông tin chi tiết hồ sơ bệnh nhân theo id (id là MaHS)
        var hoSo = db.get($"SELECT h.MaHS, n.TenND, n.NgaySinh, n.GioiTinh, n.sdt, n.DiaChi, h.MoTaBenh, h.MaND FROM HOSO h JOIN NGUOIDUNG n ON h.MaND = n.MaND WHERE h.MaHS = {id}");
        ViewBag.HoSo = hoSo;
        return View();
    }

    public IActionResult QuanLyCaNhan()
    {
        var userId = _session.GetString("userId");
        var info = db.get($"SELECT * FROM NGUOIDUNG WHERE MaND = {userId}");
        ViewBag.Info = info != null && info.Count > 0 ? info[0] : null;
        return View();
    }

    [HttpGet]
    public IActionResult CapNhatCaNhan()
    {
        var userId = _session.GetString("userId");
        var info = db.get($"SELECT * FROM NGUOIDUNG WHERE MaND = {userId}");
        ViewBag.Info = info != null && info.Count > 0 ? info[0] : null;
        return View();
    }

    [HttpPost]
    public IActionResult CapNhatCaNhan(string TenND, string Email, string sdt, string DiaChi, string NgaySinh, string GioiTinh, IFormFile AnhCaNhan)
    {
        var userId = _session.GetString("userId");
        string fileName = null;
        if (AnhCaNhan != null && AnhCaNhan.Length > 0)
        {
            fileName = Path.GetFileName(AnhCaNhan.FileName);
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", fileName);
            using (var stream = new FileStream(path, FileMode.Create))
            {
                AnhCaNhan.CopyTo(stream);
            }
        }
        string sql = $"UPDATE NGUOIDUNG SET TenND = N'{TenND}', Email = '{Email}', sdt = '{sdt}', DiaChi = N'{DiaChi}', NgaySinh = '{NgaySinh}', GioiTinh = N'{GioiTinh}'";
        if (!string.IsNullOrEmpty(fileName))
        {
            sql += $", AnhCaNhan = '{fileName}'";
        }
        sql += $" WHERE MaND = {userId}";
        db.get(sql);
        return RedirectToAction("QuanLyCaNhan", "Bacsis");
    }

    public IActionResult DangXuat()
    {
        _session.Clear();
        return RedirectToAction("Index", "Bacsis");
    }
}
