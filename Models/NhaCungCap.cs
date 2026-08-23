using System;

namespace QuanLyKhachHang.Models
{
    /// <summary>
    /// Model đại diện cho 1 nhà cung cấp thuốc. Công nợ với từng nhà cung cấp
    /// được tính tổng hợp từ các phiếu nhập hàng (PhieuNhapHang) chưa thanh toán hết,
    /// không lưu trực tiếp ở đây để tránh dữ liệu bị lệch khi phiếu nhập thay đổi.
    /// </summary>
    public class NhaCungCap
    {
        public string MaNCC { get; set; } = string.Empty;
        public string TenNCC { get; set; } = string.Empty;
        public string DiaChi { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string NguoiLienHe { get; set; } = string.Empty;

        public DateTime NgayTao { get; set; } = DateTime.Now;

        public override string ToString() => $"{TenNCC} - {SoDienThoai}";
    }
}
