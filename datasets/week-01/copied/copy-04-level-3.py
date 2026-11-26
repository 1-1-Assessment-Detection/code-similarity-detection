class Solution:
    def countPrimes(self, n: int) -> int:
        least = [0] * n
        primes = []
        value = 2
        while value < n:
            if least[value] == 0:
                least[value] = value
                primes.append(value)
            for p in primes:
                composite = value * p
                if p > least[value] or composite >= n: break
                least[composite] = p
            value += 1
        return len(primes)