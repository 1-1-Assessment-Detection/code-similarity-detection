from math import isqrt

class Solution:
    def countPrimes(self, n: int) -> int:
        if n <= 2: return 0
        root = isqrt(n - 1)
        base = bytearray(b'\x01') * (root + 1)
        base[0:2] = b'\x00\x00'
        factor = 2
        while factor * factor <= root:
            if base[factor]:
                start = factor * factor
                base[start::factor] = b'\x00' * len(base[start::factor])
            factor += 1
        prime_list = [value for value in range(2, root + 1) if base[value]]
        total = 0
        block_size = 32768
        for low in range(2, n, block_size):
            high = min(low + block_size, n)
            chunk = bytearray(b'\x01') * (high - low)
            for prime in prime_list:
                first = max(prime * prime, ((low + prime - 1) // prime) * prime)
                if first < high:
                    for composite in range(first, high, prime):
                        chunk[composite - low] = 0
            total += chunk.count(1)
        return total