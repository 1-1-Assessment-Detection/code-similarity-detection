# Nhóm 1.1: Phát hiện mã nguồn tương đồng (Code Similarity Detection)

**Hệ thống kết nối:** Cung cấp dịch vụ quét và kết quả tương đồng qua HTTP API cho Nhóm 1.2 (AI chấm điểm và đánh giá mã nguồn).

## 1. Yêu cầu chức năng & Phạm vi

Nhóm 1.1 xây dựng hệ thống phát hiện sao chép mã nguồn cho các bài tập lập trình dạng nộp 1 file, gồm các chức năng chính:

1. **Tiếp nhận bài nộp:** Nhận danh sách bài nộp của một bài tập (qua HTTP API từ Nhóm 1.2 gửi sang, hoặc qua tính năng tải lên file `.zip` trên giao diện web của Nhóm 1.1 để kiểm thử và chạy độc lập).
2. **Loại bỏ code khung mẫu:** Tự động tách bỏ phần code mẫu do giảng viên cung cấp sẵn trong đề bài để tránh báo nhầm các đoạn khai báo giống nhau của cả lớp.
3. **Phát hiện tương đồng 2 lớp:**
   - **Lớp 1 — Lọc nhanh bằng thuật toán:** Phân tích cấu trúc mã nguồn (có thể dùng thư viện phân tích cú pháp như Tree-sitter hoặc thuật toán băm dấu vân tay Winnowing) để phát hiện các bài chép nguyên bản, đổi tên biến, đổi định dạng hoặc thay đổi cấu trúc vòng lặp (`for`/`while`), đảo thứ tự hàm.
   - **Lớp 2 — Phân tích ca khó bằng AI:** Chỉ gửi các cặp bài nghi vấn mức độ phức tạp (như viết lại logic bằng công cụ AI) sang hệ thống kiểm soát AI để đánh giá mức độ tương đồng về ý tưởng giải thuật và sinh lời giải thích ngắn gọn bằng tiếng Việt (chỉ rõ giống ở hàm nào, dòng nào, đã chỉnh sửa theo kiểu gì).
4. **Giao diện xem báo cáo trực quan:**
   - Bảng xếp hạng các cặp bài có tỷ lệ giống nhau cao nhất.
   - Màn hình so sánh 2 bài nộp cạnh nhau (Side-by-Side), tô màu đúng các đoạn dòng code giống nhau.
   - Đồ thị mạng lưới (Cluster Graph) thể hiện các nhóm sinh viên có bài làm tương đồng nhau.

**Phạm vi dữ liệu đầu vào:**

- Bài tập lập trình dạng 1 file bằng **C++** và **Python** (giai đoạn đầu ưu tiên ổn định trên 2 ngôn ngữ này).
- Không xây dựng các chức năng quản lý lớp học, điểm danh hoặc gửi thông báo email (phần quản lý bài tập thuộc về Nhóm 1.2).

## 2. Giao tiếp HTTP API với Nhóm 1.2

**Nguyên tắc dữ liệu:** Mỗi nhóm tự thiết kế và quản lý Database riêng (tự chọn MySQL, PostgreSQL, MongoDB...). Không dùng chung Database giữa 2 nhóm. Mọi trao đổi thực hiện qua HTTP API và xác thực bằng khóa `X-API-Key` trong Header.

Vì quá trình quét cả lớp tốn thời gian, API hoạt động theo cơ chế nhận yêu cầu bất đồng bộ (tạo tác vụ trước, hỏi kết quả sau):

### 2.1. Gửi yêu cầu quét: `POST /api/v1/similarity/scans`

- **Dữ liệu Nhóm 1.2 gửi sang (JSON):**

  ```json
  {
    "assignment_id": "BT01",
    "language": "python",
    "template_code": "def solve():\n    pass",
    "submissions": [
      {
        "submission_id": "sub_001",
        "student_id": "SV01",
        "source_code": "..."
      }
    ]
  }
  ```

- **Phản hồi ngay (`202 Accepted`):**

  ```json
  {
    "scan_id": "scan_2026_001",
    "status": "processing"
  }
  ```

