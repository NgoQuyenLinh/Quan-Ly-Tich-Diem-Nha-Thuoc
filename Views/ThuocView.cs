using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using QuanLyKhachHang.Helpers;
using QuanLyKhachHang.Models;
using QuanLyKhachHang.Services;

namespace QuanLyKhachHang.Views
{
    /// <summary>
    /// Màn hình Quản lý thuốc: bảng danh sách, ô tìm kiếm tức thời (theo tên / mã thuốc)
    /// và 3 nút Thêm / Sửa / Xoá — giao diện và cách hoạt động tương tự màn hình Kho quà.
    /// Dữ liệu thuốc ở đây được dùng trực tiếp trong màn hình Đơn hàng để chọn thuốc bán cho khách.
    /// </summary>
    public class ThuocView : UserControl
    {
        private readonly DataService _data;
        private readonly TextBox _txtTimKiem = new() { Width = 320, Watermark = "Nhập để tìm..." };
        private readonly ListBox _listBox = new();

        // Menu chuột phải trên 1 dòng thuốc: chọn "Sửa" trực tiếp không cần bấm nút Sửa ở trên.
        private readonly MenuItem _menuSua = new() { Header = "✏️ Sửa thuốc này" };
        private readonly MenuItem _menuXoa = new() { Header = "🗑️ Xoá thuốc này" };

        private readonly TextBlock _lblCanhBao = new()
        {
            FontSize = 13,
            FontWeight = FontWeight.SemiBold,
            Foreground = new SolidColorBrush(Color.Parse("#E4574C")),
            IsVisible = false
        };

        private Thuoc? _dangChon;

        private static string TrangThaiText(Thuoc t) =>
            t.SoLuongTon <= 0 ? "🔴 Hết hàng"
            : t.SoLuongTon <= t.NguongCanhBao ? "🟡 Tồn kho thấp"
            : t.ConHang ? "🟢 Còn hàng" : "⚪ Ngừng bán";

        private string HsdGanNhatText(Thuoc t)
        {
            var lo = _data.LoTheoMaThuoc(t.MaThuoc).FirstOrDefault(l => !l.DaHetHan && l.SoLuong > 0);
            return lo == null ? "-" : lo.HanSuDung.ToString("dd/MM/yyyy");
        }

        public ThuocView(DataService data)
        {
            _data = data;

            var goc = new StackPanel { Spacing = 12 };

            goc.Children.Add(new TextBlock
            {
                Text = "Quản lý thuốc",
                FontSize = 20,
                FontWeight = FontWeight.Bold
            });

            goc.Children.Add(new TextBlock { Text = "🔍 Tìm kiếm (theo tên / mã thuốc):", FontSize = 13 });

            var hangTimKiem = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
            _txtTimKiem.TextChanged += (s, e) => TaiLaiDuLieu(); // tìm kiếm tức thời

            var btnThem = TaoNut("➕ Thêm", "#5B8EF2");
            var btnSua = TaoNut("✏️ Sửa", "#F0B84D");
            var btnXoa = TaoNut("🗑️ Xoá", "#E4574C");
            var btnLo = TaoNut("📦 Lô hàng", "#4CAF7D");
            btnLo.Width = 120;
            btnThem.Click += BtnThem_Click;
            btnSua.Click += BtnSua_Click;
            btnXoa.Click += BtnXoa_Click;
            btnLo.Click += BtnLo_Click;

            hangTimKiem.Children.Add(_txtTimKiem);
            hangTimKiem.Children.Add(btnThem);
            hangTimKiem.Children.Add(btnSua);
            hangTimKiem.Children.Add(btnXoa);
            hangTimKiem.Children.Add(btnLo);
            goc.Children.Add(hangTimKiem);

            goc.Children.Add(_lblCanhBao);

            _listBox.SelectionChanged += (s, e) => _dangChon = _listBox.SelectedItem as Thuoc;
            _listBox.DoubleTapped += (s, e) => { if (_dangChon != null) _ = HienThiPopup(_dangChon, isMoi: false); };

            // ---- Chuột phải trên 1 dòng: tự động chọn dòng đó rồi mở menu Sửa/Xoá ----
            _menuSua.Click += BtnSua_Click;
            _menuXoa.Click += BtnXoa_Click;
            _listBox.ContextMenu = new ContextMenu { ItemsSource = new List<MenuItem> { _menuSua, _menuXoa } };
            _listBox.AddHandler(InputElement.PointerPressedEvent, ListBox_PointerPressed, Avalonia.Interactivity.RoutingStrategies.Tunnel);

            var khungBang = new Border { Background = Brushes.White, Height = 480, ClipToBounds = true };
            khungBang.Child = UiHelpers.TaoBang<Thuoc>(
                new List<Thuoc>(),
                new List<ColDef<Thuoc>>
                {
                    new("Mã thuốc", 0.8, t => t.MaThuoc),
                    new("Tên thuốc", 2, t => t.TenThuoc),
                    new("Loại thuốc", 1.5, t => string.IsNullOrEmpty(t.LoaiThuoc) ? "Chưa phân loại" : t.LoaiThuoc),
                    new("Đơn giá", 1.1, t => t.DonGia.ToString("N0") + "đ"),
                    new("Tồn kho", 0.8, t => t.SoLuongTon.ToString("N0")),
                    new("HSD gần nhất", 1, t => HsdGanNhatText(t)),
                    new("Tình trạng hàng", 1.2, t => TrangThaiText(t))
                },
                _listBox);
            goc.Children.Add(khungBang);

            Content = goc;
            TaiLaiDuLieu();
        }

