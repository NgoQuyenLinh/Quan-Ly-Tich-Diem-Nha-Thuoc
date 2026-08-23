using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using QuanLyKhachHang.Helpers;
using QuanLyKhachHang.Models;
using QuanLyKhachHang.Services;

namespace QuanLyKhachHang.Views
{
    /// <summary>
    /// Màn hình Thống kê:
    ///  - Phần trên: Top quà đã phát / tặng nhiều nhất theo Ngày / Tháng / Năm.
    ///  - Phần dưới: Lịch sử mua hàng của tất cả khách hàng, có sắp xếp và xóa đơn.
    /// </summary>
    public class ThongKeView : UserControl
    {
        private readonly DataService _data;

        // ---- Top quà ----
        private readonly RadioButton _rbNgay = new()
        {
            Content = "Ngày",
            GroupName = "kyThongKeQua",
            IsChecked = true
        };

        private readonly RadioButton _rbThang = new()
        {
            Content = "Tháng",
            GroupName = "kyThongKeQua"
        };

        private readonly RadioButton _rbNam = new()
        {
            Content = "Năm",
            GroupName = "kyThongKeQua"
        };

        private readonly ListBox _listBoxTopQua = new();

        // ---- Lịch sử mua hàng ----
        // Dùng cùng kiểu lựa chọn kỳ thống kê như phần Top quà: Ngày / Tháng / Năm.
        private readonly RadioButton _rbLichSuNgay = new()
        {
            Content = "Ngày",
            GroupName = "kyLichSuMuaHang",
            IsChecked = true
        };

        private readonly RadioButton _rbLichSuThang = new()
        {
            Content = "Tháng",
            GroupName = "kyLichSuMuaHang"
        };

        private readonly RadioButton _rbLichSuNam = new()
        {
            Content = "Năm",
            GroupName = "kyLichSuMuaHang"
        };

        private readonly ListBox _listBoxDon = new();
        private readonly TextBlock _lblTrong = new()
        {
            Text = "Chưa có đơn hàng",
            FontSize = 14,
            Foreground = Brushes.Gray,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            IsVisible = false
        };

        private DonHang? _donDangChon;

        private record QuaXepHang(
            int Hang,
            string MaQua,
            string TenQua,
            int SoLanTang,
            int TongDiemDoi);

