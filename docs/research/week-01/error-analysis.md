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
- Khi tính thử lại Levenshtein theo đúng công thức của #3 trên bản mới đã xóa comment, cặp `ORIG-01 / COPY-01` giảm khoảng cách còn 37 (điểm 0,900000, vượt ngưỡng 0,80) và 8 cặp không sao chép còn lại có chứa `copied/copy-01-level-1.py` đều dưới 0,80 (cao nhất 0,598383 với `original/solution-05.py`) nên không phát sinh thêm báo nhầm.
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
- Điểm Levenshtein của nhóm này dao động từ 0,263158 đến 0,638814, trung bình 0,420041 (tính theo bản bộ mẫu cũ; nếu chạy lại trên bản đã xóa comment thì `copied/copy-01-level-1.py` / `original/solution-05.py` tăng lên 0,598383 vào hạng 2 và `copied/copy-01-level-1.py` / `copied/copy-05-level-4.py` tăng lên 0,591667 vào hạng 4, đẩy `original/solution-01.py` / `original/solution-02.py` ra khỏi Top 5).
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
- Tổng token: 23.850 đầu vào (530 token/cặp) và 37.123 đầu ra (~825 token/cặp), tổng 60.973 token. Lượng token đầu ra gấp khoảng 1,56 lần đầu vào dù câu trả lời chỉ gồm 3 mục ngắn; log của #4 không tách riêng trường `reasoning_tokens` nên chưa xác nhận trực tiếp, nhưng nhiều khả năng model tính cả phần suy luận ngầm vào output token.
- Lượt chạy này không mất tiền vì dùng bản `:free` trên OpenRouter, nhưng con số 0 đồng không phản ánh chi phí chạy API thương mại (chưa kể giới hạn lượt như lỗi HTTP 429 ở trên). Các vấn đề về độ trễ khi tăng số lượng bài nộp trong lớp và chất lượng giải thích trong `responses/` được phân tích chi tiết ở Mục 5.

## 4. Đối chiếu hai phương pháp và so sánh theo 4 mức độ

Nhãn kết quả của #3 (`Phát hiện đúng`, `Loại đúng`, `Bỏ sót`, `Báo nhầm`) và #4 (`correct`, `false_positive`, `missed`) được quy đổi về chung 4 nhóm TP, TN, FP, FN, trong đó nhãn `correct` của #4 được tách thành TP (với 5 cặp sao chép) và TN (với 40 cặp không sao chép) dựa trên `ground-truth.csv`. Mã cặp `P01`-`P45` của #4 được dùng làm mã định danh chung khi ghép 45 cặp.

### 4.1. Bảng tổng hợp đối chiếu trên 45 cặp

