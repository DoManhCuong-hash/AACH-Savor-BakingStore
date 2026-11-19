using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AachSavor.Models
{
    [Table("tblNhanVien")]
    public class NhanVien
    {
        [Key]
        [Column("PK_iNhanVienID")]
        public int NhanVienID { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("sTenNV")]
        public string TenNV { get; set; } = string.Empty;

        [MaxLength(15)]
        [Column("sSDT")]
        public string? SDT { get; set; }

        [MaxLength(255)]
        [Column("sDiaChi")]
        public string? DiaChi { get; set; }

        // Điểm xếp hạng & Gamification (4handy style)
        public int DiemTichLuy { get; set; } = 2908;
        public string Rank { get; set; } = "A5";

        // Navigation
        public TaiKhoan? TaiKhoan { get; set; }
    }

    [Table("tblTaiKhoan")]
    public class TaiKhoan
    {
        [Key]
        [Column("PK_iTaiKhoanID")]
        public int TaiKhoanID { get; set; }

        [Column("FK_iNhanVienID")]
        public int NhanVienID { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("sTenDangNhap")]
        public string TenDangNhap { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [Column("sMatKhauHash")]
        public string MatKhauHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        [Column("sVaiTro")]
        public string VaiTro { get; set; } = "Staff"; // "Admin" or "Staff"

        [Column("bTrangThai")]
        public bool TrangThai { get; set; } = true;

        // Navigation
        [ForeignKey("NhanVienID")]
        public NhanVien? NhanVien { get; set; }
    }

    [Table("tblKhachHang")]
    public class KhachHang
    {
        [Key]
        [Column("PK_iKhachHangID")]
        public int KhachHangID { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("sTenKH")]
        public string TenKH { get; set; } = string.Empty;

        [MaxLength(15)]
        [Column("sSDT")]
        public string? SDT { get; set; }

        public string? DiaChi { get; set; }
    }

    [Table("tblSanPham")]
    public class SanPham
    {
        [Key]
        [Column("PK_iSanPhamID")]
        public int SanPhamID { get; set; }

        [Required]
        [MaxLength(30)]
        [Column("sMaSP")]
        public string MaSP { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("sTenSP")]
        public string TenSP { get; set; } = string.Empty;

        [Column("fGiaNhap")]
        public decimal GiaNhap { get; set; }

        [Column("fGiaLe")]
        public decimal GiaLe { get; set; }

        [Column("fGiaSi")]
        public decimal GiaSi { get; set; }

        [Column("iTonKho")]
        public int TonKho { get; set; }

        // Metadata bổ sung cho UI
        public string? DonViTinh { get; set; } = "Cái";
        public string? DanhMuc { get; set; } = "Nguyên liệu làm bánh";
        public string? HinhAnh { get; set; }
    }

    [Table("tblHoaDon")]
    public class HoaDon
    {
        [Key]
        [Column("PK_iHoaDonID")]
        public int HoaDonID { get; set; }

        [Column("FK_iNhanVienID")]
        public int NhanVienID { get; set; }

        [Column("FK_iKhachHangID")]
        public int? KhachHangID { get; set; }

        [Column("dNgayLap")]
        public DateTime NgayLap { get; set; } = DateTime.Now;

        [Column("fTongTien")]
        public decimal TongTien { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("sPhuongThucTT")]
        public string PhuongThucTT { get; set; } = "Tiền mặt"; // "Tiền mặt", "Chuyển khoản"

        public string? GhiChu { get; set; }

        // Navigation
        [ForeignKey("NhanVienID")]
        public NhanVien? NhanVien { get; set; }

        [ForeignKey("KhachHangID")]
        public KhachHang? KhachHang { get; set; }

        public List<ChiTietHoaDon> ChiTietHoaDons { get; set; } = new();
    }

    [Table("tblChiTietHoaDon")]
    public class ChiTietHoaDon
    {
        [Column("FK_iHoaDonID")]
        public int HoaDonID { get; set; }

        [Column("FK_iSanPhamID")]
        public int SanPhamID { get; set; }

        [Column("iSoLuong")]
        public int SoLuong { get; set; }

        [Column("fDonGia")]
        public decimal DonGia { get; set; }

        [Column("fThanhTien")]
        public decimal ThanhTien { get; set; }

        // Helper property
        public bool IsGiaSi { get; set; }

        // Navigation
        [ForeignKey("HoaDonID")]
        public HoaDon? HoaDon { get; set; }

        [ForeignKey("SanPhamID")]
        public SanPham? SanPham { get; set; }
    }

    [Table("tblPhieuNhap")]
    public class PhieuNhap
    {
        [Key]
        [Column("PK_iPhieuNhapID")]
        public int PhieuNhapID { get; set; }

        [Column("FK_iNhanVienID")]
        public int NhanVienID { get; set; }

        [Column("dNgayNhap")]
        public DateTime NgayNhap { get; set; } = DateTime.Now;

        [MaxLength(255)]
        [Column("sGhiChu")]
        public string? GhiChu { get; set; }

        public decimal TongTien { get; set; }

        public string? NhaCungCap { get; set; }

        // Navigation
        [ForeignKey("NhanVienID")]
        public NhanVien? NhanVien { get; set; }

        public List<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; } = new();
    }

    [Table("tblChiTietPhieuNhap")]
    public class ChiTietPhieuNhap
    {
        [Column("FK_iPhieuNhapID")]
        public int PhieuNhapID { get; set; }

        [Column("FK_iSanPhamID")]
        public int SanPhamID { get; set; }

        [Column("iSoLuongNhap")]
        public int SoLuongNhap { get; set; }

        [Column("fDonGiaNhap")]
        public decimal DonGiaNhap { get; set; }

        public decimal ThanhTien => SoLuongNhap * DonGiaNhap;

        // Navigation
        [ForeignKey("PhieuNhapID")]
        public PhieuNhap? PhieuNhap { get; set; }

        [ForeignKey("SanPhamID")]
        public SanPham? SanPham { get; set; }
    }

    // BẢNG CHỐT CA TRỰC (SALE REPORT)
    public class ChotCa
    {
        public int ChotCaID { get; set; }
        public int NhanVienID { get; set; }
        public DateTime NgayChot { get; set; } = DateTime.Now;
        public string CaTruc { get; set; } = "Chiều"; // Sáng, Chiều, Tối
        public string CoSo { get; set; } = "AACH Savor - 128 Cầu Giấy";
        public string NhietDoTuMat { get; set; } = "4°C"; // Nhiệt độ tủ mát bảo quản bơ kem

        // Tổng kết tài chính
        public decimal TienLeDauCa { get; set; } = 500000;
        public decimal TienHang { get; set; }
        public decimal ChenhLechShip { get; set; }
        public decimal ChiPhi { get; set; }
        public decimal ChietKhauDoiTac { get; set; }
        public decimal PhaiThuSauDoiTac { get; set; }
        public decimal QuetTheChuyenKhoan { get; set; }
        public decimal TongTienKetCuoiCa { get; set; }
        public decimal TongTienCanChuyen { get; set; }

        public string? ChiPhiMoTa { get; set; }
        public string TrangThai { get; set; } = "Đã chốt ca";

        public NhanVien? NhanVien { get; set; }
    }

    // BẢNG THƯỞNG KHÍCH LỆ (THUMB UP) CHO NHÂN VIÊN
    public class ThumbUp
    {
        public int ThumbUpID { get; set; }
        public int NhanVienID { get; set; }
        public int NguoiGuiID { get; set; }
        public string NguoiGuiTen { get; set; } = "Admin";
        public string NoiDung { get; set; } = string.Empty;
        public decimal TienThuong { get; set; }
        public int DiemThuong { get; set; } = 20;
        public DateTime NgayGui { get; set; } = DateTime.Now;

        public NhanVien? NhanVien { get; set; }
    }
}