### 2.2. Lấy kết quả quét: `GET /api/v1/similarity/scans/{scan_id}`

- **Phản hồi khi hoàn tất (`200 OK`):**

  ```json
  {
    "scan_id": "scan_2026_001",
    "status": "done",
    "results": [
      {
        "submission_id": "sub_001",
        "student_id": "SV01",
        "max_similarity": 86.5,
        "matched_with_submission_id": "sub_014",
        "matched_lines": [
          { "from_line": 12, "to_line": 34 }
        ],
        "ai_explanation": "Hai bài giống cấu trúc hàm đệ quy ở dòng 12-34, chỉ đổi tên biến và tách thêm 1 biến tạm.",
        "risk_level": "high",
        "report_url": "http://<dia-chi-nhom-1.1>/reports/scan_2026_001/sub_001"
      }
    ]
  }
  ```

*Trong Tuần 2–4, khi Nhóm 1.2 chưa hoàn thiện luồng nộp bài, Nhóm 1.1 dùng chức năng upload `.zip` để phát triển và mở sẵn API trả dữ liệu mẫu đúng cấu trúc trên để hai nhóm làm việc song song.*

## 3. Thiết kế AI và cơ chế kiểm soát (AI Harness)

Không gửi thẳng cặp code lên AI để hỏi tự do. Nhóm cần thiết kế bộ khung kiểm soát AI gồm 4 thành phần:

1. **System Prompt (lưu các phiên bản `v1`, `v2`, `v3` để so sánh):**
   - Định nghĩa vai trò đánh giá bài thi lập trình; phân biệt rõ trường hợp trùng lặp hợp lệ do cùng dùng thuật toán kinh điển (như QuickSort, DFS) với trường hợp sao chép có chỉnh sửa hoặc viết lại.
2. **Ép khuôn đầu ra (Output JSON Schema):**
   - Bắt buộc AI trả về JSON hợp lệ gồm đúng các trường: `risk_level` (`low` / `medium` / `high`), `matched_lines` (`from_line`, `to_line`), và `ai_explanation` (giải thích ngắn gọn bằng tiếng Việt).
3. **Cấp công cụ tra cứu (Tool Calling):**
   - Cài đặt 3 hàm công cụ để AI chủ động gọi khi cần đối chiếu:
     - `xem_doan_giong(bai_a, bai_b)`: đọc chi tiết các đoạn mã nghi vấn của 2 bài.
     - `xem_code_mau_giang_vien()`: đối chiếu code khung đề bài để loại trừ phần trùng hợp lệ.
     - `dem_tan_suat_ca_lop(doan_code)`: kiểm tra tần suất xuất hiện của đoạn code trong toàn bộ bài nộp của lớp.
4. **Vòng tự kiểm tra (Self-check Loop):**
   - Kiểm tra tự động xem cặp số dòng `from_line..to_line` AI trả về có nằm trong phạm vi độ dài thật của bài A và bài B hay không. Nếu sai lệch hoặc lỗi định dạng JSON, hệ thống tự động gửi lỗi ngược lại yêu cầu AI xử lý lại (tối đa 2–3 lần).

## 4. Yêu cầu nghiên cứu và thực nghiệm

- **Dữ liệu thử nghiệm:** Chuẩn bị 1 bộ dữ liệu gồm khoảng 30–50 bài nộp thật cho 1–2 đề bài (từ nguồn bài tập mẫu hoặc bộ dữ liệu công khai như CodeNet), bổ sung các cặp sao chép tự tạo theo **4 mức độ:**
  1. Chép nguyên bản / đổi khoảng trắng, chú thích.
  2. Đổi tên biến, đổi tên hàm.
  3. Đổi cấu trúc điều khiển (`for` sang `while`, đảo thứ tự hàm).
  4. Nhờ AI viết lại theo cách diễn đạt mới nhưng giữ nguyên ý tưởng.
- **Kịch bản so sánh thực nghiệm:** Chạy đánh giá trên cùng bộ dữ liệu theo **3 phương án:**
  - **Phương án A:** Chỉ sử dụng thuật toán so khớp cấu trúc (không dùng AI).
  - **Phương án B:** Gửi trực tiếp từng cặp bài qua AI API (phiên bản thô `v1`).
  - **Phương án C:** Mô hình kết hợp lọc bỏ code mẫu + thuật toán Lớp 1 + AI Harness với công cụ tra cứu và vòng tự kiểm tra (`v2` → `v3`).
