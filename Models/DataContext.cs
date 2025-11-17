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
        public DbSet<SinhVien> SinhViens { get; set; }

        public DbSet<Admin> Admins { get; set; }

        public DbSet<HocPhan> HocPhans { get; set; }
        public DbSet<LopHocPhan> LopHocPhans { get; set; }
        public DbSet<DangKyLop> DangKyLops { get; set; }
        public DbSet<DiemDanh> DiemDanhs { get; set; }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);

            // VaiTro
            mb.Entity<VaiTro>(e =>
            {
                e.ToTable("VaiTro");
                e.Property(x => x.TenVaiTro).HasMaxLength(50).IsRequired();
                e.HasIndex(x => x.TenVaiTro).IsUnique();
            });

            // AdminMenu
            mb.Entity<AdminMenu>(e =>
            {
                e.ToTable("AdminMenu");
                e.Property(x => x.ItemName).HasMaxLength(200);
                e.Property(x => x.IsActive).HasDefaultValue(true);
            });

            // KhoaVien
            mb.Entity<KhoaVien>().ToTable("KhoaVien");

            // GiangVien
            mb.Entity<GiangVien>(e =>
            {
                e.ToTable("GiangVien");
                e.Property(x => x.TrangThai).HasDefaultValue(true);

                e.HasOne(x => x.VaiTro)
                 .WithMany(v => v.GiangViens)
                 .HasForeignKey(x => x.MaVaiTro)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // SinhVien
            mb.Entity<SinhVien>(e =>
            {
                e.ToTable("SinhVien");
                e.Property(x => x.TrangThai).HasDefaultValue(true);

                e.HasOne(x => x.VaiTro)
                 .WithMany(v => v.SinhViens)
                 .HasForeignKey(x => x.MaVaiTro)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ThongBao
            mb.Entity<ThongBao>(e =>
            {
                e.ToTable("ThongBao");
                e.Property(x => x.TrangThai).HasDefaultValue(true);

                e.HasOne(x => x.VaiTro)
                 .WithMany(v => v.ThongBaos)
                 .HasForeignKey(x => x.MaVaiTro)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // HocPhan
            mb.Entity<HocPhan>(e =>
            {
                e.ToTable("HocPhan");
                e.Property(x => x.TenHP).HasMaxLength(100).IsRequired();
                e.Property(x => x.MaSoHP).HasMaxLength(10).IsRequired();
                e.Property(x => x.PhanTiet).HasMaxLength(20).IsRequired();

                e.Property(x => x.MaSoHP)
                 .HasMaxLength(10)
                 .HasComputedColumnSql(
         "('INF' + RIGHT('000' + CONVERT(varchar(10), [MaHP]), 3))",
         stored: true);
                e.Property(x => x.TrangThai).HasDefaultValue(true);
            });



            // LopHocPhan
            mb.Entity<LopHocPhan>(e =>
            {
                e.ToTable("LopHocPhan");

                e.HasKey(x => x.MaLHP);

                e.Property(x => x.HocKy).HasMaxLength(20).IsRequired();
                e.Property(x => x.NamHoc).HasMaxLength(20).IsRequired();

                // Loại & nhóm
                e.Property(x => x.LoaiLop).HasConversion<byte?>();  // enum? -> tinyint
                e.Property(x => x.TenNhom).HasMaxLength(20);

                e.Property(x => x.TrangThai).HasDefaultValue(true);

                // Quan hệ
                e.HasOne(x => x.HocPhan)
                 .WithMany()
                 .HasForeignKey(x => x.MaHP)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.GiangVien)
                 .WithMany()
                 .HasForeignKey(x => x.MaGiangVien)
                 .OnDelete(DeleteBehavior.Restrict);

                // tự tham chiếu
                e.HasOne(x => x.LopCha)
                 .WithMany(p => p.LopCon!)
                 .HasForeignKey(x => x.MaLopCha)
                 .OnDelete(DeleteBehavior.Restrict);

                // unique tránh trùng
                e.HasIndex(x => new { x.MaHP, x.MaGiangVien, x.HocKy, x.NamHoc, x.LoaiLop, x.TenNhom })
                 .IsUnique();
            });



            // DangKyLop
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

            // DiemDanh
            mb.Entity<DiemDanh>(e =>
            {
                e.ToTable("DiemDanh");
                e.Property(x => x.TrangThai).HasConversion<byte>();
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
            mb.Entity<Admin>(e =>
            {
                e.ToTable("Admin");
                e.HasIndex(x => x.TenDangNhap).IsUnique();
                e.Property(x => x.TrangThai).HasDefaultValue(true);

                e.HasOne(x => x.VaiTro)
                 .WithMany()
                 .HasForeignKey(x => x.MaVaiTro)
                 .OnDelete(DeleteBehavior.Restrict);
            });


            mb.Entity<Nganh>().ToTable("Nganh");
            mb.Entity<LopHanhChinh>().ToTable("LopHanhChinh");
        }
    }
}
