using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AachSavor.Models;
using AachSavor.Services;

namespace AachSavor.Controllers
{
    [Authorize(Roles = "Admin")]
    public class NhapKhoController : Controller
    {
        private readonly IStoreService _storeService;

        public NhapKhoController(IStoreService storeService)
        {
            _storeService = storeService;
        }

        public IActionResult Index()
        {
            var list = _storeService.GetPhieuNhaps();
            ViewBag.SanPhams = _storeService.GetSanPhams();
            return View(list);
        }

        [HttpPost]
        public IActionResult Create([FromBody] TaoPhieuNhapRequest request)
        {
            if (request == null || request.Items == null || request.Items.Count == 0)
            {
                return Json(new { success = false, message = "Dữ liệu phiếu nhập không hợp lệ!" });
            }

            int nhanVienId = 1;
            var nvClaim = User.Claims.FirstOrDefault(c => c.Type == "NhanVienID");
            if (nvClaim != null && int.TryParse(nvClaim.Value, out var parsedId))
            {
                nhanVienId = parsedId;
            }

            var result = _storeService.CreatePhieuNhap(request, nhanVienId);
            if (!result.Success || result.PhieuNhap == null)
            {
                return Json(new { success = false, message = result.Message });
            }

            return Json(new
            {
                success = true,
                message = result.Message,
                phieuNhapId = result.PhieuNhap.PhieuNhapID
            });
        }

        [HttpGet]
        public IActionResult GetDetailJson(int id)
        {
            var pn = _storeService.GetPhieuNhapById(id);
            if (pn == null) return NotFound();

            return Json(new
            {
                maPN = $"PN{pn.PhieuNhapID:D4}",
                ngayNhap = pn.NgayNhap.ToString("dd/MM/yyyy HH:mm"),
                nhanVien = pn.NhanVien?.TenNV ?? "Nhân viên",
                nhaCungCap = pn.NhaCungCap ?? "Nhà cung cấp chính",
                ghiChu = pn.GhiChu ?? "",
                tongTien = pn.TongTien,
                items = pn.ChiTietPhieuNhaps.Select(ct => new
                {
                    maSP = ct.SanPham?.MaSP ?? "",
                    tenSP = ct.SanPham?.TenSP ?? "",
                    donViTinh = ct.SanPham?.DonViTinh ?? "Cái",
                    soLuong = ct.SoLuongNhap,
                    donGia = ct.DonGiaNhap,
                    thanhTien = ct.ThanhTien
                }).ToList()
            });
        }
    }
}
