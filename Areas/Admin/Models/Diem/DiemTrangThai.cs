namespace aznews.Areas.Admin.Models.Diem
{
    // Khớp với cột DiemStatus trong bảng LopHocPhan: 0=Editable,1=Submitted,2=Approved,3=Rejected
    public enum DiemTrangThai : byte
    {
        Editable = 0,
        Submitted = 1,
        Approved = 2,
        Rejected = 3
    }
}
