using System;
using System.Collections.Generic;

namespace QuanLyKhachHang.Models
{
    /// <summary>
    /// Model đại diện cho 1 đơn thuốc (toa thuốc) do bác sĩ kê cho khách hàng,
    /// được nhân viên nhà thuốc lưu lại để tiện bán theo đúng toa và tra cứu lại
    /// khi khách quay lại mua tiếp / mua thêm.
    /// </summary>
    public class DonThuoc
    {
        public string MaDonThuoc { get; set; } = string.Empty;

        /// <summary>Mã khách hàng gắn với đơn thuốc này (có thể rỗng nếu khách vãng lai).</summary>
        public string MaKH { get; set; } = string.Empty;
        public string TenKH { get; set; } = string.Empty;

        public string TenBacSi { get; set; } = string.Empty;
        public string NoiKeDon { get; set; } = string.Empty; // Bệnh viện / phòng khám

        public DateTime NgayKeDon { get; set; } = DateTime.Now;
        public DateTime NgayLuu { get; set; } = DateTime.Now;

        public List<ChiTietDonThuoc> DanhSachThuoc { get; set; } = new();

        public string GhiChu { get; set; } = string.Empty;

        /// <summary>Đơn thuốc này đã được bán (chuyển thành đơn hàng) hay chưa.</summary>
        public bool DaBan { get; set; } = false;

        /// <summary>Mã đơn hàng đã tạo ra từ đơn thuốc này (nếu đã bán).</summary>
        public string MaDonHangLienKet { get; set; } = string.Empty;

        public override string ToString() =>
            $"{MaDonThuoc} - {TenKH} - BS {TenBacSi} ({NgayKeDon:dd/MM/yyyy}){(DaBan ? " [Đã bán]" : "")}";
    }
}