- **Chỉ số cần đo đạc:**
  1. Tỷ lệ phát hiện đúng ở từng mức độ (Mức 1 đến Mức 4).
  2. Tỷ lệ báo nhầm (bài tự làm bị đánh giá sai thành sao chép).
  3. Thời gian xử lý toàn lớp và chi phí token/API của từng phương án.
  4. Bảng ghi nhận các lỗi thực tế ở `v1` và mức độ cải thiện qua các phiên bản `v2`, `v3`.

## 5. Gợi ý phân công công việc

- **2 thành viên — Bộ lọc & Phân tích cấu trúc:** Xử lý tách bỏ code khung mẫu, cài đặt thuật toán so khớp Lớp 1 (Tree-sitter / Winnowing) và tạo danh sách cặp nghi vấn kèm tọa độ dòng.
- **1–2 thành viên — AI Harness & Thực nghiệm:** Xây dựng các phiên bản System Prompt (`v1` → `v3`), cài đặt 3 công cụ tra cứu và vòng tự kiểm tra; tạo bộ dữ liệu 4 mức độ và chạy thực nghiệm đo bằng số liệu.
- **1 thành viên — Backend & HTTP API:** Xây dựng hàng đợi xử lý tác vụ quét (`scan_id`), quản lý Database riêng của nhóm và cung cấp 2 HTTP API chuẩn kết nối với Nhóm 1.2.
- **1 thành viên — Giao diện Web & Kiểm thử:** Xây dựng màn hình upload file `.zip`, bảng xếp hạng cặp tương đồng, trình so sánh 2 bài song song tô màu đoạn giống và đồ thị nhóm sao chép.

## 6. Công việc cụ thể Tuần 1

- Thống nhất 1 đề bài C++/Python đơn giản với Nhóm 1.2 và chuẩn bị 10 bài nộp mẫu (5 bài tự viết khác nhau + 5 bài chép theo 4 mức: nguyên bản, đổi tên biến, đổi vòng lặp/cấu trúc, nhờ AI viết lại).
- Thử so sánh 10 bài mẫu bằng công cụ so khớp văn bản thường và bằng cách hỏi trực tiếp AI; lập bảng ghi nhận các cặp báo đúng, báo nhầm và nguyên nhân sai.
- Làm việc với Nhóm 1.2 để vẽ sơ đồ luồng gửi/nhận dữ liệu và chốt 1 file JSON ví dụ thật cho 2 HTTP API.
- **Bàn giao cuối Tuần 1:** Thư mục 10 bài mẫu, bảng phân tích lỗi (1 trang), sơ đồ luồng và file JSON thống nhất với Nhóm 1.2.

## 7. Mốc bàn giao

- **Tuần 1:** Hoàn tất bộ mẫu 10 bài, bảng khảo sát lỗi cách làm thô và chốt thiết kế API với Nhóm 1.2 (chi tiết tại Mục 6).
- **Tuần 2–3:** Hoàn thiện chức năng loại bỏ code khung mẫu, bộ lọc Lớp 1, khung Backend và giao diện so sánh song song.
- **Tuần 4–5 (Mốc MVP):** Chạy hoàn chỉnh trên tập ~30–50 bài C++/Python: loại bỏ được code mẫu, phát hiện được các ca đổi tên biến/cấu trúc, gọi AI Harness giải thích được ca khó, hiển thị trên màn hình so sánh song song và trả JSON qua API cho Nhóm 1.2.
- **Tuần 6:** Hoàn thiện đồ thị mạng lưới sao chép, tối ưu tốc độ xử lý và ghép nối chạy thật qua HTTP API với Nhóm 1.2.
- **Tuần 7–8:** Chạy thực nghiệm lấy đầy đủ số liệu so sánh Phương án A / B / C (`v1` → `v3`), hoàn thiện tài liệu báo cáo và kịch bản demo chung.
