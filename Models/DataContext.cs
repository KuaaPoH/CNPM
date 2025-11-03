using aznews.Areas.Admin.Models;
using Microsoft.EntityFrameworkCore;

namespace aznews.Models
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<AdminMenu> AdminMenus { get; set; }
        public DbSet<ThongBao> ThongBaos { get; set; }
        public DbSet<VaiTro> VaiTros { get; set; }
        public DbSet<KhoaVien> KhoaViens { get; set; }
        public DbSet<Nganh> Nganhs { get; set; }
        public DbSet<LopHanhChinh> LopHanhChinhs { get; set; }
        public DbSet<GiangVien> GiangViens { get; set; }
        public DbSet<HocPhan> HocPhans { get; set; }
        public DbSet<LopHocPhan> LopHocPhans { get; set; }
        public DbSet<DangKyLop> DangKyLops { get; set; }
        public DbSet<SinhVien> SinhViens { get; set; }
        public DbSet<DiemDanh> DiemDanhs { get; set; }
        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);

            // ===== NguoiDung
            mb.Entity<NguoiDung>(e =>
            {
                e.ToTable("NguoiDung");
                e.Property(x => x.TenDangNhap).HasMaxLength(100).IsRequired();
                e.Property(x => x.MatKhau).HasMaxLength(200).IsRequired();
                e.HasIndex(x => x.TenDangNhap).IsUnique();

                e.HasOne(x => x.VaiTro)
                 .WithMany()                    // hoặc .WithMany(v => v.NguoiDungs) nếu bạn thêm navigation ngược
                 .HasForeignKey(x => x.MaVaiTro)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== VaiTro
            mb.Entity<VaiTro>(e =>
            {
                e.ToTable("VaiTro");
                e.Property(x => x.TenVaiTro).HasMaxLength(50).IsRequired();
                e.HasIndex(x => x.TenVaiTro).IsUnique();
            });

            // ===== Bảng khác (nếu có)
            mb.Entity<AdminMenu>(e =>
            {
                e.ToTable("AdminMenu");
                e.Property(x => x.ItemName).HasMaxLength(200);
                e.Property(x => x.IsActive).HasDefaultValue(true);
            });

            mb.Entity<KhoaVien>(e =>
            {
                e.ToTable("KhoaVien");
                // e.HasIndex(x => x.TenKhoaVien).IsUnique(); // bật nếu muốn khóa unique ở DB
            });

            mb.Entity<GiangVien>(e =>
            {
                e.ToTable("GiangVien");
                // e.HasIndex(x => x.MaSoGV).IsUnique();       // bật nếu muốn khóa unique ở DB
            });
            mb.Entity<HocPhan>(e =>
            {
                e.ToTable("HocPhan");
                e.Property(x => x.TenHP).HasMaxLength(100).IsRequired();
            });

            mb.Entity<LopHocPhan>(e =>
            {
                e.ToTable("LopHocPhan");
                e.Property(x => x.HocKy).HasMaxLength(20).IsRequired();
                e.Property(x => x.NamHoc).HasMaxLength(20).IsRequired();

                e.HasOne(x => x.HocPhan)
                 .WithMany()
                 .HasForeignKey(x => x.MaHP)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.GiangVien)
                 .WithMany()
                 .HasForeignKey(x => x.MaGiangVien)
                 .OnDelete(DeleteBehavior.Restrict);
            });
            mb.Entity<DangKyLop>(e =>
            {
                e.ToTable("DangKyLop");
                e.HasIndex(x => new { x.MaLHP, x.MaSinhVien }).IsUnique();

                e.HasOne(x => x.LopHocPhan)
                 .WithMany()
                 .HasForeignKey(x => x.MaLHP)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.SinhVien)
                 .WithMany()
                 .HasForeignKey(x => x.MaSinhVien)
                 .OnDelete(DeleteBehavior.Restrict);
            });
            mb.Entity<DiemDanh>(e =>
            {
                e.ToTable("DiemDanh");
                e.Property(x => x.TrangThai).HasConversion<byte>(); // enum -> tinyint
                e.HasIndex(x => new { x.MaLHP, x.Ngay, x.MaSinhVien }).IsUnique();

                e.HasOne(x => x.LopHocPhan)
                 .WithMany()
                 .HasForeignKey(x => x.MaLHP)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.SinhVien)
                 .WithMany()
                 .HasForeignKey(x => x.MaSinhVien)
                 .OnDelete(DeleteBehavior.Restrict);
            });
            mb.Entity<Nganh>().ToTable("Nganh");
            mb.Entity<LopHanhChinh>().ToTable("LopHanhChinh");
            mb.Entity<ThongBao>().ToTable("ThongBao");
        }
    }
}
