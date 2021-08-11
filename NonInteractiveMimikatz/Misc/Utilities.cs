// Author: Ryan Cobb (@cobbr_io)
// Project: SharpSploit (https://github.com/cobbr/SharpSploit)
// License: BSD 3-Clause

using System.IO;
using System.Security.Cryptography;


namespace SSploit.Misc
{
    public static class Utilities
    {

        private static byte[] PerformCryptography(ICryptoTransform cryptoTransform, byte[] data)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(data, 0, data.Length);
                    cryptoStream.FlushFinalBlock();
                    return memoryStream.ToArray();
                }
            }
        }

        static readonly byte[] myIV = NonInteractiveMimikatz.Properties.Resources.IV; //new byte[] { 40, 58, 32, 174, 187, 91, 201, 68, 81, 201, 230, 98, 222, 237, 26, 145 };
        static readonly byte[] myKey = NonInteractiveMimikatz.Properties.Resources.key;// new byte[] { 141, 46, 84, 223, 171, 106, 191, 231, 223, 254, 202, 22, 25, 216, 114, 211, 134, 201, 239, 179, 80, 64, 238, 106, 174, 7, 173, 31, 14, 142, 140, 179 };


        public static byte[] Encrypt(byte[] data)
        {
            Aes _algorithm = Aes.Create();

            using (var encryptor = _algorithm.CreateEncryptor(myKey, myIV))
            {
                return PerformCryptography(encryptor, data);
            }

        }

        public static byte[] Decrypt(byte[] data)
        {
            Aes _algorithm = Aes.Create();
            using (var decryptor = _algorithm.CreateDecryptor(myKey, myIV))
            {
                return PerformCryptography(decryptor, data);
            }

        }

    }
}