using System;
using System.Security.Cryptography;

namespace DataSaveLoad_Encryption
{
    public class RSAKeyHandler
    {
        private RSACryptoServiceProvider _rsa;
        private RSAParameters _publicKey;
        private RSAParameters _privateKey;

        public RSAKeyHandler(int keySize = 2048)
        {
            _rsa = new RSACryptoServiceProvider(keySize);
        }

        public void SetKeysFromData(RSAKeyData keyData)
        {
            _publicKey = keyData.publicKey.ToRSAParameters();
            _privateKey = keyData.privateKey.ToRSAParameters();
        }

        public string EncryptAESKey(byte[] aesKey)
        {
            _rsa.ImportParameters(_publicKey);
            byte[] encryptedKey = _rsa.Encrypt(aesKey, false);
            return Convert.ToBase64String(encryptedKey);
        }

        public byte[] DecryptAESKey(string encryptedAesKey)
        {
            _rsa.ImportParameters(_privateKey);
            byte[] encryptedBytes = Convert.FromBase64String(encryptedAesKey);
            return _rsa.Decrypt(encryptedBytes, false);
        }
    }
}