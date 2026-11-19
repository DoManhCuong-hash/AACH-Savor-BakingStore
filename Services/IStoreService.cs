using System;
using System.Collections.Generic;
using AachSavor.Models;

namespace AachSavor.Services
{
    public interface IStoreService
    {
        // Auth
        (TaiKhoan? TaiKhoan, NhanVien? NhanVien) Authenticate(string username, string password);
        List<TaiKhoan> GetTaiKhoans();
        (bool Success, string Message) DoiMatKhau(int taiKhoanId, string matKhauCu, string matKhauMoi);

        // Nhan Vien
        List<NhanVien> GetNhanViens();
        NhanVien? GetNhanVienById(int id);
        NhanVien SaveNhanVien(NhanVien nv, string? username = null, string? password = null, string? vaiTro = null);
        bool DeleteNhanVien(int id);

        // San Pham
        List<SanPham> GetSanPhams(string? keyword = null, string? danhMuc = null, bool? sapHetHang = null);
        SanPham? GetSanPhamById(int id);
        SanPham? GetSanPhamByMa(string maSP);
        SanPham SaveSanPham(SanPham sp);
        bool DeleteSanPham(int id);

        // Khach Hang
        List<KhachHang> GetKhachHangs(string? keyword = null);
        KhachHang? GetKhachHangById(int id);
        KhachHang SaveKhachHang(KhachHang kh);
        List<HoaDon> GetLichSuMuaHang(int khachHangId);

        // Ban Hang / Hoa Don
        (bool Success, string Message, HoaDon? HoaDon) CreateHoaDon(PosCheckoutRequest req, int nhanVienId);
        List<HoaDon> GetHoaDons(DateTime? tuNgay = null, DateTime? denNgay = null);
        HoaDon? GetHoaDonById(int id);

        // Nhap Kho
        (bool Success, string Message, PhieuNhap? PhieuNhap) CreatePhieuNhap(TaoPhieuNhapRequest req, int nhanVienId);
        List<PhieuNhap> GetPhieuNhaps();
        PhieuNhap? GetPhieuNhapById(int id);

        // Chot Ca Truc (Sale Report)
        List<ChotCa> GetChotCas(int? nhanVienId = null);
        ChotCa? GetChotCaById(int id);
        ChotCa SaveChotCa(ChotCa chotCa, int nhanVienId);
        (decimal TongTienHang, decimal TienChuyenKhoan, decimal TienMat, int SoDon) GetThongKeCaHienTai(int? nhanVienId = null);

        // Thumb Up (Khen thuong & Diem cong)
        List<ThumbUp> GetThumbUps(int? nhanVienId = null);
        ThumbUp SendThumbUp(int nhanVienId, int adminId, string adminTen, string noiDung, decimal tienThuong, int diemThuong);

        // Dashboard & Bao Cao
        DashboardViewModel GetDashboardStats(int? currentNhanVienId = null);
        BaoCaoViewModel GetBaoCao(DateTime tuNgay, DateTime denNgay);
    }
}
