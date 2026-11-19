using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AachSavor.Models;
using AachSavor.Services;

namespace AachSavor.Controllers
{
    [Authorize(Roles = "Admin")]
    public class NhanVienController : Controller
    {
        private readonly IStoreService _storeService;

        public NhanVienController(IStoreService storeService)
        {
            _storeService = storeService;
        }

        public IActionResult Index()
        {
            var list = _storeService.GetNhanViens();
            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Save(NhanVien nv, string? username, string? password, string? vaiTro)
        {
            if (string.IsNullOrWhiteSpace(nv.TenNV))
            {
                TempData["ErrorMessage"] = "Tên nhân viên không được để trống!";
                return RedirectToAction(nameof(Index));
            }

            _storeService.SaveNhanVien(nv, username, password, vaiTro);
            TempData["SuccessMessage"] = nv.NhanVienID == 0 ? "Thêm nhân viên và tạo tài khoản thành công!" : "Cập nhật thông tin nhân viên thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            if (id == 1)
            {
                TempData["ErrorMessage"] = "Không thể xóa tài khoản Quản trị viên (Admin) mặc định!";
                return RedirectToAction(nameof(Index));
            }

            var success = _storeService.DeleteNhanVien(id);
            if (success)
            {
                TempData["SuccessMessage"] = "Đã xóa nhân viên thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy nhân viên cần xóa!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
