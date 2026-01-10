using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace aznews.Areas.Admin.Models
{
    [Table("DiemLop")]
    public class DiemLop
    {
        [Key]
        public int Id { get; set; }

        // FK
        public int MaLHP { get; set; }
        public int MaSinhVien { get; set; }

        // 0..10, có thể null
        [Column(TypeName = "decimal(4,2)")]
        public decimal? DiemQT { get; set; }

        [Column(TypeName = "decimal(4,2)")]
        public decimal? DiemThi { get; set; }

        // Nếu cột trong DB là computed thì để nullable và không set ở code
        [Column(TypeName = "decimal(4,2)")]
        public decimal? DiemTong { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? UpdatedAt { get; set; }
        [MaxLength(50)]
        public string? UpdatedBy { get; set; }

        [ForeignKey(nameof(MaLHP))]
        public virtual LopHocPhan? LopHocPhan { get; set; }

        [ForeignKey(nameof(MaSinhVien))]
        public virtual SinhVien? SinhVien { get; set; }
    }
}
