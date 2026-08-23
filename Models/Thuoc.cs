using System;

namespace QuanLyKhachHang.Models
{
    /// <summary>
    /// Model đại diện cho 1 loại thuốc trong kho.
    /// Đây là lớp dữ liệu thuần (POCO), được (de)serialize trực tiếp
    /// từ/ra file Data/thuoc.json bằng System.Text.Json.
    /// </summary>
    public class Thuoc
    {
        public string MaThuoc { get; set; } = string.Empty;
        public string TenThuoc { get; set; } = string.Empty;
        public string LoaiThuoc { get; set; } = string.Empty;

        /// <summary>Đơn giá / số tiền của 1 đơn vị thuốc.</summary>
        public decimal DonGia { get; set; }

        /// <summary>
        /// Tình trạng hàng THỦ CÔNG: true = "Còn hàng" (có thể chọn để bán trong Đơn hàng),
        /// false = "Hết hàng" (không cho phép chọn thuốc này khi tạo đơn hàng mới).
        /// Mặc định thuốc mới thêm luôn ở trạng thái "Còn hàng".
        /// </summary>
        public bool ConHang { get; set; } = true;

        /// <summary>
        /// Tổng số lượng tồn kho hiện tại, được tự động tính lại từ tổng số lượng
        /// các lô (LoThuoc) còn hàng và chưa hết hạn của thuốc này. Không sửa trực
        /// tiếp giá trị này - DataService sẽ tự đồng bộ mỗi khi lô hàng thay đổi
        /// (nhập lô mới, sửa/xoá lô, hoặc bán hàng).
        /// </summary>
        public int SoLuongTon { get; set; } = 0;

        /// <summary>
        /// Ngưỡng cảnh báo tồn kho thấp: khi SoLuongTon &lt;= NguongCanhBao,
        /// thuốc sẽ xuất hiện trong danh sách "Tồn kho thấp" để nhắc nhập thêm.
        /// </summary>
        public int NguongCanhBao { get; set; } = 10;

        public DateTime NgayTao { get; set; } = DateTime.Now;

        // ================== ĐA ĐƠN VỊ TÍNH ==================
        // Quy đổi: 1 Hộp = SoViMoiHop Vỉ, 1 Vỉ = SoVienMoiVi Viên.
        // DonGia (ở trên) LUÔN là đơn giá bán lẻ theo đơn vị nhỏ nhất (Viên).
        // Khi nhập hàng có thể nhập theo Hộp/Vỉ/Viên, khi bán có thể bán lẻ theo Viên.

        /// <summary>Số viên trong 1 vỉ (mặc định 1 = thuốc không đóng vỉ, ví dụ chai/lọ).</summary>
        public int SoVienMoiVi { get; set; } = 1;

        /// <summary>Số vỉ trong 1 hộp (mặc định 1 = thuốc không đóng hộp theo vỉ).</summary>
        public int SoViMoiHop { get; set; } = 1;

        /// <summary>Tên đơn vị nhỏ nhất dùng để bán lẻ, mặc định "Viên" (có thể là "Viên", "Gói", "Ống"...).</summary>
        public string DonViNhoNhat { get; set; } = "Viên";

        /// <summary>Tên đơn vị trung gian, mặc định "Vỉ".</summary>
        public string DonViVi { get; set; } = "Vỉ";

        /// <summary>Tên đơn vị lớn nhất dùng khi nhập hàng, mặc định "Hộp".</summary>
        public string DonViHop { get; set; } = "Hộp";

        /// <summary>Số lượng viên trong 1 hộp (= SoVienMoiVi * SoViMoiHop), tiện dùng khi quy đổi nhập kho.</summary>
        public int SoVienMoiHop => SoVienMoiVi * SoViMoiHop;

        /// <summary>Đơn giá bán theo Vỉ = DonGia (giá viên) * SoVienMoiVi.</summary>
        public decimal DonGiaVi => DonGia * SoVienMoiVi;

        /// <summary>Đơn giá bán theo Hộp = DonGia (giá viên) * SoVienMoiHop.</summary>
        public decimal DonGiaHop => DonGia * SoVienMoiHop;

        /// <summary>Quy đổi 1 số lượng theo 1 đơn vị (Hộp/Vỉ/Viên) sang tổng số Viên.</summary>
        public int QuyDoiSangVien(int soLuong, string donVi)
        {
            if (string.Equals(donVi, DonViHop, StringComparison.OrdinalIgnoreCase))
                return soLuong * SoVienMoiHop;

            if (string.Equals(donVi, DonViVi, StringComparison.OrdinalIgnoreCase))
                return soLuong * SoVienMoiVi;

            return soLuong; // mặc định coi như đơn vị nhỏ nhất (Viên)
        }

        /// <summary>Danh sách các đơn vị tính khả dụng của thuốc này, dùng để đổ vào ComboBox.</summary>
        public string[] DanhSachDonViTinh()
        {
            var ds = new List<string> { DonViNhoNhat };

            if (SoVienMoiVi > 1)
                ds.Add(DonViVi);

            if (SoViMoiHop > 1)
                ds.Add(DonViHop);

            return ds.ToArray();
        }

        public override string ToString() => $"{TenThuoc} - {DonGia:N0}đ/{DonViNhoNhat} ({(ConHang ? "Còn hàng" : "Hết hàng")})";
    }
}