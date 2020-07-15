using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace AESCryption
{
    class AESCryption
    {
        private static string DEFAULT_AES_IV = "pf69DL6GrWFyZcMK";
        private static string DEFAULT_AES_KEY = "9Fix4L4HB4PKeKWY";
        private static string aes_iv_data = DEFAULT_AES_IV;
        private static string aes_key_data = DEFAULT_AES_KEY;

        public static string AES_IV { get { return aes_iv_data; } }
        public static string AES_KEY { get { return aes_key_data; } }
        public static string Encrypt(string text,string iv, string key)
        {
            byte[] encrypted;
            using (RijndaelManaged rijndael = new RijndaelManaged())
            {
                rijndael.BlockSize = 128;
                rijndael.KeySize = 128;
                rijndael.Mode = CipherMode.CBC;
                rijndael.Padding = PaddingMode.PKCS7;

                rijndael.IV = Encoding.UTF8.GetBytes(iv);
                rijndael.Key = Encoding.UTF8.GetBytes(key);

                ICryptoTransform encryptor = rijndael.CreateEncryptor(rijndael.Key, rijndael.IV);
                using (MemoryStream mStream = new MemoryStream())
                {
                    using(CryptoStream ctStream=new CryptoStream(mStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(ctStream))
                        {
                            sw.Write(text);
                        }
                        encrypted = mStream.ToArray();
                    }
                }
            }
            return (System.Convert.ToBase64String(encrypted));
        }

        public static string Decrypt(string cipher,string iv,string key)
        {
            string plain = string.Empty;
            using (RijndaelManaged rijndael = new RijndaelManaged())
            {
                rijndael.BlockSize = 128;
                rijndael.KeySize = 128;
                rijndael.Mode = CipherMode.CBC;
                rijndael.Padding = PaddingMode.PKCS7;

                rijndael.IV = Encoding.UTF8.GetBytes(iv);
                rijndael.Key = Encoding.UTF8.GetBytes(key);

                ICryptoTransform decryptor = rijndael.CreateDecryptor(rijndael.Key, rijndael.IV);
                using (MemoryStream mStream = new MemoryStream(System.Convert.FromBase64String(cipher)))
                {
                    using (CryptoStream ctStream = new CryptoStream(mStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(ctStream))
                        {
                            plain = sr.ReadLine();
                        }
                    }
                }
            }
            return plain;
        }
    }
}
