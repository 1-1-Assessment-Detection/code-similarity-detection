# Thực nghiệm Tuần 1 — Hỏi trực tiếp AI (Direct AI)

Issue liên quan: #4 *Evaluate Direct AI-Based Similarity Detection*. Kết quả được dùng cho #5 *Analyze and Document Detection Errors*.

> **Trạng thái:** Có khung và chương trình chạy thực nghiệm (`DirectAiExperiment/`, C#), gọi AI qua OpenRouter Chat Completions. Bộ dữ liệu #2 đã có trên `main`; chế độ xem trước đã kiểm tra đủ 10 bài / 45 cặp / 5 cặp sao chép. Chưa chạy thật, kết quả và ngày chạy còn `TBD`.

## Mục tiêu

Đo khả năng phát hiện mã nguồn tương đồng khi đưa **trực tiếp từng cặp bài** cho AI, **chưa dùng** AI Harness, Tool Calling hay Self-check Loop. Đây là Phương án B (bản thô `v1`) trong tài liệu mentor.

## Quy tắc thực nghiệm

- Đánh giá đủ **45 cặp** không trùng lặp từ 10 bài mẫu (`datasets/week-01/`).
- Dùng **cùng một prompt và cấu hình** cho mọi cặp (xem [`prompt.md`](prompt.md)).
- **Không** đưa nhãn Ground Truth hoặc tên file gợi ý bài gốc/bài sao chép vào prompt.
- Mỗi cặp chạy trong một lượt hỏi riêng, không giữ ngữ cảnh từ cặp trước.
- Chỉ dùng dữ liệu tự tạo của nhóm. Chưa gửi mã riêng tư ra ngoài.
- Dùng đúng **một model** cho mọi cặp. Khi gặp HTTP 429 (giới hạn tốc độ), chương trình dừng và báo rõ; chạy lại cùng lệnh với **cùng model** để tiếp tục. Không tự chuyển sang model khác hoặc biến thể trả phí.

## Cấu hình

Khóa và model đọc từ biến môi trường hoặc `.env.local` ở thư mục gốc repo (xem [`.env.example`](../../../.env.example)). Không commit khóa.

| Biến | Ý nghĩa | Giá trị mẫu |
| --- | --- | --- |
| `OPENROUTER_API_KEY` | Khóa OpenRouter của bạn | `sk-or-v1-...` (không đưa vào Git) |
| `OPENROUTER_MODEL` | Model dùng cho cả lần chạy | `baidu/ernie-5.5-flash-preview:free` |

| Mục | Giá trị |
| --- | --- |
| Provider | OpenRouter (Chat Completions) |
| Model | `baidu/ernie-5.5-flash-preview:free` (từ `OPENROUTER_MODEL`) |
| Effort / temperature | Không đặt, dùng mặc định của model; chạy lại có thể cho kết quả khác |
| Phiên bản prompt | `v1` |
| Dataset tham chiếu | `datasets/week-01/` (commit `8f2e750`) |
| Ngày chạy | TBD |

## Cấu trúc thư mục

```text
experiments/week-01/direct-ai/
├── README.md          # File này
├── DirectAiExperiment/ # Chương trình C# chạy thực nghiệm
├── prompt.md          # Prompt dùng chung cho mọi cặp
├── results.csv        # Một dòng mỗi cặp, đối chiếu với Ground Truth
└── responses/         # (tạo khi chạy) Phản hồi gốc của AI: <submission_a>__<submission_b>.md
```

`responses/` chưa tạo vì chưa có nội dung. Không đặt dữ liệu bài nộp vào đây, chỉ tham chiếu `datasets/week-01/`.

## Cột của `results.csv`

| Cột | Ý nghĩa |
| --- | --- |
| `pair_id` | Mã cặp, ví dụ `P01`…`P45` |
| `submission_a`, `submission_b` | Tên hai bài, khớp `ground-truth.csv` |
| `ai_verdict` | `copy` / `not_copy` (rút từ phản hồi AI) |
| `ai_similarity` | Điểm 0–100 AI trả về |
| `response_file` | Đường dẫn file phản hồi gốc trong `responses/` |
| `ground_truth` | `copy` / `not_copy` theo `ground-truth.csv` |
| `level` | Mức sao chép 1–4, để trống nếu không phải cặp sao chép |
| `outcome` | `correct` / `false_positive` / `missed` |
| `latency_seconds` | Thời gian xử lý một cặp (giây) |
| `input_tokens`, `output_tokens` | Số token của lượt hỏi nếu API có trả, để trống nếu không |
| `notes` | Nguyên nhân sai hoặc lỗi của AI (hallucination, sai dòng, không nhất quán…) |

## Quy trình

1. Kiểm tra dataset vẫn ở commit đã ghi trong bảng Cấu hình; nếu đổi, cập nhật bảng trước khi chạy.
2. Điền `OPENROUTER_API_KEY` và `OPENROUTER_MODEL` vào `.env.local`.
3. Xem trước (không gọi API), kiểm tra đủ 10 bài, 45 cặp và 5 cặp sao chép, rồi chạy thật. Chạy từ thư mục gốc repo:

   ```powershell
   # Xem trước (mặc định đọc datasets/week-01/original, copied và ground-truth.csv)
   dotnet run --project experiments/week-01/direct-ai/DirectAiExperiment
   # Chạy thật (thêm --run); lưu phản hồi gốc vào responses/
   dotnet run --project experiments/week-01/direct-ai/DirectAiExperiment -- --run
   ```

   Tham số tùy chọn: `--submissions <thư mục>` (có thể lặp lại, thay thế mặc định), `--ground-truth <file.csv>`, `--prompt <file>`, `--out <thư mục>`. Thư mục `verify/` của dataset không phải bài nộp nên không được quét.

   Mã thoát: `0` khi đủ 45/45 cặp có kết luận hợp lệ; `2` khi gặp HTTP 429 (đợi rồi chạy lại cùng lệnh để tiếp tục); `3` khi còn cặp lỗi API hoặc phản hồi không đọc được.

   Model miễn phí chưa ổn định. Lỗi API, JSON thiếu `choices` và phản hồi rỗng **không** ghi file, nên chạy lại sẽ hỏi lại đúng các cặp đó. Phản hồi có nội dung nhưng không đọc được `KET_LUAN`/`DIEM_GIONG` vẫn được lưu và ghi `khong_parse_duoc` trong `notes`; đó là lỗi của AI ở `v1`, không hỏi lại. Bộ đọc chấp nhận nhãn có dấu và định dạng Markdown quanh nhãn, ví dụ `**KẾT_LUẬN:** copy`.
4. Chương trình tự điền `results.csv` và đối chiếu `ground-truth.csv` để gán `ground_truth`, `level`, `outcome`. Cặp không có trong ground truth là `not_copy`.
5. Viết nhận xét sơ bộ: hiệu quả theo 4 mức, tỷ lệ báo nhầm, thời gian, chi phí token.
6. Bàn giao kết quả cho #5.

## Việc cần chốt

- Giới hạn tốc độ của model miễn phí có đủ cho 45 cặp trong một lần chạy hay không (nếu không, chạy làm nhiều đợt cùng model).
- Dữ liệu gửi qua OpenRouter: chỉ bài mẫu do nhóm tự tạo, không gửi mã riêng tư.
- Ground truth (`datasets/week-01/ground-truth.csv`) chỉ liệt kê 5 cặp sao chép; chương trình coi 40 cặp còn lại là không sao chép. Cặp "bài sao chép với bài sao chép khác" và "bài sao chép với bài gốc khác" đều tính là không sao chép; cần mentor/nhóm xác nhận cách gán nhãn này.
