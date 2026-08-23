using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using QuanLyKhachHang.Helpers;
using QuanLyKhachHang.Models;
using QuanLyKhachHang.Services;

namespace QuanLyKhachHang.Views
{
    /// <summary>
    /// Cửa sổ quản lý lô hàng (số lô + hạn sử dụng + số lượng) của 1 thuốc cụ thể.
    /// Tồn kho (Thuoc.SoLuongTon) được DataService tự động tính lại từ tổng các lô
    /// còn hàng & chưa hết hạn mỗi khi thêm/sửa/xoá lô ở đây.
    /// Các dòng lô sắp hết hạn (trong 30 ngày) tô vàng, đã hết hạn tô đỏ.
    /// </summary>
    public class LoThuocWindow : Window
    {
        private readonly DataService _data;
        private readonly Thuoc _thuoc;
        private readonly ListBox _listBox = new();
        private readonly TextBlock _txtTonKho = new() { FontWeight = FontWeight.Bold, FontSize = 14 };
        private LoThuoc? _dangChon;

        public LoThuocWindow(DataService data, Thuoc thuoc)
        {
            _data = data;
            _thuoc = thuoc;

            Title = $"Lô hàng - {thuoc.TenThuoc}";
            Width = 640;
            Height = 520;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            CanResize = true;

            var goc = new StackPanel { Spacing = 10, Margin = new Thickness(16) };

            goc.Children.Add(new TextBlock { Text = $"📦 Quản lý lô hàng: {thuoc.TenThuoc}", FontSize = 18, FontWeight = FontWeight.Bold });
            goc.Children.Add(_txtTonKho);

            var hang = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
            var btnThem = new Button { Content = "➕ Nhập lô mới", Background = new SolidColorBrush(Color.Parse("#5B8EF2")), Foreground = Brushes.White, Width = 130 };
            var btnSua = new Button { Content = "✏️ Sửa lô", Background = new SolidColorBrush(Color.Parse("#F0B84D")), Foreground = Brushes.White, Width = 110 };
            var btnXoa = new Button { Content = "🗑️ Xoá lô", Background = new SolidColorBrush(Color.Parse("#E4574C")), Foreground = Brushes.White, Width = 110 };
            btnThem.Click += (_, _) => _ = MoPopupLo(new LoThuoc { MaLo = _data.TaoMaLoMoi(), MaThuoc = _thuoc.MaThuoc, HanSuDung = DateTime.Now.AddMonths(6) }, true);
            btnSua.Click += (_, _) =>
            {
                if (_dangChon == null) { _ = ThongBaoWindow.ThongBao(this, "Thông báo", "Vui lòng chọn 1 lô cần sửa."); return; }
                _ = MoPopupLo(_dangChon, false);
            };
            btnXoa.Click += async (_, _) =>
            {
                if (_dangChon == null) { await ThongBaoWindow.ThongBao(this, "Thông báo", "Vui lòng chọn 1 lô cần xoá."); return; }
                bool dongY = await ThongBaoWindow.XacNhan(this, "Xác nhận xoá", $"Bạn có chắc muốn xoá lô '{_dangChon.SoLo}'?");
                if (dongY)
                {
                    _data.XoaLoThuoc(_dangChon.MaLo);
                    _dangChon = null;
                    TaiLai();
                }
            };
            hang.Children.Add(btnThem);
            hang.Children.Add(btnSua);
            hang.Children.Add(btnXoa);
            goc.Children.Add(hang);

            var khungBang = new Border { Background = Brushes.White, Height = 340, ClipToBounds = true };
            khungBang.Child = UiHelpers.TaoBang<LoThuoc>(
                new List<LoThuoc>(),
                new List<ColDef<LoThuoc>>
                {
                    new("Số lô", 1, l => l.SoLo),
                    new("Hạn sử dụng", 1.1, l => l.HanSuDung.ToString("dd/MM/yyyy")),
                    new("Số lượng", 0.8, l => l.SoLuong.ToString()),
                    new("Ngày nhập", 1.1, l => l.NgayNhap.ToString("dd/MM/yyyy")),
                    new("Trạng thái", 1.3, l => TrangThaiLo(l))
                },
                _listBox);
            _listBox.SelectionChanged += (_, _) => _dangChon = _listBox.SelectedItem as LoThuoc;
            goc.Children.Add(khungBang);

            goc.Children.Add(new TextBlock
            {
                Text = "🟡 Vàng: sắp hết hạn trong 30 ngày   🔴 Đỏ: đã hết hạn (không tính vào tồn kho có thể bán)",
                FontSize = 11,
                Foreground = Brushes.Gray
            });

            Content = goc;
            TaiLai();
        }