        private Button TaoNut(string text, string maMau)
        {
            return new Button
            {
                Content = text,
                Width = 100,
                Background = new SolidColorBrush(Color.Parse(maMau)),
                Foreground = Brushes.White
            };
        }

        /// <summary>
        /// Khi người dùng bấm chuột phải, tự động trỏ (chọn) đúng dòng thuốc nằm dưới
        /// con trỏ chuột trước khi hiện menu Sửa/Xoá.
        /// </summary>
        private void ListBox_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (!e.GetCurrentPoint(_listBox).Properties.IsRightButtonPressed) return;

            if (e.Source is Visual v)
            {
                var dong = v.FindAncestorOfType<ListBoxItem>();
                if (dong?.DataContext is Thuoc t)
                {
                    _listBox.SelectedItem = t;
                    _dangChon = t;
                }
            }
        }

        private void TaiLaiDuLieu()
        {
            _listBox.ItemsSource = _data.TimKiemThuoc(_txtTimKiem.Text ?? string.Empty);
            CapNhatCanhBao();
        }

        private void CapNhatCanhBao()
        {
            int soHetHang = _data.ThuocHetHang().Count;
            int soTonKhoThap = _data.ThuocTonKhoThap().Count;
            int soLoSapHetHan = _data.LoSapHetHan().Count;
            int soLoDaHetHan = _data.LoDaHetHan().Count;

            var phan = new List<string>();
            if (soHetHang > 0) phan.Add($"🔴 {soHetHang} thuốc hết hàng");
            if (soTonKhoThap > 0) phan.Add($"🟡 {soTonKhoThap} thuốc tồn kho thấp");
            if (soLoSapHetHan > 0) phan.Add($"⏳ {soLoSapHetHan} lô sắp hết hạn (30 ngày)");
            if (soLoDaHetHan > 0) phan.Add($"⛔ {soLoDaHetHan} lô đã hết hạn còn trong kho");

            if (phan.Count == 0)
            {
                _lblCanhBao.IsVisible = false;
                return;
            }

            _lblCanhBao.Text = "⚠️ Cảnh báo: " + string.Join("   •   ", phan);
            _lblCanhBao.IsVisible = true;
        }

        private async void BtnLo_Click(object? sender, RoutedEventArgs e)
        {
            if (_dangChon == null)
            {
                await ThongBaoWindow.ThongBao(TopLevel.GetTopLevel(this) as Window, "Thông báo", "Vui lòng chọn 1 thuốc để xem/quản lý lô hàng.");
                return;
            }

            var cuaSoCha = TopLevel.GetTopLevel(this) as Window;
            var cuaSoLo = new LoThuocWindow(_data, _dangChon);
            if (cuaSoCha != null) await cuaSoLo.ShowDialog(cuaSoCha);
            else cuaSoLo.Show();

            TaiLaiDuLieu();
        }

        private async void BtnThem_Click(object? sender, RoutedEventArgs e)
        {
            await HienThiPopup(new Thuoc { MaThuoc = _data.TaoMaThuocMoi() }, isMoi: true);
        }

        private async void BtnSua_Click(object? sender, RoutedEventArgs e)
        {
            if (_dangChon == null)
            {
                await ThongBaoWindow.ThongBao(TopLevel.GetTopLevel(this) as Window, "Thông báo", "Vui lòng chọn 1 thuốc cần sửa.");
                return;
            }
            await HienThiPopup(_dangChon, isMoi: false);
        }

