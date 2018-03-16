using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Common.Helpers
{
    public class SkillWarsEncoder
    {
        public static string Encript(string inputText)
        {
            byte[] encrypted = null;
            string key = "SW_SKILL_WARS_24_BYTES!_";

            using (TripleDESCryptoServiceProvider myTripleDES = new TripleDESCryptoServiceProvider())
            {
                myTripleDES.Key = Encoding.ASCII.GetBytes(key);
                myTripleDES.IV = Encoding.ASCII.GetBytes("_8BYTES_");

                // Encrypt the string to an array of bytes.
                encrypted = EncryptStringToBytes(inputText, myTripleDES.Key, myTripleDES.IV);
            }

            return Convert.ToBase64String(encrypted);
        }

        public static string Decript(string inputText)
        {
            var encrypted = Convert.FromBase64String(inputText);

            string roundtrip = null;
            string key = "SW_SKILL_WARS_24_BYTES!_";

            using (TripleDESCryptoServiceProvider myTripleDES = new TripleDESCryptoServiceProvider())
            {
                myTripleDES.Key = Encoding.ASCII.GetBytes(key);
                myTripleDES.IV = Encoding.ASCII.GetBytes("_8BYTES_");

                // Decrypt the bytes to a string.
                roundtrip = DecryptStringFromBytes(encrypted, myTripleDES.Key, myTripleDES.IV);
            }

            return roundtrip;
        }

        static byte[] EncryptStringToBytes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;
            // Create an TripleDESCryptoServiceProvider object
            // with the specified key and IV.
            using (TripleDESCryptoServiceProvider tdsAlg = new TripleDESCryptoServiceProvider())
            {
                tdsAlg.Key = Key;
                tdsAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = tdsAlg.CreateEncryptor(tdsAlg.Key, tdsAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {

                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }

        static string DecryptStringFromBytes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an TripleDESCryptoServiceProvider object
            // with the specified key and IV.
            using (TripleDESCryptoServiceProvider tdsAlg = new TripleDESCryptoServiceProvider())
            {
                tdsAlg.Key = Key;
                tdsAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = tdsAlg.CreateDecryptor(tdsAlg.Key, tdsAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }

            }

            return plaintext;

        }
    }
}
