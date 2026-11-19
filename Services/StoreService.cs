using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using AachSavor.Models;

namespace AachSavor.Services
{
    public class StoreDataContainer
    {
        public List<NhanVien> NhanViens { get; set; } = new();
        public List<TaiKhoan> TaiKhoans { get; set; } = new();
        public List<KhachHang> KhachHangs { get; set; } = new();
        public List<SanPham> SanPhams { get; set; } = new();
        public List<HoaDon> HoaDons { get; set; } = new();
        public List<PhieuNhap> PhieuNhaps { get; set; } = new();
        public List<ChotCa> ChotCas { get; set; } = new();
        public List<ThumbUp> ThumbUps { get; set; } = new();
    }

    public class StoreService : IStoreService
    {
        private readonly string _filePath;
        private readonly object _lock = new();
        private StoreDataContainer _data = new();

        public StoreService(IWebHostEnvironment env)
        {
            var dataDir = Path.Combine(env.ContentRootPath, "App_Data");
            if (!Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir);
            }
            _filePath = Path.Combine(dataDir, "aachsavor_store.json");

            LoadOrSeedData();
        }

        private void LoadOrSeedData()
        {
            lock (_lock)
            {
                if (File.Exists(_filePath))
                {
                    try
                    {
                        var json = File.ReadAllText(_filePath);
                        var loaded = JsonSerializer.Deserialize<StoreDataContainer>(json);
                        if (loaded != null && loaded.SanPhams.Count > 0)
                        {
                            _data = loaded;
                            AttachRelationships();
                            return;
                        }
                    }
                    catch
                    {
                        // Fall back to seed
                    }
                }

                SeedInitialData();
                SaveData();
            }
        }

        private void AttachRelationships()
        {
            var nvDict = _data.NhanViens.ToDictionary(n => n.NhanVienID);
            var khDict = _data.KhachHangs.ToDictionary(k => k.KhachHangID);
            var spDict = _data.SanPhams.ToDictionary(s => s.SanPhamID);

            foreach (var nv in _data.NhanViens)
            {
                nv.Rank = RankHelper.GetRankName(nv.DiemTichLuy);
            }

            foreach (var tk in _data.TaiKhoans)
            {
                if (nvDict.TryGetValue(tk.NhanVienID, out var nv))
                {
                    tk.NhanVien = nv;
                    nv.TaiKhoan = tk;
                }
            }

            foreach (var hd in _data.HoaDons)
            {
                if (nvDict.TryGetValue(hd.NhanVienID, out var nv)) hd.NhanVien = nv;
                if (hd.KhachHangID.HasValue && khDict.TryGetValue(hd.KhachHangID.Value, out var kh)) hd.KhachHang = kh;

                foreach (var ct in hd.ChiTietHoaDons)
                {
                    if (spDict.TryGetValue(ct.SanPhamID, out var sp)) ct.SanPham = sp;
                }
            }

            foreach (var pn in _data.PhieuNhaps)
            {
                if (nvDict.TryGetValue(pn.NhanVienID, out var nv)) pn.NhanVien = nv;

                foreach (var ct in pn.ChiTietPhieuNhaps)
                {
                    if (spDict.TryGetValue(ct.SanPhamID, out var sp)) ct.SanPham = sp;
                }
            }

            foreach (var cc in _data.ChotCas)
            {
                if (nvDict.TryGetValue(cc.NhanVienID, out var nv)) cc.NhanVien = nv;
            }

            foreach (var tu in _data.ThumbUps)
            {
                if (nvDict.TryGetValue(tu.NhanVienID, out var nv)) tu.NhanVien = nv;
            }
        }

