# Khảo sát so khớp văn bản tuần 1

## Dữ liệu và cách chạy

Bộ mẫu [Count Primes](../../../datasets/week-01/README.md) gồm 5 bài Python gốc và 5 bản sao. Chương trình đọc 10 file dưới dạng văn bản UTF-8 rồi chuẩn hóa ký tự xuống dòng về LF. Comment, tên biến, khoảng trắng và việc có hay không có ký tự xuống dòng cuối file đều được giữ.

Chạy từ thư mục gốc repo với .NET 10 SDK. Kết quả được tạo bằng SDK 10.0.301.

```powershell
dotnet build experiments/week-01/text-based/TextSimilarityExperiment
dotnet run --no-build --project experiments/week-01/text-based/TextSimilarityExperiment -- datasets/week-01 experiments/week-01/text-based/results.csv
```

[results.csv](results.csv) có một dòng cho mỗi cặp không thứ tự. [Ground Truth](../../../datasets/week-01/ground-truth.csv) xác định 5 cặp dương tính có mức 1 đến 4. Theo nhãn bộ mẫu, 40 cặp còn lại là âm tính. Ground Truth được dùng để chấm dự đoán của từng phương pháp.

## Phương pháp

| Phương pháp | Điều kiện dự đoán tương đồng |
| --- | --- |
| `string.Equals` | Hai chuỗi trùng toàn bộ theo `StringComparison.Ordinal`. |
| `string.Contains` | Toàn bộ chuỗi A xuất hiện liên tục trong B hoặc ngược lại, theo `StringComparison.Ordinal`. CSV ghi cả hai chiều và kết quả hợp nhất. |
| Levenshtein | Điểm bằng `(độ dài lớn nhất − khoảng cách) / độ dài lớn nhất` và đạt ít nhất 0,80. Hai chuỗi rỗng có điểm 1. |

Khoảng cách Levenshtein đếm số lần chèn, xóa hoặc thay thế ít nhất trên đơn vị UTF-16. Quy hoạch động dùng hai hàng, với thời gian O(m × n) và bộ nhớ phụ O(n) cho hai chuỗi có độ dài m và n. Ngưỡng 0,80 được chọn trước khi chạy, dùng điểm chưa làm tròn. CSV hiển thị sáu chữ số thập phân.

## Kết quả trên 45 cặp

| Phương pháp | TP · phát hiện đúng | FP · báo nhầm | FN · bỏ sót | TN · loại đúng |
| --- | ---: | ---: | ---: | ---: |
| Equals | 0 | 0 | 5 | 40 |
| Contains | 0 | 0 | 5 | 40 |
| Levenshtein | 1 | 0 | 4 | 40 |

Cả hai phương pháp so khớp chính xác bỏ sót toàn bộ 5 cặp sao chép. Equals yêu cầu toàn bộ văn bản giống nhau. Contains chấp nhận phần thêm ở ngoài đoạn khớp, nhưng các bản sao đều sửa bên trong chuỗi nên cả hai chiều đều trả về false.

| Cặp Ground Truth | Mức | Điểm Levenshtein | Kết quả | Thay đổi quan sát được |
| --- | ---: | ---: | --- | --- |
| COPY-01 / ORIG-01 | 1 | 0,776224 | Bỏ sót | Chèn comment dưới khai báo lớp và tách các lệnh một dòng thành khối thụt lề. |
| COPY-02 / ORIG-02 | 2 | 0,646774 | Bỏ sót | Đổi nhiều tên xuất hiện lặp lại như `crossed` thành `composite_flags` và tách các câu lệnh một dòng. |
| COPY-03 / ORIG-03 | 3 | 0,661793 | Bỏ sót | Đổi vòng lặp tạo sàng cơ sở, đổi cách duyệt từng khối và chuyển từ gán lát cắt sang đánh dấu từng phần tử. |
| COPY-04 / ORIG-04 | 3 | 0,892934 | Phát hiện đúng | Đổi vòng ngoài từ `for` sang `while` và giữ phần lớn thân sàng tuyến tính. |
| COPY-05 / ORIG-05 | 4 | 0,552083 | Bỏ sót | Bản viết lại giữ ý tưởng sàng bằng `bytearray`, đồng thời đổi tên, cách khởi tạo, biểu thức đánh dấu và cách đếm. |

Levenshtein phát hiện 1/5 cặp ở ngưỡng đã chọn. Hai cặp mức 3 cho kết quả khác nhau vì lượng văn bản giữ lại khác nhau. Cặp âm tính có điểm cao nhất là `solution-01.py` và `solution-05.py` (0,638814). Điểm của cặp này cao hơn cặp sao chép mức 4 (0,552083), nên việc hạ ngưỡng đủ để bắt cặp mức 4 cũng sẽ báo nhầm ít nhất cặp âm tính này. Kết quả 0 báo nhầm tại 0,80 chỉ được quan sát trên 40 cặp âm tính của bộ mẫu.
