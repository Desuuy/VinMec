using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DoAnCNPM.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;


public class KhachHangAPI : Controller
{
    DataModel db =new DataModel();
    public JsonResult Index()
    {
        DataModel db =new DataModel();
        ArrayList a = db.get("EXEC getAllBenhVien");
        return Json(a);
    }
    public JsonResult KhoaBenh()
    {
        DataModel db =new DataModel();
        ArrayList a = db.get("EXEC getAllKhoaBenh");
        return Json(a);
    }
    public JsonResult BacSi()
    {
        
        ArrayList a = db.get("EXEC getAllBacSi");
        return Json(a);
    }
    [HttpPost]
    public JsonResult DangNhap(string sdt, string password)
    {
        DataModel db = new DataModel();
        string query = $"EXEC CheckLogin '{sdt}', '{password}'";
        ArrayList result = db.get(query);

        if (result.Count > 0)
        {
            return Json(new { success = true, data = result });
        }
        else
        {
            return Json(new { success = false, message = "Số điện thoại hoặc mật khẩu không đúng." });
        }
    }

    public JsonResult ListBenhVien()
    {
        try
        {
            // Lấy danh sách bệnh viện từ database
            var listBV = db.get("EXEC getAllBenhVien");

            // Lấy danh sách top 7 bác sĩ
            var listBS5 = db.get("EXEC GetTop7Doctors");

            // Trả về dữ liệu dưới dạng mảng JSON
            return Json(new object[]
            {
                true, // Trạng thái thành công
                listBV, // Danh sách bệnh viện
                listBS5 // Danh sách top 7 bác sĩ
            });
        }
        catch (Exception ex)
        {
            // Trả về lỗi dưới dạng mảng JSON
            return Json(new object[]
            {
                false, // Trạng thái lỗi
                ex.Message // Thông báo lỗi
            });
        }
    }


    [HttpGet]
    public JsonResult BookExamine(string MaBS)
    {
        try
        {

            DataModel db = new DataModel();
            var taikhoan = HttpContext.Session.GetString("taikhoan");

            var userInfo = new object();
            if (!string.IsNullOrEmpty(taikhoan))
            {
                var result = db.get("SELECT * from NGUOIDUNG where manD=" + taikhoan);
                if (result != null && result.Count > 0)
                {
                    userInfo = result[0]; // Lấy thông tin người dùng
                }
            }

            var doctorDetails = db.get($"EXEC DetDETAILlBACSI1 {MaBS}");

            return Json(new object[]
            {
                true, // success
                "Data retrieved successfully", // message
                taikhoan, // TaiKhoan
                userInfo, // UserInfo
                doctorDetails // DoctorDetails
            });
        }
        catch (Exception ex)
        {
            return Json(new object[]
            {
                false, // success
                ex.Message // message
            });
        }
    }

    [HttpPost]
    public JsonResult BookExamineProcess(string MaBS, string MoTaBenh, List<IFormFile> HinhAnhBenhs)
    {
        try
        {
            DataModel db = new DataModel();
            var taikhoan = HttpContext.Session.GetString("taikhoan");
            var MaND = taikhoan;

            // Lưu thông tin hồ sơ bệnh
            var result = db.get($"DECLARE @MaHS INT; EXEC SAVEHOSO {MaND}, N'{MoTaBenh}', @MaHS = @MaHS OUTPUT; SELECT @MaHS;");
            if (result == null || result.Count == 0)
            {
                return Json(new object[]
                {
                    false, // success
                    "Failed to save medical record" // message
                });
            }

            int MaHS = int.Parse(result[0].ToString());

            // Lưu hình ảnh bệnh
            var uploadedFiles = new List<string>();
            foreach (var file in HinhAnhBenhs)
            {
                string nameFile = Path.GetFileName(file.FileName);
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads");
                Directory.CreateDirectory(uploadsFolder); // Tạo thư mục nếu chưa tồn tại
                string filePath = Path.Combine(uploadsFolder, nameFile);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                // Lưu thông tin hình ảnh vào cơ sở dữ liệu
                db.get($"EXEC SAVEHINHANHBENH {MaHS}, '{nameFile}'");
                uploadedFiles.Add(nameFile);
            }

            // Lưu cuộc hẹn khám
            db.get($"EXEC SAVECUOCHENKHAM {MaND}, {MaBS}, {MaHS}, null");

            return Json(new object[]
            {
                true, // success
                "Booking process completed successfully", // message
                MaHS, // MedicalRecordId
                uploadedFiles // UploadedFiles array
            });
        }
        catch (Exception ex)
        {
            return Json(new object[]
            {
                false, // success
                ex.Message // message
            });
        }
    }

        

}
