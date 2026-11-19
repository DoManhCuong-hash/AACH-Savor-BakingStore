using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AachSavor.Models
{
    public class LoginViewModel
    {
        public string TenDangNhap { get; set; } = "admin";
        public string MatKhau { get; set; } = "123456";
        public bool RememberMe { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại")]
        [Display(Name = "Mật khẩu hiện tại")]
        public string MatKhauHienTai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        [MinLength(4, ErrorMessage = "Mật khẩu mới phải từ 4 ký tự trở lên")]
        [Display(Name = "Mật khẩu mới")]
        public string MatKhauMoi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu mới")]
        [Compare("MatKhauMoi", ErrorMessage = "Mật khẩu xác nhận không khớp với mật khẩu mới")]
        [Display(Name = "Xác nhận mật khẩu mới")]
        public string XacNhanMatKhauMoi { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
    }

    public class ChangePasswordRequest
    {
        public string MatKhauHienTai { get; set; } = string.Empty;
        public string MatKhauMoi { get; set; } = string.Empty;
        public string XacNhanMatKhauMoi { get; set; } = string.Empty;
    }

    public class DashboardViewModel
    {
        public decimal DoanhThuHomNay { get; set; }
        public int SoDonHangHomNay { get; set; }
        public int TongSanPham { get; set; }
        public int SanPhamSapHet { get; set; }
        public int TongNhanVien { get; set; }
        public List<HoaDon> HoaDonGanDay { get; set; } = new();
        public List<PhieuNhap> PhieuNhapGanDay { get; set; } = new();
        public List<SanPham> TopSanPhamDashboard { get; set; } = new();
        public List<ThumbUp> ThumbUpGanDay { get; set; } = new();
        public NhanVien? CurrentNhanVien { get; set; }
        public List<NhanVien> DanhSachNhanVien { get; set; } = new();
    }

    public class PosCartItem
    {
        public int SanPhamID { get; set; }
        public string MaSP { get; set; } = string.Empty;
        public string TenSP { get; set; } = string.Empty;
        public string DonViTinh { get; set; } = "Cái";
        public int SoLuong { get; set; }
        public decimal GiaLe { get; set; }
        public decimal GiaSi { get; set; }
        public int TonKho { get; set; }
        public bool IsGiaSi => SoLuong >= 10;
        public decimal DonGia => IsGiaSi ? GiaSi : GiaLe;
        public decimal ThanhTien => SoLuong * DonGia;
    }

    public class PosCheckoutRequest
    {
        public int? KhachHangID { get; set; }
        public string? TenKhachHangMoi { get; set; }
        public string? SdtKhachHangMoi { get; set; }
        public string PhuongThucTT { get; set; } = "Tiền mặt"; // Tiền mặt / Chuyển khoản
        public decimal TienKhachDua { get; set; }
        public string? GhiChu { get; set; }
        public List<PosCartItem> Items { get; set; } = new();
    }

    public class BaoCaoViewModel
    {
        public DateTime TuNgay { get; set; }
        public DateTime DenNgay { get; set; }
        public decimal TongDoanhThu { get; set; }
        public decimal DoanhThuTienMat { get; set; }
        public decimal DoanhThuChuyenKhoan { get; set; }
        public int TongSoDonHang { get; set; }
        public List<TopSanPhamReportItem> TopSanPham { get; set; } = new();
        public List<DoanhThuTheoNgayItem> DoanhThuTheoNgay { get; set; } = new();
    }

    public class TopSanPhamReportItem
    {
        public int SanPhamID { get; set; }
        public string MaSP { get; set; } = string.Empty;
        public string TenSP { get; set; } = string.Empty;
        public int SoLuongDaBan { get; set; }
        public decimal DoanhThu { get; set; }
    }

    public class DoanhThuTheoNgayItem
    {
        public string Ngay { get; set; } = string.Empty;
        public decimal DoanhThu { get; set; }
        public decimal TienMat { get; set; }
        public decimal ChuyenKhoan { get; set; }
    }

    public class TaoPhieuNhapRequest
    {
        public string? NhaCungCap { get; set; }
        public string? GhiChu { get; set; }
        public List<ChiTietNhapItem> Items { get; set; } = new();
    }

    public class ChiTietNhapItem
    {
        public int SanPhamID { get; set; }
        public int SoLuongNhap { get; set; }
        public decimal DonGiaNhap { get; set; }
    }

    public class SendThumbUpRequest
    {
        public int NhanVienID { get; set; }
        public string NoiDung { get; set; } = string.Empty;
        public decimal TienThuong { get; set; } = 20000;
        public int DiemThuong { get; set; } = 50;
    }

    public class TaoChotCaViewModel
    {
        public string CaTruc { get; set; } = "Chiều";
        public string CoSo { get; set; } = "AACH Savor - 128 Cầu Giấy";
        public string NhietDoTuMat { get; set; } = "4°C";
        public decimal TienLeDauCa { get; set; } = 500000;
        public decimal TienHang { get; set; }
        public decimal ChenhLechShip { get; set; } = 0;
        public decimal ChiPhi { get; set; } = 0;
        public decimal ChietKhauDoiTac { get; set; } = 0;
        public decimal PhaiThuSauDoiTac { get; set; } = 0;
        public decimal QuetTheChuyenKhoan { get; set; } = 0;
        public decimal TongTienKetCuoiCa { get; set; }
        public decimal TongTienCanChuyen { get; set; }
        public string? ChiPhiMoTa { get; set; }
    }
}
