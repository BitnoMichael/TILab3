using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TI3
{
    internal class EncryptMath
    {
        public long[] FindPrimitiveRoots(long num)
        {
            // Проверка условий существования первообразных корней
            if (!IsValidForPrimitiveRoots(num))
                return Array.Empty<long>();

            var roots = new List<long>();
            long phi = EulerPhi(num);
            var primeFactors = GetUniquePrimeFactors(phi);

            for (long g = 2; g < num; g++)
            {
                if (GCD(g, num) != 1) continue;

                bool isPrimitive = true;
                foreach (var factor in primeFactors)
                {
                    if (PowMulMod(g, phi / factor, num) == 1)
                    {
                        isPrimitive = false;
                        break;
                    }
                }

                if (isPrimitive) roots.Add(g);
            }

            return roots.ToArray();
        }

        // 2. Вычисление произведения степеней по модулю
        public long PowMulMod(long[] nums, long[] pows, long mod)
        {
            if (nums.Length != pows.Length)
                throw new ArgumentException("Arrays must have same length");

            long result = 1;
            for (int i = 0; i < nums.Length; i++)
            {
                if (pows[i] > 0)
                    result = (result * PowMulMod(nums[i], pows[i], mod)) % mod;
                else
                    result = (result * PowMulMod(nums[i], (mod - 2) * (- pows[i]), mod)) % mod;
            }
            return result;
        }

        // 3. Оптимизированное возведение в степень по модулю
        public long PowMulMod(long num, long pow, long mod)
        {
            long result = 1;
            num %= mod;

            while (pow > 0)
            {
                if ((pow & 1) == 1)
                    result = (result * num) % mod;

                num = (num * num) % mod;
                pow >>= 1;
            }
            return result;
        }

        // 5. Поиск взаимно простых чисел
        public long[] GetCoprimes(long num)
        {
            var coprimes = new List<long>();
            for (long i = 1; i < num; i++)
                if (GCD(i, num) == 1)
                    coprimes.Add(i);
            return coprimes.ToArray();
        }
        private long GCD(long a, long b) =>
            b == 0 ? a : GCD(b, a % b);

        private long EulerPhi(long n)
        {
            if (IsPrime(n)) return n - 1;

            long result = n;
            for (long p = 2; p * p <= n; p++)
            {
                if (n % p == 0)
                {
                    while (n % p == 0)
                        n /= p;
                    result -= result / p;
                }
            }
            if (n > 1)
                result -= result / n;
            return result;
        }

        private bool IsValidForPrimitiveRoots(long num)
        {
            if (num == 2 || num == 4) return true;
            if (num % 2 == 0) num /= 2;

            if (!IsPrime(num))
            {
                // Проверка для p^k
                for (long p = 3; p * p <= num; p += 2)
                {
                    if (num % p != 0) continue;
                    while (num % p == 0) num /= p;
                    return num == 1;
                }
            }
            return IsPrime(num);
        }

        private List<long> GetUniquePrimeFactors(long n)
        {
            var factors = new List<long>();
            if (n % 2 == 0) factors.Add(2);
            while (n % 2 == 0) n /= 2;

            for (long i = 3; i * i <= n; i += 2)
            {
                if (n % i == 0)
                {
                    factors.Add(i);
                    while (n % i == 0) n /= i;
                }
            }

            if (n > 2) factors.Add(n);
            return factors;
        }
        public bool IsPrime(long num)
        {
            var lastPossibleDevider = (long)Math.Ceiling(Math.Sqrt(num));
            for (var i = 2; i < lastPossibleDevider + 1; i++)
            {
                if (num % i == 0)
                    return false;
            }
            return true;
        }
    }
}
