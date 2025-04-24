using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TI3
{
    internal class ElHamalCipherer
    {
        private Random _rd = new Random((int)DateTime.Now.Ticks);
        private EncryptMath _math;
        private long _p;
        public long P { get => _p;
            set
            {
                _p = value;
            }
        }
        public int AOrBLength { get => (int)Math.Ceiling(Math.Ceiling(Math.Log((double)P, 2)) / 8); }
        private long _g;
        public long G { get => _g; set => _g = value; }
        private long _x;
        public long X { get => _x; set => _x = value; }
        public long Y { get => _math.PowMulMod(G, X, P); }
        public long K { get; set; }
        public ElHamalCipherer(EncryptMath math, long p, long g, long x, long k)
        {
            _math = math;
            P = p;
            G = g;
            X = x;
            K = k;  
        }
        private long _k
        {
            get { return _k; }
        }
        public long[] CipherByte(byte cipheredByte)
        {
            if (P < 256)
                throw new ArgumentException("P слишком мало для шифровани побайтово");
            long k, a, b;
            k = K;
            a = _math.PowMulMod(G, k, P);
            b = _math.PowMulMod(new long[] { Y, (long)cipheredByte }, new long[] { k, 1 }, P);

            var result = new long[2];
            result[0] = b;
            result[1] = a;
            return result;
        }
        public byte Decipher(byte[] cipheredBytes)
        {
            if (cipheredBytes.Length != AOrBLength * 2)
                throw new ArgumentException();
            long result = 0;

            switch (AOrBLength)
            {
                case 1:
                    var b8 = cipheredBytes[0];
                    var a8 = cipheredBytes[1];
                    result = _math.PowMulMod(new long[] { (long)b8, (long)a8 }, new long[] { 1, -X }, P);
                    break;
                case 2:
                    var b16 = BitConverter.ToInt16(cipheredBytes, 0);
                    var a16 = BitConverter.ToInt16(cipheredBytes, cipheredBytes.Length / 2);
                    result = _math.PowMulMod(new long[] { (long)b16, (long)a16 }, new long[] { 1, -X }, P);
                    break;
                case 3:
                    var b32 = BitConverter.ToInt32(cipheredBytes, 0);
                    var a32 = BitConverter.ToInt32(cipheredBytes, cipheredBytes.Length / 2);
                    result = _math.PowMulMod(new long[] { (long)b32, (long)a32 }, new long[] { 1, -X }, P);
                    break;
                case 4:
                    var b64 = BitConverter.ToInt64(cipheredBytes, 0);
                    var a64 = BitConverter.ToInt64(cipheredBytes, cipheredBytes.Length / 2);
                    result = _math.PowMulMod(new long[] { (long)b64, (long)a64 }, new long[] { 1, -X }, P);
                    break;
                default:
                    throw new ArgumentException("P слишком велико");
            }
            return (byte) result;
        }
    }
}
