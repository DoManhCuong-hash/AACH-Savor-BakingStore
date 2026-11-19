using Microsoft.EntityFrameworkCore;
using AachSavor.Models;

namespace AachSavor.Data
{
    public class AachSavorDbContext : DbContext
    {
        public AachSavorDbContext(DbContextOptions<AachSavorDbContext> options) : base(options)
        {
        }

        public DbSet<NhanVien> NhanViens { get; set; } = null!;
        public DbSet<TaiKhoan> TaiKhoans { get; set; } = null!;
        public DbSet<KhachHang> KhachHangs { get; set; } = null!;
        public DbSet<SanPham> SanPhams { get; set; } = null!;
        public DbSet<HoaDon> HoaDons { get; set; } = null!;
        public DbSet<ChiTietHoaDon> ChiTietHoaDons { get; set; } = null!;
        public DbSet<PhieuNhap> PhieuNhaps { get; set; } = null!;
        public DbSet<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Composite Key for tblChiTietHoaDon
            modelBuilder.Entity<ChiTietHoaDon>()
                .HasKey(c => new { c.HoaDonID, c.SanPhamID });

            modelBuilder.Entity<ChiTietHoaDon>()
                .HasOne(c => c.HoaDon)
                .WithMany(h => h.ChiTietHoaDons)
                .HasForeignKey(c => c.HoaDonID);

            modelBuilder.Entity<ChiTietHoaDon>()
                .HasOne(c => c.SanPham)
                .WithMany()
                .HasForeignKey(c => c.SanPhamID);

            // Composite Key for tblChiTietPhieuNhap
            modelBuilder.Entity<ChiTietPhieuNhap>()
                .HasKey(c => new { c.PhieuNhapID, c.SanPhamID });

            modelBuilder.Entity<ChiTietPhieuNhap>()
                .HasOne(c => c.PhieuNhap)
                .WithMany(p => p.ChiTietPhieuNhaps)
                .HasForeignKey(c => c.PhieuNhapID);

            modelBuilder.Entity<ChiTietPhieuNhap>()
                .HasOne(c => c.SanPham)
                .WithMany()
                .HasForeignKey(c => c.SanPhamID);

            // TaiKhoan <-> NhanVien
            modelBuilder.Entity<TaiKhoan>()
                .HasOne(t => t.NhanVien)
                .WithOne(n => n.TaiKhoan)
                .HasForeignKey<TaiKhoan>(t => t.NhanVienID);
        }
    }
}