        private static string TrangThaiLo(LoThuoc l)
        {
            if (l.DaHetHan) return "🔴 Đã hết hạn";
            if (l.SoNgayConLai <= 30) return $"🟡 Còn {l.SoNgayConLai} ngày";
            return "🟢 Bình thường";
        }

        private void TaiLai()
        {
            var ds = _data.LoTheoMaThuoc(_thuoc.MaThuoc);
            _listBox.ItemsSource = null;
            _listBox.ItemsSource = ds;

            var thuocMoi = _data.DanhSachThuoc.FirstOrDefault(t => t.MaThuoc == _thuoc.MaThuoc);
            int tonKho = thuocMoi?.SoLuongTon ?? 0;
            _txtTonKho.Text = $"Tồn kho hiện tại: {tonKho:N0}"
                + (tonKho <= (thuocMoi?.NguongCanhBao ?? 0) ? "  ⚠️ Tồn kho thấp!" : "");
            _txtTonKho.Foreground = tonKho <= (thuocMoi?.NguongCanhBao ?? 0)
                ? new SolidColorBrush(Color.Parse("#E4574C"))
                : new SolidColorBrush(Color.Parse("#3B9463"));
        }

        private async System.Threading.Tasks.Task MoPopupLo(LoThuoc lo, bool isMoi)
        {
            var popup = new Window
            {
                Title = isMoi ? "Nhập lô mới" : "Sửa lô hàng",
                Width = 360,
                Height = 300,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false
            };

            var panel = new StackPanel { Spacing = 8, Margin = new Thickness(15) };

            var txtSoLo = new TextBox { Text = lo.SoLo, Watermark = "Số lô (VD: SL0125)" };
            var dpHsd = new DatePicker { SelectedDate = new DateTimeOffset(lo.HanSuDung.Date) };
            var numSoLuong = new NumericUpDown { Value = lo.SoLuong, Minimum = 0, Maximum = 1000000, FormatString = "0" };

            var btnLuu = new Button
            {
                Content = "💾 Lưu",
                Background = new SolidColorBrush(Color.Parse("#4CAF7D")),
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            panel.Children.Add(new TextBlock { Text = "Số lô:" });
            panel.Children.Add(txtSoLo);
            panel.Children.Add(new TextBlock { Text = "Hạn sử dụng:" });
            panel.Children.Add(dpHsd);
            panel.Children.Add(new TextBlock { Text = "Số lượng nhập:" });
            panel.Children.Add(numSoLuong);
            panel.Children.Add(btnLuu);

            popup.Content = panel;

            btnLuu.Click += (_, _) =>
            {
                if (string.IsNullOrWhiteSpace(txtSoLo.Text)) return;

                lo.SoLo = txtSoLo.Text!;
                lo.HanSuDung = dpHsd.SelectedDate?.Date ?? DateTime.Now.Date;
                lo.SoLuong = (int)(numSoLuong.Value ?? 0);

                if (isMoi) _data.ThemLoThuoc(lo);
                else _data.SuaLoThuoc(lo);

                TaiLai();
                popup.Close();
            };

            await popup.ShowDialog(this);
        }
    }
}
