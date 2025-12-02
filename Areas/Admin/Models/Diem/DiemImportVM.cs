using Microsoft.AspNetCore.Http;

namespace aznews.Areas.Admin.Models.Diem
{
    // ViewModel cho import Excel điểm
    public class DiemImportVM
    {
        public int MaLHP { get; set; }
        public IFormFile? File { get; set; }
    }
}
