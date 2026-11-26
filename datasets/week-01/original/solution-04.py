class Solution:
    def countPrimes(self, n: int) -> int:
        least = [0] * n
        primes = []
        for value in range(2, n):
            if least[value] == 0:
                least[value] = value
                primes.append(value)
            for p in primes:
                composite = value * p
                if p > least[value] or composite >= n: break
                least[composite] = p
        return len(primes)