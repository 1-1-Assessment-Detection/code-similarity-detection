class Solution:
    def countPrimes(self, n: int) -> int:
        if n <= 2: return 0
        size = (n - 2) // 2
        crossed = bytearray(size)
        for i in range(size):
            p = 2 * i + 3
            if p * p >= n: break
            if not crossed[i]:
                start = (p * p - 3) // 2
                length = len(crossed[start::p])
                crossed[start::p] = b'\x01' * length
        return 1 + crossed.count(0)