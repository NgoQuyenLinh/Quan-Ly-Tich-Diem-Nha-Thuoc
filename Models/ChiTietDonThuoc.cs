namespace QuanLyKhachHang.Models
{
    /// <summary>
    /// 1 dòng thuốc trong đơn thuốc (toa thuốc) do bác sĩ kê.
    /// Lưu lại TenThuoc tại thời điểm kê để không đổi theo nếu sau này sửa/xoá thuốc gốc.
    /// </summary>
    public class ChiTietDonThuoc
    {
        public string MaThuoc { get; set; } = string.Empty;
        public string TenThuoc { get; set; } = string.Empty;

        /// <summary>Số lượng bác sĩ kê (theo đơn vị nhỏ nhất - Viên/Gói/Ống...).</summary>
        public int SoLuong { get; set; }

        /// <summary>Cách dùng / liều dùng, ví dụ: "Uống ngày 2 lần, mỗi lần 1 viên sau ăn".</summary>
        public string CachDung { get; set; } = string.Empty;

        public override string ToString() => $"{TenThuoc} x{SoLuong} - {CachDung}";
    }
}
