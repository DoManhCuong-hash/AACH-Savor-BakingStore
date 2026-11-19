using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AachSavor.Models;
using AachSavor.Services;

namespace AachSavor.Controllers
{
    [Authorize]
    public class KhachHangController : Controller
    {
        private readonly IStoreService _storeService;

        public KhachHangController(IStoreService storeService)
        {
            _storeService = storeService;
        }

        public IActionResult Index(string? keyword)
        {
            var list = _storeService.GetKhachHangs(keyword);
            ViewBag.Keyword = keyword;

            // Đếm số hóa đơn cho từng khách hàng
            var allHoaDons = _storeService.GetHoaDons();
            ViewBag.InvoiceCountMap = allHoaDons
                .Where(h => h.KhachHangID.HasValue)
                .GroupBy(h => h.KhachHangID!.Value)
                .ToDictionary(g => g.Key, g => g.Count());

            ViewBag.TotalSpentMap = allHoaDons
                .Where(h => h.KhachHangID.HasValue)
                .GroupBy(h => h.KhachHangID!.Value)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.TongTien));

            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Save(KhachHang kh)
        {
            if (string.IsNullOrWhiteSpace(kh.TenKH))
            {
                TempData["ErrorMessage"] = "Tên khách hàng không được để trống!";
                return RedirectToAction(nameof(Index));
            }

            _storeService.SaveKhachHang(kh);
            TempData["SuccessMessage"] = kh.KhachHangID == 0 ? "Thêm khách hàng thành công!" : "Cập nhật khách hàng thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult GetLichSuMuaHang(int id)
        {
            var kh = _storeService.GetKhachHangById(id);
            if (kh == null) return NotFound();

            var history = _storeService.GetLichSuMuaHang(id);
            return Json(new
            {
                khachHang = new
                {
                    maKH = $"KH{kh.KhachHangID:D4}",
                    tenKH = kh.TenKH,
                    sdt = kh.SDT,
                    diaChi = kh.DiaChi
                },
                invoices = history.Select(h => new
                {
                    maHD = $"HD{h.HoaDonID:D5}",
                    ngayLap = h.NgayLap.ToString("dd/MM/yyyy HH:mm"),
                    phuongThucTT = h.PhuongThucTT,
                    tongTien = h.TongTien,
                    soLuongSanPham = h.ChiTietHoaDons.Sum(x => x.SoLuong)
                }).ToList()
            });
        }

        [HttpGet]
        public IActionResult GetListJson(string? keyword)
        {
            var list = _storeService.GetKhachHangs(keyword);
            return Json(list);
        }
    }
}
