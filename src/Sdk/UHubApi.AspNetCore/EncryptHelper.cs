using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace UHubApi.AspNetCore
{
    public class EncryptHelper
    {
        /// <summary>
        /// AES 加密
        /// </summary>
        /// <param name="source"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Encrypt(string source, string key)
        {
            if (string.IsNullOrEmpty(source)) return null;
            Byte[] toEncryptArray = Encoding.UTF8.GetBytes(source);

            RijndaelManaged rm = new RijndaelManaged
            {
                Key = Encoding.UTF8.GetBytes(key),
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };

            ICryptoTransform cTransform = rm.CreateEncryptor();
            Byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        /// <summary>
        /// AES 解密
        /// </summary>
        /// <param name="encryptStr"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Decrypt(string encryptStr, string key)
        {
            if (string.IsNullOrEmpty(encryptStr)) return null;
            Byte[] toEncryptArray = Convert.FromBase64String(encryptStr);

            RijndaelManaged rm = new RijndaelManaged
            {
                Key = Encoding.UTF8.GetBytes(key),
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };

            ICryptoTransform cTransform = rm.CreateDecryptor();
            Byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return Encoding.UTF8.GetString(resultArray);
        }
    }
}
