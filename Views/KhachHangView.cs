using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using QuanLyKhachHang.Helpers;
using QuanLyKhachHang.Models;
using QuanLyKhachHang.Services;

namespace QuanLyKhachHang.Views
{
    /// <summary>
    /// Màn hình Quản lý khách hàng:
    /// - Hiển thị danh sách khách hàng
    /// - Tìm kiếm tức thời theo tên / SĐT / mã KH
    /// - Thêm / Sửa / Xóa khách hàng
    /// - Sau khi thêm, sửa, xóa dữ liệu sẽ cập nhật ngay trên màn hình
    /// </summary>
    public class KhachHangView : UserControl
    {
        private readonly DataService _data;

        private readonly TextBox _txtTimKiem = new()
        {
            Width = 320,
            Watermark = "Nhập để tìm..."
        };

        private readonly ListBox _listBox = new();

        private KhachHang? _dangChon;

        public KhachHangView(DataService data)
        {
            _data = data;

            var goc = new StackPanel
            {
                Spacing = 12
            };

            goc.Children.Add(new TextBlock
            {
                Text = "Quản lý khách hàng",
                FontSize = 20,
                FontWeight = FontWeight.Bold
            });

            goc.Children.Add(new TextBlock
            {
                Text = "🔍 Tìm kiếm (theo tên / SĐT / mã KH):",
                FontSize = 13
            });

            var hangTimKiem = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10
            };

            // ================= TÌM KIẾM =================

            _txtTimKiem.TextChanged += (s, e) =>
            {
                TaiLaiDuLieu();
            };

            // ================= NÚT =================

            var btnThem = TaoNut("➕ Thêm", "#5B8EF2");
            var btnSua = TaoNut("✏️ Sửa", "#F0B84D");
            var btnXoa = TaoNut("🗑️ Xoá", "#E4574C");

            btnThem.Click += BtnThem_Click;
            btnSua.Click += BtnSua_Click;
            btnXoa.Click += BtnXoa_Click;

            hangTimKiem.Children.Add(_txtTimKiem);
            hangTimKiem.Children.Add(btnThem);
            hangTimKiem.Children.Add(btnSua);
            hangTimKiem.Children.Add(btnXoa);

            goc.Children.Add(hangTimKiem);

            // ================= CHỌN KHÁCH HÀNG =================

            _listBox.SelectionChanged += (s, e) =>
            {
                _dangChon = _listBox.SelectedItem as KhachHang;
            };

            // Nhấn đúp vào khách hàng để sửa nhanh
            _listBox.DoubleTapped += ListBox_DoubleTapped;

            // ================= BẢNG KHÁCH HÀNG =================

            var khungBang = new Border
            {
                Background = Brushes.White,
                Height = 480,
                ClipToBounds = true
            };

            khungBang.Child = UiHelpers.TaoBang<KhachHang>(
                new List<KhachHang>(),
                new List<ColDef<KhachHang>>
                {
                    new(
                        "Mã KH",
                        0.8,
                        kh => kh.MaKH
                    ),

                    new(
                        "Họ tên",
                        2,
                        kh => kh.HoTen
                    ),

                    new(
                        "Số điện thoại",
                        1.3,
                        kh => kh.SoDienThoai
                    ),

                    new(
                        "Điểm tích luỹ",
                        1,
                        kh => kh.DiemTichLuy.ToString()
                    ),

                    new(
                        "Ngày tạo",
                        1.2,
                        kh => kh.NgayTao.ToString("dd/MM/yyyy")
                    )
                },
                _listBox
            );

            goc.Children.Add(khungBang);

            Content = goc;

            // Nạp dữ liệu lần đầu
            TaiLaiDuLieu();
        }

        // ============================================================
        // TẠO NÚT
        // ============================================================

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

        // ============================================================
        // TẢI LẠI DỮ LIỆU
        // ============================================================

        private void TaiLaiDuLieu()
        {
            string tuKhoa = _txtTimKiem.Text?.Trim() ?? string.Empty;

            // Lấy danh sách khách hàng mới
            var ketQua = _data
                .TimKiemKhachHang(tuKhoa)
                .ToList();

            // Quan trọng:
            // Xóa ItemsSource cũ trước khi gán dữ liệu mới.
            // Việc này giúp ListBox render lại ngay.
            _listBox.ItemsSource = null;

            // Gán danh sách mới
            _listBox.ItemsSource = ketQua;

            // Xóa lựa chọn cũ
            _listBox.SelectedItem = null;
            _dangChon = null;
        }

        // ============================================================
        // THÊM KHÁCH HÀNG
        // ============================================================

        private async void BtnThem_Click(
            object? sender,
            Avalonia.Interactivity.RoutedEventArgs e)
        {
            var cuaSo = new KhachHangEditWindow(
                _data.TaoMaKhachHangMoi()
            );

            if (TopLevel.GetTopLevel(this) is Window parentWindow)
            {
                await cuaSo.ShowDialog(parentWindow);
            }
            else
            {
                cuaSo.Show();
            }

            // Nếu người dùng bấm Lưu
            if (cuaSo.KetQua != null)
            {
                _data.ThemKhachHang(cuaSo.KetQua);

                // Cập nhật bảng NGAY LẬP TỨC
                TaiLaiDuLieu();
            }
        }

        // ============================================================
        // SỬA KHÁCH HÀNG
        // ============================================================

