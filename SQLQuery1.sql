CREATE DATABASE AachSavorDB;
GO

USE AachSavorDB;
GO

-- 1. BANG NHAN VIEN
CREATE TABLE tblNhanVien
(
    PK_iNhanVienID INT IDENTITY(1,1) PRIMARY KEY,
    sTenNV NVARCHAR(100) NOT NULL,
    sSDT VARCHAR(15),
    sDiaChi NVARCHAR(255)
);
GO

-- 2. BANG TAI KHOAN
CREATE TABLE tblTaiKhoan
(
    PK_iTaiKhoanID INT IDENTITY(1,1) PRIMARY KEY,
    FK_iNhanVienID INT NOT NULL UNIQUE,
    sTenDangNhap VARCHAR(50) NOT NULL UNIQUE,
    sMatKhauHash NVARCHAR(255) NOT NULL,
    sVaiTro NVARCHAR(20) NOT NULL,
    bTrangThai BIT NOT NULL DEFAULT 1,

    CONSTRAINT FK_TaiKhoan_NhanVien
        FOREIGN KEY (FK_iNhanVienID)
        REFERENCES tblNhanVien(PK_iNhanVienID),

    CONSTRAINT CK_TaiKhoan_VaiTro
        CHECK (sVaiTro IN ('Admin', 'Staff'))
);
GO

-- 3. BANG KHACH HANG
CREATE TABLE tblKhachHang
(
    PK_iKhachHangID INT IDENTITY(1,1) PRIMARY KEY,
    sTenKH NVARCHAR(100) NOT NULL,
    sSDT VARCHAR(15)
);
GO

-- 4. BANG SAN PHAM
CREATE TABLE tblSanPham
(
    PK_iSanPhamID INT IDENTITY(1,1) PRIMARY KEY,
    sMaSP VARCHAR(30) NOT NULL UNIQUE,
    sTenSP NVARCHAR(100) NOT NULL,
    fGiaNhap DECIMAL(18,2) NOT NULL DEFAULT 0,
    fGiaLe DECIMAL(18,2) NOT NULL,
    fGiaSi DECIMAL(18,2) NOT NULL,
    iTonKho INT NOT NULL DEFAULT 0,

    CONSTRAINT CK_SanPham_GiaNhap
        CHECK (fGiaNhap >= 0),

    CONSTRAINT CK_SanPham_GiaLe
        CHECK (fGiaLe >= 0),

    CONSTRAINT CK_SanPham_GiaSi
        CHECK (fGiaSi >= 0),

    CONSTRAINT CK_SanPham_TonKho
        CHECK (iTonKho >= 0)
);
GO

-- 5. BANG HOA DON
CREATE TABLE tblHoaDon
(
    PK_iHoaDonID INT IDENTITY(1,1) PRIMARY KEY,
    FK_iNhanVienID INT NOT NULL,
    FK_iKhachHangID INT NULL,
    dNgayLap DATETIME NOT NULL DEFAULT GETDATE(),
    fTongTien DECIMAL(18,2) NOT NULL DEFAULT 0,
    sPhuongThucTT NVARCHAR(20) NOT NULL,

    CONSTRAINT FK_HoaDon_NhanVien
        FOREIGN KEY (FK_iNhanVienID)
        REFERENCES tblNhanVien(PK_iNhanVienID),

    CONSTRAINT FK_HoaDon_KhachHang
        FOREIGN KEY (FK_iKhachHangID)
        REFERENCES tblKhachHang(PK_iKhachHangID),

    CONSTRAINT CK_HoaDon_TongTien
        CHECK (fTongTien >= 0),

    CONSTRAINT CK_HoaDon_PhuongThuc
        CHECK (sPhuongThucTT IN (N'Tiền mặt', N'Chuyển khoản'))
);
GO

-- 6. BANG CHI TIET HOA DON
CREATE TABLE tblChiTietHoaDon
(
    FK_iHoaDonID INT NOT NULL,
    FK_iSanPhamID INT NOT NULL,
    iSoLuong INT NOT NULL,
    fDonGia DECIMAL(18,2) NOT NULL,
    fThanhTien DECIMAL(18,2) NOT NULL,

    CONSTRAINT PK_ChiTietHoaDon
        PRIMARY KEY (FK_iHoaDonID, FK_iSanPhamID),

    CONSTRAINT FK_CTHD_HoaDon
        FOREIGN KEY (FK_iHoaDonID)
        REFERENCES tblHoaDon(PK_iHoaDonID),

    CONSTRAINT FK_CTHD_SanPham
        FOREIGN KEY (FK_iSanPhamID)
        REFERENCES tblSanPham(PK_iSanPhamID),

    CONSTRAINT CK_CTHD_SoLuong
        CHECK (iSoLuong > 0),

    CONSTRAINT CK_CTHD_DonGia
        CHECK (fDonGia >= 0),

    CONSTRAINT CK_CTHD_ThanhTien
        CHECK (fThanhTien >= 0)
);
GO

-- 7. BANG PHIEU NHAP
CREATE TABLE tblPhieuNhap
(
    PK_iPhieuNhapID INT IDENTITY(1,1) PRIMARY KEY,
    FK_iNhanVienID INT NOT NULL,
    dNgayNhap DATETIME NOT NULL DEFAULT GETDATE(),
    sGhiChu NVARCHAR(255),

    CONSTRAINT FK_PhieuNhap_NhanVien
        FOREIGN KEY (FK_iNhanVienID)
        REFERENCES tblNhanVien(PK_iNhanVienID)
);
GO

-- 8. BANG CHI TIET PHIEU NHAP
CREATE TABLE tblChiTietPhieuNhap
(
    FK_iPhieuNhapID INT NOT NULL,
    FK_iSanPhamID INT NOT NULL,
    iSoLuongNhap INT NOT NULL,
    fDonGiaNhap DECIMAL(18,2) NOT NULL,

    CONSTRAINT PK_ChiTietPhieuNhap
        PRIMARY KEY (FK_iPhieuNhapID, FK_iSanPhamID),

    CONSTRAINT FK_CTPN_PhieuNhap
        FOREIGN KEY (FK_iPhieuNhapID)
        REFERENCES tblPhieuNhap(PK_iPhieuNhapID),

    CONSTRAINT FK_CTPN_SanPham
        FOREIGN KEY (FK_iSanPhamID)
        REFERENCES tblSanPham(PK_iSanPhamID),

    CONSTRAINT CK_CTPN_SoLuong
        CHECK (iSoLuongNhap > 0),

    CONSTRAINT CK_CTPN_DonGia
        CHECK (fDonGiaNhap >= 0)
);
GO
SELECT
    @@SERVERNAME AS TenMayChu,
    @@VERSION AS PhienBanSQL;