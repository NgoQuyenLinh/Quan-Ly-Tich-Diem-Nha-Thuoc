using System;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using QuanLyKhachHang.Models;
using QuanLyKhachHang.Services;

namespace QuanLyKhachHang.Views
{
    /// <summary>
    /// Cửa sổ hiển thị chi tiết hoá đơn dạng popup/receipt.
    /// Hiển thị đầy đủ thuốc, quà tặng, mã quà và tổng điểm đổi.
    /// </summary>
    public class HoaDonChiTietWindow : Window
    {
        private readonly DataService _data;

        public HoaDonChiTietWindow(DonHang don, DataService data)
        {
            _data = data;

            Title = $"Chi tiết hoá đơn - {don.MaDon}";
            Width = 500;
            MinWidth = 460;
            MaxWidth = 560;
            SizeToContent = SizeToContent.Height;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            CanResize = false;
            Background = new SolidColorBrush(Color.Parse("#F1F5F9"));

            var mainPanel = new StackPanel
            {
                Margin = new Thickness(16),
                Spacing = 12
            };

            // =========================================================
            // RECEIPT CARD
            // =========================================================

            var card = new Border
            {
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.Parse("#E2E8F0")),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(22, 18)
            };

            var receiptStack = new StackPanel
            {
                Spacing = 8
            };

            // =========================================================
            // 1. MÃ HOÁ ĐƠN
            // =========================================================

            receiptStack.Children.Add(
                new TextBlock
                {
                    Text = $"HOÁ ĐƠN {don.MaDon}",
                    FontSize = 20,
                    FontWeight = FontWeight.Bold,
                    Foreground = new SolidColorBrush(Color.Parse("#1E293B")),
                    HorizontalAlignment = HorizontalAlignment.Center
                }
            );

            // =========================================================
            // 2. NGÀY TẠO
            // =========================================================

            receiptStack.Children.Add(
                new TextBlock
                {
                    Text = $"Ngày tạo: {don.NgayTao:dd/MM/yyyy HH:mm:ss}",
                    FontSize = 13,
                    Foreground = new SolidColorBrush(Color.Parse("#64748B")),
                    HorizontalAlignment = HorizontalAlignment.Center
                }
            );

            // =========================================================
            // KHÁCH HÀNG
            // =========================================================

            if (!string.IsNullOrEmpty(don.TenKH))
            {
                receiptStack.Children.Add(
                    new TextBlock
                    {
                        Text = $"Khách hàng: {don.TenKH} ({don.MaKH})",
                        FontSize = 13,
                        Foreground = new SolidColorBrush(Color.Parse("#475569")),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 2, 0, 0)
                    }
                );
            }

            receiptStack.Children.Add(TaoDuongPhanCach());

            // =========================================================
            // 3. DANH SÁCH THUỐC
            // =========================================================

            receiptStack.Children.Add(
                new TextBlock
                {
                    Text = "Danh sách thuốc mua:",
                    FontSize = 13,
                    FontWeight = FontWeight.SemiBold,
                    Foreground = new SolidColorBrush(Color.Parse("#334155"))
                }
            );

            var panelThuoc = new StackPanel
            {
                Spacing = 6,
                Margin = new Thickness(4, 0, 4, 0)
            };