        private async void BtnSua_Click(
            object? sender,
            Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_dangChon == null)
            {
                await ThongBaoWindow.ThongBao(
                    TopLevel.GetTopLevel(this) as Window,
                    "Thông báo",
                    "Vui lòng chọn 1 khách hàng cần sửa."
                );

                return;
            }

            var cuaSo = new KhachHangEditWindow(
                _dangChon.MaKH,
                _dangChon
            );

            if (TopLevel.GetTopLevel(this) is Window parentWindow)
            {
                await cuaSo.ShowDialog(parentWindow);
            }
            else
            {
                cuaSo.Show();
            }

            // Nếu người dùng bấm Lưu
            if (cuaSo.KetQua != null)
            {
                _data.SuaKhachHang(cuaSo.KetQua);

                // Cập nhật bảng NGAY LẬP TỨC
                TaiLaiDuLieu();
            }
        }

        // ============================================================
        // XÓA KHÁCH HÀNG
        // ============================================================

        private async void BtnXoa_Click(
            object? sender,
            Avalonia.Interactivity.RoutedEventArgs e)
        {
            var cuaSoCha =
                TopLevel.GetTopLevel(this) as Window;

            if (_dangChon == null)
            {
                await ThongBaoWindow.ThongBao(
                    cuaSoCha,
                    "Thông báo",
                    "Vui lòng chọn 1 khách hàng cần xoá."
                );

                return;
            }

            bool dongY = await ThongBaoWindow.XacNhan(
                cuaSoCha,
                "Xác nhận xoá",
                $"Bạn có chắc muốn xoá khách hàng\n'{_dangChon.HoTen}'?"
            );

            if (!dongY)
                return;

            // Lưu mã khách hàng trước khi xóa
            string maKH = _dangChon.MaKH;

            // Xóa trong DataService
            _data.XoaKhachHang(maKH);

            // Cập nhật bảng NGAY LẬP TỨC
            TaiLaiDuLieu();
        }

        // ============================================================
        // NHẤN ĐÚP - SỬA NHANH
        // ============================================================

        private async void ListBox_DoubleTapped(
            object? sender,
            TappedEventArgs e)
        {
            if (_dangChon == null)
                return;

            var container =
                _listBox.ContainerFromItem(_dangChon);

            if (container == null)
                return;

            var editPopup = new Window
            {
                Title = "Sửa nhanh khách hàng",
                Width = 350,
                Height = 160,
                WindowStartupLocation =
                    WindowStartupLocation.CenterOwner,
                CanResize = false
            };

            var panel = new StackPanel
            {
                Spacing = 10,
                Margin = new Thickness(15)
            };

            var txtHoTen = new TextBox
            {
                Text = _dangChon.HoTen,
                Watermark = "Họ tên"
            };

            var txtSdt = new TextBox
            {
                Text = _dangChon.SoDienThoai,
                Watermark = "Số điện thoại"
            };

            var btnLuu = new Button
            {
                Content = "💾 Lưu (Enter)",
                Background =
                    new SolidColorBrush(
                        Color.Parse("#4CAF7D")
                    ),
                Foreground = Brushes.White,
                HorizontalAlignment =
                    HorizontalAlignment.Right
            };

            panel.Children.Add(
                new TextBlock
                {
                    Text = "Chỉnh sửa thông tin nhanh:",
                    FontWeight = FontWeight.Bold
                }
            );

            panel.Children.Add(txtHoTen);
            panel.Children.Add(txtSdt);
            panel.Children.Add(btnLuu);

            editPopup.Content = panel;

            // ========================================================
            // HÀM LƯU SỬA NHANH
            // ========================================================

            async void LuuVaDong()
            {
                var ten =
                    txtHoTen.Text?.Trim() ?? "";

                var sdt =
                    txtSdt.Text?.Trim() ?? "";

                // Kiểm tra họ tên
                if (string.IsNullOrWhiteSpace(ten))
                {
                    await ThongBaoWindow.ThongBao(
                        editPopup,
                        "Thiếu thông tin",
                        "Vui lòng nhập họ tên."
                    );

                    return;
                }

                // Kiểm tra số điện thoại
                if (
                    sdt.Length != 10 ||
                    !sdt.All(char.IsDigit) ||
                    !sdt.StartsWith("0")
                )
                {
                    await ThongBaoWindow.ThongBao(
                        editPopup,
                        "Số điện thoại không hợp lệ",
                        "Số điện thoại phải gồm đúng 10 chữ số và bắt đầu bằng số 0."
                    );

                    return;
                }

                // Cập nhật đối tượng đang chọn
                _dangChon.HoTen = ten;
                _dangChon.SoDienThoai = sdt;

                // Lưu xuống DataService
                _data.SuaKhachHang(_dangChon);

                // Cập nhật bảng NGAY LẬP TỨC
                TaiLaiDuLieu();

                // Đóng cửa sổ
                editPopup.Close();
            }

            btnLuu.Click += (s, ev) =>
            {
                LuuVaDong();
            };

            txtHoTen.KeyDown += (s, ev) =>
            {
                if (ev.Key == Key.Enter)
                    LuuVaDong();
            };

            txtSdt.KeyDown += (s, ev) =>
            {
                if (ev.Key == Key.Enter)
                    LuuVaDong();
            };

            // ========================================================
            // HIỂN THỊ CỬA SỔ
            // ========================================================

            if (TopLevel.GetTopLevel(this) is Window parentWindow)
            {
                await editPopup.ShowDialog(parentWindow);
            }
            else
            {
                editPopup.Show();
            }
        }
    }
}