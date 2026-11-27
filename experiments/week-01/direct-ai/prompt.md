# Prompt `v1` — Hỏi trực tiếp AI

Prompt thô dùng chung cho mọi cặp. Không dùng System Prompt có kiểm soát, không ép JSON schema, không Tool Calling, không Self-check Loop. Mọi thay đổi nội dung phải tăng phiên bản và ghi lại ở cuối file.

## Prompt

```text
Bạn là người chấm bài tập lập trình. Dưới đây là hai bài nộp cho cùng một đề bài.
Hãy cho biết bài B có phải được sao chép hoặc viết lại từ bài A (hoặc ngược lại) hay không.

Trả lời đúng theo định dạng sau:
KET_LUAN: copy hoặc not_copy
DIEM_GIONG: một số nguyên từ 0 đến 100
GIAI_THICH: giải thích ngắn gọn bằng tiếng Việt, nêu rõ giống ở hàm nào, dòng nào, đã chỉnh sửa kiểu gì

=== BÀI A ===
{source_a}

=== BÀI B ===
{source_b}
```

`{source_a}` và `{source_b}` được thay bằng nội dung file. Không chèn tên file, ghi chú, nhãn hoặc bất kỳ thông tin nào gợi ý đáp án.

## Ghi chú

- Hai bài trong cặp được đưa vào theo thứ tự cố định theo `pair_id` để kết quả lặp lại được.
- Thực nghiệm chỉ ghi nhận `KET_LUAN` và `DIEM_GIONG` để đối chiếu. Phản hồi gốc được lưu nguyên để phân tích lỗi.

## Lịch sử phiên bản

| Phiên bản | Ngày | Thay đổi |
| --- | --- | --- |
| `v1` | 2026-11-25 | Bản nháp đầu, chưa chạy. |
