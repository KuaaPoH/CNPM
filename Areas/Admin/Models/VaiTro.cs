using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace aznews.Areas.Admin.Models
{
    [Table("VaiTro")]
    public class VaiTro
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaVaiTro { get; set; }

        [Required, StringLength(50)]
        [Display(Name = "Tên Vai Trò")]
        public string TenVaiTro { get; set; }
        
    }
}