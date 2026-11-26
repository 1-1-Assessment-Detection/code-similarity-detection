class Solution:
    def countPrimes(self, n: int) -> int:
        if n <= 2: return 0
        flags = bytearray(b'\x01') * n
        flags[0:2] = b'\x00\x00'
        p = 2
        while p * p < n:
            if flags[p]:
                start = p * p
                flags[start::p] = b'\x00' * ((n - 1 - start) // p + 1)
            p += 1
        return flags.count(1)