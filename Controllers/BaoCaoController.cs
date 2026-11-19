using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AachSavor.Services;

namespace AachSavor.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BaoCaoController : Controller
    {
        private readonly IStoreService _storeService;

        public BaoCaoController(IStoreService storeService)
        {
            _storeService = storeService;
        }

        public IActionResult Index(DateTime? tuNgay, DateTime? denNgay)
        {
            var end = denNgay ?? DateTime.Today;
            var start = tuNgay ?? DateTime.Today.AddDays(-14);

            var report = _storeService.GetBaoCao(start, end);
            return View(report);
        }

        [HttpGet]
        public IActionResult GetChartData(DateTime? tuNgay, DateTime? denNgay)
        {
            var end = denNgay ?? DateTime.Today;
            var start = tuNgay ?? DateTime.Today.AddDays(-14);

            var report = _storeService.GetBaoCao(start, end);
            return Json(new
            {
                labels = report.DoanhThuTheoNgay.Select(d => d.Ngay).ToList(),
                revenue = report.DoanhThuTheoNgay.Select(d => d.DoanhThu).ToList(),
                cash = report.DoanhThuTheoNgay.Select(d => d.TienMat).ToList(),
                transfer = report.DoanhThuTheoNgay.Select(d => d.ChuyenKhoan).ToList()
            });
        }
    }
}
