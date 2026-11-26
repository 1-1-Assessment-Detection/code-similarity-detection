class Solution:
    def countPrimes(self, limit: int) -> int:
        if limit <= 2:
            return 0
        odd_slots = (limit - 2) // 2
        composite_flags = bytearray(odd_slots)
        for slot in range(odd_slots):
            divisor = 2 * slot + 3
            if divisor * divisor >= limit:
                break
            if not composite_flags[slot]:
                first_slot = (divisor * divisor - 3) // 2
                view_length = len(composite_flags[first_slot::divisor])
                composite_flags[first_slot::divisor] = b'\x01' * view_length
        return 1 + composite_flags.count(0)