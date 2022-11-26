using System;
using System.IO;
using System.Security.Cryptography;

namespace CustomFTP
{
    class SymmetryCrypt
    {

        public static Stream Decrypt(Stream streamToDecrypt, byte[] key)
        {
            try
            {
                SymmetricAlgorithm algorithm = new RijndaelManaged();

                algorithm.Mode = CipherMode.CFB;
                algorithm.Padding = PaddingMode.PKCS7;
                byte[] _salt = new byte[] { 0x25, 0xdc, 0xff, 0x00, 0xad, 0xed, 0x7a, 0xee, 0xc5, 0xfe, 0x07, 0xaf, 0x4d, 0x08, 0x12, 0x3c };

                Rfc2898DeriveBytes rfc = new Rfc2898DeriveBytes(key, _salt, 100);
                algorithm.Key = rfc.GetBytes(algorithm.KeySize / 8);
                algorithm.IV = rfc.GetBytes(algorithm.BlockSize / 8);


                ICryptoTransform decryptor = algorithm.CreateDecryptor();
                CryptoStream cryptoStream = new CryptoStream(streamToDecrypt, decryptor, CryptoStreamMode.Read);
                return cryptoStream;//уничтожение будет на стороне вызывающей функции
            }
            catch (Exception e)
            {
                UniversalIO.Print("Ошибка дешифратора");
                FileFuncs.Log(e.Message);
                return null;
            }

        }
    }
    public enum EncryptionAlgorithm
    {
        TripleDES,
        AES,
    }

}
