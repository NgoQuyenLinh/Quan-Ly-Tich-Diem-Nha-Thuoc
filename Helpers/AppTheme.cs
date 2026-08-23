using System;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Styling;

namespace QuanLyKhachHang.Helpers
{
    /// <summary>
    /// Bảng màu và các hằng số giao diện dùng chung cho toàn bộ ứng dụng.
    /// Mục tiêu: tông màu dịu mắt hơn (bớt độ chói/tương phản gắt), nhất quán giữa các màn hình.
    /// Dùng chung 1 nơi để sau này đổi theme chỉ cần sửa ở đây.
    /// </summary>
    public static class AppTheme
    {
        // ----- Màu thương hiệu / hành động chính -----
        public static readonly Color Primary = Color.Parse("#5B8EF2");       // xanh dương dịu (thay cho #2563EB gắt)
        public static readonly Color PrimaryHover = Color.Parse("#4C7EE0");
        public static readonly Color PrimaryLight = Color.Parse("#EEF3FC");  // nền nhạt khi hover/chọn

        // ----- Nền -----
        public static readonly Color BgApp = Color.Parse("#F1F4FA");         // nền vùng nội dung
        public static readonly Color BgSidebar = Color.Parse("#FFFFFF");
        public static readonly Color BgCard = Color.Parse("#FFFFFF");
        public static readonly Color BgHeader = Color.Parse("#F5F8FC");

        // ----- Viền / phân cách -----
        public static readonly Color BorderColor = Color.Parse("#E8ECF3");
        public static readonly Color BorderSoft = Color.Parse("#F0F3F8");

        // ----- Chữ -----
        public static readonly Color TextPrimary = Color.Parse("#33415C");
        public static readonly Color TextSecondary = Color.Parse("#7A8699");
        public static readonly Color TextMuted = Color.Parse("#A3ADBE");

        // ----- Trạng thái -----
        public static readonly Color Success = Color.Parse("#4CAF7D");
        public static readonly Color Danger = Color.Parse("#E4574C");
        public static readonly Color Warning = Color.Parse("#F0B34E");
        public static readonly Color Info = Color.Parse("#4FB3DE");
        public static readonly Color Purple = Color.Parse("#9B85F0");

        // ----- Bo góc chuẩn -----
        public static readonly CornerRadius RadiusSmall = new(6);
        public static readonly CornerRadius RadiusMedium = new(10);
        public static readonly CornerRadius RadiusLarge = new(16);

        // ----- Thời lượng hiệu ứng chuẩn -----
        public static readonly TimeSpan FastAnim = TimeSpan.FromMilliseconds(120);
        public static readonly TimeSpan NormalAnim = TimeSpan.FromMilliseconds(200);

        public static SolidColorBrush Brush(Color c) => new(c);

        /// <summary>
        /// Gắn hiệu ứng chuyển màu nền mượt (thay vì đổi màu "giật cục") cho Border/Button/...
        /// Gọi 1 lần sau khi control được tạo.
        /// </summary>
        public static void SmoothBackground(TemplatedControl control, TimeSpan? duration = null)
        {
            control.Transitions ??= new Transitions();
            control.Transitions.Add(new BrushTransition
            {
                Property = Avalonia.Controls.Primitives.TemplatedControl.BackgroundProperty,
                Duration = duration ?? FastAnim,
                Easing = new CubicEaseOut()
            });
        }

        public static void SmoothBackground(Border border, TimeSpan? duration = null)
        {
            border.Transitions ??= new Transitions();
            border.Transitions.Add(new BrushTransition
            {
                Property = Avalonia.Controls.Border.BackgroundProperty,
                Duration = duration ?? FastAnim,
                Easing = new CubicEaseOut()
            });
        }

        /// <summary>
        /// Hiệu ứng mờ dần khi 1 View/Border xuất hiện, giúp thao tác chuyển màn hình mượt hơn.
        /// </summary>
        public static void FadeIn(Control control, TimeSpan? duration = null)
        {
            control.Opacity = 0;
            control.Transitions = new Transitions
            {
                new DoubleTransition
                {
                    Property = Visual.OpacityProperty,
                    Duration = duration ?? NormalAnim,
                    Easing = new CubicEaseOut()
                }
            };
            // Đẩy việc set Opacity=1 sang sau khi control đã vào visual tree để hiệu ứng chạy
            Avalonia.Threading.Dispatcher.UIThread.Post(() => control.Opacity = 1,
                Avalonia.Threading.DispatcherPriority.Loaded);
        }

        /// <summary>
        /// Bo tròn + đổ bóng nhẹ chuẩn cho các thẻ (card) nội dung, tạo cảm giác nổi khối dịu mắt.
        /// </summary>
        public static BoxShadows CardShadow => new(new BoxShadow
        {
            OffsetX = 0,
            OffsetY = 2,
            Blur = 10,
            Spread = 0,
            Color = Color.FromArgb(24, 51, 65, 92) // đen-xanh rất nhạt, mờ 24/255
        });
    }
}
