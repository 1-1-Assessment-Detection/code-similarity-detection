# Phân tích lỗi phát hiện mã nguồn tương đồng - Tuần 1

Issue liên quan: #5 Analyze and Document Detection Errors.

Tài liệu tổng hợp kết quả thực nghiệm Tuần 1 trên bài mẫu [204. Đếm Số Nguyên Tố (Count Primes)](../sample-problem.md) bằng Python (10 bài nộp, tổng cộng 45 cặp).

## 1. Dữ liệu đầu vào

- Bộ dữ liệu mẫu (#2, nhánh `datasets/week-01`, commit cuối `8f2e750`, đã merge vào `main` qua PR #12 tại commit `c224034`): gồm 5 bài tự viết (`ORIG-01` đến `ORIG-05`), 5 bài sao chép theo 4 mức (`COPY-01` đến `COPY-05`) và file nhãn [`ground-truth.csv`](../../../datasets/week-01/ground-truth.csv). Trong 45 cặp có 5 cặp sao chép (1 cặp Mức 1, 1 cặp Mức 2, 2 cặp Mức 3, 1 cặp Mức 4) và 40 cặp không sao chép.
- Kết quả so khớp văn bản (#3, nhánh `feature/week-01-text-based`, commit `8aafc83`): lấy từ [`results.csv`](../../../experiments/week-01/text-based/results.csv) và [`README.md`](../../../experiments/week-01/text-based/README.md).
- Kết quả hỏi trực tiếp AI (#4, nhánh `feature/direct-ai-experiment`, commit `12cb179`): lấy từ [`results.csv`](../../../experiments/week-01/direct-ai/results.csv), [`prompt.md`](../../../experiments/week-01/direct-ai/prompt.md), [`README.md`](../../../experiments/week-01/direct-ai/README.md) và 45 file phản hồi gốc trong `responses/`.

### Ghi chú về lệch phiên bản dữ liệu

Khi kiểm tra lại commit giữa #2, #3 và #4 thì thấy hai phương pháp đang đo trên hai bản dữ liệu khác nhau:

- Trên nhánh `datasets/week-01`, commit `8f2e750` đã xóa dòng comment `# Same code; only whitespace and this comment were changed.` trong `copied/copy-01-level-1.py` (file giảm từ 429 xuống 370 ký tự khi chuẩn hóa LF).
- Thực nghiệm #4 chạy trên bản mới `8f2e750`. Còn #3 dù đã có commit mới `8aafc83` cũng chỉ đổi tên nhãn trong CSV sang tiếng Việt, số đo vẫn chạy trên bản cũ `c11949a` khi vẫn còn dòng comment này, nên cặp `ORIG-01 / COPY-01` có khoảng cách Levenshtein là 96 (điểm 0,776224).
- Khi tính thử lại Levenshtein theo đúng công thức của #3 trên bản `8f2e750`, cặp `ORIG-01 / COPY-01` giảm khoảng cách còn 37 (điểm 0,900000, vượt ngưỡng 0,80) và 8 cặp không sao chép còn lại có chứa `copied/copy-01-level-1.py` đều dưới 0,80 (cao nhất 0,601078 với `original/solution-05.py`) nên không phát sinh thêm báo nhầm.
- Các bảng bên dưới vẫn giữ số chính thức từ `results.csv` của #3; cần báo bên #3 chạy lại trên `main` hiện tại để hai phương pháp dùng cùng một bộ file.

## 2. Kết quả phương pháp so khớp văn bản (#3)

Chương trình C# của #3 đọc nguyên văn 10 file UTF-8, chỉ đổi xuống dòng về LF, còn comment, khoảng trắng và tên biến giữ nguyên. Thực nghiệm #3 chưa ghi nhận thời gian chạy trong `results.csv` hay `README.md`. Có 3 cách đo được chạy trên đủ 45 cặp:

- `string.Equals`: so khớp giống hệt toàn bộ chuỗi (`Ordinal`).
- `string.Contains`: kiểm tra 2 chiều xem bài này có chứa nguyên bài kia không (`Ordinal`).
- Levenshtein: tính điểm `(max_len - distance) / max_len`, chọn ngưỡng từ 0,80 trở lên là tương đồng.

### 2.1. Tổng hợp trên 45 cặp

| Phương pháp                 | TP (đúng có chép) | TN (đúng không chép) | FP (báo nhầm) | FN (bỏ sót) |
| --------------------------- | ----------------- | -------------------- | ------------- | ----------- |
| `string.Equals`             | 0                 | 40                   | 0             | 5           |
| `string.Contains` (2 chiều) | 0                 | 40                   | 0             | 5           |
| Levenshtein (ngưỡng 0,80)   | 1                 | 40                   | 0             | 4           |

### 2.2. Chi tiết 5 cặp sao chép

| Cặp bài           | File đối chiếu                          | Mức | Equals | Contains | Dist | Điểm Lev. | KQ Lev. |
| ----------------- | --------------------------------------- | --- | ------ | -------- | ---- | --------- | ------- |
| ORIG-01 / COPY-01 | `solution-01.py` / `copy-01-level-1.py` | 1   | FN     | FN       | 96   | 0,776224  | FN      |
| ORIG-02 / COPY-02 | `solution-02.py` / `copy-02-level-2.py` | 2   | FN     | FN       | 219  | 0,646774  | FN      |
| ORIG-03 / COPY-03 | `solution-03.py` / `copy-03-level-3.py` | 3   | FN     | FN       | 347  | 0,661793  | FN      |
| ORIG-04 / COPY-04 | `solution-04.py` / `copy-04-level-3.py` | 3   | FN     | FN       | 50   | 0,892934  | TP      |
| ORIG-05 / COPY-05 | `solution-05.py` / `copy-05-level-4.py` | 4   | FN     | FN       | 215  | 0,552083  | FN      |

Các chỉnh sửa trong 5 bản sao:

- **ORIG-01 / COPY-01 (Mức 1):** chỉ thêm 1 dòng comment (ở bản `c11949a`) và tách 2 câu lệnh `if`, `for` viết 1 dòng ra thành nhiều dòng.
- **ORIG-02 / COPY-02 (Mức 2):** logic sàng số lẻ không đổi nhưng đổi hết tên tham số và biến cục bộ (`n` -> `limit`, `crossed` -> `composite_flags`, `p` -> `divisor`...), cộng thêm tách dòng ở lệnh `if`.
- **ORIG-03 / COPY-03 (Mức 3):** sửa nhiều vòng lặp (vòng sàng cơ sở đổi `for` -> `while`, vòng duyệt block đổi `while` -> `for`, đoạn gán slice chuyển sang `for` đánh dấu từng phần tử), đồng thời đổi tên biến (`sieve` -> `base`, `segment` -> `chunk`).
- **ORIG-04 / COPY-04 (Mức 3):** sửa ít nhất trong nhóm Mức 3, chỉ đổi vòng `for value in range(2, n)` ngoài cùng thành `while value < n` với `value += 1`, phần còn lại gần như y nguyên.
- **ORIG-05 / COPY-05 (Mức 4):** bản AI viết lại vẫn dùng `bytearray` và cùng cách tính độ dài slice, nhưng đổi sạch tên biến và viết khác các biểu thức (`b'\x01'` -> `[1]`, `flags.count(1)` -> `sum(is_prime)`).

### 2.3. Thống kê 40 cặp không sao chép

- Cả 40 cặp đều được xếp đúng là TN ở cả 3 cách đo (không có báo nhầm ở ngưỡng 0,80).
- Điểm Levenshtein của nhóm này dao động từ 0,263158 đến 0,638814, trung bình 0,420041 (tính theo bản cũ `c11949a`; nếu chạy lại trên `8f2e750` thì cặp `copied/copy-01-level-1.py` / `original/solution-05.py` tăng lên 0,601078 và nhảy từ hạng 5 lên hạng 2).
- 5 cặp không sao chép có điểm Levenshtein cao nhất:

| Cặp bài                                                 | Dist | Điểm Lev. | So với cặp Mức 4 (0,552083) |
| ------------------------------------------------------- | ---- | --------- | --------------------------- |
| `original/solution-01.py` / `original/solution-05.py`   | 134  | 0,638814  | Cao hơn                     |
| `copied/copy-05-level-4.py` / `original/solution-01.py` | 195  | 0,593750  | Cao hơn                     |
| `original/solution-02.py` / `original/solution-05.py`   | 190  | 0,573034  | Cao hơn                     |
| `original/solution-01.py` / `original/solution-02.py`   | 206  | 0,537079  | Thấp hơn                    |
| `copied/copy-01-level-1.py` / `original/solution-05.py` | 208  | 0,515152  | Thấp hơn                    |

Trong nhóm này có 3 cặp không sao chép đạt điểm cao hơn cặp Mức 4 (`ORIG-05 / COPY-05`, 0,552083), thậm chí `copied/copy-05-level-4.py` còn có điểm văn bản gần `original/solution-01.py` hơn cả bài gốc `original/solution-05.py`. Điểm này sẽ được phân tích kỹ hơn ở bước đối chiếu và giải thích nguyên nhân sai.

## 3. Kết quả phương pháp hỏi trực tiếp AI (#4)

Thực nghiệm #4 chạy ngày 2026-11-27 qua OpenRouter trên model `nvidia/nemotron-3-ultra-550b-a55b:free` (cỡ 550B tham số). Chương trình dùng prompt thô `v1` (chưa có JSON schema, Tool Calling hay vòng tự kiểm tra), giữ temperature mặc định và chỉ chạy 1 lượt duy nhất. Trước đó nhóm thử `google/gemma-4-31b-it:free` nhưng bị lỗi HTTP 429 ngay cặp đầu nên phải đổi sang nemotron.

Chương trình C# chạy tuần tự từng cặp, không chèn thời gian nghỉ (`delay`) giữa các lượt và không tự gọi lại khi gặp HTTP 429; cột `latency_seconds` đo trực tiếp thời gian phản hồi API của từng cặp bằng `Stopwatch`. Mỗi cặp được gửi trong một lượt hỏi riêng và chỉ chứa nội dung code (không đưa tên file hay đáp án). 
Prompt `v1` chỉ hỏi bài B có phải được sao chép/viết lại từ bài A (hoặc ngược lại) hay không, yêu cầu trả 3 dòng `KET_LUAN`, `DIEM_GIONG` (0-100) và `GIAI_THICH` (nêu rõ hàm nào, dòng nào, chỉnh sửa kiểu gì) mà không định nghĩa thang điểm hay tiêu chí phân biệt trùng thuật toán phổ biến. Nhãn `copy` / `not_copy` do AI tự kết luận trực tiếp ở dòng `KET_LUAN`, không áp ngưỡng cứng trên điểm `DIEM_GIONG` như ngưỡng 0,80 ở Mục 2. Mã cặp `P01`-`P45` do script #4 tự đánh số khi ghép cặp theo thứ tự bảng chữ cái của tên file.

### 3.1. Kết quả tổng hợp và 5 cặp sao chép

Trên cả 45 cặp, AI kết luận đúng **5 TP, 40 TN, 0 FP, 0 FN**; cả 45 câu trả lời đều đúng định dạng mẫu (0 lỗi `khong_parse_duoc`). Để khớp chiều với Mục 2.2, bảng dưới ghi tên file theo thứ tự gốc / sao (riêng trong `results.csv` của #4, do sắp theo bảng chữ cái nên `copy-*` đứng ở vị trí Bài A còn `solution-*` ở vị trí Bài B).

| Mã cặp | Cặp bài           | File đối chiếu                          | Mức | Kết luận | Điểm AI | Thời gian (giây) |
| ------ | ----------------- | --------------------------------------- | --- | -------- | ------- | ---------------- |
| P05    | ORIG-01 / COPY-01 | `solution-01.py` / `copy-01-level-1.py` | 1   | copy     | 99      | 15,90            |
| P14    | ORIG-02 / COPY-02 | `solution-02.py` / `copy-02-level-2.py` | 2   | copy     | 95      | 25,26            |
| P22    | ORIG-03 / COPY-03 | `solution-03.py` / `copy-03-level-3.py` | 3   | copy     | 90      | 47,18            |
| P29    | ORIG-04 / COPY-04 | `solution-04.py` / `copy-04-level-3.py` | 3   | copy     | 95      | 20,60            |
| P35    | ORIG-05 / COPY-05 | `solution-05.py` / `copy-05-level-4.py` | 4   | copy     | 95      | 32,05            |

### 3.2. Nhóm 40 cặp không sao chép

- Cả 40 cặp đều ra `not_copy` (0 báo nhầm), điểm AI từ 0 đến 40/100, trung bình 12,88.
- Các cặp không sao chép có điểm AI từ 30 trở lên (giữ cùng đường dẫn file như Mục 2.3 để tiện đối chiếu):

| Mã cặp | Cặp bài                                                   | Điểm AI | Thời gian (giây) |
| ------ | --------------------------------------------------------- | ------- | ---------------- |
| P39    | `original/solution-01.py` / `original/solution-05.py`     | 40      | 54,92            |
| P04    | `copied/copy-01-level-1.py` / `copied/copy-05-level-4.py` | 35      | 46,80            |
| P09    | `copied/copy-01-level-1.py` / `original/solution-05.py`   | 35      | 27,66            |
| P42    | `original/solution-02.py` / `original/solution-05.py`     | 35      | 27,12            |
| P31    | `copied/copy-05-level-4.py` / `original/solution-01.py`   | 30      | 21,87            |
| P32    | `copied/copy-05-level-4.py` / `original/solution-02.py`   | 30      | 43,19            |

### 3.3. Thời gian chạy và token ghi nhận

- Tổng thời gian phản hồi API cho 45 cặp là 1.283,63 giây (~21,4 phút), trung bình 28,53 giây/cặp. Nhanh nhất là `P38` (`original/solution-01.py` / `original/solution-04.py`, 5 điểm) mất 10,49 giây; chậm nhất là `P23` (`copied/copy-03-level-3.py` / `original/solution-04.py`, 5 điểm) mất 65,27 giây.
- Tổng token: 23.850 đầu vào (~530 token/cặp) và 37.123 đầu ra (~825 token/cặp), tổng 60.973 token. Lượng token đầu ra gấp khoảng 1,56 lần đầu vào dù câu trả lời chỉ gồm 3 mục ngắn; log của #4 không tách riêng trường `reasoning_tokens` nên chưa xác nhận trực tiếp, nhưng nhiều khả năng model tính cả phần suy luận ngầm vào output token.
- Lượt chạy này không mất tiền vì dùng bản `:free` trên OpenRouter, nhưng con số 0 đồng không phản ánh chi phí chạy API thương mại (chưa kể giới hạn lượt như lỗi HTTP 429 ở trên). Các vấn đề về độ trễ khi tăng số lượng bài nộp trong lớp và chất lượng giải thích trong `responses/` sẽ được phân tích ở các bước tiếp theo.
