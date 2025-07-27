var builder = WebApplication.CreateBuilder(args);

// Thêm các dịch vụ cần thiết vào container.
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache(); // Cài đặt bộ nhớ đệm phân tán
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian hết hạn của session
    options.Cookie.HttpOnly = true; // Cookie chỉ có thể truy cập từ HTTP
    options.Cookie.IsEssential = true; // Đảm bảo cookie cần thiết cho ứng dụng
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

// Thêm SignalR vào container
builder.Services.AddSignalR();

var app = builder.Build();

// Cấu hình pipeline yêu cầu HTTP.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); // Xử lý lỗi khi ứng dụng không phải môi trường phát triển
    app.UseHsts(); // Sử dụng HTTP Strict Transport Security (HSTS)
}

app.UseHttpsRedirection(); // Chuyển hướng tất cả các yêu cầu HTTP sang HTTPS
app.UseStaticFiles(); // Cung cấp các tệp tĩnh (CSS, JavaScript, hình ảnh, v.v.)
app.UseRouting(); // Cấu hình routing cho ứng dụng

// Kích hoạt session middleware
app.UseSession(); 

app.UseAuthorization(); // Kích hoạt middleware kiểm tra quyền truy cập

// Định tuyến các yêu cầu tới các controller
app.MapControllerRoute(
    name: "admin",
    pattern: "Admin/{action=AdminLogin}/{id?}",
    defaults: new { controller = "Admin" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Định tuyến SignalR Hub


app.Run(); // Khởi chạy ứng dụng
app.Run(); // Khởi chạy ứng dụng