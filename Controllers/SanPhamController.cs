using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AachSavor.Models;
using AachSavor.Services;

namespace AachSavor.Controllers
{
    [Authorize]
    public class SanPhamController : Controller
    {
        private readonly IStoreService _storeService;

        public SanPhamController(IStoreService storeService)
        {
            _storeService = storeService;
        }

        public IActionResult Index(string? keyword, string? danhMuc, bool? sapHetHang)
        {
            var list = _storeService.GetSanPhams(keyword, danhMuc, sapHetHang);
            ViewBag.Keyword = keyword;
            ViewBag.DanhMuc = danhMuc;
            ViewBag.SapHetHang = sapHetHang;
            return View(list);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public IActionResult Save(SanPham sp)
        {
            if (string.IsNullOrWhiteSpace(sp.TenSP))
            {
                TempData["ErrorMessage"] = "Tên sản phẩm không được để trống!";
                return RedirectToAction(nameof(Index));
            }

            if (sp.GiaLe < 0 || sp.GiaSi < 0 || sp.GiaNhap < 0)
            {
                TempData["ErrorMessage"] = "Giá sản phẩm không được là số âm!";
                return RedirectToAction(nameof(Index));
            }

            _storeService.SaveSanPham(sp);
            TempData["SuccessMessage"] = sp.SanPhamID == 0 ? "Thêm sản phẩm mới thành công!" : "Cập nhật thông tin sản phẩm thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var success = _storeService.DeleteSanPham(id);
            if (success)
            {
                TempData["SuccessMessage"] = "Đã xóa sản phẩm thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy sản phẩm cần xóa!";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult GetListJson(string? keyword, string? danhMuc)
        {
            var list = _storeService.GetSanPhams(keyword, danhMuc);
            return Json(list);
        }

        [HttpGet]
        public IActionResult GetDetailJson(int id)
        {
            var sp = _storeService.GetSanPhamById(id);
            if (sp == null) return NotFound();
            return Json(sp);
        }
    }
}
