// Services/DataService.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using QuanLyKhachHang.Models;

namespace QuanLyKhachHang.Services
{
    /// <summary>
    /// Lớp trung tâm chịu trách nhiệm:
    /// - Đọc / ghi dữ liệu JSON.
    /// - Quản lý khách hàng.
    /// - Quản lý thuốc.
    /// - Quản lý đơn hàng.
    /// - Quản lý quà tặng.
    /// - Tìm kiếm, tính điểm và thống kê.
    /// </summary>
    public class DataService
    {

        
        private readonly string _dataFolder;
        private readonly string _khFile;
        private readonly string _donFile;
        private readonly string _quaFile;
        private readonly string _thuocFile;
        private readonly string _loFile;
        private readonly string _donThuocFile;
        private readonly string _tuongTacFile;
        private readonly string _nccFile;
        private readonly string _phieuNhapFile;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true
        };

        // ================== DANH SÁCH DỮ LIỆU ==================

        public List<KhachHang> DanhSachKhachHang { get; private set; } = new();
        public List<DonHang> DanhSachDonHang { get; private set; } = new();
        public List<QuaTang> DanhSachQuaTang { get; private set; } = new();
        public List<Thuoc> DanhSachThuoc { get; private set; } = new();
        public List<LoThuoc> DanhSachLoThuoc { get; private set; } = new();
        public List<DonThuoc> DanhSachDonThuoc { get; private set; } = new();
        public List<TuongTacThuoc> DanhSachTuongTacThuoc { get; private set; } = new();
        public List<NhaCungCap> DanhSachNhaCungCap { get; private set; } = new();
        public List<PhieuNhapHang> DanhSachPhieuNhap { get; private set; } = new();


        // ================== KHỞI TẠO ==================

        public DataService()
        {
            
            _dataFolder = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Data"
            );

            Directory.CreateDirectory(_dataFolder);

            _khFile = Path.Combine(_dataFolder, "khachhang.json");
            _donFile = Path.Combine(_dataFolder, "donhang.json");
            _quaFile = Path.Combine(_dataFolder, "quatang.json");
            _thuocFile = Path.Combine(_dataFolder, "thuoc.json");
            _loFile = Path.Combine(_dataFolder, "lothuoc.json");
            _donThuocFile = Path.Combine(_dataFolder, "donthuoc.json");
            _tuongTacFile = Path.Combine(_dataFolder, "tuongtacthuoc.json");
            _nccFile = Path.Combine(_dataFolder, "nhacungcap.json");
            _phieuNhapFile = Path.Combine(_dataFolder, "phieunhap.json");

            TaiDuLieu();

            KhoiTaoQuaTangMacDinh();

            MigrateNgayTaoQuaTang();

            DongBoTonKhoTatCa();

