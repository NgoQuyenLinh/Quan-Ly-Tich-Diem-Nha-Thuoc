# 💊 Phần mềm Quản lý Nhà thuốc & Tích điểm khách hàng

Ứng dụng desktop viết bằng **C# / .NET 8** với giao diện **Avalonia UI** (dựng hoàn toàn bằng
code, không dùng file `.axaml`), chạy được đa nền tảng (Windows, macOS, Linux). Ứng dụng
**không dùng database** — toàn bộ dữ liệu được đọc/ghi trực tiếp từ các file **JSON** trong
thư mục `Data/`, phù hợp làm đồ án/bài tập môn lập trình để minh hoạ các kiến thức: CRUD,
tìm kiếm — lọc dữ liệu, xử lý file, quản lý kho theo lô (FEFO), và tích/dùng điểm khách hàng.

![Giao diện Trang chủ](QuanLyKhachHang/docs/imagess/Home-page.png)

## Mục lục

- [1. Tính năng chính](#1-tính-năng-chính)
- [2. Công nghệ sử dụng](#2-công-nghệ-sử-dụng)
- [3. Yêu cầu hệ thống](#3-yêu-cầu-hệ-thống)
- [4. Cách chạy chương trình](#4-cách-chạy-chương-trình)
- [5. Cấu trúc thư mục](#5-cấu-trúc-thư-mục)
- [6. Kiến trúc & luồng hoạt động](#6-kiến-trúc--luồng-hoạt-động)
- [7. Quy tắc nghiệp vụ](#7-quy-tắc-nghiệp-vụ)
- [8. Dữ liệu mẫu](#8-dữ-liệu-mẫu)
- [9. Ảnh chụp màn hình](#9-ảnh-chụp-màn-hình)

## 1. Tính năng chính

Sidebar bên trái gồm 6 mục điều hướng chính (Trang chủ, Khách hàng, Kho quà, Thuốc, Đơn hàng,
Thống kê) cùng nút Thoát:

- **🏠 Trang chủ (Dashboard)**
  - Thẻ tổng quan: tổng số khách hàng, tổng đơn hàng, tổng doanh thu, tổng điểm — lọc theo
    khoảng thời gian (hôm nay/tuần/tháng/năm...).
  - **Tìm kiếm nhanh theo số điện thoại**: nếu khách đã tồn tại → chuyển thẳng sang tab Đơn
    hàng với khách đã được chọn sẵn; nếu chưa có → chuyển sang tab Khách hàng để tạo mới.
  - Khung **Ghi chú / nhắc việc** (thêm, đánh dấu hoàn thành, đổi màu, xoá ghi chú).
- **👤 Khách hàng**: danh sách dạng bảng, tìm kiếm theo tên/SĐT/mã KH, thêm/sửa/xoá, có thể
  double-click vào 1 dòng để sửa nhanh (popup `KhachHangEditWindow`).
- **💊 Thuốc (kho thuốc)**
  - Danh mục thuốc: mã, tên, loại, đơn giá, tình trạng còn/hết hàng, ngưỡng cảnh báo tồn thấp.
  - Hỗ trợ **đa đơn vị tính** (Hộp / Vỉ / Viên) với quy đổi tự động qua lại.
  - Quản lý **lô thuốc** theo từng thuốc (`LoThuocWindow`): số lô, hạn sử dụng, số lượng —
    tồn kho của thuốc được tự động đồng bộ từ tổng các lô còn hàng, chưa hết hạn.
  - Cảnh báo lô sắp hết hạn / đã hết hạn, thuốc tồn kho thấp, thuốc đã hết hàng.
- **🧾 Đơn hàng & Tích điểm**
  - Tạo đơn hàng: chọn khách hàng, thêm nhiều loại thuốc (kiểm tra tồn kho theo lô, trừ kho
    theo nguyên tắc **FEFO** — hết hạn trước xuất trước), tuỳ chọn dùng điểm để giảm giá và/hoặc
    đổi quà tặng, ước tính điểm cộng và thành tiền theo thời gian thực.
  - Danh sách đơn hàng đã tạo, xem lại chi tiết từng đơn.
- **🎁 Kho quà tặng**: quản lý danh mục quà đổi điểm (thêm/sửa/xoá), theo dõi số lượng còn
  lại, chuyển trạng thái Đang bán/Chưa bán, lọc quà theo trạng thái hoặc theo tháng thêm vào.
- **📊 Thống kê**
  - Chọn khoảng ngày để xem: số đơn hàng, tổng doanh thu, tổng điểm đã tích/đã dùng.
  - Biểu đồ doanh thu theo thời gian (line chart) và biểu đồ tròn (dùng thư viện LiveCharts).
  - Bảng Top khách hàng có điểm tích luỹ cao nhất.
  - Cho phép xoá đơn hàng trực tiếp từ màn hình thống kê.

> **Ghi chú:** tầng nghiệp vụ (`DataService`) đã có sẵn đầy đủ CRUD và các hàm nghiệp vụ cho
> **Đơn thuốc (toa thuốc bác sĩ kê)**, **Nhà cung cấp**, **Phiếu nhập hàng** và **Tương tác
> thuốc** (models: `DonThuoc`, `NhaCungCap`, `PhieuNhapHang`, `TuongTacThuoc`), nhưng hiện
> **chưa có màn hình (View) riêng** cho các module này trong sidebar — đây là phần có thể phát
> triển tiếp.

## 2. Công nghệ sử dụng

| Thành phần         | Công nghệ |
|--------------------|-----------|
| Ngôn ngữ / nền tảng | C# 12, .NET 8 (`net8.0` — không ràng buộc Windows nên build/chạy được trên macOS, Linux, Windows) |
| Giao diện           | [Avalonia UI](https://avaloniaui.net/) 11.1.3 + Fluent Theme, giao diện dựng 100% bằng code (không XAML) |
| Biểu đồ             | LiveChartsCore.SkiaSharpView.Avalonia 2.0.5 |
| Font                | Avalonia.Fonts.Inter |
| Lưu trữ dữ liệu     | File JSON (`System.Text.Json`), không dùng cơ sở dữ liệu |

## 3. Yêu cầu hệ thống

- **.NET 8 SDK**: https://dotnet.microsoft.com/download
- Windows, macOS hoặc Linux (nhờ Avalonia UI đa nền tảng — không giống bản WinForms chỉ chạy
  trên Windows).
- (Tuỳ chọn) Visual Studio 2022 / JetBrains Rider / VS Code với extension C# nếu muốn debug
  bằng IDE.

## 4. Cách chạy chương trình

1. Cài **.NET 8 SDK** theo hướng dẫn tại link ở trên, kiểm tra bằng:
   ```bash
   dotnet --version
   ```
2. Clone/giải nén dự án, mở Terminal tại thư mục chứa project:
   ```bash
   cd Quan-Ly-Tich-Diem-Nha-Thuoc/QuanLyKhachHang
   dotnet restore
   dotnet run
   ```
   Hoặc mở file `QuanLyKhachHang.sln` (ở thư mục gốc) bằng Visual Studio 2022 / Rider rồi
   nhấn **F5**.
3. Khi chạy lần đầu, ứng dụng tự đọc dữ liệu mẫu có sẵn trong thư mục `Data/` (các file JSON
   được copy tự động vào thư mục build nhờ cấu hình `CopyToOutputDirectory` trong file
   `.csproj`). Không cần cài đặt hay cấu hình database.

## 5. Cấu trúc thư mục

```
Quan-Ly-Tich-Diem-Nha-Thuoc/
├── QuanLyKhachHang.sln
├── README.md                        # File này
└── QuanLyKhachHang/
    ├── QuanLyKhachHang.csproj
    ├── Program.cs                   # Điểm khởi động, cấu hình Avalonia AppBuilder
    ├── App.cs                       # Lớp Application gốc (theme, khởi tạo MainWindow)
    ├── Models/                      # Các lớp dữ liệu thuần (POCO), map trực tiếp sang JSON
    │   ├── KhachHang.cs
    │   ├── Thuoc.cs
    │   ├── LoThuoc.cs
    │   ├── DonHang.cs / chiTietDonHang.cs
    │   ├── QuaTang.cs
    │   ├── GhiChu.cs
    │   ├── DonThuoc.cs / ChiTietDonThuoc.cs
    │   ├── NhaCungCap.cs
    │   ├── PhieuNhapHang.cs / ChiTietPhieuNhap.cs
    │   └── TuongTacThuoc.cs
    ├── Services/
    │   └── DataService.cs           # "Trái tim" ứng dụng: đọc/ghi JSON + toàn bộ nghiệp vụ
    ├── Views/                       # Mỗi màn hình là 1 UserControl/Window dựng bằng code C#
    │   ├── MainWindow.cs            # Cửa sổ chính: sidebar điều hướng + vùng nội dung
    │   ├── TrangChuView.cs          # Trang chủ / Dashboard
    │   ├── KhachHangView.cs / KhachHangEditWindow.cs
    │   ├── ThuocView.cs / LoThuocWindow.cs
    │   ├── DonHangView.cs
    │   ├── KhoQuaView.cs
    │   ├── ThongKeView.cs
    │   ├── BieuDoDoanhThuLineView.cs / BieuDoTron.cs
    │   ├── GhiChuView.cs
    │   └── ThongBaoWindow.cs        # Popup thông báo/xác nhận dùng chung
    ├── Helpers/
    │   ├── AppTheme.cs              # Màu sắc, bo góc, hiệu ứng fade/hover dùng chung
    │   └── UiHelpers.cs
    ├── Data/                        # Dữ liệu lưu trữ dạng JSON (đóng vai trò "database")
    │   ├── khachhang.json
    │   ├── thuoc.json
    │   ├── lothuoc.json
    │   ├── donhang.json
    │   └── ghichu.json
    └── docs/
        ├── imagess/                 # Ảnh chụp giao diện & icon sidebar
        └── tasks/                   # Tài liệu mô tả yêu cầu từng phần (thiết kế UI, Figma...)
```

## 6. Kiến trúc & luồng hoạt động

- **Models**: các lớp dữ liệu thuần (POCO), được (de)serialize trực tiếp từ/ra các file JSON
  trong `Data/` bằng `System.Text.Json`. Các model chi tiết (`ChiTietDonHang`,
  `ChiTietPhieuNhap`, `ChiTietDonThuoc`) lưu lại tên/đơn giá **tại thời điểm giao dịch**, không
  tham chiếu ngược lại bản ghi gốc, để lịch sử không bị thay đổi nếu sau này thuốc gốc bị sửa/xoá.
- **Services/DataService.cs**: đóng vai trò database giả lập và toàn bộ tầng nghiệp vụ:
  - `TaiDuLieu()` đọc các file JSON vào bộ nhớ khi khởi động; mỗi thao tác Thêm/Sửa/Xoá đều
    cập nhật danh sách trong bộ nhớ rồi gọi `Luu...()` tương ứng để ghi đè lại file JSON ngay.
  - Sinh mã tự động cho từng loại đối tượng (`TaoMaKhachHangMoi`, `TaoMaThuocMoi`,
    `TaoMaDonHangMoi`,...).
  - `TaoDonHang(...)`: nghiệp vụ trung tâm — kiểm tra tồn kho, kiểm tra điểm hợp lệ, trừ kho
    theo lô theo FEFO (lô hết hạn sớm nhất trừ trước), trừ điểm dùng/quà đổi, cộng điểm mới,
    và ghi lại đơn hàng.
  - `DongBoTonKho...`, `LoSapHetHan`, `LoDaHetHan`, `ThuocTonKhoThap`, `ThuocHetHang`: các hàm
    hỗ trợ quản lý kho theo lô và cảnh báo.
  - Các hàm thống kê: `TongDoanhThu`, `TongDiemDaTichLuy`, `TongDiemDaSuDung`,
    `TopKhachHangDiemCao`, `SoLuongDonHang`,... phục vụ Trang chủ và Thống kê, có thể lọc theo
    khoảng ngày.
- **Views**: dựng UI hoàn toàn bằng code C# (không dùng `.axaml`) để dễ đọc, dễ giải thích khi
  thuyết trình. `MainWindow` giữ 1 sidebar cố định và hoán đổi các `UserControl` (Trang chủ /
  Khách hàng / Thuốc / Kho quà / Đơn hàng / Thống kê) trong 1 vùng nội dung khi bấm menu, có
  hiệu ứng fade khi chuyển màn hình (`AppTheme.FadeIn`). Các popup nhỏ (sửa khách hàng, quản lý
  lô thuốc, thông báo/xác nhận) dùng `Window` riêng (`KhachHangEditWindow`, `LoThuocWindow`,
  `ThongBaoWindow`).

## 7. Quy tắc nghiệp vụ

- **Tích điểm khi mua hàng**: điểm cộng = *tổng tiền đơn hàng / 1000* (làm tròn xuống).
- **Dùng điểm giảm giá**: **100 điểm = giảm 1.000đ**. Không cho phép dùng vượt quá số điểm
  khách hiện có, và không cho dùng điểm giảm quá giá trị đơn hàng.
- **Đổi quà tặng**: có thể chọn nhiều món quà trong cùng 1 đơn; điểm dùng để giảm giá + tổng
  điểm quy đổi quà không được vượt quá điểm khách đang có; hệ thống tự trừ tồn kho quà.
- **Trừ kho theo FEFO** (First-Expired-First-Out): khi bán thuốc, hệ thống ưu tiên trừ ở lô có
  hạn sử dụng gần nhất trước; nếu tổng tồn của thuốc về 0 thì tự chuyển trạng thái "Hết hàng".
- **Đa đơn vị tính thuốc**: 1 Hộp = *SoViMoiHop* Vỉ, 1 Vỉ = *SoVienMoiVi* Viên; đơn giá lưu
  theo đơn vị nhỏ nhất (Viên) và được quy đổi tự động khi nhập/bán theo Hộp hoặc Vỉ.

## 8. Dữ liệu mẫu

Thư mục `Data/` có sẵn dữ liệu mẫu để demo ngay mà không cần nhập liệu thủ công:

| File              | Nội dung |
|-------------------|----------|
| `khachhang.json`  | 10 khách hàng mẫu |
| `thuoc.json`      | 45 loại thuốc mẫu |
| `lothuoc.json`    | 45 lô thuốc mẫu (gắn với hạn sử dụng khác nhau) |
| `donhang.json`    | 17 đơn hàng mẫu trải nhiều ngày khác nhau |
| `ghichu.json`     | 5 ghi chú mẫu trên Trang chủ |

## 9. Ảnh chụp màn hình

Xem thêm ảnh giao diện chi tiết từng màn hình tại `QuanLyKhachHang/docs/imagess/`.

![Trang chủ - tìm kiếm nhanh](QuanLyKhachHang/docs/imagess/Screenshot%202026-08-12%20at%2022.01.02.png)
![Quản lý khách hàng](QuanLyKhachHang/docs/imagess/Screenshot%202026-08-12%20at%2022.01.29.png)
![Thống kê](QuanLyKhachHang/docs/imagess/thongKe.png)