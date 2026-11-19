using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AachSavor.Models;
using AachSavor.Services;

namespace AachSavor.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IStoreService _storeService;

        public HomeController(IStoreService storeService)
        {
            _storeService = storeService;
        }

        public IActionResult Index()
        {
            int? currentNvId = null;
            var nvClaim = User.Claims.FirstOrDefault(c => c.Type == "NhanVienID");
            if (nvClaim != null && int.TryParse(nvClaim.Value, out var parsedId))
            {
                currentNvId = parsedId;
            }

            var stats = _storeService.GetDashboardStats(currentNvId);
            return View(stats);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult SendThumbUp([FromBody] SendThumbUpRequest request)
        {
            if (request == null || request.NhanVienID <= 0 || string.IsNullOrWhiteSpace(request.NoiDung))
            {
                return Json(new { success = false, message = "Vui lòng nhập đầy đủ nội dung khen thưởng!" });
            }

            int adminId = 1;
            var nvClaim = User.Claims.FirstOrDefault(c => c.Type == "NhanVienID");
            if (nvClaim != null && int.TryParse(nvClaim.Value, out var parsedId))
            {
                adminId = parsedId;
            }

            var adminTen = User.Identity?.Name ?? "Admin";
            var tu = _storeService.SendThumbUp(request.NhanVienID, adminId, adminTen, request.NoiDung, request.TienThuong, request.DiemThuong);

            return Json(new
            {
                success = true,
                message = $"Đã gửi Thumb Up và thưởng +{request.TienThuong:N0} đ (+{request.DiemThuong} điểm) cho nhân viên {tu.NhanVien?.TenNV}! Rank hiện tại: {tu.NhanVien?.Rank} ({tu.NhanVien?.DiemTichLuy} điểm)",
                thumbUp = new
                {
                    thumbUpId = tu.ThumbUpID,
                    nhanVienId = tu.NhanVienID,
                    noiDung = tu.NoiDung,
                    tienThuong = tu.TienThuong,
                    diemThuong = tu.DiemThuong,
                    newPoints = tu.NhanVien?.DiemTichLuy ?? 0,
                    newRank = tu.NhanVien?.Rank ?? "A1"
                }
            });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
