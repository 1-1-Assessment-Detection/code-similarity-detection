# Thực nghiệm tuần 1: hỏi trực tiếp AI

Thư mục này phục vụ issue #4. Kết quả sẽ được chuyển sang #5 để tổng hợp bảng phân tích lỗi.

> **Trạng thái:** Đã chạy đủ 45/45 cặp ngày 2026-11-27. Kết quả ở [`results.csv`](results.csv) và thư mục [`responses/`](responses/), nhận xét ở cuối file.

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
| Model | `nvidia/nemotron-3-ultra-550b-a55b:free` |
| Temperature | Để mặc định, nên chạy lại có thể ra kết quả hơi khác |
| Prompt | `v1` |
| Dữ liệu | `datasets/week-01/`, commit `8f2e750` |
| Ngày chạy | 2026-11-27 |


Model AI chính thức của sản phẩm chưa chốt; DeepSeek đang là lựa chọn dự kiến. Khi nhóm chốt, cần chạy lại `v1` bằng đúng model đó. Có như vậy thì kết quả so sánh Phương án B với Phương án C (`v2`, `v3`) ở tuần 7–8 mới công bằng.

Cấu hình đặt trong file `.env.local` ở thư mục gốc repo, xem mẫu ở [`.env.example`](../../../.env.example). File này đã nằm trong `.gitignore`, đừng commit khóa lên.

| Biến | Ý nghĩa |
| --- | --- |
| `AI_ENDPOINT` | Địa chỉ API. Để trống thì dùng OpenRouter. Với DeepSeek thì điền `https://api.deepseek.com/chat/completions` |
| `AI_API_KEY` | Khóa API của dịch vụ đó |
| `AI_MODEL` | Tên model, ví dụ `nvidia/nemotron-3-ultra-550b-a55b:free` hoặc `deepseek-chat` |

Chương trình dùng được mọi dịch vụ theo chuẩn OpenAI Chat Completions, nên muốn đổi model hay đổi nơi gọi AI chỉ cần sửa `.env.local`, không phải sửa code. Tên biến cũ `OPENROUTER_API_KEY` và `OPENROUTER_MODEL` vẫn dùng được.

### Đổi sang model khác

Dịch vụ nào theo chuẩn OpenAI Chat Completions cũng dùng được. Một số cấu hình thường gặp:

| Muốn dùng | `AI_ENDPOINT` | `AI_MODEL` ví dụ |
| --- | --- | --- |
| OpenRouter: một khóa dùng được nhiều model như Claude, GPT, Gemini, DeepSeek, Llama | để trống | `nvidia/nemotron-3-ultra-550b-a55b:free`, `deepseek/deepseek-chat` |
| DeepSeek gọi trực tiếp | `https://api.deepseek.com/chat/completions` | `deepseek-chat` |
| OpenAI gọi trực tiếp | `https://api.openai.com/v1/chat/completions` | tên model của OpenAI |
| Model chạy trên máy bằng Ollama | `http://localhost:11434/v1/chat/completions` | `qwen3`, `llama3` (khóa điền gì cũng được) |

Claude và Gemini không gọi thẳng được vì API riêng của Anthropic và Google có định dạng khác. Muốn dùng thì gọi qua OpenRouter. Hiện nhóm mới chạy thật với OpenRouter; các dòng còn lại lấy theo tài liệu của từng dịch vụ, nên kiểm tra lại tên model trước khi chạy.

Khi đổi model, nhớ:

- **Mỗi model ghi vào một thư mục riêng**, ví dụ:

  ```powershell
  dotnet run --project experiments/week-01/direct-ai/DirectAiExperiment -- --run --out experiments/week-01/direct-ai/deepseek
  ```

  Nếu chạy model mới vào thư mục đã có kết quả của model khác, chương trình sẽ dừng để không trộn kết quả.
- Model loại "suy luận" đôi khi trả lời rỗng vì dùng hết token để nghĩ. Chương trình sẽ hỏi lại các cặp đó ở lần chạy sau.
- Model nhỏ hay trả lời sai định dạng `KET_LUAN` / `DIEM_GIONG`. Những cặp đó được ghi `khong_parse_duoc` và tính là lỗi của model.
- Dịch vụ trả phí sẽ tính tiền. Cả 45 cặp tốn khoảng 60 nghìn token, tính theo lần chạy nemotron.

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