            if (don.DanhSachThuoc != null &&
                don.DanhSachThuoc.Count > 0)
            {
                for (int i = 0; i < don.DanhSachThuoc.Count; i++)
                {
                    var t = don.DanhSachThuoc[i];

                    var grid = new Grid
                    {
                        ColumnDefinitions = new ColumnDefinitions("*,Auto")
                    };

                    var left = new StackPanel
                    {
                        Spacing = 2
                    };

                    left.Children.Add(
                        new TextBlock
                        {
                            Text = $"{i + 1}. {t.TenThuoc}",
                            FontWeight = FontWeight.Medium,
                            FontSize = 13,
                            Foreground = new SolidColorBrush(Color.Parse("#1E293B")),
                            TextWrapping = TextWrapping.Wrap
                        }
                    );

                    left.Children.Add(
                        new TextBlock
                        {
                            Text = $"    SL: {t.SoLuong} × {t.DonGia:N0} đ",
                            FontSize = 12,
                            Foreground = new SolidColorBrush(Color.Parse("#64748B"))
                        }
                    );

                    var right = new TextBlock
                    {
                        Text = $"{t.ThanhTien:N0} đ",
                        FontWeight = FontWeight.SemiBold,
                        FontSize = 13,
                        VerticalAlignment = VerticalAlignment.Center,
                        Foreground = new SolidColorBrush(Color.Parse("#0F172A"))
                    };

                    Grid.SetColumn(right, 1);

                    grid.Children.Add(left);
                    grid.Children.Add(right);

                    panelThuoc.Children.Add(grid);
                }
            }
            else
            {
                panelThuoc.Children.Add(
                    new TextBlock
                    {
                        Text = "(Không có chi tiết danh sách thuốc)",
                        FontSize = 12.5,
                        FontStyle = FontStyle.Italic,
                        Foreground = new SolidColorBrush(Color.Parse("#94A3B8"))
                    }
                );
            }

            receiptStack.Children.Add(panelThuoc);

            receiptStack.Children.Add(TaoDuongPhanCach());

            // =========================================================
            // 4. DANH SÁCH QUÀ TẶNG
            // =========================================================

            receiptStack.Children.Add(
                new TextBlock
                {
                    Text = "Quà tặng đã đổi:",
                    FontSize = 13,
                    FontWeight = FontWeight.SemiBold,
                    Foreground = new SolidColorBrush(Color.Parse("#334155"))
                }
            );

            var panelQua = new StackPanel
            {
                Spacing = 7,
                Margin = new Thickness(4, 0, 4, 0)
            };

