using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace QuanLyKhachHang.Views
{
    /// <summary>
    /// Cửa sổ thông báo dùng chung:
    /// - Thông báo: 1 nút OK
    /// - Xác nhận: 2 nút Không / Có
    /// </summary>
    public class ThongBaoWindow : Window
    {
        private bool _ketQua;

        private ThongBaoWindow(
            string tieuDe,
            string noiDung,
            bool coHaiNut)
        {
            Title = tieuDe;

            Width = 420;
            MinHeight = 170;
            MaxWidth = 520;

            CanResize = false;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            // =====================================================
            // NỘI DUNG
            // =====================================================

            var lblNoiDung = new TextBlock
            {
                Text = noiDung,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 13,
                Margin = new Thickness(24, 24, 24, 18)
            };

            // =====================================================
            // KHU VỰC NÚT
            // =====================================================

            var panelNut = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Spacing = 10,
                Margin = new Thickness(24, 0, 24, 20)
            };

            if (coHaiNut)
            {
                // =========================
                // NÚT KHÔNG
                // =========================

                var btnKhong = new Button
                {
                    Content = "Không",
                    Width = 90,
                    Height = 36,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center
                };

                btnKhong.Click += (_, _) =>
                {
                    _ketQua = false;
                    Close();
                };

                // =========================
                // NÚT CÓ
                // =========================

                var btnCo = new Button
                {
                    Content = "Có",
                    Width = 90,
                    Height = 36,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    Background = new SolidColorBrush(
                        Color.Parse("#5B8EF2")
                    ),
                    Foreground = Brushes.White
                };

                btnCo.Click += (_, _) =>
                {
                    _ketQua = true;
                    Close();
                };

                panelNut.Children.Add(btnKhong);
                panelNut.Children.Add(btnCo);
            }
            else
            {
                // =========================
                // NÚT OK
                // =========================

                var btnOk = new Button
                {
                    Content = "OK",
                    Width = 90,
                    Height = 36,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    Background = new SolidColorBrush(
                        Color.Parse("#5B8EF2")
                    ),
                    Foreground = Brushes.White
                };

                btnOk.Click += (_, _) =>
                {
                    _ketQua = true;
                    Close();
                };

                panelNut.Children.Add(btnOk);
            }

            // =====================================================
            // GIAO DIỆN CHÍNH
            // Không dùng DockPanel để tránh lỗi phần nút
            // bị khuất khi SizeToContent.
            // =====================================================

            var root = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 0
            };

            root.Children.Add(lblNoiDung);
            root.Children.Add(panelNut);

            Content = root;

            // Tự điều chỉnh chiều cao theo nội dung
            SizeToContent = SizeToContent.Height;
        }

        // =========================================================
        // THÔNG BÁO - CHỈ CÓ OK
        // =========================================================

        public static Task ThongBao(
            Window? cha,
            string tieuDe,
            string noiDung)
        {
            var cuaSo = new ThongBaoWindow(
                tieuDe,
                noiDung,
                coHaiNut: false
            );

            return cha != null
                ? cuaSo.ShowDialog(cha)
                : ShowKhongCha(cuaSo);
        }

        // =========================================================
        // XÁC NHẬN - CÓ KHÔNG / CÓ
        // =========================================================

        public static async Task<bool> XacNhan(
            Window? cha,
            string tieuDe,
            string noiDung)
        {
            var cuaSo = new ThongBaoWindow(
                tieuDe,
                noiDung,
                coHaiNut: true
            );

            if (cha != null)
            {
                await cuaSo.ShowDialog(cha);
            }
            else
            {
                await ShowKhongCha(cuaSo);
            }

            return cuaSo._ketQua;
        }

        // =========================================================
        // TRƯỜNG HỢP KHÔNG CÓ CỬA SỔ CHA
        // =========================================================

        private static Task ShowKhongCha(
            ThongBaoWindow cuaSo)
        {
            var tcs = new TaskCompletionSource();

            cuaSo.Closed += (_, _) =>
            {
                tcs.TrySetResult();
            };

            cuaSo.Show();

            return tcs.Task;
        }
    }
}