            KhoiTaoTuongTacMacDinh();
        }


        // =========================================================
        // ĐỌC / GHI FILE
        // =========================================================

        public void TaiDuLieu()
        {
            DanhSachKhachHang = DocFile<KhachHang>(_khFile);

            DanhSachDonHang = DocFile<DonHang>(_donFile);

            DanhSachQuaTang = DocFile<QuaTang>(_quaFile);

            DanhSachThuoc = DocFile<Thuoc>(_thuocFile);

            DanhSachLoThuoc = DocFile<LoThuoc>(_loFile);

            DanhSachDonThuoc = DocFile<DonThuoc>(_donThuocFile);

            DanhSachTuongTacThuoc = DocFile<TuongTacThuoc>(_tuongTacFile);

            DanhSachNhaCungCap = DocFile<NhaCungCap>(_nccFile);

            DanhSachPhieuNhap = DocFile<PhieuNhapHang>(_phieuNhapFile);
        }


        private List<T> DocFile<T>(string duongDan)
        {
            try
            {
                if (!File.Exists(duongDan))
                    return new List<T>();

                string noiDung = File.ReadAllText(duongDan);

                if (string.IsNullOrWhiteSpace(noiDung))
                    return new List<T>();

                // Thêm cấu hình case-insensitive để tránh lỗi không khớp hoa/thường
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                return JsonSerializer.Deserialize<List<T>>(noiDung, options)
                       ?? new List<T>();
            }
            catch (Exception ex)
            {
                // Ghi log lỗi ra cửa sổ Output hoặc hiển thị thông báo để debug
                System.Diagnostics.Debug.WriteLine($"Lỗi đọc file {duongDan}: {ex.Message}");

                // Trả về danh sách rỗng thay vì làm crash toàn bộ ứng dụng
                return new List<T>();
            }
        }


        private void GhiFile<T>(
            string duongDan,
            List<T> danhSach)
        {
            string noiDung =
                JsonSerializer.Serialize(
                    danhSach,
                    _jsonOptions
                );

            File.WriteAllText(
                duongDan,
                noiDung
            );
        }


        public void LuuKhachHang()
        {
            GhiFile(
                _khFile,
                DanhSachKhachHang
            );
        }


        public void LuuDonHang()
        {
            GhiFile(
                _donFile,
                DanhSachDonHang
            );
        }


        public void LuuQuaTang()
        {
            GhiFile(
                _quaFile,
                DanhSachQuaTang
            );
        }


        public void LuuThuoc()
        {
            GhiFile(
                _thuocFile,
                DanhSachThuoc
            );
        }


        public void LuuLoThuoc()
        {
            GhiFile(
                _loFile,
                DanhSachLoThuoc
            );
        }


        public void LuuDonThuoc()
        {
            GhiFile(_donThuocFile, DanhSachDonThuoc);
        }


        public void LuuTuongTacThuoc()
        {
            GhiFile(_tuongTacFile, DanhSachTuongTacThuoc);
        }


        public void LuuNhaCungCap()
        {
            GhiFile(_nccFile, DanhSachNhaCungCap);
        }


        public void LuuPhieuNhap()
        {
            GhiFile(_phieuNhapFile, DanhSachPhieuNhap);
        }


        // =========================================================
        // KHỞI TẠO QUÀ TẶNG
        // =========================================================

        private void KhoiTaoQuaTangMacDinh()
        {
            if (DanhSachQuaTang.Count == 0)
            {
                DanhSachQuaTang.Add(
                    new QuaTang
                    {
                        MaQua = "Q01",
                        TenQua = "Áo mưa",
                        DiemQuyDoi = 1000,
                        SoLuong = 10,
                        NgayTao = DateTime.Now
                    }
                );

                DanhSachQuaTang.Add(
                    new QuaTang
                    {
                        MaQua = "Q02",
                        TenQua = "Khẩu trang",
                        DiemQuyDoi = 100,
                        SoLuong = 50,
                        NgayTao = DateTime.Now
                    }
                );

                DanhSachQuaTang.Add(
                    new QuaTang
                    {
                        MaQua = "Q03",
                        TenQua = "Nước muối",
                        DiemQuyDoi = 50,
                        SoLuong = 100,
                        NgayTao = DateTime.Now
                    }
                );

                DanhSachQuaTang.Add(
                    new QuaTang
                    {
                        MaQua = "Q04",
                        TenQua = "Giấy",
                        DiemQuyDoi = 50,
                        SoLuong = 100,
                        NgayTao = DateTime.Now
                    }
                );

                LuuQuaTang();
            }
        }


        private void MigrateNgayTaoQuaTang()
        {
            bool coThayDoi = false;

            foreach (var qua in DanhSachQuaTang)
            {
                if (qua.NgayTao == default)
                {
                    qua.NgayTao = DateTime.Now;

                    coThayDoi = true;
                }
            }

            if (coThayDoi)
            {
                LuuQuaTang();
            }
        }


        // =========================================================
        // KHÁCH HÀNG - CRUD
        // =========================================================

        public string TaoMaKhachHangMoi()
        {
            int soThuTu =
                DanhSachKhachHang.Count == 0
                    ? 1
                    : DanhSachKhachHang
                        .Select(
                            kh =>
                                int.TryParse(
                                    kh.MaKH.Replace("KH", ""),
                                    out int n
                                )
                                    ? n
                                    : 0
                        )
                        .DefaultIfEmpty(0)
                        .Max() + 1;

            return $"KH{soThuTu:D3}";
        }


        public void ThemKhachHang(KhachHang kh)
        {
            DanhSachKhachHang.Add(kh);

            LuuKhachHang();
        }


        public bool SuaKhachHang(KhachHang khMoi)
        {
            var kh =
                DanhSachKhachHang.FirstOrDefault(
                    x => x.MaKH == khMoi.MaKH
                );

            if (kh == null)
                return false;

            kh.HoTen = khMoi.HoTen;
            kh.SoDienThoai = khMoi.SoDienThoai;
            kh.DiemTichLuy = khMoi.DiemTichLuy;

            LuuKhachHang();

            return true;
        }


        public bool XoaKhachHang(string maKH)
        {
            var kh =
                DanhSachKhachHang.FirstOrDefault(
                    x => x.MaKH == maKH
                );

            if (kh == null)
                return false;

            DanhSachKhachHang.Remove(kh);

            LuuKhachHang();

            return true;
        }


        public List<KhachHang> TimKiemKhachHang(
            string tuKhoa)
        {
            if (string.IsNullOrWhiteSpace(tuKhoa))
                return DanhSachKhachHang;

            tuKhoa = tuKhoa
                .Trim()
                .ToLower();

            return DanhSachKhachHang
                .Where(
                    kh =>
                        kh.HoTen.ToLower().Contains(tuKhoa)
                        || kh.SoDienThoai
                            .ToLower()
                            .Contains(tuKhoa)
                        || kh.MaKH
                            .ToLower()
                            .Contains(tuKhoa)
                )
                .ToList();
        }


        public List<KhachHang> TimKhachHangTheoSoDienThoai(
            string chuoiSo)
        {
            if (string.IsNullOrWhiteSpace(chuoiSo))
                return new List<KhachHang>();

            chuoiSo = chuoiSo.Trim();

            return DanhSachKhachHang
                .Where(
                    kh =>
                        !string.IsNullOrEmpty(
                            kh.SoDienThoai
                        )
                        && kh.SoDienThoai.Contains(chuoiSo)
                )
                .OrderBy(
                    kh =>
                        kh.SoDienThoai.IndexOf(
                            chuoiSo,
                            StringComparison.Ordinal
                        )
                )
                .ThenBy(kh => kh.HoTen)
                .ToList();
        }


        // =========================================================
        // THUỐC - CRUD
        // =========================================================

        public string TaoMaThuocMoi()
        {
            int soThuTu =
                DanhSachThuoc.Count == 0
                    ? 1
                    : DanhSachThuoc
                        .Select(
                            t =>
                                int.TryParse(
                                    t.MaThuoc.Replace("T", ""),
                                    out int n
                                )
                                    ? n
                                    : 0
                        )
                        .DefaultIfEmpty(0)
                        .Max() + 1;

            return $"T{soThuTu:D3}";
        }


        public void ThemThuoc(Thuoc thuoc)
        {
            DanhSachThuoc.Add(thuoc);

            LuuThuoc();
        }


        public bool SuaThuoc(Thuoc thuocMoi)
        {
            var thuoc =
                DanhSachThuoc.FirstOrDefault(
                    x =>
                        x.MaThuoc
                        == thuocMoi.MaThuoc
                );

            if (thuoc == null)
                return false;

            thuoc.TenThuoc = thuocMoi.TenThuoc;

            thuoc.LoaiThuoc = thuocMoi.LoaiThuoc;

            thuoc.DonGia = thuocMoi.DonGia;

            thuoc.ConHang = thuocMoi.ConHang;

            thuoc.NguongCanhBao = thuocMoi.NguongCanhBao;

            LuuThuoc();

            return true;
        }


        public bool XoaThuoc(string maThuoc)
        {
            var thuoc =
                DanhSachThuoc.FirstOrDefault(
                    x =>
                        x.MaThuoc
                        == maThuoc
                );

            if (thuoc == null)
                return false;

            DanhSachThuoc.Remove(thuoc);

            LuuThuoc();

            return true;
        }


        public List<Thuoc> TimKiemThuoc(
            string? tuKhoa = null)
        {
            var ds =
                DanhSachThuoc.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                string tk =
                    tuKhoa
                        .Trim()
                        .ToLower();

                ds =
                    ds.Where(
                        t =>
                            t.TenThuoc
                                .ToLower()
                                .Contains(tk)
                            || t.MaThuoc
                                .ToLower()
                                .Contains(tk)
                    );
            }

            return ds
                .OrderBy(t => t.TenThuoc)
                .ToList();
        }


        public List<Thuoc> ThuocConHang()
        {
            return DanhSachThuoc
                .Where(t => t.ConHang && t.SoLuongTon > 0)
                .OrderBy(t => t.TenThuoc)
                .ToList();
        }


        // =========================================================
        // LÔ HÀNG & HẠN SỬ DỤNG
        // =========================================================

        public string TaoMaLoMoi()
        {
            int soThuTu =
                DanhSachLoThuoc.Count == 0
                    ? 1
                    : DanhSachLoThuoc
                        .Select(
                            l =>
                                int.TryParse(
                                    l.MaLo.Replace("LO", ""),
                                    out int n
                                )
                                    ? n
                                    : 0
                        )
                        .DefaultIfEmpty(0)
                        .Max() + 1;

            return $"LO{soThuTu:D4}";
        }

        /// <summary>Danh sách lô của 1 thuốc, sắp theo HSD tăng dần (hết hạn trước lên trên - FEFO).</summary>
        public List<LoThuoc> LoTheoMaThuoc(string maThuoc)
        {
            return DanhSachLoThuoc
                .Where(l => l.MaThuoc == maThuoc)
                .OrderBy(l => l.HanSuDung)
                .ToList();
        }

        public void ThemLoThuoc(LoThuoc lo)
        {
            DanhSachLoThuoc.Add(lo);
            LuuLoThuoc();
            DongBoTonKho(lo.MaThuoc);
        }

        public bool SuaLoThuoc(LoThuoc loMoi)
        {
            var lo = DanhSachLoThuoc.FirstOrDefault(x => x.MaLo == loMoi.MaLo);
            if (lo == null) return false;

            lo.SoLo = loMoi.SoLo;
            lo.HanSuDung = loMoi.HanSuDung;
            lo.SoLuong = loMoi.SoLuong;

            LuuLoThuoc();
            DongBoTonKho(lo.MaThuoc);
            return true;
        }

        public bool XoaLoThuoc(string maLo)
        {
            var lo = DanhSachLoThuoc.FirstOrDefault(x => x.MaLo == maLo);
            if (lo == null) return false;

            string maThuoc = lo.MaThuoc;
            DanhSachLoThuoc.Remove(lo);

            LuuLoThuoc();
            DongBoTonKho(maThuoc);
            return true;
        }

        /// <summary>Tính lại SoLuongTon của 1 thuốc = tổng SoLuong các lô còn hàng & chưa hết hạn.</summary>
        public void DongBoTonKho(string maThuoc)
        {
            var thuoc = DanhSachThuoc.FirstOrDefault(t => t.MaThuoc == maThuoc);
            if (thuoc == null) return;

            thuoc.SoLuongTon = DanhSachLoThuoc
                .Where(l => l.MaThuoc == maThuoc && !l.DaHetHan)
                .Sum(l => l.SoLuong);

            LuuThuoc();
        }

        /// <summary>Đồng bộ lại tồn kho cho toàn bộ thuốc, gọi 1 lần khi khởi động ứng dụng.</summary>
        public void DongBoTonKhoTatCa()
        {
            foreach (var thuoc in DanhSachThuoc)
            {
                thuoc.SoLuongTon = DanhSachLoThuoc
                    .Where(l => l.MaThuoc == thuoc.MaThuoc && !l.DaHetHan)
                    .Sum(l => l.SoLuong);
            }

            LuuThuoc();
        }

        /// <summary>Trừ số lượng khi bán hàng, ưu tiên trừ lô hết hạn sớm nhất trước (FEFO).</summary>
        private void TruTonKhoTheoLo(string maThuoc, int soLuongBan)
        {
            int conPhaiTru = soLuongBan;

            var loHopLe = DanhSachLoThuoc
                .Where(l => l.MaThuoc == maThuoc && !l.DaHetHan && l.SoLuong > 0)
                .OrderBy(l => l.HanSuDung)
                .ToList();

            foreach (var lo in loHopLe)
            {
                if (conPhaiTru <= 0) break;

                int truO = Math.Min(lo.SoLuong, conPhaiTru);
                lo.SoLuong -= truO;
                conPhaiTru -= truO;
            }

            LuuLoThuoc();
            DongBoTonKho(maThuoc);
        }

        /// <summary>Danh sách lô sắp hết hạn trong vòng soNgay ngày tới (mặc định 30 ngày), còn hàng.</summary>
        public List<LoThuoc> LoSapHetHan(int soNgay = 30)
        {
            return DanhSachLoThuoc
                .Where(l => l.SoLuong > 0 && !l.DaHetHan && l.SoNgayConLai <= soNgay)
                .OrderBy(l => l.HanSuDung)
                .ToList();
        }

        /// <summary>Danh sách lô đã hết hạn nhưng vẫn còn hàng trong kho (cần loại bỏ).</summary>
        public List<LoThuoc> LoDaHetHan()
        {
            return DanhSachLoThuoc
                .Where(l => l.SoLuong > 0 && l.DaHetHan)
                .OrderBy(l => l.HanSuDung)
                .ToList();
        }

        /// <summary>Thuốc đang ở mức tồn kho thấp (SoLuongTon &lt;= NguongCanhBao) nhưng vẫn còn hàng.</summary>
        public List<Thuoc> ThuocTonKhoThap()
        {
            return DanhSachThuoc
                .Where(t => t.SoLuongTon > 0 && t.SoLuongTon <= t.NguongCanhBao)
                .OrderBy(t => t.SoLuongTon)
                .ToList();
        }

        /// <summary>Thuốc đã hết sạch hàng trong kho (SoLuongTon &lt;= 0).</summary>
        public List<Thuoc> ThuocHetHang()
        {
            return DanhSachThuoc
                .Where(t => t.SoLuongTon <= 0)
                .OrderBy(t => t.TenThuoc)
                .ToList();
        }


        // =========================================================
        // ĐƠN THUỐC / TOA THUỐC (Bác sĩ kê)
        // =========================================================

        public string TaoMaDonThuocMoi()
        {
            int soThuTu =
                DanhSachDonThuoc.Count == 0
                    ? 1
                    : DanhSachDonThuoc
                        .Select(d => int.TryParse(d.MaDonThuoc.Replace("DT", ""), out int n) ? n : 0)
                        .DefaultIfEmpty(0)
                        .Max() + 1;

            return $"DT{soThuTu:D4}";
        }

        public void ThemDonThuoc(DonThuoc donThuoc)
        {
            DanhSachDonThuoc.Add(donThuoc);
            LuuDonThuoc();
        }

        public bool SuaDonThuoc(DonThuoc donMoi)
        {
            var don = DanhSachDonThuoc.FirstOrDefault(x => x.MaDonThuoc == donMoi.MaDonThuoc);
            if (don == null) return false;

            don.MaKH = donMoi.MaKH;
            don.TenKH = donMoi.TenKH;
            don.TenBacSi = donMoi.TenBacSi;
            don.NoiKeDon = donMoi.NoiKeDon;
            don.NgayKeDon = donMoi.NgayKeDon;
            don.DanhSachThuoc = donMoi.DanhSachThuoc;
            don.GhiChu = donMoi.GhiChu;

            LuuDonThuoc();
            return true;
        }

        public bool XoaDonThuoc(string maDonThuoc)
        {
            var don = DanhSachDonThuoc.FirstOrDefault(x => x.MaDonThuoc == maDonThuoc);
            if (don == null) return false;

            DanhSachDonThuoc.Remove(don);
            LuuDonThuoc();
            return true;
        }

        public List<DonThuoc> TimKiemDonThuoc(string? tuKhoa = null)
        {
            var ds = DanhSachDonThuoc.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                string tk = tuKhoa.Trim().ToLower();
                ds = ds.Where(d =>
                    d.TenKH.ToLower().Contains(tk)
                    || d.MaDonThuoc.ToLower().Contains(tk)
                    || d.TenBacSi.ToLower().Contains(tk));
            }

            return ds.OrderByDescending(d => d.NgayLuu).ToList();
        }

        /// <summary>
        /// Gợi ý các thuốc đang có trong kho (còn hàng) khớp tên với các dòng thuốc
        /// đã kê trong đơn thuốc - dùng để nhân viên chọn nhanh khi lên đơn hàng bán theo toa.
        /// </summary>
        public List<Thuoc> GoiYThuocTheoDonThuoc(DonThuoc donThuoc)
        {
            var ketQua = new List<Thuoc>();

            foreach (var dong in donThuoc.DanhSachThuoc)
            {
                var thuoc = DanhSachThuoc.FirstOrDefault(t => t.MaThuoc == dong.MaThuoc)
                    ?? DanhSachThuoc.FirstOrDefault(t =>
                        t.TenThuoc.Trim().ToLower() == dong.TenThuoc.Trim().ToLower());

                if (thuoc != null && !ketQua.Contains(thuoc))
                    ketQua.Add(thuoc);
            }

            return ketQua;
        }

        /// <summary>Đánh dấu đơn thuốc đã được bán, gắn với mã đơn hàng vừa tạo.</summary>
        public void DanhDauDaBan(string maDonThuoc, string maDonHang)
        {
            var don = DanhSachDonThuoc.FirstOrDefault(x => x.MaDonThuoc == maDonThuoc);
            if (don == null) return;

            don.DaBan = true;
            don.MaDonHangLienKet = maDonHang;
            LuuDonThuoc();
        }


        // =========================================================
        // CẢNH BÁO TƯƠNG TÁC THUỐC (CƠ BẢN)
        // =========================================================

        private void KhoiTaoTuongTacMacDinh()
        {
            if (DanhSachTuongTacThuoc.Count > 0) return;

            // Danh mục cảnh báo mẫu theo TÊN THUỐC (không phải mã), sẽ được đối chiếu
            // theo tên khi kiểm tra vì tên thuốc phổ biến và dễ nhận diện hơn mã nội bộ.
            // Đây là cảnh báo mang tính tham khảo/nhắc nhở, không thay thế tư vấn chuyên môn.
            var mau = new (string, string, MucDoTuongTac, string)[]
            {
                ("Aspirin", "Warfarin", MucDoTuongTac.NguyHiem, "Tăng nguy cơ chảy máu khi dùng đồng thời."),
                ("Paracetamol", "Warfarin", MucDoTuongTac.TrungBinh, "Có thể tăng nhẹ tác dụng chống đông, cần theo dõi."),
                ("Ibuprofen", "Aspirin", MucDoTuongTac.TrungBinh, "Giảm tác dụng bảo vệ tim mạch của Aspirin liều thấp."),
                ("Amoxicillin", "Methotrexate", MucDoTuongTac.NguyHiem, "Tăng độc tính của Methotrexate."),
                ("Ciprofloxacin", "Canxi", MucDoTuongTac.TrungBinh, "Canxi làm giảm hấp thu Ciprofloxacin, nên uống cách xa nhau."),
                ("Omeprazole", "Clopidogrel", MucDoTuongTac.TrungBinh, "Có thể làm giảm hiệu quả chống kết tập tiểu cầu của Clopidogrel."),
            };

            foreach (var (t1, t2, mucDo, moTa) in mau)
            {
                DanhSachTuongTacThuoc.Add(new TuongTacThuoc
                {
                    MaThuoc1 = t1,
                    MaThuoc2 = t2,
                    MucDo = mucDo,
                    MoTa = moTa
                });
            }

            LuuTuongTacThuoc();
        }

        public void ThemTuongTacThuoc(TuongTacThuoc tt)
        {
            DanhSachTuongTacThuoc.Add(tt);
            LuuTuongTacThuoc();
        }

        public bool XoaTuongTacThuoc(int index)
        {
            if (index < 0 || index >= DanhSachTuongTacThuoc.Count) return false;
            DanhSachTuongTacThuoc.RemoveAt(index);
            LuuTuongTacThuoc();
            return true;
        }

        /// <summary>
        /// Kiểm tra danh sách tên thuốc (trong 1 đơn thuốc/đơn hàng) có cặp nào nằm trong
        /// danh mục cảnh báo tương tác hay không (so khớp gần đúng theo tên, không phân biệt hoa/thường).
        /// Trả về danh sách mô tả cảnh báo để hiển thị cho nhân viên trước khi bán.
        /// </summary>
        public List<string> KiemTraTuongTac(IEnumerable<string> tenThuocList)
        {
            var canhBao = new List<string>();
            var ten = tenThuocList.Where(t => !string.IsNullOrWhiteSpace(t)).ToList();

            for (int i = 0; i < ten.Count; i++)
            {
                for (int j = i + 1; j < ten.Count; j++)
                {
                    var match = DanhSachTuongTacThuoc.FirstOrDefault(tt =>
                        (ChuaTen(ten[i], tt.MaThuoc1) && ChuaTen(ten[j], tt.MaThuoc2))
                        || (ChuaTen(ten[i], tt.MaThuoc2) && ChuaTen(ten[j], tt.MaThuoc1)));

                    if (match != null)
                    {
                        string mucDo = match.MucDo switch
                        {
                            MucDoTuongTac.NguyHiem => "NGUY HIỂM",
                            MucDoTuongTac.TrungBinh => "Trung bình",
                            _ => "Nhẹ - theo dõi"
                        };

                        canhBao.Add($"[{mucDo}] {ten[i]} + {ten[j]}: {match.MoTa}");
                    }
                }
            }

            return canhBao.Distinct().ToList();
        }

        private static bool ChuaTen(string tenDayDu, string tuKhoa) =>
            tenDayDu.Trim().ToLower().Contains(tuKhoa.Trim().ToLower());


        // =========================================================
        // NHÀ CUNG CẤP
        // =========================================================

        public string TaoMaNhaCungCapMoi()
        {
            int soThuTu =
                DanhSachNhaCungCap.Count == 0
                    ? 1
                    : DanhSachNhaCungCap
                        .Select(n => int.TryParse(n.MaNCC.Replace("NCC", ""), out int n2) ? n2 : 0)
                        .DefaultIfEmpty(0)
                        .Max() + 1;

            return $"NCC{soThuTu:D3}";
        }

        public void ThemNhaCungCap(NhaCungCap ncc)
        {
            DanhSachNhaCungCap.Add(ncc);
            LuuNhaCungCap();
        }

        public bool SuaNhaCungCap(NhaCungCap nccMoi)
        {
            var ncc = DanhSachNhaCungCap.FirstOrDefault(x => x.MaNCC == nccMoi.MaNCC);
            if (ncc == null) return false;

            ncc.TenNCC = nccMoi.TenNCC;
            ncc.DiaChi = nccMoi.DiaChi;
            ncc.SoDienThoai = nccMoi.SoDienThoai;
            ncc.Email = nccMoi.Email;
            ncc.NguoiLienHe = nccMoi.NguoiLienHe;

            LuuNhaCungCap();
            return true;
        }

        public bool XoaNhaCungCap(string maNCC)
        {
            if (DanhSachPhieuNhap.Any(p => p.MaNCC == maNCC))
                return false; // đã có phiếu nhập, không cho xoá để tránh mất dữ liệu công nợ

            var ncc = DanhSachNhaCungCap.FirstOrDefault(x => x.MaNCC == maNCC);
            if (ncc == null) return false;

            DanhSachNhaCungCap.Remove(ncc);
            LuuNhaCungCap();
            return true;
        }

        public List<NhaCungCap> TimKiemNhaCungCap(string? tuKhoa = null)
        {
            var ds = DanhSachNhaCungCap.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                string tk = tuKhoa.Trim().ToLower();
                ds = ds.Where(n =>
                    n.TenNCC.ToLower().Contains(tk)
                    || n.MaNCC.ToLower().Contains(tk)
                    || n.SoDienThoai.Contains(tk));
            }

            return ds.OrderBy(n => n.TenNCC).ToList();
        }

        /// <summary>Tổng công nợ hiện tại (chưa thanh toán hết) với 1 nhà cung cấp.</summary>
        public decimal CongNoNhaCungCap(string maNCC) =>
            DanhSachPhieuNhap.Where(p => p.MaNCC == maNCC).Sum(p => p.ConNo);


        // =========================================================
        // PHIẾU NHẬP HÀNG
        // =========================================================

        public string TaoMaPhieuNhapMoi()
        {
            int soThuTu =
                DanhSachPhieuNhap.Count == 0
                    ? 1
                    : DanhSachPhieuNhap
                        .Select(p => int.TryParse(p.MaPhieuNhap.Replace("PN", ""), out int n) ? n : 0)
                        .DefaultIfEmpty(0)
                        .Max() + 1;

            return $"PN{soThuTu:D4}";
        }

        /// <summary>
        /// Xác nhận (lưu) 1 phiếu nhập hàng: tính lại tổng tiền, lưu phiếu,
        /// đồng thời tự động tạo lô thuốc (LoThuoc) tương ứng cho từng dòng thuốc
        /// (đã quy đổi số lượng nhập theo Hộp/Vỉ sang Viên) để cộng vào tồn kho.
        /// </summary>
        public void XacNhanPhieuNhap(PhieuNhapHang phieu)
        {
            foreach (var dong in phieu.DanhSachThuoc)
            {
                var thuoc = DanhSachThuoc.FirstOrDefault(t => t.MaThuoc == dong.MaThuoc);
                dong.SoLuongQuyDoiVien = thuoc != null
                    ? thuoc.QuyDoiSangVien(dong.SoLuongNhap, dong.DonViNhap)
                    : dong.SoLuongNhap;
            }

            phieu.TongTien = phieu.DanhSachThuoc.Sum(d => d.ThanhTien);

            DanhSachPhieuNhap.Add(phieu);
            LuuPhieuNhap();

            // Tạo lô thuốc tương ứng để cộng vào tồn kho (nhập trước - hết hạn trước).
            foreach (var dong in phieu.DanhSachThuoc)
            {
                if (dong.SoLuongQuyDoiVien <= 0) continue;

                ThemLoThuoc(new LoThuoc
                {
                    MaLo = TaoMaLoMoi(),
                    MaThuoc = dong.MaThuoc,
                    SoLo = string.IsNullOrWhiteSpace(dong.SoLo) ? phieu.MaPhieuNhap : dong.SoLo,
                    HanSuDung = dong.HanSuDung,
                    SoLuong = dong.SoLuongQuyDoiVien,
                    NgayNhap = phieu.NgayNhap
                });
            }
        }

        public bool CapNhatThanhToanPhieuNhap(string maPhieuNhap, decimal soTienThemVao)
        {
            var phieu = DanhSachPhieuNhap.FirstOrDefault(x => x.MaPhieuNhap == maPhieuNhap);
            if (phieu == null) return false;

            phieu.DaThanhToan = Math.Min(phieu.TongTien, phieu.DaThanhToan + soTienThemVao);
            LuuPhieuNhap();
            return true;
        }

        public List<PhieuNhapHang> TimKiemPhieuNhap(string? maNCC = null)
        {
            var ds = DanhSachPhieuNhap.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(maNCC))
                ds = ds.Where(p => p.MaNCC == maNCC);

            return ds.OrderByDescending(p => p.NgayNhap).ToList();
        }


        // =========================================================
        // ĐƠN HÀNG
        // =========================================================

        public string TaoMaDonHangMoi()
        {
            int soThuTu =
                DanhSachDonHang.Count == 0
                    ? 1
                    : DanhSachDonHang
                        .Select(
                            d =>
                                int.TryParse(
                                    d.MaDon.Replace("DH", ""),
                                    out int n
                                )
                                    ? n
                                    : 0
                        )
                        .DefaultIfEmpty(0)
                        .Max() + 1;

            return $"DH{soThuTu:D3}";
        }


        /// <summary>
        /// Tạo đơn hàng có nhiều loại thuốc.
        /// </summary>
        public (
            bool ThanhCong,
            string ThongBao,
            DonHang? Don
        ) TaoDonHang(
            string maKH,
            List<ChiTietDonHang> danhSachThuoc,
            int diemSuDung,
            List<QuaTang>? danhSachQua = null)
        {
            danhSachQua ??= new List<QuaTang>();

            var kh =
                DanhSachKhachHang.FirstOrDefault(
                    x => x.MaKH == maKH
                );

            if (kh == null)
            {
                return (
                    false,
                    "Không tìm thấy khách hàng.",
                    null
                );
            }


            if (
                danhSachThuoc == null
                || danhSachThuoc.Count == 0
            )
            {
                return (
                    false,
                    "Đơn hàng phải có ít nhất 1 loại thuốc.",
                    null
                );
            }


            if (
                danhSachThuoc.Any(
                    t => t.SoLuong <= 0
                )
            )
            {
                return (
                    false,
                    "Số lượng thuốc không hợp lệ.",
                    null
                );
            }


            // Kiểm tra tồn kho đủ để bán (gộp số lượng nếu 1 thuốc xuất hiện nhiều dòng)
            var nhomThuoc = danhSachThuoc
                .GroupBy(t => t.MaThuoc)
                .ToList();

            foreach (var nhom in nhomThuoc)
            {
                var thuocGoc = DanhSachThuoc.FirstOrDefault(t => t.MaThuoc == nhom.Key);
                int soLuongCan = nhom.Sum(t => t.SoLuong);

                if (thuocGoc != null && thuocGoc.SoLuongTon < soLuongCan)
                {
                    return (
                        false,
                        $"Thuốc \"{thuocGoc.TenThuoc}\" chỉ còn {thuocGoc.SoLuongTon} trong kho, không đủ để bán {soLuongCan}.",
                        null
                    );
                }
            }


            decimal soTien =
                danhSachThuoc.Sum(
                    t => t.ThanhTien
                );


            if (soTien <= 0)
            {
                return (
                    false,
                    "Số tiền đơn hàng phải lớn hơn 0.",
                    null
                );
            }


            if (diemSuDung < 0)
            {
                return (
                    false,
                    "Điểm sử dụng không hợp lệ.",
                    null
                );
            }


            if (
                diemSuDung
                > kh.DiemTichLuy
            )
            {
                return (
                    false,
                    $"Khách hàng chỉ có {kh.DiemTichLuy} điểm, không đủ để sử dụng {diemSuDung} điểm.",
                    null
                );
            }


            // 100 điểm = giảm 1.000đ
            if (
                diemSuDung * 1000m / 100m
                > soTien
            )
            {
                return (
                    false,
                    "Số điểm sử dụng vượt quá giá trị đơn hàng.",
                    null
                );
            }


            // Kiểm tra tồn kho của từng món quà (cho phép chọn nhiều món quà,
            // kể cả trùng loại - miễn là còn đủ số lượng trong kho).
            var nhomQua = danhSachQua
                .GroupBy(q => q.MaQua)
                .ToList();

            foreach (var nhom in nhomQua)
            {
                var quaGoc =
                    DanhSachQuaTang.FirstOrDefault(
                        q => q.MaQua == nhom.Key
                    );

                int soLuongCan = nhom.Count();

                if (
                    quaGoc == null
                    || quaGoc.SoLuong < soLuongCan
                )
                {
                    return (
                        false,
                        $"Quà \"{nhom.First().TenQua}\" không đủ số lượng trong kho.",
                        null
                    );
                }
            }


            int tongDiemTru =
                diemSuDung
                + danhSachQua.Sum(
                    q => q.DiemQuyDoi
                );


            if (
                tongDiemTru
                > kh.DiemTichLuy
            )
            {
                return (
                    false,
                    $"Khách hàng không đủ điểm. Cần {tongDiemTru} điểm, hiện có {kh.DiemTichLuy} điểm.",
                    null
                );
            }


            int diemCong =
                (int)(soTien / 1000);


            kh.DiemTichLuy =
                kh.DiemTichLuy
                - tongDiemTru
                + diemCong;


            // Xử lý quà (trừ tồn kho theo từng loại quà đã chọn, có thể nhiều món)
            if (danhSachQua.Count > 0)
            {
                foreach (var nhom in nhomQua)
                {
                    var quaGoc =
                        DanhSachQuaTang.FirstOrDefault(
                            q => q.MaQua == nhom.Key
                        );

                    if (quaGoc != null)
                    {
                        quaGoc.SoLuong -= nhom.Count();
                        quaGoc.DangBan = true;
                    }
                }

                LuuQuaTang();
            }


            // Trừ tồn kho theo lô (ưu tiên lô hết hạn sớm nhất - FEFO), gộp theo từng loại thuốc
            foreach (var nhom in nhomThuoc)
            {
                TruTonKhoTheoLo(nhom.Key, nhom.Sum(t => t.SoLuong));

                var thuocGoc = DanhSachThuoc.FirstOrDefault(t => t.MaThuoc == nhom.Key);
                if (thuocGoc != null && thuocGoc.SoLuongTon <= 0)
                {
                    thuocGoc.ConHang = false;
                    LuuThuoc();
                }
            }


            var don =
                new DonHang
                {
                    MaDon =
                        TaoMaDonHangMoi(),

                    MaKH =
                        kh.MaKH,

                    TenKH =
                        kh.HoTen,

                    DanhSachThuoc =
                        danhSachThuoc,

                    SoTien =
                        soTien,

                    NgayTao =
                        DateTime.Now,

                    DiemCong =
                        diemCong,

                    DiemSuDung =
                        diemSuDung,

                    QuaTangDoi =
                        danhSachQua.Count == 0
                        ? string.Empty
                        : string.Join(
                            ", ",
                            danhSachQua.Select(q => q.TenQua)
                        ),

                    DiemDoiQua =
                        danhSachQua.Sum(
                            q => q.DiemQuyDoi
                        ),

                    TongDiemSauGiaoDich =
                        kh.DiemTichLuy
                };


            DanhSachDonHang.Add(don);

            LuuDonHang();

            LuuKhachHang();

            return (
                true,
                "Tạo đơn hàng thành công.",
                don
            );
        }


        public bool XoaDonHang(
            string maDon)
        {
            var don =
                DanhSachDonHang.FirstOrDefault(
                    x =>
                        x.MaDon
                        == maDon
                );

            if (don == null)
                return false;

            DanhSachDonHang.Remove(don);

            LuuDonHang();

            return true;
        }


        // =========================================================
        // QUÀ TẶNG - CRUD
        // =========================================================

        public string TaoMaQuaTangMoi()
        {
            int soThuTu =
                DanhSachQuaTang.Count == 0
                    ? 1
                    : DanhSachQuaTang
                        .Select(
                            q =>
                                int.TryParse(
                                    q.MaQua.Replace("Q", ""),
                                    out int n
                                )
                                    ? n
                                    : 0
                        )
                        .DefaultIfEmpty(0)
                        .Max() + 1;

            return $"Q{soThuTu:D2}";
        }


        public void ThemQuaTang(
            QuaTang qua)
        {
            if (qua.NgayTao == default)
                qua.NgayTao =
                    DateTime.Now;

            DanhSachQuaTang.Add(qua);

            LuuQuaTang();
        }


        public bool SuaQuaTang(
            QuaTang quaMoi)
        {
            var qua =
                DanhSachQuaTang.FirstOrDefault(
                    x =>
                        x.MaQua
                        == quaMoi.MaQua
                );

            if (qua == null)
                return false;

            qua.TenQua =
                quaMoi.TenQua;

            qua.DiemQuyDoi =
                quaMoi.DiemQuyDoi;

            qua.SoLuong =
                quaMoi.SoLuong;

            qua.DangBan =
                quaMoi.DangBan;

            LuuQuaTang();

            return true;
        }


        public bool XoaQuaTang(
            string maQua)
        {
            var qua =
                DanhSachQuaTang.FirstOrDefault(
                    x =>
                        x.MaQua
                        == maQua
                );

            if (qua == null)
                return false;

            DanhSachQuaTang.Remove(qua);

            LuuQuaTang();

            return true;
        }


        public List<QuaTang> QuaTangTrongThang(
            string? tuKhoa = null)
        {
            var now =
                DateTime.Now;

            var ds =
                DanhSachQuaTang.Where(
                    q =>
                        q.NgayTao.Year
                            == now.Year
                        && q.NgayTao.Month
                            == now.Month
                );


            if (
                !string.IsNullOrWhiteSpace(
                    tuKhoa
                )
            )
            {
                var tk =
                    tuKhoa
                        .Trim()
                        .ToLower();

                ds =
                    ds.Where(
                        q =>
                            q.TenQua
                                .ToLower()
                                .Contains(tk)
                            || q.MaQua
                                .ToLower()
                                .Contains(tk)
                    );
            }


            return ds
                .OrderByDescending(
                    q => q.NgayTao
                )
                .ToList();
        }


        public List<QuaTang> QuaTangChuaBan(
            string? tuKhoa = null)
        {
            var ds =
                DanhSachQuaTang.Where(
                    q =>
                        !q.DangBan
                        && q.SoLuong > 0
                );


            if (
                !string.IsNullOrWhiteSpace(
                    tuKhoa
                )
            )
            {
                var tk =
                    tuKhoa
                        .Trim()
                        .ToLower();

                ds =
                    ds.Where(
                        q =>
                            q.TenQua
                                .ToLower()
                                .Contains(tk)
                            || q.MaQua
                                .ToLower()
                                .Contains(tk)
                    );
            }


            return ds
                .OrderByDescending(
                    q => q.NgayTao
                )
                .ToList();
        }


        public List<QuaTang> QuaTangDangBan(
            string? tuKhoa = null)
        {
            var ds =
                DanhSachQuaTang.Where(
                    q =>
                        q.DangBan
                        && q.SoLuong > 0
                );


            if (
                !string.IsNullOrWhiteSpace(
                    tuKhoa
                )
            )
            {
                var tk =
                    tuKhoa
                        .Trim()
                        .ToLower();

                ds =
                    ds.Where(
                        q =>
                            q.TenQua
                                .ToLower()
                                .Contains(tk)
                            || q.MaQua
                                .ToLower()
                                .Contains(tk)
                    );
            }


            return ds
                .OrderByDescending(
                    q => q.NgayTao
                )
                .ToList();
        }


        public List<QuaTang> QuaTangDaHetHang(
            string? tuKhoa = null)
        {
            var ds =
                DanhSachQuaTang.Where(
                    q => q.SoLuong <= 0
                );


            if (
                !string.IsNullOrWhiteSpace(
                    tuKhoa
                )
            )
            {
                var tk =
                    tuKhoa
                        .Trim()
                        .ToLower();

                ds =
                    ds.Where(
                        q =>
                            q.TenQua
                                .ToLower()
                                .Contains(tk)
                            || q.MaQua
                                .ToLower()
                                .Contains(tk)
                    );
            }


            return ds
                .OrderByDescending(
                    q => q.NgayTao
                )
                .ToList();
        }


        public List<QuaTang> QuaTangCoTheDoi()
        {
            return DanhSachQuaTang
                .Where(
                    q =>
                        q.DangBan
                        && q.SoLuong > 0
                )
                .ToList();
        }


        public bool ChuyenTrangThaiQuaTang(
            string maQua)
        {
            var qua =
                DanhSachQuaTang.FirstOrDefault(
                    x =>
                        x.MaQua
                        == maQua
                );

            if (qua == null)
                return false;

            qua.DangBan =
                !qua.DangBan;

            LuuQuaTang();

            return qua.DangBan;
        }


        // =========================================================
        // THỐNG KÊ ĐƠN HÀNG
        // =========================================================

        public List<DonHang> LocDonHangTheoNgay(
            DateTime tuNgay,
            DateTime denNgay)
        {
            denNgay =
                denNgay.Date
                    .AddDays(1)
                    .AddTicks(-1);

            return DanhSachDonHang
                .Where(
                    d =>
                        d.NgayTao
                            >= tuNgay.Date
                        && d.NgayTao
                            <= denNgay
                )
                .ToList();
        }


        public int SoLuongDonHang(
            DateTime tuNgay,
            DateTime denNgay)
        {
            return LocDonHangTheoNgay(
                tuNgay,
                denNgay
            ).Count;
        }


        public decimal TongDoanhThu(
            DateTime tuNgay,
            DateTime denNgay)
        {
            return LocDonHangTheoNgay(
                tuNgay,
                denNgay
            ).Sum(
                d => d.ThanhTien
            );
        }


        public int TongDiemDaTichLuy(
            DateTime tuNgay,
            DateTime denNgay)
        {
            return LocDonHangTheoNgay(
                tuNgay,
                denNgay
            ).Sum(
                d => d.DiemCong
            );
        }


        public int TongDiemDaSuDung(
            DateTime tuNgay,
            DateTime denNgay)
        {
            return LocDonHangTheoNgay(
                tuNgay,
                denNgay
            ).Sum(
                d => d.DiemSuDung
            );
        }


        public List<KhachHang> TopKhachHangDiemCao(
            int soLuong = 5)
        {
            return DanhSachKhachHang
                .OrderByDescending(
                    kh => kh.DiemTichLuy
                )
                .Take(soLuong)
                .ToList();
        }


        // =========================================================
        // THỐNG KÊ TỔNG QUAN
        // =========================================================

        public int TongSoDonHang()
        {
            return DanhSachDonHang.Count;
        }


        public decimal TongDoanhThuTuThuoc()
        {
            return DanhSachDonHang
                .Sum(
                    d => d.ThanhTien
                );
        }


        public int TongSoThuocDaBan()
        {
            return DanhSachDonHang
                .Sum(
                    d =>
                        d.DanhSachThuoc?
                            .Sum(
                                ct => ct.SoLuong
                            )
                        ?? 0
                );
        }


        public int TongSoQuaDaTang()
        {
            return DanhSachDonHang
                .Count(
                    d =>
                        !string.IsNullOrEmpty(
                            d.QuaTangDoi
                        )
                );
        }


        public string QuaDuocTangNhieuNhat()
        {
            var top =
                DanhSachDonHang
                    .Where(
                        d =>
                            !string.IsNullOrEmpty(
                                d.QuaTangDoi
                            )
                    )
                    .GroupBy(
                        d => d.QuaTangDoi
                    )
                    .OrderByDescending(
                        g => g.Count()
                    )
                    .FirstOrDefault();


            return top != null
                ? $"{top.Key} ({top.Count()} lần)"
                : "-";
        }


        public int TongDiemDaCongToanBo()
        {
            return DanhSachDonHang
                .Sum(
                    d => d.DiemCong
                );
        }


        public int TongDiemDaSuDungToanBo()
        {
            return DanhSachDonHang
                .Sum(
                    d => d.DiemSuDung
                );
        }


        // =========================================================
        // GHI CHÚ
        // =========================================================

        public List<GhiChu> LayDanhSachGhiChu()
        {
            string path =
                Path.Combine(
                    _dataFolder,
                    "ghichu.json"
                );

            if (!File.Exists(path))
                return new List<GhiChu>();

            string json =
                File.ReadAllText(path);

            return JsonSerializer.Deserialize<List<GhiChu>>(
                       json
                   )
                   ?? new List<GhiChu>();
        }


        public void LuuDanhSachGhiChu(
            List<GhiChu> danhSach)
        {
            string path =
                Path.Combine(
                    _dataFolder,
                    "ghichu.json"
                );

            string json =
                JsonSerializer.Serialize(
                    danhSach,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    }
                );

            File.WriteAllText(
                path,
                json
            );
        }
        
    }
    
}