            if (!string.IsNullOrWhiteSpace(don.QuaTangDoi))
            {
                // DataService lưu:
                // "Tên quà 1, Tên quà 2, Tên quà 3"
                var danhSachTenQua = don.QuaTangDoi
                    .Split(
                        ',',
                        StringSplitOptions.RemoveEmptyEntries
                    )
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList();

                for (int i = 0; i < danhSachTenQua.Count; i++)
                {
                    string tenQua = danhSachTenQua[i];

                    // Tìm chính xác quà trong DataService
                    var quaGoc = TimQuaTheoTen(tenQua);

                    string maQua = quaGoc?.MaQua ?? "-";

                    var giftBorder = new Border
                    {
                        Background = new SolidColorBrush(Color.Parse("#F0FDF4")),
                        BorderBrush = new SolidColorBrush(Color.Parse("#BBF7D0")),
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(7),
                        Padding = new Thickness(10, 8)
                    };

                    var giftGrid = new Grid
                    {
                        ColumnDefinitions = new ColumnDefinitions("32,75,*")
                    };

                    // -------------------------------------------------
                    // STT
                    // -------------------------------------------------

                    var txtSTT = new TextBlock
                    {
                        Text = $"{i + 1}.",
                        FontSize = 13,
                        FontWeight = FontWeight.Bold,
                        Foreground = new SolidColorBrush(Color.Parse("#059669")),
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    Grid.SetColumn(txtSTT, 0);

                    // -------------------------------------------------
                    // MÃ QUÀ
                    // -------------------------------------------------

                    var txtMaQua = new TextBlock
                    {
                        Text = maQua,
                        FontSize = 13,
                        FontWeight = FontWeight.Bold,
                        Foreground = new SolidColorBrush(Color.Parse("#2563EB")),
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    Grid.SetColumn(txtMaQua, 1);

                    // -------------------------------------------------
                    // TÊN QUÀ
                    // -------------------------------------------------

                    var txtTenQua = new TextBlock
                    {
                        Text = tenQua,
                        FontSize = 13,
                        FontWeight = FontWeight.SemiBold,
                        Foreground = new SolidColorBrush(Color.Parse("#047857")),
                        TextWrapping = TextWrapping.Wrap,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    Grid.SetColumn(txtTenQua, 2);

                    giftGrid.Children.Add(txtSTT);
                    giftGrid.Children.Add(txtMaQua);
                    giftGrid.Children.Add(txtTenQua);

                    giftBorder.Child = giftGrid;

                    panelQua.Children.Add(giftBorder);
                }

                // -----------------------------------------------------
                // TỔNG ĐIỂM ĐỔI QUÀ
                // -----------------------------------------------------

                var gridTongDiemQua = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitions("*,Auto"),
                    Margin = new Thickness(0, 3, 0, 0)
                };

                gridTongDiemQua.Children.Add(
                    new TextBlock
                    {
                        Text = "Tổng điểm đổi quà:",
                        FontSize = 12.5,
                        Foreground = new SolidColorBrush(Color.Parse("#64748B"))
                    }
                );

                var txtTongDiemQua = new TextBlock
                {
                    Text = $"{don.DiemDoiQua:N0} điểm",
                    FontSize = 12.5,
                    FontWeight = FontWeight.Bold,
                    Foreground = new SolidColorBrush(Color.Parse("#EF4444"))
                };

                Grid.SetColumn(txtTongDiemQua, 1);

                gridTongDiemQua.Children.Add(txtTongDiemQua);

                panelQua.Children.Add(gridTongDiemQua);
            }
            else
            {
                panelQua.Children.Add(
                    new TextBlock
                    {
                        Text = "(Không đổi quà)",
                        FontSize = 12.5,
                        FontStyle = FontStyle.Italic,
                        Foreground = new SolidColorBrush(Color.Parse("#94A3B8"))
                    }
                );
            }

            receiptStack.Children.Add(panelQua);

            // =========================================================
            // 5. GHI CHÚ
            // =========================================================

            receiptStack.Children.Add(TaoDuongPhanCach());

            receiptStack.Children.Add(
                new TextBlock
                {
                    Text = string.IsNullOrWhiteSpace(don.GhiChu)
                        ? "Ghi chú: (Không có)"
                        : $"Ghi chú: {don.GhiChu}",
                    FontSize = 13,
                    TextWrapping = TextWrapping.Wrap,
                    FontStyle = string.IsNullOrWhiteSpace(don.GhiChu)
                        ? FontStyle.Italic
                        : FontStyle.Normal,
                    Foreground = new SolidColorBrush(Color.Parse("#475569"))
                }
            );

            receiptStack.Children.Add(TaoDuongPhanCach());

            // =========================================================
            // 6. TỔNG TIỀN THUỐC
            // =========================================================

            var gridTienThuoc = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("*,Auto")
            };

            gridTienThuoc.Children.Add(
                new TextBlock
                {
                    Text = "Tổng tiền thuốc:",
                    FontSize = 13.5,
                    Foreground = new SolidColorBrush(Color.Parse("#475569"))
                }
            );

            var txtTongTienThuoc = new TextBlock
            {
                Text = $"{don.SoTien:N0} đ",
                FontSize = 13.5,
                FontWeight = FontWeight.SemiBold,
                Foreground = new SolidColorBrush(Color.Parse("#1E293B"))
            };

            Grid.SetColumn(txtTongTienThuoc, 1);

            gridTienThuoc.Children.Add(txtTongTienThuoc);

            receiptStack.Children.Add(gridTienThuoc);

            // =========================================================
            // 7. TỔNG ĐIỂM CỘNG
            // =========================================================

            var gridDiemCong = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("*,Auto")
            };

            gridDiemCong.Children.Add(
                new TextBlock
                {
                    Text = "Tổng điểm cộng:",
                    FontSize = 13.5,
                    Foreground = new SolidColorBrush(Color.Parse("#475569"))
                }
            );

            var txtDiemCong = new TextBlock
            {
                Text = $"+{don.DiemCong:N0} điểm",
                FontSize = 13.5,
                FontWeight = FontWeight.Bold,
                Foreground = new SolidColorBrush(Color.Parse("#10B981"))
            };

            Grid.SetColumn(txtDiemCong, 1);

            gridDiemCong.Children.Add(txtDiemCong);

            receiptStack.Children.Add(gridDiemCong);

            // =========================================================
            // ĐIỂM SỬ DỤNG
            // =========================================================