        private async void BtnXoa_Click(object? sender, RoutedEventArgs e)
        {
            var cuaSoCha = TopLevel.GetTopLevel(this) as Window;

            if (_dangChon == null)
            {
                await ThongBaoWindow.ThongBao(cuaSoCha, "Thông báo", "Vui lòng chọn 1 thuốc cần xoá.");
                return;
            }

            bool dongY = await ThongBaoWindow.XacNhan(cuaSoCha, "Xác nhận xoá", $"Bạn có chắc muốn xoá thuốc \n'{_dangChon.TenThuoc}'?");
            if (dongY)
            {
                _data.XoaThuoc(_dangChon.MaThuoc);
                _dangChon = null;
                TaiLaiDuLieu();
            }
        }
        private async System.Threading.Tasks.Task HienThiPopup(Thuoc thuoc, bool isMoi)
        {
            var popup = new Window
            {
                Title = isMoi ? "Thêm Thuốc" : "Sửa Thuốc",
                Width = 380,
                Height = isMoi ? 420 : 460, // Tăng chiều cao để đủ chỗ cho ComboBox + tồn kho
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false
            };

            var panel = new StackPanel { Spacing = 10, Margin = new Thickness(15) };

            var txtTenThuoc = new TextBox { Text = thuoc.TenThuoc, Watermark = "Tên thuốc" };

            // ComboBox chọn Loại thuốc
            var cboLoaiThuoc = new ComboBox
            {
                ItemsSource = new[] { "Thuốc kê đơn", "Thuốc không kê đơn", "Thực phẩm chức năng", "Thiết bị y tế" },
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            // Mặc định chọn Loại thuốc hiện tại nếu đang Sửa
            if (!string.IsNullOrEmpty(thuoc.LoaiThuoc))
            {
                cboLoaiThuoc.SelectedItem = thuoc.LoaiThuoc;
            }
            else
            {
                cboLoaiThuoc.SelectedIndex = 1; // Mặc định là "Thuốc không kê đơn"
            }

            var numDonGia = new NumericUpDown { Value = thuoc.DonGia, Minimum = 0, Increment = 1000, FormatString = "N0" };
            var chkConHang = new CheckBox { Content = "Cho phép bán (còn hàng)", IsChecked = thuoc.ConHang };
            var numNguong = new NumericUpDown { Value = thuoc.NguongCanhBao, Minimum = 0, Maximum = 100000, FormatString = "0" };

            var btnLuu = new Button
            {
                Content = "💾 Lưu",
                Background = new SolidColorBrush(Color.Parse("#4CAF7D")),
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            panel.Children.Add(new TextBlock { Text = "Tên thuốc:" });
            panel.Children.Add(txtTenThuoc);
            panel.Children.Add(new TextBlock { Text = "Loại thuốc:" });
            panel.Children.Add(cboLoaiThuoc); // <--- THÊM COMBOBOX VÀO GIAO DIỆN POPUP
            panel.Children.Add(new TextBlock { Text = "Đơn giá (đ):" });
            panel.Children.Add(numDonGia);
            panel.Children.Add(new TextBlock { Text = "Ngưỡng cảnh báo tồn kho thấp:" });
            panel.Children.Add(numNguong);
            panel.Children.Add(chkConHang);

            if (!isMoi)
            {
                panel.Children.Add(new TextBlock
                {
                    Text = $"Tồn kho hiện tại: {thuoc.SoLuongTon:N0} (quản lý qua nút \"📦 Lô hàng\")",
                    FontSize = 11,
                    Foreground = Brushes.Gray
                });
            }

            panel.Children.Add(btnLuu);

            popup.Content = panel;

            btnLuu.Click += (s, ev) =>
            {
                if (string.IsNullOrWhiteSpace(txtTenThuoc.Text)) return;

                thuoc.TenThuoc = txtTenThuoc.Text;
                thuoc.LoaiThuoc = cboLoaiThuoc.SelectedItem?.ToString() ?? "Thuốc không kê đơn"; // <--- LƯU LOẠI THUỐC
                thuoc.DonGia = numDonGia.Value ?? 0;
                thuoc.ConHang = chkConHang.IsChecked ?? false;
                thuoc.NguongCanhBao = (int)(numNguong.Value ?? 10);

                if (isMoi) _data.ThemThuoc(thuoc);
                else _data.SuaThuoc(thuoc);

                TaiLaiDuLieu();
                popup.Close();
            };

            if (TopLevel.GetTopLevel(this) is Window parentWindow)
            {
                await popup.ShowDialog(parentWindow);
            }
            else
            {
                popup.Show();
            }
        }
    }
}