        public ThongKeView(DataService data)
        {
            _data = data;
            Background = new SolidColorBrush(Color.Parse("#F5F8FC"));

            var content = new StackPanel
            {
                Margin = new Thickness(18),
                Spacing = 14
            };

            content.Children.Add(new TextBlock
            {
                Text = "Thống kê",
                FontSize = 26,
                FontWeight = FontWeight.Bold,
                Foreground = new SolidColorBrush(Color.Parse("#33415C"))
            });

            // ================= TOP QUÀ =================
            var topCard = Card(TaoHeaderIcon("docs/imagess/gift_icon.png", "Top quà đã phát / tặng nhiều nhất"));
            var topStack = new StackPanel { Spacing = 10 };

            // Dùng chung cách tạo hàng RadioButton cho các bộ lọc/sắp xếp.
            topStack.Children.Add(TaoHangRadio("Trong:", _rbNgay, _rbThang, _rbNam));

            _rbNgay.IsCheckedChanged += (_, _) => CapNhatTopQua();
            _rbThang.IsCheckedChanged += (_, _) => CapNhatTopQua();
            _rbNam.IsCheckedChanged += (_, _) => CapNhatTopQua();

            var khungBangQua = new Border
            {
                Height = 260,
                BorderBrush = new SolidColorBrush(Color.Parse("#E8ECF3")),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Child = UiHelpers.TaoBang<QuaXepHang>(
                    new List<QuaXepHang>(),
                    new List<ColDef<QuaXepHang>>
                    {
                        new("Hạng", 0.5, x => x.Hang.ToString()),
                        new("Mã quà", 0.8, x => x.MaQua),
                        new("Tên quà", 1.8, x => x.TenQua),
                        new("Số lần tặng", 1, x => x.SoLanTang.ToString()),
                        new("Tổng điểm đổi", 1, x => x.TongDiemDoi.ToString())
                    },
                    _listBoxTopQua)
            };
            topStack.Children.Add(khungBangQua);
            ((StackPanel)topCard.Child!).Children.Add(topStack);
            content.Children.Add(topCard);

            // ================= LỊCH SỬ MUA HÀNG CỦA TẤT CẢ KHÁCH HÀNG =================
            var historyCard = Card("🕘  Lịch sử mua hàng của tất cả khách hàng");
            var historyStack = new StackPanel { Spacing = 10 };

            var hangDieuKhien = new DockPanel
            {
                Margin = new Thickness(0, 0, 0, 4)
            };

            // Lịch sử mua hàng dùng chính hàm tạo hàng RadioButton của Top quà.
            // Chỉ có 3 lựa chọn kỳ: Ngày / Tháng / Năm.
            var leftControls = TaoHangRadio(
                "Trong:",
                _rbLichSuNgay,
                _rbLichSuThang,
                _rbLichSuNam);

            _rbLichSuNgay.IsCheckedChanged += (_, _) => TaiLaiDuLieuDon();
            _rbLichSuThang.IsCheckedChanged += (_, _) => TaiLaiDuLieuDon();
            _rbLichSuNam.IsCheckedChanged += (_, _) => TaiLaiDuLieuDon();

            // Nút Xóa đơn đã chọn có kèm icon png bên trái
            var stackXoa = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 6,
                VerticalAlignment = VerticalAlignment.Center
            };
            var bmpTrash = TaoBitmap("docs/imagess/trash_icon.png");
            if (bmpTrash != null)
            {
                stackXoa.Children.Add(new Image
                {
                    Source = bmpTrash,
                    Width = 16,
                    Height = 16,
                    VerticalAlignment = VerticalAlignment.Center
                });
            }
            stackXoa.Children.Add(new TextBlock
            {
                Text = "Xóa đơn đã chọn",
                Foreground = Brushes.White,
                FontWeight = FontWeight.SemiBold,
                VerticalAlignment = VerticalAlignment.Center
            });

            var btnXoaDon = new Button
            {
                Content = stackXoa,
                Background = new SolidColorBrush(Color.Parse("#E4574C")),
                Padding = new Thickness(12, 6),
                CornerRadius = new CornerRadius(6),
                Cursor = new Cursor(StandardCursorType.Hand),
                HorizontalAlignment = HorizontalAlignment.Right
            };
            btnXoaDon.Click += BtnXoaDon_Click;

            DockPanel.SetDock(btnXoaDon, Dock.Right);
            hangDieuKhien.Children.Add(btnXoaDon);
            hangDieuKhien.Children.Add(leftControls);
            historyStack.Children.Add(hangDieuKhien);

            _listBoxDon.SelectionChanged += (_, _) =>
                _donDangChon = _listBoxDon.SelectedItem as DonHang;

            // Nhấn đúp chuột vào đơn hàng để xem chi tiết hoá đơn
            _listBoxDon.DoubleTapped += ListBoxDon_DoubleTapped;

            var gridBang = new Grid();
            var bang = UiHelpers.TaoBang<DonHang>(
                new List<DonHang>(),
                new List<ColDef<DonHang>>
                {
                    new("Mã đơn", 0.8, d => d.MaDon),
                    new("Khách hàng", 1.4, d => d.TenKH),
                    new("Tổng tiền", 1.0, d => $"{d.SoTien:N0} đ"),
                    new("Điểm cộng", 0.8, d => d.DiemCong.ToString()),
                    new("Quà đã đổi", 1.1, d => string.IsNullOrEmpty(d.QuaTangDoi) ? "-" : d.QuaTangDoi),
                    new("Điểm dùng", 0.8, d => d.DiemSuDung.ToString()),
                    new("Ghi chú", 1.2, d => string.IsNullOrEmpty(d.GhiChu) ? "-" : d.GhiChu),
                    new("Ngày tạo", 1.2, d => d.NgayTao.ToString("dd/MM/yyyy HH:mm"))
                },
                _listBoxDon);

