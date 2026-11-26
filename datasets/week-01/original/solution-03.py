from math import isqrt

class Solution:
    def countPrimes(self, n: int) -> int:
        if n <= 2: return 0
        limit = isqrt(n - 1)
        sieve = bytearray(b'\x01') * (limit + 1)
        sieve[0:2] = b'\x00\x00'
        for p in range(2, isqrt(limit) + 1):
            if sieve[p]:
                start = p * p
                sieve[start::p] = b'\x00' * len(sieve[start::p])
        primes = [p for p in range(2, limit + 1) if sieve[p]]
        count, block, low = 0, 32768, 2
        while low < n:
            high = min(low + block, n)
            segment = bytearray(b'\x01') * (high - low)
            for p in primes:
                start = max(p * p, ((low + p - 1) // p) * p)
                if start < high:
                    segment[start - low::p] = b'\x00' * len(segment[start - low::p])
            count += segment.count(1)
            low = high
        return count