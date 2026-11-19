using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AachSavor.Models;
using AachSavor.Services;

namespace AachSavor.Controllers
{
    public class AccountController : Controller
    {
        private readonly IStoreService _storeService;

        public AccountController(IStoreService storeService)
        {
            _storeService = storeService;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(model.TenDangNhap) || string.IsNullOrWhiteSpace(model.MatKhau))
            {
                model.ErrorMessage = "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu!";
                return View(model);
            }

            var (tk, nv) = _storeService.Authenticate(model.TenDangNhap, model.MatKhau);
            if (tk == null || nv == null)
            {
                model.ErrorMessage = "Tên đăng nhập hoặc mật khẩu không chính xác!";
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, tk.TaiKhoanID.ToString()),
                new Claim(ClaimTypes.Name, nv.TenNV),
                new Claim(ClaimTypes.Role, tk.VaiTro),
                new Claim("NhanVienID", nv.NhanVienID.ToString()),
                new Claim("Username", tk.TenDangNhap)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // Theo tài liệu: Admin vào Trang chủ / Báo cáo, Nhân viên vào Bán hàng hoặc Trang chủ
            if (tk.VaiTro == "Staff")
            {
                return RedirectToAction("Index", "BanHang");
            }

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.MatKhauMoi != model.XacNhanMatKhauMoi)
            {
                ModelState.AddModelError("XacNhanMatKhauMoi", "Mật khẩu xác nhận không khớp!");
                model.ErrorMessage = "Mật khẩu xác nhận không trùng khớp với mật khẩu mới!";
                return View(model);
            }

            var tkId = GetCurrentTaiKhoanId();
            if (tkId == null)
            {
                TempData["ErrorMessage"] = "Không thể xác thực thông tin tài khoản hiện tại!";
                return RedirectToAction("Login");
            }

            var result = _storeService.DoiMatKhau(tkId.Value, model.MatKhauHienTai, model.MatKhauMoi);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                model.ErrorMessage = result.Message;
                return View(model);
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction("ChangePassword");
        }

        [Authorize]
        [HttpPost]
        public IActionResult ChangePasswordApi([FromBody] ChangePasswordRequest req)
        {
            if (req == null)
            {
                return BadRequest(new { success = false, message = "Dữ liệu yêu cầu không hợp lệ!" });
            }

            if (string.IsNullOrWhiteSpace(req.MatKhauHienTai))
            {
                return BadRequest(new { success = false, message = "Vui lòng nhập mật khẩu hiện tại!" });
            }

            if (string.IsNullOrWhiteSpace(req.MatKhauMoi) || req.MatKhauMoi.Trim().Length < 4)
            {
                return BadRequest(new { success = false, message = "Mật khẩu mới phải có ít nhất 4 ký tự!" });
            }

            if (req.MatKhauMoi != req.XacNhanMatKhauMoi)
            {
                return BadRequest(new { success = false, message = "Mật khẩu xác nhận không trùng khớp!" });
            }

            var tkId = GetCurrentTaiKhoanId();
            if (tkId == null)
            {
                return BadRequest(new { success = false, message = "Không xác định được danh tính người dùng!" });
            }

            var result = _storeService.DoiMatKhau(tkId.Value, req.MatKhauHienTai, req.MatKhauMoi);
            if (!result.Success)
            {
                return BadRequest(new { success = false, message = result.Message });
            }

            return Ok(new { success = true, message = result.Message });
        }

        private int? GetCurrentTaiKhoanId()
        {
            var idClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(idClaim, out int tkId)) return tkId;

            var nvClaim = User.Claims.FirstOrDefault(c => c.Type == "NhanVienID")?.Value;
            if (int.TryParse(nvClaim, out int nvId)) return nvId;

            return null;
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
