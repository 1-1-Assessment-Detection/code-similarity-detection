# 204. Đếm Số Nguyên Tố (Count Primes)

**Nguồn đề:** PDF `204. Đếm Số Nguyên Tố (Count Primes).pdf` do người phụ trách cung cấp, dựa trên LeetCode 204.

**Độ khó theo PDF:** Trung bình (Medium).

**Tỷ lệ chấp nhận ghi trong PDF:** 35,8%.

## Phạm vi dùng cho bộ dữ liệu tuần 1

Đề bài này đã được người phụ trách Nhóm 1.1 thống nhất với lead Nhóm 1.2. Nhóm 1.2 sử dụng **C++**; Nhóm 1.1 sử dụng **Python**.

## Mô tả bài toán

Cho một số nguyên `n`, hãy trả về số lượng số nguyên tố **nhỏ hơn hẳn** `n`.

## Input

- Một tham số tên `n`, kiểu `int`.
- Ràng buộc: `0 ≤ n ≤ 5 × 10^6`.
- Khai báo kiểu trong Python:

  ```yaml
  n: int
  ```

## Output

- Một giá trị kiểu `int`: số lượng số nguyên tố nhỏ hơn `n`.
- Giá trị nhỏ nhất là `0`.
- Không tính chính `n`, kể cả khi `n` là số nguyên tố.

## Ví dụ

| Ví dụ | Input | Output | Giải thích |
| --- | --- | --- | --- |
| 1 | `n = 10` | `4` | Có 4 số nguyên tố nhỏ hơn 10: 2, 3, 5, 7. |
| 2 | `n = 0` | `0` | Không có số nguyên tố nhỏ hơn 0. |
| 3 | `n = 1` | `0` | Không có số nguyên tố nhỏ hơn 1. |
