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
    public class ChotCaController : Controller
    {
        private readonly IStoreService _storeService;

        public ChotCaController(IStoreService storeService)
        {
            _storeService = storeService;
        }

        public IActionResult Index()
        {
            int? nvId = null;
            if (!User.IsInRole("Admin"))
            {
                var nvClaim = User.Claims.FirstOrDefault(c => c.Type == "NhanVienID");
                if (nvClaim != null && int.TryParse(nvClaim.Value, out var parsedId))
                {
                    nvId = parsedId;
                }
            }

            var list = _storeService.GetChotCas(nvId);
            return View(list);
        }

        [HttpGet]
        public IActionResult Create()
        {
            int nhanVienId = 1;
            var nvClaim = User.Claims.FirstOrDefault(c => c.Type == "NhanVienID");
            if (nvClaim != null && int.TryParse(nvClaim.Value, out var parsedId))
            {
                nhanVienId = parsedId;
            }

            var nv = _storeService.GetNhanVienById(nhanVienId);
            var (tongTienHang, tienChuyenKhoan, tienMat, soDon) = _storeService.GetThongKeCaHienTai(nhanVienId);

            decimal tienLe = 500000;
            decimal chiPhi = 0;
            decimal chenhLechShip = 0;
            decimal chietKhau = 0;
            decimal phaiThuSau = 0;

            // Công thức tính tiền két cuối ca:
            // Tiền két = Tiền lẻ + Tiền hàng + Chênh lệch ship - Chi phí - Chiết khấu - Phải thu sau - Quẹt thẻ/Chuyển khoản
            decimal tienKet = tienLe + tongTienHang + chenhLechShip - chiPhi - chietKhau - phaiThuSau - tienChuyenKhoan;
            decimal tienCanChuyen = Math.Max(0, tienKet - tienLe);

            var vm = new TaoChotCaViewModel
            {
                CaTruc = DateTime.Now.Hour < 12 ? "Sáng" : (DateTime.Now.Hour < 18 ? "Chiều" : "Tối"),
                CoSo = "AACH Savor - 128 Cầu Giấy",
                NhietDoTuMat = "4°C",
                TienLeDauCa = tienLe,
                TienHang = tongTienHang,
                ChenhLechShip = chenhLechShip,
                ChiPhi = chiPhi,
                ChietKhauDoiTac = chietKhau,
                PhaiThuSauDoiTac = phaiThuSau,
                QuetTheChuyenKhoan = tienChuyenKhoan,
                TongTienKetCuoiCa = tienKet,
                TongTienCanChuyen = tienCanChuyen,
                ChiPhiMoTa = ""
            };

            ViewBag.TenNhanVien = nv?.TenNV ?? User.Identity?.Name ?? "Thu ngân";
            ViewBag.SoDonTrongCa = soDon;
            ViewBag.TienMatBan = tienMat;

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(TaoChotCaViewModel model)
        {
            int nhanVienId = 1;
            var nvClaim = User.Claims.FirstOrDefault(c => c.Type == "NhanVienID");
            if (nvClaim != null && int.TryParse(nvClaim.Value, out var parsedId))
            {
                nhanVienId = parsedId;
            }

            var chotCa = new ChotCa
            {
                NhanVienID = nhanVienId,
                CaTruc = model.CaTruc,
                CoSo = model.CoSo,
                NhietDoTuMat = model.NhietDoTuMat,
                TienLeDauCa = model.TienLeDauCa,
                TienHang = model.TienHang,
                ChenhLechShip = model.ChenhLechShip,
                ChiPhi = model.ChiPhi,
                ChietKhauDoiTac = model.ChietKhauDoiTac,
                PhaiThuSauDoiTac = model.PhaiThuSauDoiTac,
                QuetTheChuyenKhoan = model.QuetTheChuyenKhoan,
                TongTienKetCuoiCa = model.TongTienKetCuoiCa,
                TongTienCanChuyen = model.TongTienCanChuyen,
                ChiPhiMoTa = model.ChiPhiMoTa,
                TrangThai = "Đã chốt ca"
            };

            _storeService.SaveChotCa(chotCa, nhanVienId);
            TempData["SuccessMessage"] = $"Chốt ca trực [{chotCa.CaTruc} - {DateTime.Now:dd/MM/yyyy}] thành công! Bạn nhận được +20 điểm tích lũy.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult GetDetailJson(int id)
        {
            var cc = _storeService.GetChotCaById(id);
            if (cc == null) return NotFound();

            return Json(new
            {
                chotCaId = cc.ChotCaID,
                maCa = $"CA{cc.ChotCaID:D4}",
                ngayChot = cc.NgayChot.ToString("dd/MM/yyyy HH:mm"),
                caTruc = cc.CaTruc,
                coSo = cc.CoSo,
                nhanVien = cc.NhanVien?.TenNV ?? "Nhân viên",
                nhietDoTuMat = cc.NhietDoTuMat,
                tienLeDauCa = cc.TienLeDauCa,
                tienHang = cc.TienHang,
                chenhLechShip = cc.ChenhLechShip,
                chiPhi = cc.ChiPhi,
                chiPhiMoTa = cc.ChiPhiMoTa ?? "",
                chietKhauDoiTac = cc.ChietKhauDoiTac,
                phaiThuSauDoiTac = cc.PhaiThuSauDoiTac,
                quetTheChuyenKhoan = cc.QuetTheChuyenKhoan,
                tongTienKetCuoiCa = cc.TongTienKetCuoiCa,
                tongTienCanChuyen = cc.TongTienCanChuyen,
                trangThai = cc.TrangThai
            });
        }
    }
}
