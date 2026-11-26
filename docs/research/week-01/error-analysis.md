# Phân tích lỗi phát hiện mã nguồn tương đồng - Tuần 1

Issue liên quan: #5 Analyze and Document Detection Errors.

Tài liệu tổng hợp kết quả thực nghiệm Tuần 1 trên bài mẫu [204. Đếm Số Nguyên Tố (Count Primes)](../sample-problem.md) bằng Python (10 bài nộp, tổng cộng 45 cặp).

## 1. Dữ liệu đầu vào

- Bộ dữ liệu mẫu (#2, nhánh `datasets/week-01`): gồm 5 bài tự viết (`ORIG-01` đến `ORIG-05`), 5 bài sao chép theo 4 mức (`COPY-01` đến `COPY-05`) và file nhãn [`ground-truth.csv`](../../../datasets/week-01/ground-truth.csv). Trong 45 cặp có 5 cặp sao chép (1 cặp Mức 1, 1 cặp Mức 2, 2 cặp Mức 3, 1 cặp Mức 4) và 40 cặp không sao chép.
- Kết quả so khớp văn bản (#3, nhánh `feature/week-01-text-based`, commit `81b61b7`): lấy từ [`results.csv`](../../../experiments/week-01/text-based/results.csv) và [`README.md`](../../../experiments/week-01/text-based/README.md).

### Ghi chú về lệch phiên bản dữ liệu

Khi kiểm tra lại commit giữa #2 và #3 thì thấy có chỗ lệch:

- Ở nhánh `datasets/week-01`, commit `8f2e750` đã xóa dòng comment `# Same code; only whitespace and this comment were changed.` trong `copied/copy-01-level-1.py` (file giảm từ 429 xuống 369 ký tự khi chuẩn hóa LF).
- Tuy nhiên file `results.csv` của #3 (`81b61b7`) lại chạy trên bản cũ (`c11949a`) khi dòng comment đó vẫn còn, nên cặp `COPY-01 / ORIG-01` bị kéo khoảng cách Levenshtein lên 96 (điểm 0,776224).
- Cần báo bên #3 chạy lại trên commit mới nhất của #2 để sau này so sánh với #4 trên cùng một bộ file.

## 2. Kết quả phương pháp so khớp văn bản (#3)

Chương trình C# của #3 đọc nguyên văn 10 file UTF-8, chỉ đổi xuống dòng về LF, còn comment, khoảng trắng và tên biến giữ nguyên. Có 3 cách đo được chạy trên đủ 45 cặp:

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

| Cặp bài           | File đối chiếu                           | Mức | Equals | Contains | Dist | Điểm Lev. | KQ Lev. |
| ----------------- | ---------------------------------------- | --- | ------ | -------- | ---- | --------- | ------- |
| COPY-01 / ORIG-01 | `solution-01.py` / `copy-01-level-1.py`  | 1   | FN     | FN       | 96   | 0,776224  | FN      |
| COPY-02 / ORIG-02 | `solution-02.py` / `copy-02-level-2.py`  | 2   | FN     | FN       | 219  | 0,646774  | FN      |
| COPY-03 / ORIG-03 | `solution-03.py` / `copy-03-level-3.py`  | 3   | FN     | FN       | 347  | 0,661793  | FN      |
| COPY-04 / ORIG-04 | `solution-04.py` / `copy-04-level-3.py`  | 3   | FN     | FN       | 50   | 0,892934  | TP      |
| COPY-05 / ORIG-05 | `solution-05.py` / `copy-05-level-4.py`  | 4   | FN     | FN       | 215  | 0,552083  | FN      |

Ghi nhận thay đổi trong code của 5 cặp trên:

- **COPY-01 / ORIG-01 (Mức 1):** chỉ thêm 1 dòng comment (ở bản `c11949a`) và tách 2 câu lệnh `if`, `for` viết 1 dòng ra thành nhiều dòng.
- **COPY-02 / ORIG-02 (Mức 2):** logic sàng số lẻ không đổi nhưng đổi hết tên tham số và biến cục bộ (`n` -> `limit`, `crossed` -> `composite_flags`, `p` -> `divisor`...), cộng thêm tách dòng ở lệnh `if`.
- **COPY-03 / ORIG-03 (Mức 3):** sửa nhiều vòng lặp (vòng sàng cơ sở đổi `for` -> `while`, vòng duyệt block đổi `while` -> `for`, đoạn gán slice chuyển sang `for` đánh dấu từng phần tử), đồng thời đổi tên biến (`sieve` -> `base`, `segment` -> `chunk`).
- **COPY-04 / ORIG-04 (Mức 3):** sửa ít nhất trong nhóm Mức 3, chỉ đổi vòng `for value in range(2, n)` ngoài cùng thành `while value < n` với `value += 1`, phần còn lại gần như y nguyên.
- **COPY-05 / ORIG-05 (Mức 4):** bản AI viết lại vẫn dùng `bytearray` và cùng cách tính độ dài slice, nhưng đổi sạch tên biến và viết khác các biểu thức (`b'\x01'` -> `[1]`, `flags.count(1)` -> `sum(is_prime)`).

### 2.3. Thống kê 40 cặp không sao chép

- Cả 40 cặp đều được xếp đúng là TN ở cả 3 cách đo (không có báo nhầm ở ngưỡng 0,80).
- Điểm Levenshtein của nhóm này dao động từ 0,263158 đến 0,638814, trung bình 0,420041.
- 5 cặp không sao chép có điểm Levenshtein cao nhất:

| Cặp bài                                                 | Dist | Điểm Lev. | So với cặp Mức 4 (0,552083) |
| ------------------------------------------------------- | ---- | --------- | --------------------------- |
| `original/solution-01.py` / `original/solution-05.py`   | 134  | 0,638814  | Cao hơn                     |
| `copied/copy-05-level-4.py` / `original/solution-01.py` | 195  | 0,593750  | Cao hơn                     |
| `original/solution-02.py` / `original/solution-05.py`   | 190  | 0,573034  | Cao hơn                     |
| `original/solution-01.py` / `original/solution-02.py`   | 206  | 0,537079  | Thấp hơn                    |
| `copied/copy-01-level-1.py` / `original/solution-05.py` | 208  | 0,515152  | Thấp hơn                    |

Đáng chú ý là có 3 cặp không sao chép đạt điểm cao hơn cặp Mức 4 (`COPY-05 / ORIG-05`, 0,552083), thậm chí `copy-05-level-4.py` còn có điểm văn bản gần `solution-01.py` hơn cả bài gốc `solution-05.py`. Điểm này sẽ cần phân tích kỹ hơn ở bước đối chiếu và giải thích nguyên nhân sai.
