# Code Similarity Detection — Nhóm 1.1

Dự án xây dựng hệ thống phát hiện mã nguồn tương đồng cho bài tập lập trình nộp một file. Hệ thống nhận bài qua HTTP API từ Nhóm 1.2 hoặc qua giao diện upload `.zip` để kiểm thử độc lập, loại bỏ code khung mẫu, phân tích mức độ tương đồng và trả kết quả cho Nhóm 1.2. Phạm vi ban đầu tập trung vào C++ và Python.

> **Trạng thái:** Cấu trúc dưới đây là **dự kiến**, dùng để thống nhất nơi lưu tài liệu, mã nguồn, dữ liệu và kết quả thực nghiệm. Các thư mục chưa được tạo chỉ vì xuất hiện trong sơ đồ. Repository hiện có tài liệu mentor tại [`docs/requirments/nhom-1.1-code-similarity-detection.md`](docs/requirments/nhom-1.1-code-similarity-detection.md), [đề bài mẫu tuần 1](docs/research/sample-problem.md), `.gitignore` và `AGENTS.md`; chưa có ứng dụng để build hoặc chạy.

## Cấu trúc thư mục dự kiến

```text
code-similarity-detection/
├── docs/                         # Tài liệu dự án
│   ├── 01-requirements/          # Yêu cầu và phạm vi
│   ├── 02-design/                # Thiết kế hệ thống
│   ├── 03-api/                   # Hợp đồng API
│   ├── 04-research/              # Nghiên cứu phương pháp
│   ├── 05-project-management/    # Quản lý công việc
│   ├── 06-testing/               # Kế hoạch và báo cáo kiểm thử
│   ├── 07-delivery/              # Tài liệu bàn giao
│   └── research/                 # Đề bài mẫu tuần 1 (đã tạo)
├── src/                          # Mã nguồn sản phẩm
│   ├── backend/
│   ├── frontend/
│   ├── code-analysis/
│   └── ai-harness/
├── datasets/                     # Dữ liệu và nhãn chuẩn
│   ├── week-01/
│   └── benchmark/
└── experiments/                  # Mã và kết quả thực nghiệm
    ├── week-01/
    ├── approach-a/
    ├── approach-b/
    └── approach-c/
```

### `docs/` — Tài liệu dự án

| Thư mục | Nội dung nên lưu |
| --- | --- |
| `01-requirements/` | Yêu cầu từ mentor, phạm vi MVP, tiêu chí chấp nhận và các yêu cầu đã được nhóm thống nhất. |
| `02-design/` | Sơ đồ kiến trúc, luồng xử lý quét, mô hình dữ liệu và quyết định thiết kế. |
| `03-api/` | Hợp đồng HTTP API với Nhóm 1.2: endpoint, xác thực, mẫu request/response, trạng thái và mã lỗi đã thống nhất. |
| `04-research/` | Tài liệu tham khảo, khảo sát Tree-sitter/Winnowing, phương án phát hiện tương đồng và lý do chọn phương pháp. |
| `research/` | Đề bài mẫu tuần 1 tại `sample-problem.md`, theo đường dẫn đã được người phụ trách yêu cầu. |
| `05-project-management/` | Kế hoạch theo tuần, phân công, mốc bàn giao, biên bản họp và tiến độ. |
| `06-testing/` | Kế hoạch kiểm thử, danh sách test case và báo cáo kiểm thử; mã test tự động đặt cùng module hoặc trong `tests/` khi cấu trúc test được chốt. |
| `07-delivery/` | Hướng dẫn demo, triển khai, bàn giao và báo cáo cuối cùng. |

`docs/04-research/` dành cho **cơ sở nghiên cứu và lựa chọn phương pháp**; dữ liệu chạy và số liệu đo đạc đặt trong `experiments/`.

### `src/` — Mã nguồn sản phẩm