| Phương pháp                | Điều kiện / Cấu hình                       | TP | TN | FP | FN | Tỷ lệ phát hiện | Thời gian (45 cặp)         | Token / chi phí            |
| -------------------------- | ------------------------------------------ | -- | -- | -- | -- | --------------- | -------------------------- | -------------------------- |
| `string.Equals` (#3)       | Khớp toàn bộ chuỗi (`Ordinal`)             | 0  | 40 | 0  | 5  | 0% (0/5)        | Chưa đo trong thực nghiệm  | Không dùng API             |
| `string.Contains` (#3)     | A chứa B hoặc B chứa A (`Ordinal`)         | 0  | 40 | 0  | 5  | 0% (0/5)        | Chưa đo trong thực nghiệm  | Không dùng API             |
| Levenshtein (#3)           | `(max_len - dist) / max_len >= 0,80`       | 1  | 40 | 0  | 4  | 20% (1/5)       | Chưa đo trong thực nghiệm  | Không dùng API             |
| Hỏi trực tiếp AI `v1` (#4) | `nemotron-3-ultra-550b-a55b:free` (1 lượt) | 5  | 40 | 0  | 0  | 100% (5/5)      | 1.283,63 giây (~21,4 phút) | 60.973 token (bản `:free`) |

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

Trên đủ 45 cặp, cả 4 cách đo đều đạt 40/40 TN và 0 FP ở nhóm không sao chép; sự khác biệt nằm hoàn toàn ở 5 cặp sao chép (TP và FN) theo từng mức độ:

- **Mức 1:** `Equals` và `Contains` đều FN; Levenshtein 0/1 TP (`P05` FN với 0,776224; trên bản đã xóa comment đạt 1/1 TP với 0,900000); Direct AI `v1` đạt 1/1 TP (99/100).
- **Mức 2:** `Equals`, `Contains` và Levenshtein đều FN (`P14` đạt 0,646774); Direct AI `v1` đạt 1/1 TP (95/100).
- **Mức 3:** `Equals` và `Contains` đều FN; Levenshtein đạt 1/2 TP (`P29` TP với 0,892934, `P22` FN với 0,661793); Direct AI `v1` đạt 2/2 TP (90 và 95/100).
- **Mức 4:** `Equals`, `Contains` và Levenshtein đều FN (`P35` đạt 0,552083); Direct AI `v1` đạt 1/1 TP (95/100).

Điểm Levenshtein không giảm đơn điệu theo mức độ sao chép: cả hai cặp Mức 3 (`0,661793` và `0,892934`) đều cao hơn cặp Mức 2 (`0,646774`), trong khi điểm của Direct AI `v1` giữ ổn định trong khoảng 90-99/100 trên cả 4 mức.

### 4.3. Đối chiếu cụm cặp không sao chép có điểm cao nhất

Bảng dưới gộp 7 cặp không sao chép thuộc Top 5 Levenshtein (Mục 2.3) hoặc có điểm AI từ 30 trở lên (Mục 3.2); cả 7 cặp đều cho kết quả TN ở cả hai phương pháp. Với Levenshtein (#3), bảng giữ cột **bản cũ** (số chính thức từ `results.csv` của #3) cạnh cột **bản mới** (sau khi xóa dòng comment `# Same code; only whitespace and this comment were changed.` ở `copied/copy-01-level-1.py`, cùng bản #4 chạy):

| Mã cặp | Cặp bài                                                   | Levenshtein (bản cũ) | Levenshtein (bản mới) | Direct AI `v1` (#4) |
| ------ | --------------------------------------------------------- | -------------------- | --------------------- | ------------------- |
| P39    | `original/solution-01.py` / `original/solution-05.py`     | 0,638814             | Không đổi             | 40/100              |
| P31    | `copied/copy-05-level-4.py` / `original/solution-01.py`   | 0,593750             | Không đổi             | 30/100              |
| P42    | `original/solution-02.py` / `original/solution-05.py`     | 0,573034             | Không đổi             | 35/100              |
| P36    | `original/solution-01.py` / `original/solution-02.py`     | 0,537079             | Không đổi             | 15/100              |
| P09    | `copied/copy-01-level-1.py` / `original/solution-05.py`   | 0,515152             | 0,598383              | 35/100              |
| P32    | `copied/copy-05-level-4.py` / `original/solution-02.py`   | 0,481250             | Không đổi             | 30/100              |
| P04    | `copied/copy-01-level-1.py` / `copied/copy-05-level-4.py` | 0,468750             | 0,591667              | 35/100              |

Nhìn từ khoảng phân tách điểm giữa 5 cặp sao chép và 40 cặp không sao chép:

- Với Levenshtein (#3), khoảng điểm sao chép (0,552083 - 0,892934 trên bản cũ; tăng cận trên lên 0,900000 ở bản đã xóa comment) và không sao chép (0,263158 - 0,638814) bị chồng lấn ở Mức 4: trên bản cũ có 3 cặp không sao chép (`P39`, `P31`, `P42`) vượt điểm cặp Mức 4 `P35` (0,552083); trên bản đã xóa comment có thêm `P09` (0,598383) và `P04` (0,591667) cùng vượt `P35` (tổng 5 cặp).
- Với Direct AI `v1` (#4), hai nhóm cách nhau 50 điểm (nhóm không sao chép cao nhất 40/100 ở `P39`, nhóm sao chép thấp nhất 90/100 ở `P22`). Hai phương pháp có cùng cặp đứng đầu nhóm không sao chép là `P39` (`original/solution-01.py` / `original/solution-05.py`), nhưng ngược chiều về thứ tự điểm giữa `P36` (Levenshtein 0,537079, AI 15/100) và `P32` (Levenshtein 0,481250, AI 30/100) - lưu ý rằng điểm AI chỉ là ước lượng theo bội số 5 không có rubric cố định nên sự chênh lệch này mới mang tính tham khảo. Nguyên nhân của các ca bỏ sót ở #3, rủi ro báo nhầm quanh cụm này và chất lượng giải thích trong `responses/` của #4 được kiểm chứng và phân tích chi tiết ở Mục 5.

## 5. Phân tích nguyên nhân sai đối với từng trường hợp

### 5.1. Nguyên nhân bỏ sót của so khớp văn bản (#3)

Bảng dưới tổng hợp độ dài thật (chuẩn hóa LF), mẫu số `max_len`, khoảng cách chỉnh sửa và tỷ lệ ký tự bị đổi của 5 cặp sao chép:

| Mã cặp | Cặp bài           | Mức | Độ dài gốc | Độ dài sao | `max_len` | `dist` | Tỷ lệ sửa (`dist/max_len`) | Điểm Lev. | Kết quả |
| ------ | ----------------- | --- | ---------- | ---------- | --------- | ------ | -------------------------- | --------- | ------- |
| P05    | ORIG-01 / COPY-01 | 1   | 333        | 429 (cũ)   | 429       | 96     | 22,38%                     | 0,776224  | FN      |
| P05*   | ORIG-01 / COPY-01 | 1   | 333        | 370 (mới)  | 370       | 37     | 10,00%                     | 0,900000  | TP      |
| P14    | ORIG-02 / COPY-02 | 2   | 445        | 620        | 620       | 219    | 35,32%                     | 0,646774  | FN      |
| P22    | ORIG-03 / COPY-03 | 3   | 895        | 1.026      | 1.026     | 347    | 33,82%                     | 0,661793  | FN      |
| P29    | ORIG-04 / COPY-04 | 3   | 435        | 467        | 467       | 50     | 10,71%                     | 0,892934  | TP      |
| P35    | ORIG-05 / COPY-05 | 4   | 371        | 480        | 480       | 215    | 44,79%                     | 0,552083  | FN      |

Ghi chú: `P05*` là kết quả tính lại cho cặp `P05` trên bản `copied/copy-01-level-1.py` mới đã xóa dòng comment.

Nguyên nhân cụ thể cho từng trường hợp:

- **`string.Equals` và `string.Contains` bỏ sót toàn bộ 5/5 cặp (`P05`, `P14`, `P22`, `P29`, `P35`):** `Equals` yêu cầu hai chuỗi trùng khớp từng ký tự, còn `Contains` chỉ bắt được khi toàn bộ bài này nằm liên tục trong bài kia (chỉ thêm code ở đầu hoặc cuối file). Tuy nhiên cả 5 bản sao đều sửa xen vào giữa thân hàm; ngay ở Mức 1 (`P05`), việc tách `if n <= 2: return 0` và vòng `for` từ 1 dòng thành nhiều dòng đã làm đứt chuỗi khớp liên tục nên cả hai chiều đều trả về `False`.
- **Levenshtein bỏ sót Mức 1 (`P05`, 0,776224):** thuật toán đo trên nguyên văn ký tự mà không xóa comment hay chuẩn hóa khoảng trắng. Bài gốc `original/solution-01.py` chỉ dài 333 ký tự; khi bản cũ của `copied/copy-01-level-1.py` thêm dòng comment dài và các dấu xuống dòng thụt lề, độ dài tăng lên 429 ký tự. Khoảng cách 96 ký tự chiếm 22,38% độ dài `copy-01` (`96/429`), kéo điểm xuống dưới ngưỡng 0,80. Bỏ riêng dòng comment đó ở bản mới (`P05*`) làm `dist` giảm còn 37/370 ký tự (10,00%, điểm 0,900000), cho thấy trên bài ngắn chỉ một dòng chú thích đã đủ làm phương pháp này bỏ sót.
- **Levenshtein bỏ sót Mức 2 (`P14`, 0,646774):** thuật toán sàng số lẻ giữa `solution-02.py` và `copy-02-level-2.py` giữ nguyên, nhưng toàn bộ tên tham số và biến ngắn (`n`, `size`, `crossed`, `i`, `p`, `start`, `length`) bị đổi sang tên dài hơn (`limit`, `odd_slots`, `composite_flags`, `slot`, `divisor`, `first_slot`, `view_length`) lặp lại nhiều dòng. Cách đổi tên này đẩy độ dài file từ 445 lên 620 ký tự và tạo ra 219 phép sửa (35,32%) dù cấu trúc cú pháp gần như giữ nguyên.
- **Tính không đơn điệu ở Mức 3 (`P22` 0,661793 và `P29` 0,892934 đều cao hơn Mức 2 `P14` 0,646774):** Levenshtein chỉ phản ánh tỷ lệ `dist / max_len` chứ không đo mức độ sửa cấu trúc:
  - `P29` (`solution-04.py` / `copy-04-level-3.py`) chỉ đổi vòng `for value in range(2, n)` ngoài cùng thành `while value < n` kèm `value += 1` và giữ nguyên toàn bộ tên biến cùng thân sàng tuyến tính, nên chỉ lệch 50/467 ký tự (10,71%) và vượt ngưỡng 0,80.
  - Ngược lại, `P22` (`solution-03.py` / `copy-03-level-3.py`) sửa đồng thời 3 vị trí vòng lặp/slice và đổi nhiều tên biến nên `dist` lên tới 347 ký tự (lớn hơn hẳn 219 của `P14`). Điểm của `P22` vẫn cao hơn `P14` là vì mẫu số `max_len` lớn hơn nhiều: `copy-03-level-3.py` dài 1.026 ký tự nên tỷ lệ sửa `347/1.026` (33,82%) vẫn thấp hơn `219/620` (35,32%) của `P14`.
- **Levenshtein bỏ sót Mức 4 (`P35`, 0,552083):** bản AI viết lại `copy-05-level-4.py` vẫn dùng sàng `bytearray` với cùng công thức độ dài slice `(n - 1 - first) // candidate + 1`, nhưng đổi toàn bộ tên biến và viết khác các biểu thức (`n <= 2` -> `n < 3`, `b'\x01'` -> `[1]`, `b'\x00' * (...)` -> `bytearray(count)`, `flags.count(1)` -> `sum(is_prime)`), đẩy tỷ lệ sửa lên 44,79% (`215/480`).
- **Không chọn được ngưỡng cố định cho Levenshtein:** hạ ngưỡng xuống 0,55 để bắt Mức 4 (`P35`, 0,552083) sẽ dính ngay 3 ca báo nhầm trên bản cũ (`P39`, `P31`, `P42`) và 5 ca trên bản đã xóa comment (thêm `P09`, `P04`). Nếu chọn ngưỡng sau khi biết đáp án ở mức 0,64 để bắt 4/5 cặp mà không báo nhầm, khe hở giữa cặp không sao chép cao nhất (`P39`, 0,638814) và cặp Mức 2 (`P14`, 0,646774) cũng chỉ là 0,007960 - quá hẹp để áp dụng sang đề bài hoặc lớp học khác.

### 5.2. Nguyên nhân điểm cao và rủi ro báo nhầm quanh cụm `solution-01`, `solution-02`, `solution-05`

Cụm 7 cặp không sao chép điểm cao nhất ở Bảng 4.3 đều xoay quanh các biến thể của Sàng Eratosthenes (`solution-01`, `solution-02`, `solution-05` cùng `copy-01`, `copy-05`), nhưng hai phương pháp bị kéo điểm cao theo hai hướng khác nhau:

- Với Levenshtein (#3), các bài trong cụm này đều ngắn (11-14 dòng, 333-480 ký tự) và dùng chung phần khung đề bài (`class Solution:`, `def countPrimes(self, n: int) -> int:`) cùng đoạn kiểm tra biên đầu hàm (`if n <= 2: return 0` trên 3 bài gốc, tách thành 2 dòng ở `copy-01` và đổi thành `if n < 3: return 0` ở `copy-05`). Riêng `solution-01`, `solution-05` và `copy-01` còn trùng nguyên cấu trúc vòng lặp `while p * p < n:` (còn `solution-02` dùng `for i in range(size):` và `copy-05` đổi biến sang `candidate`). Tỷ trọng phần nền giống nhau lớn khiến `P39` (`solution-01` / `solution-05`) đạt 0,638814 dù khác cách đánh dấu hợp số (`list` boolean + vòng `for` so với `bytearray` + gán slice). Tương tự, `P31` (`copy-05` / `solution-01`) đạt 0,593750 - cao hơn cả điểm giữa `copy-05` với bài gốc `solution-05` (0,552083) - do bản AI viết lại dùng `is_prime[0] = is_prime[1] = 0` và `return sum(is_prime)`, rất gần về bề mặt ký tự với `prime[0] = prime[1] = False` và `return sum(prime)` của `solution-01`.
- Với Direct AI `v1` (#4), điểm được chấm theo mức độ trùng khớp về kỹ thuật cài đặt. `P39` đứng cao nhất (40/100) do cùng cài sàng Eratosthenes duyệt từ 2 đến căn bậc hai của `n`; `P42` (`solution-02` / `solution-05`, 35/100) và `P32` (`copy-05` / `solution-02`, 30/100) ở mức 30-35 điểm vì cùng dùng `bytearray` kết hợp gán slice. Ngược lại, `P36` (`solution-01` / `solution-02`) tuy đạt 0,537079 ở Levenshtein nhưng khác cả cấu trúc dữ liệu lẫn cách ánh xạ chỉ số (`list` boolean đầy đủ so với `bytearray` chỉ lưu số lẻ) nên AI chỉ chấm 15/100. Trên bộ mẫu nhỏ AI chưa báo nhầm cặp nào, nhưng việc cả hai phương pháp đều đẩy điểm cao nhất về cụm bài cùng cài họ Sàng Eratosthenes cơ bản và sàng số lẻ cho thấy khi lớp có nhiều bài dùng chung mẫu tối ưu quen thuộc, nguy cơ báo nhầm vẫn đáng kể nếu AI không biết tần suất trong cả lớp.

### 5.3. Kiểm chứng chi tiết 45 file phản hồi gốc (`responses/`) của hỏi trực tiếp AI (#4)

Rà soát toàn bộ 45 file trong `responses/` cho thấy về nhận diện thuật toán, AI phân biệt đúng bản chất giải thuật (Sàng Eratosthenes chuẩn, sàng số lẻ, sàng phân đoạn, sàng tuyến tính Euler) trên cả 5 cặp sao chép lẫn 40 cặp không sao chép, không bịa tên hàm không tồn tại; riêng ở 5 cặp sao chép (`P05`, `P14`, `P22`, `P29`, `P35`), AI trích đúng các tên biến đổi và vị trí đổi vòng lặp/slice.

Tuy nhiên, khi đọc kỹ 45 câu trả lời, vẫn có 6 nhóm lỗi và hạn chế thực tế trong phần `GIAI_THICH` và điểm số:

- **Lỗi 1 - 0/45 câu trả lời nêu được số dòng cụ thể:** dù `prompt v1` yêu cầu *"nêu rõ giống ở hàm nào, dòng nào"*, không có file nào ghi số dòng mà chỉ mô tả bằng tên biến và câu lệnh. Có hai nguyên nhân: script C# của #4 đọc nguyên văn code qua `File.ReadAllText` mà không đánh số dòng trước khi đưa vào prompt, và `prompt v1` cho trả lời văn bản tự do, không ép JSON schema có `from_line` / `to_line` cũng như không có bước tự kiểm tra.
- **Lỗi 2 - Suy đoán ngược chiều sao chép ở 4/5 cặp sao chép (`P05`, `P14`, `P22`, `P35`):** do script #4 ghép cặp theo bảng chữ cái nên ở 5 cặp sao chép, `copy-*` đứng ở Bài A còn `solution-*` đứng ở Bài B. Trừ `P29` được AI diễn đạt trung lập, ở 4 cặp còn lại AI đều suy đoán ngược rằng Bài B chép từ Bài A - ví dụ `P14` viết *"Bài B chỉ đổi tên biến (`limit`->`n`, `odd_slots`->`size`...)"*, `P22` viết *"Bài B là phiên bản viết lại (paraphrase) của bài A"*, `P35` viết *"Bài B chỉ đổi tên biến hệ thống (`is_prime`->`flags`...)"*. Nguyên nhân là không có mốc thời gian nộp bài, trong khi câu hỏi trong `prompt v1` đặt theo chiều *"bài B có phải được sao chép hoặc viết lại từ bài A (hoặc ngược lại)"* khiến model mặc định Bài A là bài gốc, lại không có yêu cầu cấm khẳng định hướng sao chép.
- **Lỗi 3 - Nhầm nhãn Bài A / Bài B ở 2/45 câu trả lời (`P28` và `P31`):** AI gọi cả hai bài đều là "Bài A" khi so sánh hai cách cài đặt khác nhau. Cụ thể, `P31` (`copy-05` / `solution-01`) viết *"Bài A dùng `bytearray` và gán cắt (slice assignment)...; Bài A dùng `list` boolean và vòng lặp `for` lồng nhau tiêu chuẩn"*; `P28` (`copy-04` / `solution-03`) viết *"Bài A triển khai Sieve tuyến tính... Bài A triển khai Segmented Sieve..."*. Lỗi xảy ra do văn bản sinh tự do không qua bước kiểm tra lại đầu ra.
- **Lỗi 4 - Mô tả chi tiết cú pháp code chưa chính xác ở 2/40 cặp không sao chép (`P01` và `P40`):** `P01` (`copy-01` / `copy-02`) viết hai bài *"Chỉ giống nhau ở chữ ký hàm và điều kiện `n <= 2`"*, nhưng `copy-02-level-2.py` thực tế dùng `if limit <= 2:`; còn `P40` (`solution-02` / `solution-03`) viết cả hai bài dùng `array[start::p] = b'\x00' * len(...)`, trong khi `solution-02.py` khởi tạo mảng 0 và đánh dấu hợp số bằng `b'\x01' * length`. Lỗi phát sinh do LLM khái quát hóa theo mẫu sàng quen thuộc thay vì đối chiếu trực tiếp từng dòng.
- **Lỗi 5 - Rò rỉ ký tự ngoại ngữ và lỗi chính tả tiếng Việt ở 4/45 câu trả lời (`P04`, `P10`, `P15`, `P21`):** 3 file bị lẫn chữ Hàn Quốc ngay giữa câu tiếng Việt - `P10` (*"đây là 관 dùng phổ biến"*), `P15` (*"điểm giống 표 diện"*), `P21` (*"không phải 증거 sao chép"*) - còn `P04` lẫn từ ngoại ngữ sai dấu (*"vòng lặp for explícite"*), dính chữ (*"gánslice"*) và từ không tự nhiên (*"thuật pháp kinh điển"*). Nguyên nhân là model đa ngôn ngữ cỡ lớn sinh văn bản tiếng Việt tự do ở temperature mặc định dễ lẫn token ngôn ngữ khác khi không có bước lọc đầu ra.
- **Lỗi 6 - Thang điểm `DIEM_GIONG` dao động mạnh trên các cặp thuộc cùng đôi họ thuật toán:** do `prompt v1` không định nghĩa rubric chấm điểm và chạy ở temperature mặc định, các cặp ghép giữa bài gốc và các bản sao thuộc cùng một đôi họ thuật toán bị AI chấm lệch nhau 10-20 điểm:

| Đôi họ thuật toán so sánh            | Các cặp đồng dạng trong bộ 45 cặp (tên cặp file và điểm AI)                                                                                        | Biên độ điểm AI   |
| ------------------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------- |
| Sàng số lẻ vs Sàng `bytearray` chuẩn | `P12` (`copy-02`/`copy-05`: 15), `P17` (`copy-02`/`solution-05`: 25), `P32` (`copy-05`/`solution-02`: 30), `P42` (`solution-02`/`solution-05`: 35) | 15 - 35 (lệch 20) |
| Sàng `list` chuẩn vs Sàng phân đoạn  | `P07` (`copy-01`/`solution-03`: 0), `P37` (`solution-01`/`solution-03`: 5), `P02` (`copy-01`/`copy-03`: 10), `P20` (`copy-03`/`solution-01`: 15)   | 0 - 15 (lệch 15)  |
| Sàng số lẻ vs Sàng phân đoạn         | `P10` (`copy-02`/`copy-03`: 10), `P15` (`copy-02`/`solution-03`: 10), `P21` (`copy-03`/`solution-02`: 15), `P40` (`solution-02`/`solution-03`: 20) | 10 - 20 (lệch 10) |

Tách riêng những cặp mà bản sao chỉ thay đổi bề mặt ở Mức 1-2 (không đổi cấu trúc hay kỹ thuật gán slice), điểm AI vẫn dịch chuyển rõ: thay `solution-02` bằng `copy-02` làm điểm **tụt cùng một chiều** ở cả 4 cặp trong bảng trên (`P32` 30 -> `P12` 15, `P42` 35 -> `P17` 25, `P40` 20 -> `P15` 10 và `P21` 15 -> `P10` 10), trong khi đổi định dạng Mức 1 từ `solution-01` sang `copy-01` làm điểm nhảy hai chiều (`P39` 40 -> `P09` 35, nhưng `P31` 30 -> `P04` 35). Xu hướng tụt cùng chiều này gợi ý giả thuyết rằng ngay cả model 550B ở `v1` vẫn có thể bị tác động bởi thay đổi bề mặt (đổi tên biến, tách dòng), dù chưa tách riêng được nhiễu từ việc đảo thứ tự Bài A/B ở 2/4 cặp (`P32 -> P12`, `P21 -> P10`) và việc mới chạy 1 lượt ở temperature mặc định. Trong mọi trường hợp, biên độ lệch này cho thấy điểm `DIEM_GIONG` ở `v1` chưa đủ ổn định để làm ngưỡng phân loại độc lập.

### 5.4. Rủi ro vận hành khi mở rộng quy mô và giới hạn của kết quả AI (#4)

- **Ước tính cận dưới khi mở rộng theo N*(N-1)/2 cặp:** thực nghiệm #4 tốn trung bình 28,53 giây/cặp và khoảng 1.355 token/cặp (530 đầu vào + ~825 đầu ra). Nếu giữ nguyên cách gửi tuần tự từng cặp cho một lớp thực tế:
  - Lớp 30 bài (435 cặp): ước tính mất khoảng 3,45 giờ và ~589,41 nghìn token (230,55 nghìn đầu vào, ~358,86 nghìn đầu ra).
  - Lớp 50 bài (1.225 cặp): ước tính mất khoảng 9,71 giờ và ~1,66 triệu token (~0,65 triệu đầu vào, ~1,01 triệu đầu ra).
  - Đây mới là ước tính cận dưới vì 10 bài mẫu Tuần 1 rất ngắn (11-27 dòng, 530 input token/cặp); với bài tập dài 50-150 dòng, lượng token và độ trễ sẽ còn cao hơn.
  - Có thể rút ngắn thời gian chờ bằng cách gọi song song hoặc đổi sang model không suy luận nhanh hơn, nhưng gọi song song sẽ sớm chạm giới hạn lượt (như `google/gemma-4-31b-it:free` bị HTTP 429 ở cặp đầu) và tổng số lượt gọi cùng lượng token vẫn tăng theo O(N^2) nếu thiếu bộ lọc ở Lớp 1.
- **Giới hạn của kết quả 45/45 trên bộ mẫu Tuần 1:** kết quả 5/5 TP và 0/40 FP chưa đủ khẳng định cách hỏi trực tiếp luôn đáng tin cậy khi triển khai thật vì 4 lý do:
  - Cỡ mẫu dương tính rất nhỏ (chỉ 5 cặp, mỗi mức 1-2 cặp), chưa phủ các biến thể phức tạp như tách hàm, gộp hàm hay đổi cấu trúc dữ liệu.
  - Đề *Count Primes* (LeetCode 204) quá quen thuộc trong dữ liệu huấn luyện của LLM và 10 bài nộp chỉ dài 11-27 dòng, dễ nhận diện hơn nhiều so với bài tập tự thiết kế dài hàng trăm dòng.
  - Thực nghiệm dùng model miễn phí rất lớn (~550B tham số) trên OpenRouter, không phải model mục tiêu dự kiến dùng khi triển khai sản phẩm chính thức.
  - Do để temperature mặc định và chỉ chạy 1 lượt duy nhất, thực nghiệm chưa đo được mức độ dao động nhãn ở cụm không sao chép đạt 30-40 điểm (`P39`, `P04`, `P09`, `P42`, `P31`, `P32`), cũng như độ ổn định thời gian phản hồi ở các cặp chậm bất thường (`P39` 54,92 giây, `P23` 65,27 giây dù `P23` chỉ đạt 5 điểm và tốn 688 output token).

## 6. Đánh giá hai phương pháp, kết luận sơ bộ và hướng cải thiện

### 6.1. Ưu điểm và hạn chế của từng phương pháp

| Tiêu chí                           | So khớp văn bản (#3)                                                                                                                             | Hỏi trực tiếp AI `v1` (#4)                                                                                                                                    |
| ---------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Khả năng phát hiện theo 4 mức      | `Equals`/`Contains` bỏ sót 5/5 cặp; Levenshtein chỉ bắt 1/5 bản cũ (2/5 bản đã xóa comment), sót Mức 2, 1/2 Mức 3 và Mức 4.                      | Hiểu ý tưởng thuật toán, bắt đủ 5/5 cặp trên cả 4 mức (90-99/100), kể cả Mức 4 AI viết lại (`P35`).                                                           |
| Phân tách nhóm & báo nhầm          | Đạt 0/40 báo nhầm ở ngưỡng 0,80, nhưng Mức 4 chồng lấn với 3-5 cặp không chép; bỏ qua Mức 4 thì khe hở giữa `P39` và `P14` cũng chỉ là 0,007960. | Đạt 0/40 báo nhầm, tách biệt 50 điểm (40 so với 90) và nhận ra trùng thuật toán kinh điển ở `P39` (40/100 -> `not_copy`).                                     |
| Giải thích & chất lượng đầu ra     | Chỉ trả nhãn và điểm số, không giải thích được đoạn giống hay kiểu chỉnh sửa.                                                                    | Giải thích đúng bản chất thuật toán bằng tiếng Việt, nhưng phát sinh 6 nhóm lỗi trong `responses/` (chi tiết ở Mục 5.3) nên chưa đưa thẳng ra giao diện được. |
| Tốc độ, chi phí & khả năng mở rộng | Xử lý cục bộ không qua mạng, chi phí API bằng 0; nhược điểm là thực nghiệm #3 chưa đo thời gian chạy.                                            | Tăng theo O(N^2) (cận dưới ~3,45 giờ / 30 bài và ~9,71 giờ / 50 bài), phụ thuộc giới hạn API (HTTP 429) và output token phình 1,56 lần input.                 |
| Độ ổn định & phạm vi kiểm chứng    | Kết quả xác định, chạy lại bao nhiêu lần vẫn khớp 100%.                                                                                          | Mới chạy 1 lượt ở temperature mặc định (chưa đo độ ổn định lặp lại) và cỡ mẫu nhỏ (10 bài ngắn, model 550B `:free`) nên chưa chứng minh khả năng tổng quát.   |

### 6.2. Nhận xét và kết luận sơ bộ

- Cả hai cách làm thô đều không thể dùng đơn lẻ cho sản phẩm hoàn chỉnh: so khớp văn bản bỏ sót từ Mức 1-2 và không tách được Mức 4 khỏi bài tự làm, còn hỏi trực tiếp AI `v1` tuy kết luận đúng nhãn cả 45/45 cặp trên bộ mẫu nhỏ nhưng phần giải thích còn nhiều lỗi và vỡ hiệu năng khi số cặp tăng theo O(N^2).
- Hai phương pháp bù trừ rõ rệt cho nhau, là cơ sở trực tiếp cho kiến trúc 2 lớp của nhóm: phân tích thuật toán cục bộ (không tốn API, kết quả xác định) cắt giảm bài toán O(N^2) và xử lý các mức biến đổi cấu trúc, còn AI tập trung vào ca khó Mức 4 và sinh lời giải thích có kiểm soát.
- Về độ tin cậy của kết luận Tuần 1, các nhược điểm của so khớp văn bản (#3) và 6 nhóm lỗi đầu ra của Direct AI `v1` (#4) đã được chứng minh trực tiếp từ code và 45 file phản hồi; riêng tỷ lệ đúng 45/45 của AI cần kiểm chứng lại trên tập 30-50 bài dài hơn (C++ và Python) bằng model chính thức.

### 6.3. Đề xuất hướng cải thiện cho các giai đoạn tiếp theo

Các đề xuất dưới đây bám theo từng nguyên nhân lỗi đã chỉ ra ở Mục 5 (tham chiếu thêm đặc tả đầu ra tại [`API_CONTRACT.md`](../../api/API_CONTRACT.md)):

| Vấn đề ở Mục 5                                                                   | Hướng cải thiện đề xuất                                                                                                                                                                                                                                                                                                                                                                                                | Giai đoạn áp dụng                |
| -------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------- |
| Sót Mức 1 (`P05`) và nền điểm cao do khung đề (`P39`, `P31`)                     | Tiền xử lý (xóa comment, chuẩn hóa khoảng trắng), tách bỏ phần code khung do giảng viên cung cấp (`template_code`) và lưu bảng ánh xạ ngược về số dòng gốc cho Lớp 2.                                                                                                                                                                                                                                                  | Tuần 2-3 (Tiền xử lý)            |
| Sót Mức 2-3 (`P14`, `P22`), lệ thuộc `max_len`, nút thắt O(N^2)                  | Xây Lớp 1 kết hợp Tree-sitter (chuẩn hóa định danh, phân tích AST) với Winnowing (băm dấu vân tay); đặt ngưỡng lọc thấp ưu tiên Recall để không loại mất Mức 4 trước khi đưa sang Lớp 2.                                                                                                                                                                                                                               | Tuần 2-3 (Lớp 1)                 |
| 0/45 câu trả lời nêu số dòng                                                     | Đánh số dòng mã nguồn trước khi đưa vào prompt; ép JSON Schema gồm `risk_level` (`low`/`medium`/`high`), `matched_lines` (`from_line`, `to_line`) và `ai_explanation`, đồng thời tách tọa độ cho từng bài ở schema nội bộ để hậu kiểm hai phía rồi chuyển đổi về đúng cấu trúc đầu ra trong `API_CONTRACT.md`.                                                                                                         | Tuần 4-5 (AI Harness `v2`)       |
| Đoán ngược chiều sao chép, nhầm Bài A/B và dao động điểm                         | Sửa System Prompt (`v2` -> `v3`): bỏ câu hỏi thiên lệch theo chiều "Bài B chép từ Bài A", cấm khẳng định hướng sao chép khi thiếu mốc thời gian, dùng mã bài nộp thay cho "Bài A"/"Bài B", định nghĩa rõ tiêu chí phân mức `risk_level` và cố định `temperature = 0`.                                                                                                                                                  | Tuần 4-5 (AI Harness `v2`->`v3`) |
| Sai chi tiết cú pháp (`P01`, `P40`) và rủi ro ở cụm trùng mẫu sàng               | Cấp 3 công cụ (Tool Calling): `xem_doan_giong` để đối chiếu trực tiếp đoạn mã, `xem_code_mau_giang_vien` để loại phần khung đề và `dem_tan_suat_ca_lop` đếm trên cấu trúc đã chuẩn hóa của Lớp 1.                                                                                                                                                                                                                      | Tuần 4-5 (AI Harness `v2`->`v3`) |
| Thiếu hậu kiểm đầu ra: rò rỉ chữ Hàn (`P10`, `P15`, `P21`), lỗi chính tả (`P04`) | Cài vòng tự kiểm tra (Self-check Loop, tối đa 2-3 lượt): kiểm tra JSON hợp lệ cho schema mới ở `v2`, phạm vi tọa độ dòng trên độ dài thật của hai bài và chặn ký tự ngoài hệ Latin/tiếng Việt cùng bộ ký tự cú pháp code ASCII. Quá giới hạn retry thì gắn cờ kiểm tra thủ công; lưu ý cách này chưa bắt được lỗi chính tả hệ Latin như `P04` (cần giảm thêm bằng `temperature = 0` và chọn model ổn định tiếng Việt). | Tuần 4-5 (AI Harness `v2`->`v3`) |
| Lỗi HTTP 429 và phình output token 1,56 lần input                                | Thêm retry kèm exponential backoff, timeout và giới hạn luồng song song; chọn model không sinh chuỗi suy luận ẩn quá dài (hoặc giới hạn reasoning token) và tách log `reasoning_tokens` nếu API hỗ trợ.                                                                                                                                                                                                                | Tuần 4-5 (Backend & AI)          |
| Lệch bản dữ liệu (#3 vs #4), thiếu thời gian #3 và mẫu nhỏ                       | Khóa duy nhất 1 bản dữ liệu cho mọi phương án; đo đủ thời gian Lớp 1; thống nhất với mentor quy ước gắn nhãn cặp chéo (gồm hai bản sao cùng nguồn, khác nguồn hoặc bản sao với bài gốc khác) cho tập benchmark 30-50 bài (C++, Python).                                                                                                                                                                                | Tuần 6-7 (Chuẩn bị benchmark)    |
| Chưa đo độ ổn định lặp lại và so sánh công bằng các phương án                    | Chạy so sánh cả 3 phương án trên cùng một bộ benchmark: Phương án A (chỉ Lớp 1), Phương án B (Direct AI `v1`) và Phương án C (2 lớp kết hợp AI Harness `v2` -> `v3`), trong đó B và C dùng chung model AI chính thức; chốt quy ước quy đổi `risk_level` về nhãn nhị phân (ví dụ có tính `medium` là dương tính không) và chạy lặp 2-3 lượt trên các cặp biên ở `temperature = 0`.                                      | Tuần 7-8 (Thực nghiệm)           |

## 7. Tổng hợp phân tích lỗi phục vụ demo Tuần 1

Trên bộ mẫu 10 bài Python đề *Count Primes* (45 cặp, gồm 5 cặp sao chép đủ 4 mức và 40 cặp không sao chép): so khớp văn bản (#3) đạt 0/5 với `Equals`/`Contains` và 1/5 với Levenshtein ở ngưỡng >= 0,80 trên bản cũ (2/5 trên bản đã xóa comment), 0/40 báo nhầm; hỏi trực tiếp AI `v1` (#4) đạt 5/5 (90-99/100) và 0/40 báo nhầm (cao nhất 40/100), nhưng mất 21,4 phút, tốn 60.973 token và có 6 nhóm lỗi trong `responses/` (chi tiết ở Mục 5.3).

| #  | Nhóm lỗi / Hạn chế                            | Phương pháp           | Dẫn chứng cụ thể                                                                                                                                | Nguyên nhân gốc                                                                                                     | Hướng khắc phục (Tuần 2+)                                                                                                                                                  |
| -- | --------------------------------------------- | --------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1  | Bỏ sót toàn bộ 5/5 cặp (FN)                   | Equals, Contains (#3) | 0/5 TP, cả hai chiều `Contains` đều `False` kể cả Mức 1 (`P05`).                                                                                | Mọi bản sao đều sửa xen vào giữa thân hàm, cắt đứt chuỗi khớp liên tục.                                             | Thay bằng tiền xử lý và phân tích cấu trúc Lớp 1.                                                                                                                          |
| 2  | Bỏ sót Mức 1 do comment/format                | Levenshtein (#3)      | `P05` bản cũ 0,776224 (`96/429`); xóa comment lên 0,900000 (`37/370`).                                                                          | Không xóa comment, không chuẩn hóa khoảng trắng trên bài ngắn (333 ký tự).                                          | Tiền xử lý (xóa comment, chuẩn hóa khoảng trắng) kèm bảng ánh xạ số dòng gốc.                                                                                              |
| 3  | Bỏ sót Mức 2, 1/2 Mức 3 & lệch `max_len`      | Levenshtein (#3)      | `P14` Mức 2 (0,646774; `219/620`) thấp hơn cả `P22` Mức 3 (0,661793; `347/1.026`); `P29` TP (0,892934).                                         | Không chuẩn hóa tên biến; điểm phụ thuộc tỷ lệ ký tự `dist/max_len` thay vì cấu trúc.                               | Lớp 1 kết hợp Tree-sitter (chuẩn hóa định danh) và Winnowing.                                                                                                              |
| 4  | Bỏ sót Mức 4 & không chọn được ngưỡng         | Levenshtein (#3)      | `P35` (0,552083) thấp hơn 3 cặp không chép bản cũ (`P39` 0,638814, `P31`, `P42`) và 5 cặp bản mới; khe `P39`-`P14` chỉ 0,007960.                | AI viết lại đổi toàn bộ tên biến và viết khác nhiều biểu thức; bài ngắn (11-14 dòng) trùng khung `class Solution`.  | Tách `template_code`; đặt ngưỡng Lớp 1 thấp ưu tiên Recall để Mức 4 lọt sang Lớp 2.                                                                                        |
| 5  | 0/45 câu trả lời nêu số dòng                  | Direct AI (#4)        | Toàn bộ 45 file `responses/` chỉ mô tả theo tên biến, không có số dòng dù `prompt v1` yêu cầu.                                                  | Input đọc bằng `File.ReadAllText` không đánh số dòng; prompt tự do không ép schema.                                 | Đánh số dòng đầu vào; ép JSON Schema có `matched_lines` (tách 2 phía ở nội bộ) kèm Self-check.                                                                             |
| 6  | Suy đoán ngược chiều sao chép                 | Direct AI (#4)        | 4/5 cặp sao chép (`P05`, `P14`, `P22`, `P35`), ví dụ `P14` ghi *"Bài B chỉ đổi tên biến"*.                                                      | Thiếu mốc thời gian nộp, câu hỏi đặt một chiều và `copy-*` đứng ở Bài A.                                            | Sửa System Prompt `v2`: bỏ câu hỏi một chiều, cấm khẳng định hướng sao chép.                                                                                               |
| 7  | Nhầm Bài A/B & rò rỉ ngoại ngữ                | Direct AI (#4)        | `P28`, `P31` gọi cả hai bài là "Bài A"; `P10`, `P15`, `P21` lọt chữ Hàn; `P04` sai chính tả.                                                    | Văn bản sinh tự do ở temperature mặc định, không có bước kiểm tra lại đầu ra.                                       | Dùng mã bài nộp thay Bài A/B; Self-check chặn chữ ngoài hệ Latin/tiếng Việt & ký tự code; cố định `temperature = 0` và chọn model ổn định tiếng Việt hơn cho ca như `P04`. |
| 8  | Sai chi tiết cú pháp & cụm bài trùng mẫu sàng | Direct AI (#4)        | `P01` ghi `n <= 2` (`copy-02` dùng `limit <= 2`), `P40` ghi `b'\x00'` (`solution-02` dùng `b'\x01'`); `P39` 40/100, `P42` 35/100.               | LLM khái quát hóa theo mẫu quen thuộc và thiếu ngữ cảnh khung đề, tần suất cả lớp.                                  | Cấp 3 Tool Calling (`xem_doan_giong`, `xem_code_mau_giang_vien`, `dem_tan_suat_ca_lop`).                                                                                   |
| 9  | Điểm `DIEM_GIONG` thiếu ổn định               | Direct AI (#4)        | Thay `solution-02` bằng `copy-02` làm điểm tụt một chiều 5-15 điểm (`P32` 30 -> `P12` 15); đổi dòng Mức 1 lệch 5 điểm hai chiều.                | Thiếu rubric chấm điểm, temperature mặc định; có dấu hiệu nhạy với biến đổi bề mặt hoặc vị trí A/B (mới đo 1 lượt). | Cố định `temperature = 0`, định nghĩa rubric `risk_level`, chạy lặp 2-3 lượt cặp biên.                                                                                     |
| 10 | Nút thắt O(N^2), HTTP 429 & phình token       | Direct AI (#4)        | Cận dưới lớp 50 bài ~9,71 giờ và ~1,66 triệu token; model thử đầu `gemma` gặp HTTP 429 ở cặp đầu nên phải đổi model; output gấp 1,56 lần input. | Gửi trực tiếp N*(N-1)/2 cặp lên model reasoning `:free`, chưa lọc trước và chưa retry.                              | Lớp 1 cắt giảm số cặp; thêm retry/backoff kèm giới hạn luồng; kiểm soát reasoning token.                                                                                   |

Hướng triển khai tiếp theo là kiến trúc hai lớp: tiền xử lý tách `template_code` kết hợp Lớp 1 Tree-sitter/Winnowing lọc bảo thủ, sau đó đưa các cặp nghi vấn vượt ngưỡng lọc sang Lớp 2 AI Harness `v2`/`v3` gồm JSON Schema, 3 Tool Calling và Self-check Loop để đánh giá ca khó Mức 4 và sinh lời giải thích có kiểm soát. Kết quả 45/45 ở Tuần 1 mới đo 1 lượt trên 10 bài ngắn bằng model 550B `:free`, đồng thời thực nghiệm #3 cần chạy lại đồng bộ trên bản đã xóa comment. Hiệu quả thực tế sẽ được kiểm chứng công bằng trên tập benchmark 30-50 bài C++ và Python ở các tuần sau giữa ba phương án: Phương án A chỉ dùng Lớp 1, Phương án B hỏi trực tiếp AI `v1`, và Phương án C kết hợp hai lớp với AI Harness.