        private void SaveData()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(_data, options);
                File.WriteAllText(_filePath, json);
            }
            catch
            {
                // ignore
            }
        }

        private void SeedInitialData()
        {
            _data = new StoreDataContainer();

            // 1. Nhan vien
            var nvAdmin = new NhanVien
            {
                NhanVienID = 1,
                TenNV = "Nguyễn Quản Lý (Admin)",
                SDT = "0912345678",
                DiaChi = "128 Cầu Giấy, Hà Nội",
                DiemTichLuy = 4200,
                Rank = "A6"
            };
            var nvStaff = new NhanVien
            {
                NhanVienID = 2,
                TenNV = "Trần Bán Hàng (Staff)",
                SDT = "0987654321",
                DiaChi = "45 Nguyễn Trãi, Thanh Xuân, Hà Nội",
                DiemTichLuy = 2908,
                Rank = "A5"
            };
            _data.NhanViens.Add(nvAdmin);
            _data.NhanViens.Add(nvStaff);

            // 2. Tai khoan
            _data.TaiKhoans.Add(new TaiKhoan
            {
                TaiKhoanID = 1,
                NhanVienID = 1,
                TenDangNhap = "admin",
                MatKhauHash = "123456",
                VaiTro = "Admin",
                TrangThai = true,
                NhanVien = nvAdmin
            });
            _data.TaiKhoans.Add(new TaiKhoan
            {
                TaiKhoanID = 2,
                NhanVienID = 2,
                TenDangNhap = "staff",
                MatKhauHash = "123456",
                VaiTro = "Staff",
                TrangThai = true,
                NhanVien = nvStaff
            });

            // 3. Khach hang
            _data.KhachHangs.Add(new KhachHang
            {
                KhachHangID = 1,
                TenKH = "Nguyễn Văn An (Tiệm bánh Bông Lan)",
                SDT = "0901122334",
                DiaChi = "15 Hoàng Hoa Thám, Ba Đình"
            });
            _data.KhachHangs.Add(new KhachHang
            {
                KhachHangID = 2,
                TenKH = "Trần Thị Bình (Sweet Home Bakery)",
                SDT = "0988998899",
                DiaChi = "88 Láng Hạ, Đống Đa"
            });
            _data.KhachHangs.Add(new KhachHang
            {
                KhachHangID = 3,
                TenKH = "Lê Hoàng Phúc (Khách quen)",
                SDT = "0934567890",
                DiaChi = "202 Trần Hưng Đạo, Hoàn Kiếm"
            });

            // 4. San Pham
            _data.SanPhams = new List<SanPham>
            {
                new SanPham { SanPhamID = 1, MaSP = "SP001", TenSP = "Bột mì Baker's Choice số 11 (1kg)", GiaNhap = 18000, GiaLe = 25000, GiaSi = 22000, TonKho = 65, DonViTinh = "Gói", DanhMuc = "Bột & Men nở", HinhAnh = "🍰" },
                new SanPham { SanPhamID = 2, MaSP = "SP002", TenSP = "Socola Chip đen Colatta (500g)", GiaNhap = 38000, GiaLe = 55000, GiaSi = 48000, TonKho = 32, DonViTinh = "Gói", DanhMuc = "Socola & Cacao", HinhAnh = "🍫" },
                new SanPham { SanPhamID = 3, MaSP = "SP003", TenSP = "Kem Whipping Cream Anchor (1 Lít)", GiaNhap = 120000, GiaLe = 155000, GiaSi = 142000, TonKho = 28, DonViTinh = "Hộp", DanhMuc = "Bơ sữa & Kem", HinhAnh = "🥛" },
                new SanPham { SanPhamID = 4, MaSP = "SP004", TenSP = "Kem béo thực vật Rich's Lùn (454g)", GiaNhap = 24000, GiaLe = 32000, GiaSi = 27000, TonKho = 85, DonViTinh = "Hộp", DanhMuc = "Bơ sữa & Kem", HinhAnh = "🍨" },
                new SanPham { SanPhamID = 5, MaSP = "SP005", TenSP = "Bơ lạt tự nhiên Anchor New Zealand (250g)", GiaNhap = 62000, GiaLe = 85000, GiaSi = 78000, TonKho = 35, DonViTinh = "Khối", DanhMuc = "Bơ sữa & Kem", HinhAnh = "🧈" },
                new SanPham { SanPhamID = 6, MaSP = "SP006", TenSP = "Men nở khô vàng Mauripan lạt (500g)", GiaNhap = 55000, GiaLe = 75000, GiaSi = 68000, TonKho = 18, DonViTinh = "Hộp", DanhMuc = "Bột & Men nở", HinhAnh = "🌾" },
                new SanPham { SanPhamID = 7, MaSP = "SP007", TenSP = "Đường bột làm bánh Biên Hòa (1kg)", GiaNhap = 22000, GiaLe = 32000, GiaSi = 28000, TonKho = 52, DonViTinh = "Gói", DanhMuc = "Đường & Phụ gia", HinhAnh = "🍬" },
                new SanPham { SanPhamID = 8, MaSP = "SP008", TenSP = "Bột hạnh nhân cao cấp Blue Diamond (500g)", GiaNhap = 95000, GiaLe = 130000, GiaSi = 118000, TonKho = 12, DonViTinh = "Túi", DanhMuc = "Bột & Men nở", HinhAnh = "🌰" },
                new SanPham { SanPhamID = 9, MaSP = "SP009", TenSP = "Khuôn bánh tròn chống dính đế rời 20cm", GiaNhap = 45000, GiaLe = 70000, GiaSi = 60000, TonKho = 24, DonViTinh = "Cái", DanhMuc = "Khuôn & Dụng cụ", HinhAnh = "🥧" },
                new SanPham { SanPhamID = 10, MaSP = "SP010", TenSP = "Tinh mùi Vanilla Rayner's chính hãng (28ml)", GiaNhap = 35000, GiaLe = 50000, GiaSi = 45000, TonKho = 30, DonViTinh = "Chai", DanhMuc = "Đường & Phụ gia", HinhAnh = "✨" },
                new SanPham { SanPhamID = 11, MaSP = "SP011", TenSP = "Phới lồng inox đánh trứng 25cm", GiaNhap = 25000, GiaLe = 40000, GiaSi = 35000, TonKho = 16, DonViTinh = "Cái", DanhMuc = "Khuôn & Dụng cụ", HinhAnh = "🥄" },
                new SanPham { SanPhamID = 12, MaSP = "SP012", TenSP = "Bộ túi bắt kem silicon + 6 đui tạo hình", GiaNhap = 30000, GiaLe = 48000, GiaSi = 42000, TonKho = 22, DonViTinh = "Bộ", DanhMuc = "Khuôn & Dụng cụ", HinhAnh = "🧁" }
            };

            // 5. Hoa Don mau
            var today = DateTime.Today;
            _data.HoaDons = new List<HoaDon>
            {
                new HoaDon
                {
                    HoaDonID = 1,
                    NhanVienID = 2,
                    KhachHangID = 1,
                    NgayLap = today.AddHours(9).AddMinutes(15),
                    TongTien = 374000,
                    PhuongThucTT = "Chuyển khoản",
                    GhiChu = "Giao hàng cho Tiệm bánh Bông Lan",
                    ChiTietHoaDons = new List<ChiTietHoaDon>
                    {
                        new ChiTietHoaDon { HoaDonID = 1, SanPhamID = 1, SoLuong = 2, DonGia = 25000, ThanhTien = 50000, IsGiaSi = false },
                        new ChiTietHoaDon { HoaDonID = 1, SanPhamID = 4, SoLuong = 12, DonGia = 27000, ThanhTien = 324000, IsGiaSi = true }
                    }
                },
                new HoaDon
                {
                    HoaDonID = 2,
                    NhanVienID = 2,
                    KhachHangID = 2,
                    NgayLap = today.AddHours(10).AddMinutes(40),
                    TongTien = 860000,
                    PhuongThucTT = "Tiền mặt",
                    GhiChu = "Khách mua trực tiếp tại quầy",
                    ChiTietHoaDons = new List<ChiTietHoaDon>
                    {
                        new ChiTietHoaDon { HoaDonID = 2, SanPhamID = 3, SoLuong = 4, DonGia = 155000, ThanhTien = 620000, IsGiaSi = false },
                        new ChiTietHoaDon { HoaDonID = 2, SanPhamID = 5, SoLuong = 2, DonGia = 85000, ThanhTien = 170000, IsGiaSi = false },
                        new ChiTietHoaDon { HoaDonID = 2, SanPhamID = 9, SoLuong = 1, DonGia = 70000, ThanhTien = 70000, IsGiaSi = false }
                    }
                },
                new HoaDon
                {
                    HoaDonID = 3,
                    NhanVienID = 1,
                    KhachHangID = 3,
                    NgayLap = today.AddDays(-1).AddHours(14).AddMinutes(20),
                    TongTien = 660000,
                    PhuongThucTT = "Chuyển khoản",
                    ChiTietHoaDons = new List<ChiTietHoaDon>
                    {
                        new ChiTietHoaDon { HoaDonID = 3, SanPhamID = 1, SoLuong = 30, DonGia = 22000, ThanhTien = 660000, IsGiaSi = true }
                    }
                }
            };

            // 6. Phieu Nhap mau
            _data.PhieuNhaps = new List<PhieuNhap>
            {
                new PhieuNhap
                {
                    PhieuNhapID = 1,
                    NhanVienID = 1,
                    NgayNhap = today.AddDays(-3).AddHours(8).AddMinutes(30),
                    NhaCungCap = "Công ty TNHH Nhất Hương",
                    GhiChu = "Nhập bột mì Baker's Choice và kem Rich's",
                    TongTien = 2500000,
                    ChiTietPhieuNhaps = new List<ChiTietPhieuNhap>
                    {
                        new ChiTietPhieuNhap { PhieuNhapID = 1, SanPhamID = 1, SoLuongNhap = 50, DonGiaNhap = 18000 },
                        new ChiTietPhieuNhap { PhieuNhapID = 1, SanPhamID = 4, SoLuongNhap = 60, DonGiaNhap = 24000 }
                    }
                }
            };

            // 7. Chot Ca mau
            _data.ChotCas = new List<ChotCa>
            {
                new ChotCa
                {
                    ChotCaID = 1,
                    NhanVienID = 2,
                    NgayChot = today.AddDays(-1).AddHours(22).AddMinutes(30),
                    CaTruc = "Tối",
                    CoSo = "AACH Savor - 128 Cầu Giấy",
                    NhietDoTuMat = "3.8°C",
                    TienLeDauCa = 500000,
                    TienHang = 1850000,
                    ChenhLechShip = 0,
                    ChiPhi = 35000,
                    ChiPhiMoTa = "Mua đá lạnh bảo quản bơ lạt",
                    ChietKhauDoiTac = 0,
                    PhaiThuSauDoiTac = 0,
                    QuetTheChuyenKhoan = 950000,
                    TongTienKetCuoiCa = 1365000,
                    TongTienCanChuyen = 865000,
                    TrangThai = "Đã chốt ca"
                }
            };

            // 8. Thumb Up mau
            _data.ThumbUps = new List<ThumbUp>
            {
                new ThumbUp
                {
                    ThumbUpID = 1,
                    NhanVienID = 2,
                    NguoiGuiID = 1,
                    NguoiGuiTen = "Nguyễn Quản Lý",
                    NoiDung = "Cam Thumbup feedback 1176045",
                    TienThuong = 20000,
                    DiemThuong = 30,
                    NgayGui = today.AddDays(-1).AddHours(17).AddMinutes(10)
                },
                new ThumbUp
                {
                    ThumbUpID = 2,
                    NhanVienID = 2,
                    NguoiGuiID = 1,
                    NguoiGuiTen = "Nguyễn Quản Lý",
                    NoiDung = "Hỗ trợ cơ sở thiếu người ca tối",
                    TienThuong = 20000,
                    DiemThuong = 20,
                    NgayGui = today.AddDays(-2).AddHours(20).AddMinutes(15)
                },
                new ThumbUp
                {
                    ThumbUpID = 3,
                    NhanVienID = 2,
                    NguoiGuiID = 1,
                    NguoiGuiTen = "Nguyễn Quản Lý",
                    NoiDung = "Dọn tủ đông & tủ mát bảo quản kem cơ sở sạch sẽ",
                    TienThuong = 50000,
                    DiemThuong = 50,
                    NgayGui = today.AddDays(-3).AddHours(14).AddMinutes(0)
                }
            };

            AttachRelationships();
        }

        public (TaiKhoan? TaiKhoan, NhanVien? NhanVien) Authenticate(string username, string password)
        {
            lock (_lock)
            {
                var tk = _data.TaiKhoans.FirstOrDefault(t =>
                    t.TenDangNhap.Equals(username, StringComparison.OrdinalIgnoreCase) &&
                    t.MatKhauHash == password &&
                    t.TrangThai);

                if (tk == null) return (null, null);

                var nv = _data.NhanViens.FirstOrDefault(n => n.NhanVienID == tk.NhanVienID);
                return (tk, nv);
            }
        }

        public List<TaiKhoan> GetTaiKhoans()
        {
            lock (_lock)
            {
                return _data.TaiKhoans.ToList();
            }
        }

        public (bool Success, string Message) DoiMatKhau(int taiKhoanId, string matKhauCu, string matKhauMoi)
        {
            lock (_lock)
            {
                var tk = _data.TaiKhoans.FirstOrDefault(t => t.TaiKhoanID == taiKhoanId || t.NhanVienID == taiKhoanId);
                if (tk == null)
                {
                    return (false, "Không tìm thấy thông tin tài khoản trong hệ thống!");
                }

                if (tk.MatKhauHash != matKhauCu)
                {
                    return (false, "Mật khẩu hiện tại không chính xác!");
                }

                if (string.IsNullOrWhiteSpace(matKhauMoi) || matKhauMoi.Trim().Length < 4)
                {
                    return (false, "Mật khẩu mới phải có ít nhất 4 ký tự!");
                }

                if (matKhauMoi == matKhauCu)
                {
                    return (false, "Mật khẩu mới không được trùng với mật khẩu hiện tại!");
                }

                tk.MatKhauHash = matKhauMoi;
                SaveData();
                return (true, "Đổi mật khẩu thành công! Vui lòng ghi nhớ mật khẩu mới.");
            }
        }

        public List<NhanVien> GetNhanViens()
        {
            lock (_lock)
            {
                return _data.NhanViens.ToList();
            }
        }

        public NhanVien? GetNhanVienById(int id)
        {
            lock (_lock)
            {
                return _data.NhanViens.FirstOrDefault(n => n.NhanVienID == id);
            }
        }

        public NhanVien SaveNhanVien(NhanVien nv, string? username = null, string? password = null, string? vaiTro = null)
        {
            lock (_lock)
            {
                if (nv.NhanVienID == 0)
                {
                    nv.NhanVienID = _data.NhanViens.Count > 0 ? _data.NhanViens.Max(n => n.NhanVienID) + 1 : 1;
                    nv.DiemTichLuy = 0; // Nhân viên mới thêm có 0 điểm
                    nv.Rank = "A1";     // và có mức danh là A1
                    _data.NhanViens.Add(nv);

                    if (!string.IsNullOrWhiteSpace(username))
                    {
                        var tk = new TaiKhoan
                        {
                            TaiKhoanID = _data.TaiKhoans.Count > 0 ? _data.TaiKhoans.Max(t => t.TaiKhoanID) + 1 : 1,
                            NhanVienID = nv.NhanVienID,
                            TenDangNhap = username,
                            MatKhauHash = string.IsNullOrWhiteSpace(password) ? "123456" : password,
                            VaiTro = string.IsNullOrWhiteSpace(vaiTro) ? "Staff" : vaiTro,
                            TrangThai = true,
                            NhanVien = nv
                        };
                        _data.TaiKhoans.Add(tk);
                        nv.TaiKhoan = tk;
                    }
                }
                else
                {
                    var existing = _data.NhanViens.FirstOrDefault(n => n.NhanVienID == nv.NhanVienID);
                    if (existing != null)
                    {
                        existing.TenNV = nv.TenNV;
                        existing.SDT = nv.SDT;
                        existing.DiaChi = nv.DiaChi;

                        if (!string.IsNullOrWhiteSpace(username))
                        {
                            var existingTk = _data.TaiKhoans.FirstOrDefault(t => t.NhanVienID == nv.NhanVienID);
                            if (existingTk != null)
                            {
                                existingTk.TenDangNhap = username;
                                if (!string.IsNullOrWhiteSpace(password)) existingTk.MatKhauHash = password;
                                if (!string.IsNullOrWhiteSpace(vaiTro)) existingTk.VaiTro = vaiTro;
                            }
                            else
                            {
                                var tk = new TaiKhoan
                                {
                                    TaiKhoanID = _data.TaiKhoans.Count > 0 ? _data.TaiKhoans.Max(t => t.TaiKhoanID) + 1 : 1,
                                    NhanVienID = nv.NhanVienID,
                                    TenDangNhap = username,
                                    MatKhauHash = string.IsNullOrWhiteSpace(password) ? "123456" : password,
                                    VaiTro = string.IsNullOrWhiteSpace(vaiTro) ? "Staff" : vaiTro,
                                    TrangThai = true,
                                    NhanVien = existing
                                };
                                _data.TaiKhoans.Add(tk);
                                existing.TaiKhoan = tk;
                            }
                        }
                    }
                }

                SaveData();
                return nv;
            }
        }

        public bool DeleteNhanVien(int id)
        {
            lock (_lock)
            {
                var nv = _data.NhanViens.FirstOrDefault(n => n.NhanVienID == id);
                if (nv == null) return false;

                _data.NhanViens.Remove(nv);
                _data.TaiKhoans.RemoveAll(t => t.NhanVienID == id);
                SaveData();
                return true;
            }
        }

        public List<SanPham> GetSanPhams(string? keyword = null, string? danhMuc = null, bool? sapHetHang = null)
        {
            lock (_lock)
            {
                var query = _data.SanPhams.AsEnumerable();

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    var k = keyword.Trim().ToLower();
                    query = query.Where(s => s.MaSP.ToLower().Contains(k) || s.TenSP.ToLower().Contains(k));
                }

                if (!string.IsNullOrWhiteSpace(danhMuc) && danhMuc != "Tất cả")
                {
                    query = query.Where(s => s.DanhMuc != null && s.DanhMuc.Equals(danhMuc, StringComparison.OrdinalIgnoreCase));
                }

                if (sapHetHang.HasValue && sapHetHang.Value)
                {
                    query = query.Where(s => s.TonKho <= 20);
                }

                return query.OrderBy(s => s.MaSP).ToList();
            }
        }

        public SanPham? GetSanPhamById(int id)
        {
            lock (_lock)
            {
                return _data.SanPhams.FirstOrDefault(s => s.SanPhamID == id);
            }
        }

        public SanPham? GetSanPhamByMa(string maSP)
        {
            lock (_lock)
            {
                return _data.SanPhams.FirstOrDefault(s => s.MaSP.Equals(maSP, StringComparison.OrdinalIgnoreCase));
            }
        }

        public SanPham SaveSanPham(SanPham sp)
        {
            lock (_lock)
            {
                if (sp.SanPhamID == 0)
                {
                    sp.SanPhamID = _data.SanPhams.Count > 0 ? _data.SanPhams.Max(s => s.SanPhamID) + 1 : 1;
                    if (string.IsNullOrWhiteSpace(sp.MaSP))
                    {
                        sp.MaSP = $"SP{sp.SanPhamID:D3}";
                    }
                    _data.SanPhams.Add(sp);
                }
                else
                {
                    var existing = _data.SanPhams.FirstOrDefault(s => s.SanPhamID == sp.SanPhamID);
                    if (existing != null)
                    {
                        existing.MaSP = sp.MaSP;
                        existing.TenSP = sp.TenSP;
                        existing.GiaNhap = sp.GiaNhap;
                        existing.GiaLe = sp.GiaLe;
                        existing.GiaSi = sp.GiaSi;
                        existing.TonKho = sp.TonKho;
                        existing.DonViTinh = sp.DonViTinh;
                        existing.DanhMuc = sp.DanhMuc;
                        existing.HinhAnh = sp.HinhAnh;
                    }
                }

                SaveData();
                return sp;
            }
        }

        public bool DeleteSanPham(int id)
        {
            lock (_lock)
            {
                var sp = _data.SanPhams.FirstOrDefault(s => s.SanPhamID == id);
                if (sp == null) return false;

                _data.SanPhams.Remove(sp);
                SaveData();
                return true;
            }
        }

        public List<KhachHang> GetKhachHangs(string? keyword = null)
        {
            lock (_lock)
            {
                var query = _data.KhachHangs.AsEnumerable();
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    var k = keyword.Trim().ToLower();
                    query = query.Where(kh => kh.TenKH.ToLower().Contains(k) || (kh.SDT != null && kh.SDT.Contains(k)));
                }
                return query.OrderBy(kh => kh.TenKH).ToList();
            }
        }

        public KhachHang? GetKhachHangById(int id)
        {
            lock (_lock)
            {
                return _data.KhachHangs.FirstOrDefault(k => k.KhachHangID == id);
            }
        }

        public KhachHang SaveKhachHang(KhachHang kh)
        {
            lock (_lock)
            {
                if (kh.KhachHangID == 0)
                {
                    kh.KhachHangID = _data.KhachHangs.Count > 0 ? _data.KhachHangs.Max(k => k.KhachHangID) + 1 : 1;
                    _data.KhachHangs.Add(kh);
                }
                else
                {
                    var existing = _data.KhachHangs.FirstOrDefault(k => k.KhachHangID == kh.KhachHangID);
                    if (existing != null)
                    {
                        existing.TenKH = kh.TenKH;
                        existing.SDT = kh.SDT;
                        existing.DiaChi = kh.DiaChi;
                    }
                }

                SaveData();
                return kh;
            }
        }

        public List<HoaDon> GetLichSuMuaHang(int khachHangId)
        {
            lock (_lock)
            {
                return _data.HoaDons
                    .Where(h => h.KhachHangID == khachHangId)
                    .OrderByDescending(h => h.NgayLap)
                    .ToList();
            }
        }

        public (bool Success, string Message, HoaDon? HoaDon) CreateHoaDon(PosCheckoutRequest req, int nhanVienId)
        {
            lock (_lock)
            {
                if (req.Items == null || req.Items.Count == 0)
                {
                    return (false, "Giỏ hàng đang trống! Vui lòng chọn sản phẩm.", null);
                }

                foreach (var item in req.Items)
                {
                    var sp = _data.SanPhams.FirstOrDefault(s => s.SanPhamID == item.SanPhamID);
                    if (sp == null)
                    {
                        return (false, $"Sản phẩm '{item.TenSP}' không tồn tại trên hệ thống!", null);
                    }
                    if (sp.TonKho < item.SoLuong)
                    {
                        return (false, $"Sản phẩm '{sp.TenSP}' chỉ còn {sp.TonKho} {sp.DonViTinh} trong kho, không đủ đáp ứng {item.SoLuong}!", null);
                    }
                }

                int? khId = req.KhachHangID;
                if (!khId.HasValue && !string.IsNullOrWhiteSpace(req.TenKhachHangMoi))
                {
                    var newKh = new KhachHang
                    {
                        KhachHangID = _data.KhachHangs.Count > 0 ? _data.KhachHangs.Max(k => k.KhachHangID) + 1 : 1,
                        TenKH = req.TenKhachHangMoi.Trim(),
                        SDT = req.SdtKhachHangMoi?.Trim()
                    };
                    _data.KhachHangs.Add(newKh);
                    khId = newKh.KhachHangID;
                }

                var hoaDonId = _data.HoaDons.Count > 0 ? _data.HoaDons.Max(h => h.HoaDonID) + 1 : 1;
                var hoaDon = new HoaDon
                {
                    HoaDonID = hoaDonId,
                    NhanVienID = nhanVienId,
                    KhachHangID = khId,
                    NgayLap = DateTime.Now,
                    PhuongThucTT = string.IsNullOrWhiteSpace(req.PhuongThucTT) ? "Tiền mặt" : req.PhuongThucTT,
                    GhiChu = req.GhiChu,
                    ChiTietHoaDons = new List<ChiTietHoaDon>()
                };

                decimal tongTien = 0;

                foreach (var item in req.Items)
                {
                    var sp = _data.SanPhams.First(s => s.SanPhamID == item.SanPhamID);
                    sp.TonKho -= item.SoLuong;

                    bool isGiaSi = item.SoLuong >= 10;
                    decimal donGia = isGiaSi ? sp.GiaSi : sp.GiaLe;
                    decimal thanhTien = item.SoLuong * donGia;

                    tongTien += thanhTien;

                    hoaDon.ChiTietHoaDons.Add(new ChiTietHoaDon
                    {
                        HoaDonID = hoaDonId,
                        SanPhamID = sp.SanPhamID,
                        SoLuong = item.SoLuong,
                        DonGia = donGia,
                        ThanhTien = thanhTien,
                        IsGiaSi = isGiaSi,
                        SanPham = sp
                    });
                }

                hoaDon.TongTien = tongTien;
                hoaDon.NhanVien = _data.NhanViens.FirstOrDefault(n => n.NhanVienID == nhanVienId);
                if (khId.HasValue)
                {
                    hoaDon.KhachHang = _data.KhachHangs.FirstOrDefault(k => k.KhachHangID == khId.Value);
                }

                _data.HoaDons.Add(hoaDon);
                SaveData();

                return (true, "Thanh toán thành công!", hoaDon);
            }
        }

        public List<HoaDon> GetHoaDons(DateTime? tuNgay = null, DateTime? denNgay = null)
        {
            lock (_lock)
            {
                var query = _data.HoaDons.AsEnumerable();

                if (tuNgay.HasValue)
                {
                    var start = tuNgay.Value.Date;
                    query = query.Where(h => h.NgayLap >= start);
                }

                if (denNgay.HasValue)
                {
                    var end = denNgay.Value.Date.AddDays(1).AddTicks(-1);
                    query = query.Where(h => h.NgayLap <= end);
                }

                return query.OrderByDescending(h => h.NgayLap).ToList();
            }
        }

        public HoaDon? GetHoaDonById(int id)
        {
            lock (_lock)
            {
                return _data.HoaDons.FirstOrDefault(h => h.HoaDonID == id);
            }
        }

        public (bool Success, string Message, PhieuNhap? PhieuNhap) CreatePhieuNhap(TaoPhieuNhapRequest req, int nhanVienId)
        {
            lock (_lock)
            {
                if (req.Items == null || req.Items.Count == 0)
                {
                    return (false, "Chưa chọn sản phẩm nào để nhập kho!", null);
                }

                var phieuNhapId = _data.PhieuNhaps.Count > 0 ? _data.PhieuNhaps.Max(p => p.PhieuNhapID) + 1 : 1;
                var pn = new PhieuNhap
                {
                    PhieuNhapID = phieuNhapId,
                    NhanVienID = nhanVienId,
                    NgayNhap = DateTime.Now,
                    NhaCungCap = req.NhaCungCap ?? "Nhà cung cấp chính",
                    GhiChu = req.GhiChu,
                    ChiTietPhieuNhaps = new List<ChiTietPhieuNhap>()
                };

                decimal tongTien = 0;

                foreach (var item in req.Items)
                {
                    var sp = _data.SanPhams.FirstOrDefault(s => s.SanPhamID == item.SanPhamID);
                    if (sp == null || item.SoLuongNhap <= 0) continue;

                    sp.TonKho += item.SoLuongNhap;
                    if (item.DonGiaNhap > 0)
                    {
                        sp.GiaNhap = item.DonGiaNhap;
                    }

                    var thanhTien = item.SoLuongNhap * item.DonGiaNhap;
                    tongTien += thanhTien;

                    pn.ChiTietPhieuNhaps.Add(new ChiTietPhieuNhap
                    {
                        PhieuNhapID = phieuNhapId,
                        SanPhamID = sp.SanPhamID,
                        SoLuongNhap = item.SoLuongNhap,
                        DonGiaNhap = item.DonGiaNhap,
                        SanPham = sp
                    });
                }

                pn.TongTien = tongTien;
                pn.NhanVien = _data.NhanViens.FirstOrDefault(n => n.NhanVienID == nhanVienId);

                _data.PhieuNhaps.Add(pn);
                SaveData();

                return (true, "Lập phiếu nhập kho thành công! Đã cập nhật tồn kho.", pn);
            }
        }

        public List<PhieuNhap> GetPhieuNhaps()
        {
            lock (_lock)
            {
                return _data.PhieuNhaps.OrderByDescending(p => p.NgayNhap).ToList();
            }
        }

        public PhieuNhap? GetPhieuNhapById(int id)
        {
            lock (_lock)
            {
                return _data.PhieuNhaps.FirstOrDefault(p => p.PhieuNhapID == id);
            }
        }

        // CHỐT CA TRỰC (SALE REPORT)
        public List<ChotCa> GetChotCas(int? nhanVienId = null)
        {
            lock (_lock)
            {
                var query = _data.ChotCas.AsEnumerable();
                if (nhanVienId.HasValue)
                {
                    query = query.Where(c => c.NhanVienID == nhanVienId.Value);
                }
                return query.OrderByDescending(c => c.NgayChot).ToList();
            }
        }

        public ChotCa? GetChotCaById(int id)
        {
            lock (_lock)
            {
                return _data.ChotCas.FirstOrDefault(c => c.ChotCaID == id);
            }
        }

        public ChotCa SaveChotCa(ChotCa chotCa, int nhanVienId)
        {
            lock (_lock)
            {
                chotCa.ChotCaID = _data.ChotCas.Count > 0 ? _data.ChotCas.Max(c => c.ChotCaID) + 1 : 1;
                chotCa.NhanVienID = nhanVienId;
                chotCa.NgayChot = DateTime.Now;
                chotCa.NhanVien = _data.NhanViens.FirstOrDefault(n => n.NhanVienID == nhanVienId);

                _data.ChotCas.Add(chotCa);

                // Mỗi lần chốt ca được 20 điểm theo quy tắc hệ thống
                if (chotCa.NhanVien != null)
                {
                    chotCa.NhanVien.DiemTichLuy += 20;
                    chotCa.NhanVien.Rank = RankHelper.GetRankName(chotCa.NhanVien.DiemTichLuy);
                }

                SaveData();
                return chotCa;
            }
        }

        public (decimal TongTienHang, decimal TienChuyenKhoan, decimal TienMat, int SoDon) GetThongKeCaHienTai(int? nhanVienId = null)
        {
            lock (_lock)
            {
                var today = DateTime.Today;
                var list = _data.HoaDons.Where(h => h.NgayLap >= today);
                if (nhanVienId.HasValue && nhanVienId.Value > 0)
                {
                    list = list.Where(h => h.NhanVienID == nhanVienId.Value);
                }

                var listHd = list.ToList();
                decimal tong = listHd.Sum(h => h.TongTien);
                decimal ck = listHd.Where(h => h.PhuongThucTT.Contains("khoản", StringComparison.OrdinalIgnoreCase)).Sum(h => h.TongTien);
                decimal tm = listHd.Where(h => h.PhuongThucTT.Contains("mặt", StringComparison.OrdinalIgnoreCase)).Sum(h => h.TongTien);

                return (tong, ck, tm, listHd.Count);
            }
        }

        // THUMB UP (KHEN THƯỞNG)
        public List<ThumbUp> GetThumbUps(int? nhanVienId = null)
        {
            lock (_lock)
            {
                var query = _data.ThumbUps.AsEnumerable();
                if (nhanVienId.HasValue && nhanVienId.Value > 0)
                {
                    query = query.Where(t => t.NhanVienID == nhanVienId.Value);
                }
                return query.OrderByDescending(t => t.NgayGui).ToList();
            }
        }

        public ThumbUp SendThumbUp(int nhanVienId, int adminId, string adminTen, string noiDung, decimal tienThuong, int diemThuong)
        {
            lock (_lock)
            {
                var tu = new ThumbUp
                {
                    ThumbUpID = _data.ThumbUps.Count > 0 ? _data.ThumbUps.Max(t => t.ThumbUpID) + 1 : 1,
                    NhanVienID = nhanVienId,
                    NguoiGuiID = adminId,
                    NguoiGuiTen = string.IsNullOrWhiteSpace(adminTen) ? "Admin" : adminTen,
                    NoiDung = noiDung,
                    TienThuong = tienThuong,
                    DiemThuong = diemThuong,
                    NgayGui = DateTime.Now
                };

                var nv = _data.NhanViens.FirstOrDefault(n => n.NhanVienID == nhanVienId);
                if (nv != null)
                {
                    tu.NhanVien = nv;
                    nv.DiemTichLuy += diemThuong;
                    nv.Rank = RankHelper.GetRankName(nv.DiemTichLuy);
                }

                _data.ThumbUps.Add(tu);
                SaveData();
                return tu;
            }
        }

        public DashboardViewModel GetDashboardStats(int? currentNhanVienId = null)
        {
            lock (_lock)
            {
                var today = DateTime.Today;
                var endToday = today.AddDays(1).AddTicks(-1);

                var hoaDonHomNay = _data.HoaDons.Where(h => h.NgayLap >= today && h.NgayLap <= endToday).ToList();
                decimal doanhThuHomNay = hoaDonHomNay.Sum(h => h.TongTien);
                int soDonHomNay = hoaDonHomNay.Count;

                int tongSP = _data.SanPhams.Count;
                int sapHet = _data.SanPhams.Count(s => s.TonKho <= 20);
                int tongNV = _data.NhanViens.Count;

                var ganDay = _data.HoaDons.OrderByDescending(h => h.NgayLap).Take(5).ToList();
                var phieuNhapGanDay = _data.PhieuNhaps.OrderByDescending(p => p.NgayNhap).Take(5).ToList();

                NhanVien? currentNv = null;
                if (currentNhanVienId.HasValue)
                {
                    currentNv = _data.NhanViens.FirstOrDefault(n => n.NhanVienID == currentNhanVienId.Value);
                }
                if (currentNv == null)
                {
                    currentNv = _data.NhanViens.FirstOrDefault(n => n.NhanVienID == 2) ?? _data.NhanViens.FirstOrDefault();
                }

                var thumbUps = _data.ThumbUps
                    .Where(t => currentNv == null || t.NhanVienID == currentNv.NhanVienID)
                    .OrderByDescending(t => t.NgayGui)
                    .Take(5)
                    .ToList();

                return new DashboardViewModel
                {
                    DoanhThuHomNay = doanhThuHomNay,
                    SoDonHangHomNay = soDonHomNay,
                    TongSanPham = tongSP,
                    SanPhamSapHet = sapHet,
                    TongNhanVien = tongNV,
                    HoaDonGanDay = ganDay,
                    PhieuNhapGanDay = phieuNhapGanDay,
                    TopSanPhamDashboard = _data.SanPhams.OrderBy(s => s.TonKho).Take(5).ToList(),
                    ThumbUpGanDay = thumbUps,
                    CurrentNhanVien = currentNv,
                    DanhSachNhanVien = _data.NhanViens.ToList()
                };
            }
        }

        public BaoCaoViewModel GetBaoCao(DateTime tuNgay, DateTime denNgay)
        {
            lock (_lock)
            {
                var start = tuNgay.Date;
                var end = denNgay.Date.AddDays(1).AddTicks(-1);

                var listHd = _data.HoaDons.Where(h => h.NgayLap >= start && h.NgayLap <= end).ToList();

                decimal tongDT = listHd.Sum(h => h.TongTien);
                decimal dtTienMat = listHd.Where(h => h.PhuongThucTT.Contains("mặt", StringComparison.OrdinalIgnoreCase)).Sum(h => h.TongTien);
                decimal dtChuyenKhoan = listHd.Where(h => h.PhuongThucTT.Contains("khoản", StringComparison.OrdinalIgnoreCase)).Sum(h => h.TongTien);

                var spDict = _data.SanPhams.ToDictionary(s => s.SanPhamID);
                var salesGroup = listHd.SelectMany(h => h.ChiTietHoaDons)
                    .GroupBy(ct => ct.SanPhamID)
                    .Select(g =>
                    {
                        var spId = g.Key;
                        spDict.TryGetValue(spId, out var sp);
                        return new TopSanPhamReportItem
                        {
                            SanPhamID = spId,
                            MaSP = sp?.MaSP ?? $"SP{spId}",
                            TenSP = sp?.TenSP ?? "Sản phẩm",
                            SoLuongDaBan = g.Sum(x => x.SoLuong),
                            DoanhThu = g.Sum(x => x.ThanhTien)
                        };
                    })
                    .OrderByDescending(x => x.SoLuongDaBan)
                    .Take(10)
                    .ToList();

                var dailyGroup = listHd.GroupBy(h => h.NgayLap.ToString("dd/MM/yyyy"))
                    .Select(g => new DoanhThuTheoNgayItem
                    {
                        Ngay = g.Key,
                        DoanhThu = g.Sum(x => x.TongTien),
                        TienMat = g.Where(x => x.PhuongThucTT.Contains("mặt", StringComparison.OrdinalIgnoreCase)).Sum(x => x.TongTien),
                        ChuyenKhoan = g.Where(x => x.PhuongThucTT.Contains("khoản", StringComparison.OrdinalIgnoreCase)).Sum(x => x.TongTien)
                    })
                    .ToList();

                return new BaoCaoViewModel
                {
                    TuNgay = tuNgay,
                    DenNgay = denNgay,
                    TongDoanhThu = tongDT,
                    DoanhThuTienMat = dtTienMat,
                    DoanhThuChuyenKhoan = dtChuyenKhoan,
                    TongSoDonHang = listHd.Count,
                    TopSanPham = salesGroup,
                    DoanhThuTheoNgay = dailyGroup
                };
            }
        }
    }
}
