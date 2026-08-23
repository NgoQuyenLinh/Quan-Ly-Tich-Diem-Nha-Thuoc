using System;

namespace QuanLyKhachHang.Models
{
    /// <summary>
    /// Model đại diện cho 1 lô nhập của 1 loại thuốc, dùng để theo dõi
    /// hạn sử dụng (HSD) và số lượng tồn theo từng lô (nhập trước - hết hạn trước).
    /// Tổng tồn kho (Thuoc.SoLuongTon) được tính bằng tổng SoLuong của các lô
    /// còn hàng và chưa hết hạn của thuốc đó.
    /// </summary>
    public class LoThuoc
    {
        public string MaLo { get; set; } = string.Empty;
        public string MaThuoc { get; set; } = string.Empty;
        public string SoLo { get; set; } = string.Empty;
        public DateTime HanSuDung { get; set; }
        public int SoLuong { get; set; }
        public DateTime NgayNhap { get; set; } = DateTime.Now;

        public bool DaHetHan => HanSuDung.Date < DateTime.Now.Date;

        public int SoNgayConLai => (HanSuDung.Date - DateTime.Now.Date).Days;

        public override string ToString() =>
            $"Lô {SoLo} - HSD {HanSuDung:dd/MM/yyyy} - Còn: {SoLuong}";
    }
}
