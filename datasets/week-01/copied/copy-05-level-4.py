class Solution:
    def countPrimes(self, n: int) -> int:
        if n < 3: return 0
        is_prime = bytearray([1]) * n
        is_prime[0] = is_prime[1] = 0
        candidate = 2
        while candidate * candidate < n:
            if is_prime[candidate]:
                first = candidate * candidate
                count = (n - 1 - first) // candidate + 1
                is_prime[first::candidate] = bytearray(count)
            candidate += 1
        return sum(is_prime)