**AI kết luận đúng cả 45 cặp.** AI bắt được đủ 5 cặp chép và không báo nhầm cặp nào trong 40 cặp không chép. Cả 45 câu trả lời đều đúng định dạng, không cặp nào phải hỏi lại.

| Mức | Cặp | AI kết luận | Điểm AI chấm |
| --- | --- | --- | --- |
| 1: đổi khoảng trắng | `copy-01` / `solution-01` | copy ✅ | 99 |
| 2: đổi tên biến | `copy-02` / `solution-02` | copy ✅ | 95 |
| 3: đổi vòng lặp | `copy-03` / `solution-03` | copy ✅ | 90 |
| 3: đổi vòng lặp | `copy-04` / `solution-04` | copy ✅ | 95 |
| 4: AI viết lại | `copy-05` / `solution-05` | copy ✅ | 95 |

Ở 40 cặp không chép, điểm cao nhất chỉ là 40 (`solution-01` / `solution-05`, hai bài cùng dùng sàng Eratosthenes), trung bình khoảng 13. Còn 5 cặp chép đều từ 90 trở lên, nên hai nhóm cách nhau rất xa. Với cặp `solution-01` / `solution-05`, AI nhận ra hai bài chỉ giống nhau vì cùng thuật toán kinh điển chứ không phải chép. So khớp văn bản (#3) lại chấm cặp này cao hơn cả cặp mức 4.

**Thời gian và chi phí:** trung bình khoảng 28,5 giây mỗi cặp (từ 10,5 đến 65,3 giây). Cả 45 cặp mất khoảng 21 phút. Tổng cộng 23.850 token đầu vào và 37.123 token đầu ra. Model miễn phí nên không tốn tiền. Số token đầu ra cao hơn đầu vào, có thể vì model này tính cả phần suy luận vào token đầu ra.

**Hạn chế đã thấy:**

- **Không nêu số dòng.** Prompt yêu cầu nói rõ giống ở dòng nào, nhưng không câu trả lời nào ghi số dòng cụ thể. AI chỉ mô tả bằng tên biến và câu lệnh. Đây là lý do cần ép khuôn JSON có `from_line` / `to_line` và kiểm tra số dòng ở `v2`.
- **Tự đoán bài nào chép bài nào.** Cả 5 cặp sao chép đều ghép theo thứ tự bài chép là bài A, bài gốc là bài B, do chương trình sắp tên file theo bảng chữ cái. Vậy mà ở 4/5 cặp, AI nói ngược lại, tức coi bài gốc là bài đã sửa từ bài chép. Ví dụ ở `copy-02` / `solution-02`, AI viết "Bài B chỉ đổi tên biến (`limit`->`n`…)". Chỉ nhìn hai đoạn code thì không biết bài nào có trước, nên những câu khẳng định chiều sao chép này không có căn cứ.
- **Chậm và phụ thuộc nhà cung cấp.** Mỗi cặp mất 10–65 giây. Model miễn phí còn hay bị giới hạn lượt hoặc quá tải, như lần gemma ở trên. Với cả lớp 30–50 bài, tức 435–1.225 cặp, cách hỏi từng cặp sẽ rất chậm. Đó là lý do mentor đề xuất lọc nhanh bằng thuật toán trước rồi mới gửi ca khó cho AI.
- **Mẫu còn nhỏ.** Mỗi mức chỉ có 1–2 cặp, và các bài đều ngắn (10–26 dòng). Kết quả 45/45 chưa đủ để kết luận AI luôn đúng. Cũng chưa chạy lại lần hai để xem AI có trả lời giống nhau không.

## Còn cần nhóm xác nhận

`ground-truth.csv` chỉ ghi 5 cặp chép. 40 cặp còn lại đang được tính là không chép, kể cả cặp giữa hai bài chép khác nhau, hay bài chép với một bài gốc không phải bản gốc của nó. Cần mentor hoặc nhóm xác nhận cách tính này.
