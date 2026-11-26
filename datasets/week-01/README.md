# Week 01 — Count Primes (Python)

Mười bài giải đề 204, đếm số nguyên tố nhỏ hơn `n` với `0 ≤ n ≤ 5 × 10^6`. Mỗi file có lớp `Solution` và phương thức `countPrimes(n)`, tương thích chữ ký LeetCode.

Năm bài gốc dùng các cách khác nhau: sàng Eratosthenes, sàng chỉ số lẻ, sàng phân đoạn, sàng tuyến tính và sàng `bytearray` với phép gán lát cắt.

- `original/solution-01.py` … `solution-05.py`: năm bài độc lập.
- `copied/copy-01-level-1.py` … `copy-05-level-4.py`: năm bài sao chép; mỗi bài gốc có đúng một bản sao.
- `ground-truth.csv`: ánh xạ bài gốc, mức độ và nội dung thay đổi. Phân bổ mức: một bài mức 1, một bài mức 2, hai bài mức 3 và một bài mức 4.
- `verify/verify_dataset.py`: verifier dùng thư viện chuẩn Python.

Chạy từ thư mục `datasets/week-01/`:

```powershell
python verify/verify_dataset.py
```

Verifier kiểm tra các ví dụ, ca biên, 200 đầu vào ngẫu nhiên cố định, giá trị đã biết và `n = 5.000.000` (kết quả `348513`).