using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AachSavor.Models;
using AachSavor.Services;

namespace AachSavor.Controllers
{
    [Authorize]
    public class BanHangController : Controller
    {
        private readonly IStoreService _storeService;

        public BanHangController(IStoreService storeService)
        {
            _storeService = storeService;
        }

        public IActionResult Index()
        {
            var sanPhams = _storeService.GetSanPhams();
            var khachHangs = _storeService.GetKhachHangs();

            ViewBag.SanPhams = sanPhams;
            ViewBag.KhachHangs = khachHangs;
            ViewBag.Categories = sanPhams.Select(s => s.DanhMuc ?? "Khác").Distinct().ToList();

            return View();
        }

        [HttpPost]
        public IActionResult Checkout([FromBody] PosCheckoutRequest request)
        {
            if (request == null)
            {
                return Json(new { success = false, message = "Dữ liệu hóa đơn không hợp lệ!" });
            }

            // Lấy ID nhân viên đăng nhập từ Claim
            int nhanVienId = 1;
            var nvClaim = User.Claims.FirstOrDefault(c => c.Type == "NhanVienID");
            if (nvClaim != null && int.TryParse(nvClaim.Value, out var parsedId))
            {
                nhanVienId = parsedId;
            }

            var result = _storeService.CreateHoaDon(request, nhanVienId);
            if (!result.Success || result.HoaDon == null)
            {
                return Json(new { success = false, message = result.Message });
            }

            return Json(new
            {
                success = true,
                message = result.Message,
                hoaDonId = result.HoaDon.HoaDonID,
                hoaDon = new
                {
                    maHD = $"HD{result.HoaDon.HoaDonID:D5}",
                    ngayLap = result.HoaDon.NgayLap.ToString("dd/MM/yyyy HH:mm"),
                    nhanVien = result.HoaDon.NhanVien?.TenNV ?? "Nhân viên",
                    khachHang = result.HoaDon.KhachHang?.TenKH ?? "Khách lẻ vãng lai",
                    sdtKhachHang = result.HoaDon.KhachHang?.SDT ?? "",
                    phuongThucTT = result.HoaDon.PhuongThucTT,
                    tongTien = result.HoaDon.TongTien,
                    tienKhachDua = request.TienKhachDua,
                    tienThua = Math.Max(0, request.TienKhachDua - result.HoaDon.TongTien),
                    items = result.HoaDon.ChiTietHoaDons.Select(ct => new
                    {
                        maSP = ct.SanPham?.MaSP ?? "",
                        tenSP = ct.SanPham?.TenSP ?? "",
                        donViTinh = ct.SanPham?.DonViTinh ?? "Cái",
                        soLuong = ct.SoLuong,
                        donGia = ct.DonGia,
                        thanhTien = ct.ThanhTien,
                        isGiaSi = ct.IsGiaSi
                    }).ToList()
                }
            });
        }

        [HttpGet]
        public IActionResult GetHoaDonDetail(int id)
        {
            var hd = _storeService.GetHoaDonById(id);
            if (hd == null) return NotFound();

            return Json(new
            {
                hoaDonId = hd.HoaDonID,
                maHD = $"HD{hd.HoaDonID:D5}",
                ngayLap = hd.NgayLap.ToString("dd/MM/yyyy HH:mm"),
                nhanVien = hd.NhanVien?.TenNV ?? "Nhân viên",
                khachHang = hd.KhachHang?.TenKH ?? "Khách lẻ vãng lai",
                sdtKhachHang = hd.KhachHang?.SDT ?? "",
                phuongThucTT = hd.PhuongThucTT,
                tongTien = hd.TongTien,
                items = hd.ChiTietHoaDons.Select(ct => new
                {
                    maSP = ct.SanPham?.MaSP ?? "",
                    tenSP = ct.SanPham?.TenSP ?? "",
                    donViTinh = ct.SanPham?.DonViTinh ?? "Cái",
                    soLuong = ct.SoLuong,
                    donGia = ct.DonGia,
                    thanhTien = ct.ThanhTien,
                    isGiaSi = ct.IsGiaSi
                }).ToList()
            });
        }
    }
}
