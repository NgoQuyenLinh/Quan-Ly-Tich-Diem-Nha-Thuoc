using System;

namespace QuanLyKhachHang.Models
{
    /// <summary>
    /// 1 dòng thuốc trong phiếu nhập hàng. Cho phép nhập theo Hộp/Vỉ/Viên (đa đơn vị tính);
    /// khi lưu phiếu, số lượng sẽ được quy đổi sang đơn vị nhỏ nhất (Viên) để tạo lô thuốc
    /// (LoThuoc) tương ứng trong kho.
    /// </summary>
    public class ChiTietPhieuNhap
    {
        public string MaThuoc { get; set; } = string.Empty;
        public string TenThuoc { get; set; } = string.Empty;

        /// <summary>Đơn vị nhập: "Hộp", "Vỉ" hoặc "Viên" (tuỳ theo cấu hình đa đơn vị của thuốc).</summary>
        public string DonViNhap { get; set; } = "Hộp";

        /// <summary>Số lượng nhập theo DonViNhap (VD: 5 Hộp).</summary>
        public int SoLuongNhap { get; set; }

        /// <summary>Số lượng đã quy đổi ra đơn vị nhỏ nhất (Viên) - dùng để tạo lô thuốc.</summary>
        public int SoLuongQuyDoiVien { get; set; }

        /// <summary>Đơn giá nhập tính theo DonViNhap (VD: giá nhập / Hộp).</summary>
        public decimal DonGiaNhap { get; set; }

        public string SoLo { get; set; } = string.Empty;
        public DateTime HanSuDung { get; set; } = DateTime.Now.AddYears(2);

        /// <summary>Thành tiền dòng này = SoLuongNhap * DonGiaNhap.</summary>
        public decimal ThanhTien => SoLuongNhap * DonGiaNhap;
    }
}
