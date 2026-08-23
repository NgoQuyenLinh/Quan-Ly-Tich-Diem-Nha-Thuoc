using System;
using System.Collections.Generic;

namespace QuanLyKhachHang.Models
{
    /// <summary>
    /// Model đại diện cho 1 phiếu nhập hàng từ nhà cung cấp. Khi phiếu được xác nhận,
    /// DataService sẽ tự động tạo các LoThuoc tương ứng để cộng vào tồn kho, đồng thời
    /// phiếu này được dùng để tính công nợ còn lại với nhà cung cấp (TongTien - DaThanhToan).
    /// </summary>
    public class PhieuNhapHang
    {
        public string MaPhieuNhap { get; set; } = string.Empty;

        public string MaNCC { get; set; } = string.Empty;
        public string TenNCC { get; set; } = string.Empty;

        public DateTime NgayNhap { get; set; } = DateTime.Now;

        public List<ChiTietPhieuNhap> DanhSachThuoc { get; set; } = new();

        /// <summary>Tổng tiền phiếu nhập = tổng ThanhTien các dòng thuốc.</summary>
        public decimal TongTien { get; set; }

        /// <summary>Số tiền đã thanh toán cho nhà cung cấp (có thể trả từng phần).</summary>
        public decimal DaThanhToan { get; set; } = 0;

        public string GhiChu { get; set; } = string.Empty;

        /// <summary>Công nợ còn lại với nhà cung cấp cho riêng phiếu này.</summary>
        public decimal ConNo => TongTien - DaThanhToan;

        public override string ToString() =>
            $"{MaPhieuNhap} - {TenNCC} - {NgayNhap:dd/MM/yyyy} - {TongTien:N0}đ (Nợ: {ConNo:N0}đ)";
    }
}
