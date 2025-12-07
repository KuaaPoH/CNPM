public class DiemLopAudit
{
    public long AuditId { get; set; }        // PK (IDENTITY)
    public int MaLHP { get; set; }
    public int MaSinhVien { get; set; }

    public decimal? OldQT { get; set; }
    public decimal? OldThi { get; set; }
    public decimal? NewQT { get; set; }
    public decimal? NewThi { get; set; }

    public string ActionType { get; set; } = ""; // "Edit","Import","Submit","Approve","Reject"
    public string? Reason { get; set; }          // Lý do từ chối (nếu có)
    public string ChangedBy { get; set; } = "";  // username
    public DateTime ChangedAt { get; set; } = DateTime.Now;
}
