import importlib.util
import random
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]

def reference(n):
    count = 0
    for value in range(2, n):
        prime = True
        divisor = 2
        while divisor * divisor <= value:
            if value % divisor == 0:
                prime = False
                break
            divisor += 1
        count += prime
    return count

def main():
    cases = {0: 0, 1: 0, 2: 0, 3: 1, 10: 4, 100: 25, 1000: 168,
             100000: 9592, 5000000: 348513}
    rng = random.Random(204)
    for _ in range(200):
        n = rng.randrange(0, 5001)
        cases[n] = reference(n)
    paths = sorted((ROOT / 'original').glob('*.py')) + sorted((ROOT / 'copied').glob('*.py'))
    assert len(paths) == 10, f'Expected 10 submissions, found {len(paths)}'
    for i, path in enumerate(paths):
        spec = importlib.util.spec_from_file_location(f'submission_{i}', path)
        module = importlib.util.module_from_spec(spec)
        spec.loader.exec_module(module)
        solution = module.Solution()
        for n, expected in cases.items():
            actual = solution.countPrimes(n)
            assert actual == expected, f'{path.name}: n={n}, got {actual}, expected {expected}'
    print(f'PASS: {len(paths)} submissions x {len(cases)} cases')

if __name__ == '__main__':
    main()