| Thư mục | Trách nhiệm |
| --- | --- |
| `backend/` | ASP.NET Core API, tác vụ quét bất đồng bộ, xác thực, lưu trữ và tích hợp các thành phần xử lý. |
| `frontend/` | Giao diện React: upload `.zip`, xếp hạng cặp bài, so sánh song song và đồ thị nhóm tương đồng. |
| `code-analysis/` | Tiền xử lý mã nguồn, loại code khung mẫu, phân tích cú pháp/dấu vân tay và lọc cặp nghi vấn ở Lớp 1. |
| `ai-harness/` | Prompt có phiên bản, kiểm soát JSON đầu ra, công cụ tra cứu và vòng tự kiểm tra cho ca khó ở Lớp 2. |

`src/` chứa phần sẽ được tích hợp vào sản phẩm. Script thử nghiệm một phương pháp hoặc kết quả đo riêng của nó thuộc `experiments/`.

### `datasets/` — Dữ liệu nghiên cứu

| Thư mục | Nội dung nên lưu |
| --- | --- |
| `week-01/` | Bộ 10 bài nộp mẫu cho một đề: 5 bài độc lập, 5 bài sao chép theo 4 mức; đề bài, test case, `README.md` và `ground-truth.csv` ghi bài gốc, bài sao chép, mức độ và thay đổi. |
| `benchmark/` | Bộ dữ liệu đánh giá lớn hơn đã gắn nhãn, dùng để so sánh các phương án trên cùng đầu vào. |

Dữ liệu tại đây là **đầu vào và nhãn chuẩn**. Ghi rõ nguồn, ngôn ngữ, đề bài, cách tạo nhãn và quyền sử dụng trước khi đưa dữ liệu bên ngoài vào repository. Không đặt kết quả dự đoán hoặc số liệu đánh giá vào `datasets/`.

### `experiments/` — Thực nghiệm và kết quả

| Thư mục | Nội dung nên lưu |
| --- | --- |
| `week-01/` | Khảo sát ban đầu trên 10 bài mẫu: cách so khớp văn bản/AI trực tiếp, bảng báo đúng, báo nhầm và nguyên nhân. |
| `approach-a/` | Phương án A: chỉ dùng thuật toán so khớp cấu trúc, không dùng AI. |
| `approach-b/` | Phương án B: gửi trực tiếp từng cặp bài qua AI API (bản thô `v1`). |
| `approach-c/` | Phương án C: loại code mẫu + lọc Lớp 1 + AI Harness có công cụ tra cứu và tự kiểm tra (`v2` → `v3`). |

Mỗi thực nghiệm nên ghi phiên bản code/phương pháp, cấu hình, tập dữ liệu tham chiếu, cách chạy và kết quả: tỷ lệ phát hiện đúng theo 4 mức, tỷ lệ báo nhầm, thời gian xử lý và chi phí token/API. Tham chiếu dữ liệu từ `datasets/`; không sao chép cùng một bộ dữ liệu vào từng `approach-*`.

## Quy tắc đặt file và cập nhật cấu trúc

1. Chọn thư mục theo **mục đích của file**: tài liệu trong `docs/`, mã sản phẩm trong `src/`, bài nộp và ground truth trong `datasets/`, mã chạy thử cùng kết quả trong `experiments/`.
2. Đặt tên file dễ nhận biết, giữ một nguồn dữ liệu chuẩn và liên kết đến nguồn đó khi cần sử dụng ở nhiều nơi.
3. Chỉ tạo nhánh thư mục khi có nội dung thực tế; không commit thư mục trống. Khi cấu trúc được triển khai, cập nhật bảng này nếu trách nhiệm thư mục thay đổi.
4. Thư mục hiện có `docs/requirments/` là đường dẫn cũ của tài liệu mentor. Khi nhóm quyết định chuyển sang `docs/01-requirements/`, chuyển file bằng một thay đổi riêng và cập nhật mọi liên kết, đặc biệt trong `AGENTS.md` và README này.

Quy tắc làm việc cho AI Agent và quy ước kỹ thuật của nhóm nằm trong [`AGENTS.md`](AGENTS.md). Hướng dẫn cài đặt/chạy dự án sẽ được bổ sung khi backend và frontend thực sự được khởi tạo.