            gridBang.Children.Add(bang);
            gridBang.Children.Add(_lblTrong);

            historyStack.Children.Add(new Border
            {
                Height = 340,
                BorderBrush = new SolidColorBrush(Color.Parse("#E8ECF3")),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Child = gridBang
            });

            ((StackPanel)historyCard.Child!).Children.Add(historyStack);
            content.Children.Add(historyCard);

            Content = new ScrollViewer
            {
                Content = content,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            CapNhatTopQua();
            TaiLaiDuLieuDon();
        }

        private static Bitmap? TaoBitmap(string duongDan)
        {
            try
            {
                string pathFull = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, duongDan);
                if (File.Exists(pathFull)) return new Bitmap(pathFull);
                if (File.Exists(duongDan)) return new Bitmap(duongDan);
            }
            catch { }
            return null;
        }

        private static StackPanel TaoHeaderIcon(string imagePath, string title, double fontSize = 16)
        {
            var stack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                VerticalAlignment = VerticalAlignment.Center
            };
            var bmp = TaoBitmap(imagePath);
            if (bmp != null)
            {
                stack.Children.Add(new Image
                {
                    Source = bmp,
                    Width = fontSize * 1.25,
                    Height = fontSize * 1.25,
                    VerticalAlignment = VerticalAlignment.Center
                });
            }
            stack.Children.Add(new TextBlock
            {
                Text = title,
                FontSize = fontSize,
                FontWeight = FontWeight.Bold,
                Foreground = new SolidColorBrush(Color.Parse("#2C4870")),
                VerticalAlignment = VerticalAlignment.Center
            });
            return stack;
        }

        private StackPanel TaoHangRadio(string nhan, params RadioButton[] radioButtons)
        {
            var hang = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 15,
                VerticalAlignment = VerticalAlignment.Center
            };

            hang.Children.Add(new TextBlock
            {
                Text = nhan,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 13,
                Foreground = Brushes.DimGray
            });

            foreach (var radioButton in radioButtons)
                hang.Children.Add(radioButton);

            return hang;
        }

        private Border Card(Control headerControl)
        {
            var card = new Border
            {
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.Parse("#E3E9F5")),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(9),
                Padding = new Thickness(14)
            };

            var stack = new StackPanel { Spacing = 8 };
            stack.Children.Add(headerControl);
            card.Child = stack;
            return card;
        }

        private Border Card(string title)
        {
            return Card(new TextBlock
            {
                Text = title,
                FontSize = 16,
                FontWeight = FontWeight.Bold,
                Foreground = new SolidColorBrush(Color.Parse("#2C4870"))
            });
        }

        private async void ListBoxDon_DoubleTapped(object? sender, TappedEventArgs e)
        {
            if (_listBoxDon.SelectedItem is DonHang don)
            {
               var win = new HoaDonChiTietWindow(don, _data);
                if (TopLevel.GetTopLevel(this) is Window parent)
                    await win.ShowDialog(parent);
                else
                    win.Show();
            }
        }

      private void CapNhatTopQua()
{
    DateTime hienTai = DateTime.Now;
    DateTime tu;
    DateTime den;

    if (_rbThang.IsChecked == true)
    {
        tu = new DateTime(hienTai.Year, hienTai.Month, 1);
        den = tu.AddMonths(1).AddTicks(-1);
    }
    else if (_rbNam.IsChecked == true)
    {
        tu = new DateTime(hienTai.Year, 1, 1);
        den = tu.AddYears(1).AddTicks(-1);
    }
    else
    {
        tu = hienTai.Date;
        den = tu.AddDays(1).AddTicks(-1);
    }

    // Lấy các đơn hàng trong khoảng thời gian
    var danhSachDon = _data.DanhSachDonHang
        .Where(d =>
            d.NgayTao >= tu &&
            d.NgayTao <= den &&
            !string.IsNullOrWhiteSpace(d.QuaTangDoi))
        .ToList();

    // Thống kê quà theo TÊN QUÀ
    var topQua = danhSachDon
        .SelectMany(d =>
            d.QuaTangDoi!
                .Split(
                    new[] { ',', ';' },
                    StringSplitOptions.RemoveEmptyEntries)
                .Select(q => q.Trim()))
        .GroupBy(q => q, StringComparer.OrdinalIgnoreCase)
        .Select(g => new
        {
            TenQua = g.First(),
            SoLan = g.Count()
        })
        .OrderByDescending(x => x.SoLan)
        .Take(10)
        .ToList();

    var ketQua = new List<QuaXepHang>();

    for (int i = 0; i < topQua.Count; i++)
    {
        var x = topQua[i];

        // Tìm quà tương ứng trong danh sách quà
        var qua = _data.DanhSachQuaTang
            .FirstOrDefault(q =>
                !string.IsNullOrWhiteSpace(q.TenQua) &&
                string.Equals(
                    q.TenQua.Trim(),
                    x.TenQua.Trim(),
                    StringComparison.OrdinalIgnoreCase));

        string maQua = qua?.MaQua ?? "-";

        // Tính tổng điểm đổi của quà này
        int tongDiem = danhSachDon
            .Where(d =>
                !string.IsNullOrWhiteSpace(d.QuaTangDoi) &&
                d.QuaTangDoi
                    .Split(
                        new[] { ',', ';' },
                        StringSplitOptions.RemoveEmptyEntries)
                    .Any(q =>
                        string.Equals(
                            q.Trim(),
                            x.TenQua.Trim(),
                            StringComparison.OrdinalIgnoreCase)))
            .Sum(d => d.DiemDoiQua);

        ketQua.Add(new QuaXepHang(
            i + 1,
            maQua,
            x.TenQua,
            x.SoLan,
            tongDiem));
    }

    _listBoxTopQua.ItemsSource = null;
    _listBoxTopQua.ItemsSource = ketQua;
}
        private void TaiLaiDuLieuDon()
        {
            // Lọc lịch sử mua hàng theo đúng kỳ Ngày / Tháng / Năm như Top quà.
            DateTime hienTai = DateTime.Now;
            DateTime tu;
            DateTime den;

            if (_rbLichSuThang.IsChecked == true)
            {
                tu = new DateTime(hienTai.Year, hienTai.Month, 1);
                den = tu.AddMonths(1);
            }
            else if (_rbLichSuNam.IsChecked == true)
            {
                tu = new DateTime(hienTai.Year, 1, 1);
                den = tu.AddYears(1);
            }
            else
            {
                tu = hienTai.Date;
                den = tu.AddDays(1);
            }

            IEnumerable<DonHang> danhSach = _data.DanhSachDonHang
                .Where(d => d.NgayTao >= tu && d.NgayTao < den)
                .OrderByDescending(d => d.NgayTao);

            var dsKetQua = danhSach.ToList();
            _donDangChon = null;
            _lblTrong.IsVisible = dsKetQua.Count == 0;

            _listBoxDon.ItemsSource = null;
            _listBoxDon.ItemsSource = dsKetQua;
        }

        private async void BtnXoaDon_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var parent = TopLevel.GetTopLevel(this) as Window;

            if (_donDangChon == null)
            {
                await ThongBaoWindow.ThongBao(parent, "Chưa chọn đơn hàng", "Vui lòng chọn một đơn hàng cần xóa.");
                return;
            }

            bool dongY = await ThongBaoWindow.XacNhan(
                parent,
                "Xác nhận xóa",
                $"Bạn có chắc muốn xóa đơn {_donDangChon.MaDon} của khách hàng {_donDangChon.TenKH}?");

            if (!dongY)
                return;

            string maDon = _donDangChon.MaDon;
            if (_data.XoaDonHang(maDon))
            {
                await ThongBaoWindow.ThongBao(parent, "Thành công", "Đã xóa đơn hàng.");
                TaiLaiDuLieuDon();
                CapNhatTopQua();
            }
            else
            {
                await ThongBaoWindow.ThongBao(parent, "Không thể xóa", "Không tìm thấy đơn hàng cần xóa.");
            }
        }
    }
}