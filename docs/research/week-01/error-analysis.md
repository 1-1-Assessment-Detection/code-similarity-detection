# Phân tích lỗi phát hiện mã nguồn tương đồng - Tuần 1

Issue liên quan: #5 Analyze and Document Detection Errors.

Tài liệu tổng hợp kết quả thực nghiệm Tuần 1 trên bài mẫu [204. Đếm Số Nguyên Tố (Count Primes)](../sample-problem.md) bằng Python (10 bài nộp, tổng cộng 45 cặp).

## 1. Dữ liệu đầu vào

- Bộ dữ liệu mẫu (#2): gồm 5 bài tự viết (`ORIG-01` đến `ORIG-05`), 5 bài sao chép theo 4 mức (`COPY-01` đến `COPY-05`) và file nhãn [`ground-truth.csv`](../../../datasets/week-01/ground-truth.csv). Trong 45 cặp có 5 cặp sao chép (1 cặp Mức 1, 1 cặp Mức 2, 2 cặp Mức 3, 1 cặp Mức 4) và 40 cặp không sao chép.
- Kết quả so khớp văn bản (#3): lấy từ [`results.csv`](../../../experiments/week-01/text-based/results.csv) và [`README.md`](../../../experiments/week-01/text-based/README.md).
- Kết quả hỏi trực tiếp AI (#4): lấy từ [`results.csv`](../../../experiments/week-01/direct-ai/results.csv), [`prompt.md`](../../../experiments/week-01/direct-ai/prompt.md), [`README.md`](../../../experiments/week-01/direct-ai/README.md) và 45 file phản hồi gốc trong `responses/`.

### Ghi chú về lệch phiên bản dữ liệu

Khi kiểm tra lại dữ liệu giữa #2, #3 và #4 thì thấy hai phương pháp đang đo trên hai phiên bản hơi khác nhau của file `copied/copy-01-level-1.py`:

- Bản bộ mẫu mới nhất đã xóa dòng comment `# Same code; only whitespace and this comment were changed.` trong `copied/copy-01-level-1.py` để tránh lộ gợi ý cho AI (độ dài file giảm từ 429 xuống 370 ký tự khi chuẩn hóa LF).
- Thực nghiệm #4 chạy trên bản mới đã xóa comment này. Còn số liệu trong `results.csv` của #3 vẫn giữ kết quả chạy trên bản trước đó khi vẫn còn dòng comment, nên cặp `ORIG-01 / COPY-01` có khoảng cách Levenshtein là 96 (điểm 0,776224).
- Khi tính thử lại Levenshtein theo đúng công thức của #3 trên bản mới đã xóa comment, cặp `ORIG-01 / COPY-01` giảm khoảng cách còn 37 (điểm 0,900000, vượt ngưỡng 0,80) và 8 cặp không sao chép còn lại có chứa `copied/copy-01-level-1.py` đều dưới 0,80 (cao nhất 0,601078 với `original/solution-05.py`) nên không phát sinh thêm báo nhầm.
- Các bảng bên dưới vẫn giữ đúng số chính thức từ `results.csv` của #3; cần báo bên #3 chạy lại trên bản bộ mẫu mới nhất để hai phương pháp dùng cùng một bộ file.

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

- **ORIG-01 / COPY-01 (Mức 1):** chỉ thêm 1 dòng comment (ở bản cũ) và tách 2 câu lệnh `if`, `for` viết 1 dòng ra thành nhiều dòng.
- **ORIG-02 / COPY-02 (Mức 2):** logic sàng số lẻ không đổi nhưng đổi hết tên tham số và biến cục bộ (`n` -> `limit`, `crossed` -> `composite_flags`, `p` -> `divisor`...), cộng thêm tách dòng ở lệnh `if`.
- **ORIG-03 / COPY-03 (Mức 3):** sửa nhiều vòng lặp (vòng sàng cơ sở đổi `for` -> `while`, vòng duyệt block đổi `while` -> `for`, đoạn gán slice chuyển sang `for` đánh dấu từng phần tử), đồng thời đổi tên biến (`sieve` -> `base`, `segment` -> `chunk`).
- **ORIG-04 / COPY-04 (Mức 3):** sửa ít nhất trong nhóm Mức 3, chỉ đổi vòng `for value in range(2, n)` ngoài cùng thành `while value < n` với `value += 1`, phần còn lại gần như y nguyên.
- **ORIG-05 / COPY-05 (Mức 4):** bản AI viết lại vẫn dùng `bytearray` và cùng cách tính độ dài slice, nhưng đổi sạch tên biến và viết khác các biểu thức (`b'\x01'` -> `[1]`, `flags.count(1)` -> `sum(is_prime)`).

### 2.3. Thống kê 40 cặp không sao chép

- Cả 40 cặp đều được xếp đúng là TN ở cả 3 cách đo (không có báo nhầm ở ngưỡng 0,80).
- Điểm Levenshtein của nhóm này dao động từ 0,263158 đến 0,638814, trung bình 0,420041 (tính theo bản bộ mẫu cũ; nếu chạy lại trên bản đã xóa comment thì `copied/copy-01-level-1.py` / `original/solution-05.py` tăng lên 0,601078 vào hạng 2 và `copied/copy-01-level-1.py` / `copied/copy-05-level-4.py` tăng lên 0,593750 đồng hạng 3, đẩy `original/solution-01.py` / `original/solution-02.py` ra khỏi Top 5).
- 5 cặp không sao chép có điểm Levenshtein cao nhất:

| Cặp bài                                                 | Dist | Điểm Lev. | So với cặp Mức 4 (0,552083) |
| ------------------------------------------------------- | ---- | --------- | --------------------------- |
| `original/solution-01.py` / `original/solution-05.py`   | 134  | 0,638814  | Cao hơn                     |
| `copied/copy-05-level-4.py` / `original/solution-01.py` | 195  | 0,593750  | Cao hơn                     |
| `original/solution-02.py` / `original/solution-05.py`   | 190  | 0,573034  | Cao hơn                     |
| `original/solution-01.py` / `original/solution-02.py`   | 206  | 0,537079  | Thấp hơn                    |
| `copied/copy-01-level-1.py` / `original/solution-05.py` | 208  | 0,515152  | Thấp hơn                    |

Trong nhóm này có 3 cặp không sao chép đạt điểm cao hơn cặp Mức 4 (`ORIG-05 / COPY-05`, 0,552083), thậm chí `copied/copy-05-level-4.py` còn có điểm văn bản gần `original/solution-01.py` hơn cả bài gốc `original/solution-05.py`.

## 3. Kết quả phương pháp hỏi trực tiếp AI (#4)

Thực nghiệm #4 chạy ngày 2026-11-27 qua OpenRouter trên model `nvidia/nemotron-3-ultra-550b-a55b:free` (cỡ 550B tham số). Chương trình dùng prompt thô `v1` (chưa có JSON schema, Tool Calling hay vòng tự kiểm tra), giữ temperature mặc định và chỉ chạy 1 lượt duy nhất. Trước đó nhóm thử `google/gemma-4-31b-it:free` nhưng bị lỗi HTTP 429 ngay cặp đầu nên phải đổi sang nemotron.

Chương trình C# chạy tuần tự từng cặp, không chèn thời gian nghỉ (`delay`) giữa các lượt và không tự gọi lại khi gặp HTTP 429; cột `latency_seconds` đo trực tiếp thời gian phản hồi API của từng cặp bằng `Stopwatch`. Mã cặp `P01`-`P45` do script #4 tự đánh số khi ghép cặp theo thứ tự bảng chữ cái của tên file.

Mỗi cặp được gửi trong một lượt hỏi riêng và chỉ chứa nội dung code (không đưa tên file hay đáp án). Prompt `v1` chỉ hỏi bài B có phải được sao chép/viết lại từ bài A (hoặc ngược lại) hay không, yêu cầu trả 3 dòng `KET_LUAN`, `DIEM_GIONG` (0-100) và `GIAI_THICH` (nêu rõ hàm nào, dòng nào, chỉnh sửa kiểu gì) mà không định nghĩa thang điểm hay tiêu chí phân biệt trùng thuật toán phổ biến. Nhãn `copy` / `not_copy` do AI tự kết luận trực tiếp ở dòng `KET_LUAN`, không áp ngưỡng cứng trên điểm `DIEM_GIONG` như ngưỡng 0,80 ở Mục 2.

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

## 4. Đối chiếu hai phương pháp và so sánh theo 4 mức độ

Nhãn kết quả của #3 (`Phát hiện đúng`, `Loại đúng`, `Bỏ sót`, `Báo nhầm`) và #4 (`correct`, `false_positive`, `missed`) được quy đổi về chung 4 nhóm TP, TN, FP, FN, trong đó nhãn `correct` của #4 được tách thành TP (với 5 cặp sao chép) và TN (với 40 cặp không sao chép) dựa trên `ground-truth.csv`. Mã cặp `P01`-`P45` của #4 được dùng làm mã định danh chung khi ghép 45 cặp.

### 4.1. Bảng tổng hợp đối chiếu trên 45 cặp

| Phương pháp                 | Điều kiện / Cấu hình                       | TP | TN | FP | FN | Tỷ lệ phát hiện | Thời gian (45 cặp)         | Token / chi phí           |
| --------------------------- | ------------------------------------------ | -- | -- | -- | -- | --------------- | -------------------------- | ------------------------- |
| `string.Equals` (#3)        | Khớp toàn bộ chuỗi (`Ordinal`)             | 0  | 40 | 0  | 5  | 0% (0/5)        | Chưa đo trong thực nghiệm  | Không dùng API            |
| `string.Contains` (#3)      | A chứa B hoặc B chứa A (`Ordinal`)         | 0  | 40 | 0  | 5  | 0% (0/5)        | Chưa đo trong thực nghiệm  | Không dùng API            |
| Levenshtein (#3)            | `(max_len - dist) / max_len >= 0,80`       | 1  | 40 | 0  | 4  | 20% (1/5)       | Chưa đo trong thực nghiệm  | Không dùng API            |
| Hỏi trực tiếp AI `v1` (#4)  | `nemotron-3-ultra-550b-a55b:free` (1 lượt) | 5  | 40 | 0  | 0  | 100% (5/5)      | 1.283,63 giây (~21,4 phút) | 60.973 token (bản `:free`)|

Tỷ lệ báo nhầm (FPR) của cả 4 cách đo trên 40 cặp không sao chép đều là 0%. Nếu #3 chạy lại trên bản bộ mẫu mới đã xóa comment (cùng bản dữ liệu với #4), dòng Levenshtein tăng lên **2 TP, 40 TN, 0 FP, 3 FN (tỷ lệ phát hiện 40%, tức 2/5)** do `P05` (`ORIG-01 / COPY-01`) đạt 0,900000; các phương pháp còn lại giữ nguyên.

### 4.2. Phân loại kết quả và so sánh theo 4 mức độ sao chép

| Mã cặp | Cặp bài           | Mức | Equals (#3) | Contains (#3) | Levenshtein (#3) | Direct AI `v1` (#4) |
| ------ | ----------------- | --- | ----------- | ------------- | ---------------- | ------------------- |
| P05    | ORIG-01 / COPY-01 | 1   | FN          | FN            | FN (0,776224)    | TP (99/100)         |
| P14    | ORIG-02 / COPY-02 | 2   | FN          | FN            | FN (0,646774)    | TP (95/100)         |
| P22    | ORIG-03 / COPY-03 | 3   | FN          | FN            | FN (0,661793)    | TP (90/100)         |
| P29    | ORIG-04 / COPY-04 | 3   | FN          | FN            | TP (0,892934)    | TP (95/100)         |
| P35    | ORIG-05 / COPY-05 | 4   | FN          | FN            | FN (0,552083)    | TP (95/100)         |
| 40 cặp | Không sao chép    | -   | 40 TN, 0 FP | 40 TN, 0 FP   | 40 TN, 0 FP      | 40 TN, 0 FP         |

Phân loại theo kết quả trên đủ 45 cặp:

- **Phát hiện đúng (TP):** `Equals` và `Contains` đạt 0/5; Levenshtein bắt được duy nhất `P29` (`ORIG-04 / COPY-04`); Direct AI `v1` bắt đủ cả 5 cặp (`P05`, `P14`, `P22`, `P29`, `P35`).
- **Bỏ sót (FN):** `Equals` và `Contains` bỏ sót cả 5 cặp; Levenshtein bỏ sót 4 cặp (`P05`, `P14`, `P22`, `P35`); Direct AI `v1` không bỏ sót cặp nào.
- **Loại đúng (TN) và báo nhầm (FP):** tất cả các cách đo đều đạt 40/40 TN và 0 FP trên bộ mẫu này.

Tỷ lệ phát hiện theo từng mức độ sao chép (Levenshtein so với Direct AI `v1`, còn `Equals` và `Contains` đều 0/5 ở cả 4 mức):

- **Mức 1:** Levenshtein 0/1 (bản đã xóa comment đạt 1/1 với 0,900000) vs AI 1/1 (99/100).
- **Mức 2:** Levenshtein 0/1 (0,646774) vs AI 1/1 (95/100).
- **Mức 3:** Levenshtein 1/2 (`P22` 0,661793 bị sót, `P29` 0,892934 bắt được) vs AI 2/2 (90 và 95/100).
- **Mức 4:** Levenshtein 0/1 (0,552083) vs AI 1/1 (95/100).

Điểm Levenshtein không giảm đơn điệu theo mức độ sao chép: cả hai cặp Mức 3 (`0,661793` và `0,892934`) đều cao hơn cặp Mức 2 (`0,646774`), trong khi điểm của Direct AI `v1` giữ ổn định trong khoảng 90-99/100 trên cả 4 mức.

### 4.3. Đối chiếu cụm cặp không sao chép có điểm cao nhất

Bảng dưới gộp 7 cặp không sao chép thuộc Top 5 Levenshtein (Mục 2.3) hoặc có điểm AI từ 30 trở lên (Mục 3.2); cả 7 cặp đều cho kết quả TN ở cả hai phương pháp. Với Levenshtein (#3), bảng giữ cột **bản cũ** (số chính thức từ `results.csv` của #3) cạnh cột **bản mới** (sau khi xóa dòng comment `# Same code; only whitespace and this comment were changed.` ở `copied/copy-01-level-1.py`, cùng bản #4 chạy):

| Mã cặp | Cặp bài                                                   | Levenshtein (bản cũ) | Levenshtein (bản mới) | Direct AI `v1` (#4) |
| ------ | --------------------------------------------------------- | -------------------- | --------------------- | ------------------- |
| P39    | `original/solution-01.py` / `original/solution-05.py`     | 0,638814             | Không đổi             | 40/100              |
| P31    | `copied/copy-05-level-4.py` / `original/solution-01.py`   | 0,593750             | Không đổi             | 30/100              |
| P42    | `original/solution-02.py` / `original/solution-05.py`     | 0,573034             | Không đổi             | 35/100              |
| P36    | `original/solution-01.py` / `original/solution-02.py`     | 0,537079             | Không đổi             | 15/100              |
| P09    | `copied/copy-01-level-1.py` / `original/solution-05.py`   | 0,515152             | 0,601078              | 35/100              |
| P32    | `copied/copy-05-level-4.py` / `original/solution-02.py`   | 0,481250             | Không đổi             | 30/100              |
| P04    | `copied/copy-01-level-1.py` / `copied/copy-05-level-4.py` | 0,468750             | 0,593750              | 35/100              |

Nhìn từ khoảng phân tách điểm giữa 5 cặp sao chép và 40 cặp không sao chép:

- Với Levenshtein (#3), khoảng điểm sao chép (0,552083 - 0,892934) và không sao chép (0,263158 - 0,638814) bị chồng lấn ở Mức 4: trên bản bộ mẫu cũ có 3 cặp không sao chép (`P39`, `P31`, `P42`) vượt điểm cặp Mức 4 `P35` (0,552083); trên bản đã xóa comment có thêm `P09` (0,601078) và `P04` (0,593750) cùng vượt `P35` (tổng 5 cặp).
- Với Direct AI `v1` (#4), hai nhóm cách nhau 50 điểm (nhóm không sao chép cao nhất 40/100 ở `P39`, nhóm sao chép thấp nhất 90/100 ở `P22`). Hai phương pháp có cùng cặp đứng đầu nhóm không sao chép là `P39` (`original/solution-01.py` / `original/solution-05.py`), nhưng ngược chiều về thứ tự điểm giữa `P36` (Levenshtein 0,537079, AI 15/100) và `P32` (Levenshtein 0,481250, AI 30/100) - lưu ý rằng điểm AI chỉ là ước lượng theo bội số 5 không có rubric cố định nên sự chênh lệch này mới mang tính tham khảo. Nguyên nhân của các ca bỏ sót ở #3, rủi ro báo nhầm quanh cụm này và chất lượng giải thích (cùng các sai sót nếu có) trong `responses/` của #4 sẽ được kiểm chứng và phân tích ở Mục 5.

## 5. Phân tích nguyên nhân sai đối với từng trường hợp

### 5.1. Nguyên nhân bỏ sót (FN), tính không đơn điệu và bài toán chọn ngưỡng của so khớp văn bản (#3)

Để nhìn rõ cách công thức Levenshtein `(max_len - dist) / max_len` vận hành trên 5 cặp sao chép, bảng dưới tổng hợp độ dài thật (chuẩn hóa LF), mẫu số `max_len`, khoảng cách chỉnh sửa và tỷ lệ ký tự bị thay đổi của từng cặp:

| Mã cặp | Cặp bài           | Mức | Độ dài gốc | Độ dài sao | `max_len` | `dist` | Tỷ lệ sửa (`dist/max_len`) | Điểm Lev. | Kết quả |
| ------ | ----------------- | --- | ---------- | ---------- | --------- | ------ | -------------------------- | --------- | ------- |
| P05    | ORIG-01 / COPY-01 | 1   | 333        | 429 (cũ)   | 429       | 96     | 22,38%                     | 0,776224  | FN      |
| P05*   | ORIG-01 / COPY-01 | 1   | 333        | 370 (mới)  | 370       | 37     | 10,00%                     | 0,900000  | TP      |
| P14    | ORIG-02 / COPY-02 | 2   | 445        | 620        | 620       | 219    | 35,32%                     | 0,646774  | FN      |
| P22    | ORIG-03 / COPY-03 | 3   | 895        | 1.026      | 1.026     | 347    | 33,82%                     | 0,661793  | FN      |
| P29    | ORIG-04 / COPY-04 | 3   | 435        | 467        | 467       | 50     | 10,71%                     | 0,892934  | TP      |
| P35    | ORIG-05 / COPY-05 | 4   | 371        | 480        | 480       | 215    | 44,79%                     | 0,552083  | FN      |

Phân tích nguyên nhân cụ thể cho từng trường hợp:

- **`string.Equals` và `string.Contains` bỏ sót toàn bộ 5/5 cặp (`P05`, `P14`, `P22`, `P29`, `P35`):** `Equals` yêu cầu hai chuỗi trùng khớp từng ký tự, còn `Contains` chỉ bắt được khi toàn bộ bài này nằm liên tục trong bài kia (tức chỉ thêm code ở đầu hoặc cuối file). Tuy nhiên cả 5 bản sao đều sửa xen vào giữa thân hàm - ngay ở Mức 1 (`P05`), việc tách `if n <= 2: return 0` và vòng `for` từ 1 dòng thành nhiều dòng đã làm đứt đoạn chuỗi khớp liên tục, khiến cả hai chiều kiểm tra đều trả về `False`.
- **Levenshtein bỏ sót Mức 1 (`P05`, 0,776224):** thuật toán so sánh nguyên văn ký tự mà không bỏ comment hay chuẩn hóa khoảng trắng. Bài gốc `original/solution-01.py` chỉ dài 333 ký tự; khi bản cũ của `copied/copy-01-level-1.py` thêm 1 dòng comment dài cùng các dấu xuống dòng thụt lề, độ dài tăng lên 429 ký tự. Khoảng cách 96 ký tự chiếm tới 22,38% độ dài `copy-01` (`96/429`), kéo điểm xuống dưới ngưỡng 0,80. Khi xóa riêng dòng comment này ở bản mới (`P05*`), `dist` giảm còn 37 trên 370 ký tự (10,00%) và điểm tăng lên 0,900000 - cho thấy trên bài ngắn, chỉ một dòng chú thích đã đủ làm phương pháp này bỏ sót.
- **Levenshtein bỏ sót Mức 2 (`P14`, 0,646774):** thuật toán sàng số lẻ giữa `solution-02.py` và `copy-02-level-2.py` giữ nguyên hoàn toàn, nhưng toàn bộ tên tham số và biến ngắn (`n`, `size`, `crossed`, `i`, `p`, `start`, `length`) bị đổi sang tên dài hơn (`limit`, `odd_slots`, `composite_flags`, `slot`, `divisor`, `first_slot`, `view_length`) lặp lại trên nhiều dòng. Việc đổi tên này đẩy độ dài file từ 445 lên 620 ký tự và tạo ra 219 phép sửa (35,32%) dù cấu trúc cú pháp gần như không thay đổi.
- **Tính không đơn điệu ở Mức 3 (`P22` đạt 0,661793 và `P29` đạt 0,892934 đều cao hơn Mức 2 `P14` 0,646774):** Levenshtein chỉ phụ thuộc tỷ lệ `dist / max_len` chứ không phản ánh mức độ biến đổi cấu trúc:
  - `P29` (`solution-04.py` / `copy-04-level-3.py`) chỉ đổi vòng `for value in range(2, n)` ngoài cùng thành `while value < n` kèm `value += 1` và giữ nguyên toàn bộ tên biến cùng thân sàng tuyến tính, nên chỉ lệch 50/467 ký tự (10,71%) và dễ dàng vượt ngưỡng 0,80.
  - Ngược lại, `P22` (`solution-03.py` / `copy-03-level-3.py`) sửa đồng thời 3 vị trí vòng lặp/slice và đổi hàng loạt tên biến nên `dist` lên tới 347 ký tự (cao hơn nhiều so với 219 của `P14`). Dù sửa nhiều hơn, điểm của `P22` (0,661793) vẫn cao hơn `P14` (0,646774) là nhờ **mẫu số `max_len` lớn hơn hẳn**: `copy-03-level-3.py` dài tới 1.026 ký tự nên tỷ lệ sửa `347/1.026` (33,82%) vẫn thấp hơn tỷ lệ `219/620` (35,32%) của `P14`. Điều này cho thấy công thức chuẩn hóa theo `max_len` dễ bị chi phối bởi độ dài tuyệt đối của bản sao.
- **Levenshtein bỏ sót Mức 4 (`P35`, 0,552083):** bản AI viết lại `copy-05-level-4.py` vẫn dùng sàng `bytearray` với cùng công thức tính độ dài slice `(n - 1 - first) // candidate + 1`, nhưng đổi toàn bộ tên biến và viết lại hàng loạt biểu thức tương đương (`n <= 2` -> `n < 3`, `b'\x01'` -> `[1]`, `b'\x00' * (...)` -> `bytearray(count)`, `flags.count(1)` -> `sum(is_prime)`), đẩy tỷ lệ sửa lên 44,79% (`215/480`).
- **Không tồn tại ngưỡng cố định hiệu quả cho Levenshtein:** nếu hạ ngưỡng xuống 0,55 để bắt được Mức 4 (`P35`, 0,552083), hệ thống sẽ dính ngay 3 ca báo nhầm trên bản cũ (`P39` 0,638814, `P31` 0,593750, `P42` 0,573034) và tới 5 ca báo nhầm trên bản đã xóa comment (thêm `P09` 0,601078 và `P04` 0,593750). Ngay cả khi cố chọn ngưỡng sau khi biết đáp án ở mức 0,64 để bắt 4/5 cặp mà không báo nhầm, khe hở giữa cặp không sao chép cao nhất (`P39`, 0,638814) và cặp Mức 2 (`P14`, 0,646774) chỉ là **0,007960** - quá hẹp để có giá trị áp dụng cho đề bài hoặc lớp học khác.

### 5.2. Nguyên nhân điểm cao và rủi ro báo nhầm quanh cụm `solution-01`, `solution-02`, `solution-05`

Cụm 7 cặp không sao chép điểm cao nhất ở Bảng 4.3 đều xoay quanh các biến thể của Sàng Eratosthenes (`solution-01`, `solution-02`, `solution-05` cùng hai bản sao `copy-01`, `copy-05`), nhưng hai phương pháp bị kéo điểm cao theo hai cơ chế khác nhau:

- **Với Levenshtein (#3):** các bài nộp trong cụm này đều ngắn (11-14 dòng, 333-480 ký tự) và dùng chung khung đề bài (`class Solution:`, `def countPrimes(self, n: int) -> int:`, `if n <= 2: return 0`) cùng cấu trúc vòng lặp `while p * p < n:`. Vì phần khung nền giống nhau chiếm tỷ trọng lớn trong tổng độ dài, `P39` (`solution-01` / `solution-05`) đạt tới 0,638814 dù khác nhau về cách đánh dấu hợp số (`list` boolean + vòng `for` so với `bytearray` + gán slice). Tương tự, `P31` (`copy-05` / `solution-01`) đạt 0,593750 - cao hơn cả điểm giữa `copy-05` với chính bài gốc `solution-05` (0,552083) - do bản AI viết lại dùng `is_prime[0] = is_prime[1] = 0` và `return sum(is_prime)`, rất gần về bề mặt ký tự với `prime[0] = prime[1] = False` và `return sum(prime)` của `solution-01`.
- **Với Direct AI `v1` (#4):** AI chấm điểm theo mức độ trùng khớp về họ kỹ thuật cài đặt thay vì chuỗi bề mặt. Cặp `P39` đứng cao nhất (40/100) do cùng cài sàng Eratosthenes duyệt từ 2 đến căn bậc hai của `n`; các cặp `P42` (`solution-02` / `solution-05`, 35/100) và `P32` (`copy-05` / `solution-02`, 30/100) được chấm 30-35 điểm vì cùng dùng kỹ thuật `bytearray` kết hợp gán slice. Ngược lại, `P36` (`solution-01` / `solution-02`) dù đạt 0,537079 ở Levenshtein nhưng khác hẳn cả cấu trúc dữ liệu lẫn cách ánh xạ chỉ số (`list` boolean đầy đủ so với `bytearray` chỉ lưu số lẻ) nên AI chỉ chấm 15/100. Trên bộ mẫu nhỏ AI chưa báo nhầm cặp nào, nhưng việc cả hai phương pháp đều đẩy điểm cao nhất về cụm thuật toán kinh điển cho thấy khi lớp có nhiều bài cùng dùng một mẫu tối ưu phổ biến, nguy cơ báo nhầm vẫn dễ xảy ra nếu AI không biết tần suất xuất hiện trong cả lớp.

### 5.3. Kiểm chứng chi tiết 45 file phản hồi gốc (`responses/`) của hỏi trực tiếp AI (#4)

Rà soát trực tiếp toàn bộ 45 file trong `responses/` cho thấy ở tầng nhận diện thuật toán cốt lõi, AI phân biệt đúng bản chất giải thuật (Sàng Eratosthenes chuẩn, sàng số lẻ, sàng phân đoạn, sàng tuyến tính Euler) trên cả 5 cặp sao chép lẫn 40 cặp không sao chép, không bịa tên hàm không tồn tại; riêng trên 5 cặp sao chép (`P05`, `P14`, `P22`, `P29`, `P35`), AI trích đúng toàn bộ các tên biến đổi và vị trí đổi vòng lặp/slice.

Tuy nhiên, phía sau kết quả 45/45 nhãn đúng, có **6 nhóm lỗi và hạn chế thực tế** trong nội dung `GIAI_THICH` và điểm số:

- **Lỗi 1 - 0/45 câu trả lời nêu được số dòng cụ thể:** dù `prompt v1` yêu cầu rõ *"nêu rõ giống ở hàm nào, dòng nào"*, không có câu trả lời nào trong cả 45 file ghi số dòng (AI chỉ mô tả bằng tên biến và câu lệnh). Có hai nguyên nhân gốc rễ đồng thời: (1) script C# của #4 đọc nguyên văn file nguồn bằng `File.ReadAllText` mà **không đánh số dòng** trước khi chèn vào prompt, nên AI không có tọa độ dòng sẵn để tham chiếu chính xác; và (2) `prompt v1` cho trả lời văn bản tự do, không ép JSON schema có trường `from_line` / `to_line` và không có vòng tự kiểm tra.
- **Lỗi 2 - Suy đoán ngược chiều sao chép ở 4/5 cặp sao chép (`P05`, `P14`, `P22`, `P35`):** do script #4 ghép cặp theo thứ tự bảng chữ cái nên ở cả 5 cặp sao chép, bản sao `copy-*` đứng ở vị trí Bài A còn bài gốc `solution-*` đứng ở vị trí Bài B. Riêng `P29` diễn đạt trung lập, còn ở 4 cặp còn lại AI suy đoán ngược chiều rằng Bài B chép từ Bài A - ví dụ `P14` viết *"Bài B chỉ đổi tên biến (`limit`->`n`, `odd_slots`->`size`...)"*, `P22` viết *"Bài B là phiên bản viết lại (paraphrase) của bài A"*, `P35` viết *"Bài B chỉ đổi tên biến hệ thống (`is_prime`->`flags`...)"*. Có 3 nguyên nhân kết hợp: (1) không có dữ liệu mốc thời gian nộp bài; (2) `prompt v1` đặt câu hỏi chính theo chiều *"bài B có phải được sao chép hoặc viết lại từ bài A (hoặc ngược lại)"*, tạo thiên lệch vị trí (framing bias) khiến model mặc định Bài A là mốc gốc; và (3) prompt không cấm AI khẳng định chiều sao chép.
- **Lỗi 3 - Nhầm nhãn Bài A / Bài B ở 2/45 câu trả lời (`P28` và `P31`):** AI gọi cả hai bài nộp đều là `"Bài A"` khi so sánh hai cách cài đặt khác nhau. Cụ thể, `P31` (`copy-05` / `solution-01`) viết *"Bài A dùng `bytearray` và gán cắt (slice assignment)...; Bài A dùng `list` boolean và vòng lặp `for` lồng nhau tiêu chuẩn"*; `P28` (`copy-04` / `solution-03`) viết *"Bài A triển khai Sieve tuyến tính... Bài A triển khai Segmented Sieve..."*. Nguyên nhân là câu trả lời sinh tự do không qua bước kiểm tra hậu kỳ, dễ gây hiểu nhầm khi trình bày trực tiếp cho giảng viên.
- **Lỗi 4 - Mô tả chi tiết cú pháp code chưa chính xác ở 2/40 cặp không sao chép (`P01` và `P40`):** tuy kết luận đúng thuật toán, AI vẫn trượt ở chi tiết nhỏ trong code. Cụ thể, `P01` (`copy-01` / `copy-02`) viết hai bài *"Chỉ giống nhau ở chữ ký hàm và điều kiện `n <= 2`"*, nhưng `copy-02-level-2.py` đã đổi tên tham số nên thực tế dùng `if limit <= 2:`; còn `P40` (`solution-02` / `solution-03`) viết cả hai bài dùng `array[start::p] = b'\x00' * len(...)`, nhưng `solution-02.py` khởi tạo mảng 0 và đánh dấu hợp số bằng `b'\x01' * length` chứ không phải `b'\x00'`. Lỗi phát sinh do LLM khái quát hóa nhanh từ mẫu sàng phổ biến thay vì đối chiếu từng dòng bằng công cụ tra cứu.
- **Lỗi 5 - Rò rỉ ký tự ngoại ngữ và lỗi chính tả tiếng Việt ở 4/45 câu trả lời (`P04`, `P10`, `P15`, `P21`):** có 3 file bị lẫn chữ Hàn Quốc ngay giữa câu tiếng Việt - `P10` (*"đây là 관 dùng phổ biến"*), `P15` (*"điểm giống 표 diện"*), `P21` (*"không phải 증거 sao chép"*) - và `P04` lẫn từ ngoại ngữ sai dấu (*"vòng lặp for explícite"*), dính chữ (*"gánslice"*) cùng từ không tự nhiên (*"thuật pháp kinh điển"*). Nguyên nhân là model đa ngôn ngữ cỡ lớn sinh văn bản tiếng Việt tự do ở temperature mặc định dễ rò rỉ token ngôn ngữ khác nếu không có bước kiểm soát đầu ra.
- **Lỗi 6 - Thang điểm `DIEM_GIONG` dao động mạnh trên các cặp thuộc cùng đôi họ thuật toán:** do `prompt v1` không định nghĩa rubric chấm điểm và chạy ở temperature mặc định, các cặp ghép giữa bài gốc và các bản sao thuộc cùng một đôi họ thuật toán bị AI chấm lệch nhau 10-20 điểm:

| Đôi họ thuật toán so sánh            | Các cặp đồng dạng trong bộ 45 cặp              | Biên độ điểm AI   |
| ------------------------------------ | ---------------------------------------------- | ----------------- |
| Sàng số lẻ vs Sàng `bytearray` chuẩn | `P12` (15), `P17` (25), `P32` (30), `P42` (35) | 15 - 35 (lệch 20) |
| Sàng `list` chuẩn vs Sàng phân đoạn  | `P07` (0), `P37` (5), `P02` (10), `P20` (15)   | 0 - 15 (lệch 15)  |
| Sàng số lẻ vs Sàng phân đoạn         | `P10` (10), `P15` (10), `P21` (15), `P40` (20) | 10 - 20 (lệch 10) |

Ngay cả khi tách riêng những cặp mà bản sao **chỉ thay đổi bề mặt ở Mức 1 hoặc Mức 2** (không đổi cấu trúc hay kỹ thuật gán slice), điểm AI vẫn dao động rõ rệt: thay `solution-02` bằng bản đổi tên biến `copy-02` làm điểm tụt **15 điểm** khi ghép với `copy-05` (`P32` 30 -> `P12` 15) và tụt **10 điểm** khi ghép với `solution-05` (`P42` 35 -> `P17` 25) hay `solution-03` (`P40` 20 -> `P15` 10); tương tự, chỉ đổi xuống dòng Mức 1 từ `solution-01` sang `copy-01` cũng làm điểm lệch **5 điểm** (và nhảy cả hai chiều: `P39` 40 -> `P09` 35, nhưng `P31` 30 -> `P04` 35). Điều này chứng minh điểm `DIEM_GIONG` ở `v1` chưa ổn định để dùng làm ngưỡng phân loại độc lập.

### 5.4. Rủi ro vận hành khi mở rộng quy mô và giới hạn của kết quả AI (#4)

- **Ước tính cận dưới khi mở rộng theo N*(N-1)/2 cặp:** thực nghiệm #4 tốn trung bình 28,53 giây/cặp và khoảng 1.355 token/cặp (530 đầu vào + 825 đầu ra). Nếu giữ nguyên cách gửi tuần tự từng cặp cho một lớp thực tế:
  - Lớp 30 bài (435 cặp): ước tính mất khoảng **3,45 giờ** và **~589 nghìn token** (~231 nghìn đầu vào, ~359 nghìn đầu ra).
  - Lớp 50 bài (1.225 cặp): ước tính mất khoảng **9,71 giờ** và **~1,66 triệu token** (~649 nghìn đầu vào, ~1,01 triệu đầu ra).
  - Đây mới chỉ là **ước tính cận dưới** vì bộ mẫu Tuần 1 gồm các bài Python rất ngắn (11-27 dòng, ~530 input token/cặp); với bài tập thực tế dài 50-150 dòng, số token đầu vào và độ trễ sẽ còn tăng cao hơn.
  - Dù có thể giảm tổng thời gian chờ bằng cách gọi song song nhiều luồng hoặc đổi sang model không suy luận nhanh hơn, việc gọi song song sẽ sớm chạm giới hạn lượt của nhà cung cấp (minh chứng là `google/gemma-4-31b-it:free` bị HTTP 429 ngay cặp đầu tiên), còn **tổng số lượt gọi API và tổng chi phí token vẫn tăng theo bậc hai O(N^2)** nếu không có bộ lọc cấu trúc ở Lớp 1 loại bớt cặp trước.
- **Giới hạn của kết quả 45/45 trên bộ mẫu Tuần 1:** kết quả 5/5 TP và 0/40 FP mới chỉ là quan sát trên bộ mẫu nhỏ, chưa đủ khẳng định cách hỏi trực tiếp luôn đáng tin cậy khi triển khai thật vì 4 lý do:
  - **Cỡ mẫu dương tính quá nhỏ:** chỉ có 5 cặp sao chép (mỗi mức 1-2 cặp), chưa đại diện cho các biến thể phức tạp hơn như tách hàm, gộp hàm hoặc đổi cấu trúc dữ liệu.
  - **Đề bài quá kinh điển và mã nguồn ngắn:** *Count Primes* (LeetCode 204) xuất hiện dày đặc trong dữ liệu huấn luyện của LLM và toàn bộ 10 bài chỉ dài 11-27 dòng, giúp model dễ nhận diện thuật toán hơn nhiều so với bài tập tự thiết kế dài hàng trăm dòng.
  - **Chênh lệch cấu hình model:** thực nghiệm dùng model miễn phí kích thước rất lớn (~550B tham số) trên OpenRouter, không phải model mục tiêu dự kiến dùng khi triển khai sản phẩm chính thức.
  - **Chưa kiểm chứng độ ổn định lặp lại:** thực nghiệm để temperature mặc định và chỉ chạy 1 lượt duy nhất nên chưa đo được mức độ dao động nhãn/điểm qua nhiều lần chạy, nhất là ở cụm không sao chép đạt 30-40 điểm (`P39`, `P04`, `P09`, `P42`) và các cặp có thời gian suy luận bất thường (`P39` 54,92 giây, `P23` 65,27 giây).

## 6. Đánh giá hai phương pháp, kết luận sơ bộ và hướng cải thiện

### 6.1. Ưu điểm và hạn chế của từng phương pháp

| Tiêu chí                         | So khớp văn bản (#3)                                                                                                                                      | Hỏi trực tiếp AI `v1` (#4)                                                                                                                                               |
| -------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Khả năng phát hiện theo 4 mức    | **Hạn chế:** `Equals`/`Contains` bỏ sót 5/5 cặp; Levenshtein chỉ bắt 1/5 trên bản cũ (2/5 trên bản đã xóa comment), bỏ sót Mức 2, 1/2 Mức 3 và Mức 4.     | **Ưu điểm:** Hiểu được ý tưởng giải thuật, phát hiện đủ 5/5 cặp trên cả 4 mức (90-99/100), kể cả ca AI viết lại ở Mức 4 (`P35`).                                         |
| Phân tách nhóm & báo nhầm        | **Ưu điểm:** 0/40 báo nhầm tại ngưỡng 0,80. **Hạn chế:** Mức 4 chồng lấn với 3-5 cặp không chép; bỏ qua Mức 4 thì khe hở giữa `P39` và Mức 2 (`P14`) cũng chỉ là 0,007960. | **Ưu điểm:** 0/40 báo nhầm, tách biệt 50 điểm (40 so với 90) và phân biệt được trùng thuật toán kinh điển (`P39`, 40/100 -> `not_copy`).                                 |
| Giải thích & chất lượng đầu ra   | **Hạn chế:** Chỉ trả về nhãn và điểm số, không sinh được giải thích điểm giống/khác cho giảng viên.                                                       | **Ưu điểm:** Giải thích đúng bản chất thuật toán cốt lõi bằng tiếng Việt. **Hạn chế:** Dính 6 nhóm lỗi trong `responses/` (0/45 nêu số dòng, 4/5 đoán ngược chiều, nhầm Bài A/B, sai chi tiết `P01`/`P40`, lọt chữ Hàn/lỗi chính tả, dao động 10-20 điểm trên cặp đồng dạng). |
| Tốc độ, chi phí & khả năng mở rộng | **Ưu điểm:** Xử lý cục bộ không qua mạng, không tốn phí API hay token. **Hạn chế:** Thực nghiệm #3 chưa đo thời gian chạy cụ thể.                       | **Hạn chế:** Tăng chi phí và độ trễ theo O(N^2) (cận dưới ~3,45 giờ / ~589 nghìn token cho 30 bài; ~9,71 giờ / ~1,66 triệu token cho 50 bài); phụ thuộc giới hạn API (`gemma` bị HTTP 429 ngay cặp đầu); output token phình 1,56 lần input. |
| Độ ổn định & phạm vi kiểm chứng  | **Ưu điểm:** Kết quả hoàn toàn xác định, lặp lại giống nhau 100% giữa các lần chạy.                                                                       | **Hạn chế:** Mới chạy 1 lượt ở temperature mặc định (chưa kiểm chứng độ ổn định lặp lại); mẫu còn nhỏ (10 bài ngắn 11-27 dòng, đề kinh điển, model 550B `:free`) nên chưa chứng minh được khả năng tổng quát hóa. |

### 6.2. Nhận xét và kết luận sơ bộ

- **Không phương pháp đơn lẻ nào đáp ứng được yêu cầu sản phẩm:** so khớp văn bản thô bỏ sót quá nhiều ngay từ Mức 1-2 và không tách được Mức 4 khỏi nhóm bài độc lập. Ngược lại, hỏi trực tiếp AI (`v1`) dù đạt 45/45 nhãn đúng trên bộ mẫu nhỏ nhưng phần giải thích còn nhiều lỗi chưa thể hiển thị trực tiếp cho giảng viên, đồng thời chi phí và độ trễ theo O(N^2) làm cách này không khả thi khi chạy cho lớp 30-50 bài.
- **Hai phương pháp bù trừ trực tiếp cho nhau, là cơ sở trực tiếp cho hướng thiết kế 2 lớp:** ưu thế của phân tích thuật toán cục bộ (xử lý cục bộ không qua mạng, chi phí API bằng 0, kết quả xác định) giải quyết đúng nút thắt O(N^2) của AI; ngược lại, khả năng hiểu ngữ nghĩa thuật toán của AI giải quyết đúng ca khó Mức 4 mà so khớp bề mặt không xử lý nổi.
- **Mức độ tin cậy của kết luận Tuần 1:** các hạn chế của so khớp văn bản (#3) và 6 nhóm lỗi đầu ra của Direct AI `v1` (#4) đã được chứng minh trực tiếp từ mã nguồn và 45 file phản hồi. Riêng độ chính xác 45/45 của AI cần kiểm chứng thêm trên tập benchmark 30-50 bài dài hơn (C++ và Python) bằng model chính thức trước khi đánh giá hiệu quả tổng quát.

### 6.3. Đề xuất hướng cải thiện cho các giai đoạn tiếp theo

Các cải thiện dưới đây ánh xạ trực tiếp từ từng nguyên nhân lỗi ở Mục 5 theo lộ trình dự án:

| Vấn đề đã chứng minh ở Mục 5                                          | Hướng cải thiện đề xuất                                                                                                                                                                                 | Giai đoạn áp dụng     |
| --------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------- |
| Sót Mức 1 do comment/khoảng trắng (`P05`) và nền điểm cao do khung đề (`P39`, `P31`) | Tiền xử lý (xóa comment, chuẩn hóa khoảng trắng), tách bỏ code khung mẫu (`template_code`) và **lưu bảng ánh xạ ngược về số dòng gốc** để Lớp 2 báo đúng tọa độ dòng.                                   | Tuần 2-3 (Tiền xử lý) |
| Sót Mức 2-3 (`P14`, `P22`), lệ thuộc `max_len` và nút thắt O(N^2) của AI | Xây Lớp 1 **kết hợp** Tree-sitter (phân tích AST, chuẩn hóa định danh) với Winnowing (băm dấu vân tay); dùng **ngưỡng lọc thấp ưu tiên Recall** để loại cặp khác hẳn nhưng không lọc mất Mức 4 trước khi gửi sang Lớp 2. | Tuần 2-3 (Lớp 1)      |
| 0/45 câu trả lời nêu số dòng (không đánh số đầu vào + prompt tự do)   | Đánh số dòng mã nguồn trước khi đưa vào prompt; ép JSON Schema gồm `risk_level` (`low`/`medium`/`high`), `matched_lines` (`from_line`, `to_line`) và `ai_explanation`; tách tọa độ cho từng bài ở schema nội bộ để hậu kiểm hai phía rồi ánh xạ ra API Contract. | Tuần 4-5 (AI Harness `v2`) |
| Đoán ngược chiều sao chép (`P05`, `P14`, `P22`, `P35`), nhầm Bài A/B (`P28`, `P31`), dao động 10-20 điểm | Thiết kế System Prompt `v2` -> `v3`: bỏ cách hỏi thiên lệch *"Bài B chép từ Bài A"*, cấm khẳng định chiều sao chép khi thiếu mốc thời gian nộp, dùng mã bài nộp thay cho "Bài A"/"Bài B", định nghĩa rõ tiêu chí phân mức `risk_level` và cố định `temperature = 0`. | Tuần 4-5 (AI Harness `v2`->`v3`) |
| Sai chi tiết cú pháp (`P01`, `P40`) và rủi ro báo nhầm ở cụm thuật toán kinh điển (`P39`, `P42`) | Cấp 3 công cụ (Tool Calling): `xem_doan_giong` (đối chiếu trực tiếp đoạn mã), `xem_code_mau_giang_vien` (loại trừ phần trùng khung đề) và `dem_tan_suat_ca_lop` (đếm trên cấu trúc đã chuẩn hóa của Lớp 1). Lưu ý Tool Calling tăng vòng gọi nên cần Lớp 1 lọc tốt trước. | Tuần 4-5 (AI Harness `v2`->`v3`) |
| Lỗi định dạng/số dòng và rò rỉ chữ Hàn (`P10`, `P15`, `P21`)          | Cài vòng tự kiểm tra (Self-check Loop, tối đa 2-3 lượt): kiểm tra JSON hợp lệ, phạm vi `from_line..to_line` trên độ dài thật của hai bài và chặn ký tự ngoài hệ Latin/tiếng Việt kèm bộ ký tự cú pháp code ASCII (lưu ý không bắt được lỗi chính tả hệ Latin như `P04`); quá giới hạn retry thì gắn cờ kiểm tra thủ công thay vì treo luồng quét. | Tuần 4-5 (AI Harness `v2`->`v3`) |
| Lỗi HTTP 429 (`gemma`) và phình output token 1,56 lần input           | Bổ sung cơ chế retry kèm exponential backoff, timeout và giới hạn luồng song song khi gọi API; chọn model không sinh chuỗi suy luận ẩn quá dài (hoặc giới hạn reasoning token) và ghi log tách `reasoning_tokens` nếu nhà cung cấp hỗ trợ. | Tuần 4-5 (Backend & AI) |
| Lệch bản dữ liệu (#3 vs #4), chưa đo thời gian #3 và mẫu nhỏ 10 bài   | Khóa 1 bản bộ mẫu duy nhất cho mọi phương án; thống nhất với mentor quy ước gắn nhãn cặp chéo (giữa các bản sao khác nguồn hoặc bản sao với bài gốc khác) cho tập benchmark 30-50 bài (C++, Python); đo đủ thời gian của Lớp 1. | Tuần 6-7 (Chuẩn bị benchmark) |
| Chưa kiểm chứng độ ổn định lặp lại và so sánh công bằng các phương án | Chạy lại Phương án B (Direct AI `v1`) cùng Phương án A (chỉ Lớp 1) và Phương án C (2 lớp + AI Harness `v2` -> `v3`) trên cùng model chính thức; chạy lặp 2-3 lượt trên các cặp biên để kiểm chứng độ ổn định ở `temperature = 0`. | Tuần 7-8 (Thực nghiệm) |
