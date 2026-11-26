# Thực nghiệm tuần 1: hỏi trực tiếp AI

Thư mục này phục vụ issue #4. Kết quả sẽ được chuyển sang #5 để tổng hợp bảng phân tích lỗi.

> **Trạng thái:** Chương trình đã sẵn sàng nhưng chưa chạy được thật vì tài khoản OpenRouter hết lượt miễn phí trong ngày. Kết quả và ngày chạy sẽ cập nhật sau.

## Thực nghiệm này làm gì

Nhóm muốn biết nếu chỉ đưa thẳng hai bài cho AI rồi hỏi "có chép nhau không" thì AI làm tốt tới đâu. Đây là Phương án B (bản thô `v1`) trong tài liệu mentor, nên **chưa** có AI Harness, Tool Calling hay vòng tự kiểm tra.

Để kết quả công bằng, nhóm giữ mấy nguyên tắc sau:

- Chạy đủ 45 cặp ghép từ 10 bài trong [`datasets/week-01/`](../../../datasets/week-01/README.md).
- Mọi cặp dùng chung một [prompt](prompt.md) và một model. Nếu phải đổi model giữa chừng thì chạy lại từ đầu.
- AI chỉ thấy nội dung code. Tên file, đáp án hay bất cứ gợi ý nào về bài gốc đều không đưa vào prompt.
- Mỗi cặp là một lượt hỏi riêng, AI không nhớ gì từ cặp trước.
- Chỉ gửi bài mẫu do nhóm tự viết, không gửi code của người khác.

## Cấu hình

| Mục | Giá trị |
| --- | --- |
| Nơi gọi AI | OpenRouter (Chat Completions) |
| Model | `google/gemma-4-31b-it:free` |
| Temperature | Để mặc định, nên chạy lại có thể ra kết quả hơi khác |
| Prompt | `v1` |
| Dữ liệu | `datasets/week-01/`, commit `8f2e750` |
| Ngày chạy | Chưa chạy |

Khóa API và tên model đặt trong file `.env.local` ở thư mục gốc repo, xem mẫu ở [`.env.example`](../../../.env.example). File này đã nằm trong `.gitignore`, đừng commit khóa lên.

## Cách chạy

Chạy từ thư mục gốc repo. Lệnh đầu chỉ liệt kê bài và cặp để kiểm tra, không gọi AI. Thêm `--run` mới gọi thật:

```powershell
dotnet run --project experiments/week-01/direct-ai/DirectAiExperiment
dotnet run --project experiments/week-01/direct-ai/DirectAiExperiment -- --run
```

Lần chạy thử phải ra 10 bài, 45 cặp, trong đó 5 cặp sao chép.

Model miễn phí hay chập chờn nên chương trình được viết để **chạy lại bao nhiêu lần cũng được**. Cặp nào đã có câu trả lời thì bỏ qua, chỉ hỏi những cặp còn thiếu. Cụ thể:

- Bị giới hạn lượt (HTTP 429): dừng ngay, thoát với mã `2`. Đợi rồi chạy lại đúng lệnh cũ. Nếu thông báo ghi `free-models-per-day` thì phải đợi sang ngày hôm sau.
- Lỗi mạng, lỗi API hoặc AI trả về rỗng: không lưu gì, lần sau hỏi lại cặp đó.
- AI có trả lời nhưng sai định dạng: vẫn lưu lại và ghi `khong_parse_duoc`. Đây là lỗi của AI nên tính vào kết quả, không hỏi lại.
- Chưa đủ 45 cặp có kết luận thì thoát với mã `3`. Đủ hết mới thoát với mã `0`.

Lệnh còn nhận thêm `--submissions`, `--ground-truth`, `--prompt`, `--out` nếu muốn trỏ sang chỗ khác. Bình thường không cần dùng.

## Kết quả nằm ở đâu

- `responses/`: câu trả lời gốc của AI cho từng cặp, mỗi cặp một file. Thư mục này tự tạo khi chạy.
- [`results.csv`](results.csv): mỗi cặp một dòng, đã so sẵn với đáp án.

| Cột | Ý nghĩa |
| --- | --- |
| `pair_id` | Mã cặp, từ `P01` đến `P45` |
| `submission_a`, `submission_b` | Tên hai bài |
| `ai_verdict` | AI kết luận `copy` hay `not_copy` |
| `ai_similarity` | Điểm giống nhau AI chấm, từ 0 đến 100 |
| `response_file` | File câu trả lời gốc trong `responses/` |
| `ground_truth` | Đáp án thật, lấy từ `ground-truth.csv` |
| `level` | Mức sao chép 1–4. Để trống nếu cặp không chép |
| `outcome` | `correct` (AI đúng), `false_positive` (báo nhầm), `missed` (bỏ sót) |
| `latency_seconds` | Thời gian chờ AI trả lời một cặp |
| `input_tokens`, `output_tokens` | Số token, nếu OpenRouter có trả về |
| `notes` | Lỗi ghi nhận được, ví dụ không đọc được câu trả lời hoặc model trả về khác model đã chọn |

## Nhận xét

Sẽ viết sau khi có kết quả: AI bắt được bao nhiêu cặp ở mỗi mức, báo nhầm bao nhiêu, mất bao lâu, tốn bao nhiêu token, và AI hay sai kiểu gì.

Lưu ý khi đọc kết quả: mỗi mức chỉ có 1 hoặc 2 cặp, nên tỷ lệ theo mức chỉ mang tính tham khảo.

## Còn cần nhóm xác nhận

`ground-truth.csv` chỉ ghi 5 cặp chép. 40 cặp còn lại đang được tính là không chép, kể cả cặp giữa hai bài chép khác nhau, hay bài chép với một bài gốc không phải bản gốc của nó. Cần mentor hoặc nhóm xác nhận cách tính này.
