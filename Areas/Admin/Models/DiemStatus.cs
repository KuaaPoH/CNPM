// Areas/Admin/Models/DiemStatus.cs
namespace aznews.Areas.Admin.Models
{
    public enum DiemStatus : byte
    {
        Editable = 0,  // cho phép sửa
        Submitted = 1,  // GV đã gửi đề nghị chốt
        Approved = 2,  // Admin duyệt
        Rejected = 3   // Admin từ chối
    }
}