            if (don.DiemSuDung > 0)
            {
                var gridDiemDung = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitions("*,Auto")
                };

                gridDiemDung.Children.Add(
                    new TextBlock
                    {
                        Text = "Giảm giá bằng điểm:",
                        FontSize = 13.5,
                        Foreground = new SolidColorBrush(Color.Parse("#475569"))
                    }
                );

                var txtDiemDung = new TextBlock
                {
                    Text =
                        $"-{don.DiemSuDung * 1000m / 100m:N0} đ " +
                        $"({don.DiemSuDung} điểm)",
                    FontSize = 13.5,
                    FontWeight = FontWeight.SemiBold,
                    Foreground = new SolidColorBrush(Color.Parse("#EF4444"))
                };

                Grid.SetColumn(txtDiemDung, 1);

                gridDiemDung.Children.Add(txtDiemDung);

                receiptStack.Children.Add(gridDiemDung);
            }

            // =========================================================
            // 8. TỔNG TIỀN PHẢI TRẢ
            // =========================================================

            receiptStack.Children.Add(
                TaoDuongPhanCach(dam: true)
            );

            var gridTongTra = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("*,Auto"),
                Margin = new Thickness(0, 4, 0, 4)
            };

            gridTongTra.Children.Add(
                new TextBlock
                {
                    Text = "Tổng tiền phải trả:",
                    FontSize = 17,
                    FontWeight = FontWeight.Bold,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = new SolidColorBrush(Color.Parse("#DC2626"))
                }
            );

            var txtTongPhaiTra = new TextBlock
            {
                Text = $"{don.ThanhTien:N0} đ",
                FontSize = 21,
                FontWeight = FontWeight.Bold,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Color.Parse("#DC2626"))
            };

            Grid.SetColumn(txtTongPhaiTra, 1);

            gridTongTra.Children.Add(txtTongPhaiTra);

            receiptStack.Children.Add(gridTongTra);

            // =========================================================
            // HIỂN THỊ CARD
            // =========================================================

            card.Child = receiptStack;

            mainPanel.Children.Add(card);

            // =========================================================
            // NÚT ĐÓNG
            // =========================================================

            var btnDong = new Button
            {
                Content = "Đóng",
                Height = 38,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                Background = new SolidColorBrush(Color.Parse("#0EA5E9")),
                Foreground = Brushes.White,
                FontWeight = FontWeight.SemiBold,
                CornerRadius = new CornerRadius(8),
                Cursor = new Cursor(StandardCursorType.Hand)
            };

            btnDong.Click += (_, _) => Close();

            mainPanel.Children.Add(btnDong);

            // Scroll để dù có nhiều quà cũng không bị che mất
            Content = new ScrollViewer
            {
                Content = mainPanel,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
            };
        }

        // =============================================================
        // TÌM QUÀ THEO TÊN
        // =============================================================

        private QuaTang? TimQuaTheoTen(string tenQua)
        {
            if (string.IsNullOrWhiteSpace(tenQua))
                return null;

            // Tìm chính xác trước
            var qua = _data.DanhSachQuaTang.FirstOrDefault(
                q => string.Equals(
                    q.TenQua?.Trim(),
                    tenQua.Trim(),
                    StringComparison.OrdinalIgnoreCase
                )
            );

            if (qua != null)
                return qua;

            // Nếu dữ liệu có khác biệt khoảng trắng
            string tenCanTim = tenQua
                .Trim()
                .ToLower();

            return _data.DanhSachQuaTang.FirstOrDefault(
                q => (q.TenQua ?? "")
                    .Trim()
                    .ToLower() == tenCanTim
            );
        }

        // =============================================================
        // ĐƯỜNG PHÂN CÁCH
        // =============================================================

        private static Control TaoDuongPhanCach(bool dam = false)
        {
            return new Border
            {
                Height = dam ? 1.5 : 1,
                Background = new SolidColorBrush(
                    Color.Parse(
                        dam
                            ? "#94A3B8"
                            : "#E2E8F0"
                    )
                ),
                Margin = new Thickness(0, 6, 0, 6)
            };
        }
    }
}