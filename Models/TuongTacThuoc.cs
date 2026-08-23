namespace QuanLyKhachHang.Models
{
    public enum MucDoTuongTac
    {
        NhẹTheoDoi = 0,
        TrungBinh = 1,
        NguyHiem = 2
    }

    /// <summary>
    /// Model đại diện cho 1 cặp thuốc có tương tác cần lưu ý khi kê/bán chung
    /// trong cùng 1 đơn thuốc. Đây là danh mục cảnh báo CƠ BẢN (không thay thế
    /// tư vấn dược sĩ/bác sĩ), dùng để nhắc nhân viên kiểm tra kỹ trước khi bán.
    /// So sánh MaThuoc1/MaThuoc2 không phân biệt thứ tự khi kiểm tra.
    /// </summary>
    public class TuongTacThuoc
    {
        public string MaThuoc1 { get; set; } = string.Empty;
        public string MaThuoc2 { get; set; } = string.Empty;
        public MucDoTuongTac MucDo { get; set; } = MucDoTuongTac.NhẹTheoDoi;

        /// <summary>Mô tả ngắn lý do / hậu quả tương tác, hiển thị trong cảnh báo.</summary>
        public string MoTa { get; set; } = string.Empty;
    }
}
