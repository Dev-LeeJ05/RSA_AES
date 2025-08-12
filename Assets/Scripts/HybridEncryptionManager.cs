using System;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

namespace DataSaveLoad_Encryption
{
    public class HybridEncryptionManager : MonoBehaviour
    {
        private const string RSA_KEY_FILE = "rsa_key.json";
        private const string SAVE_FILE_NAME = "gamedata.sav";

        private RSAKeyHandler _rsaHandler;

        private void Awake()
        {
            _rsaHandler = new RSAKeyHandler();
            LoadOrCreateRSAKeys();
        }

        private void LoadOrCreateRSAKeys()
        {
            string keyFilePath = Path.Combine(Application.persistentDataPath, RSA_KEY_FILE);

            if (File.Exists(keyFilePath))
            {
                Debug.Log("RSA key file found. Loading keys.");
                string json = File.ReadAllText(keyFilePath);
                RSAKeyData keyData = JsonUtility.FromJson<RSAKeyData>(json);
                _rsaHandler.SetKeysFromData(keyData);
            }
            else
            {
                Debug.Log("RSA key file not found. Creating new keys.");
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048))
                {
                    RSAParameters publicKey = rsa.ExportParameters(false);
                    RSAParameters privateKey = rsa.ExportParameters(true);

                    RSAKeyData keyData = new RSAKeyData
                    {
                        publicKey = new RSAParametersData(publicKey),
                        privateKey = new RSAParametersData(privateKey)
                    };

                    string json = JsonUtility.ToJson(keyData);
                    File.WriteAllText(keyFilePath, json);
                    _rsaHandler.SetKeysFromData(keyData);
                }
            }
        }

        public void SaveData(string data)
        {
            using (Aes aes = Aes.Create())
            {
                aes.GenerateKey();
                aes.GenerateIV();

                AESHandler aesHandler = new AESHandler(aes.Key, aes.IV);
                string encryptedData = aesHandler.Encrypt(data);

                string encryptedAESKey = _rsaHandler.EncryptAESKey(aes.Key);
                string encryptedAESIV = _rsaHandler.EncryptAESKey(aes.IV);

                string saveFilePath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
                string fileContent = $"{encryptedAESKey}\n{encryptedAESIV}\n{encryptedData}";
                File.WriteAllText(saveFilePath, fileContent);

                Debug.Log($"Encryption Data : {fileContent}");
                Debug.Log("Game data saved successfully.");
            }
        }

        public string LoadData()
        {
            string saveFilePath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
            if (!File.Exists(saveFilePath))
            {
                Debug.LogWarning("Save file not found.");
                return null;
            }

            string fileContent = File.ReadAllText(saveFilePath);
            string[] lines = fileContent.Split('\n');

            string encryptedAESKey = lines[0];
            string encryptedAESIV = lines[1];
            string encryptedData = lines[2];

            byte[] aesKey = _rsaHandler.DecryptAESKey(encryptedAESKey);
            byte[] aesIV = _rsaHandler.DecryptAESKey(encryptedAESIV);

            AESHandler aesHandler = new AESHandler(aesKey, aesIV);
            string decryptedData = aesHandler.Decrypt(encryptedData);

            Debug.Log($"Decryption Data : {decryptedData}");
            Debug.Log("Game data loaded successfully.");
            return decryptedData;
        }
    }

    [Serializable]
    public class RSAKeyData
    {
        public RSAParametersData publicKey;
        public RSAParametersData privateKey;
    }

    [Serializable]
    public class RSAParametersData
    {
        public string Exponent;
        public string Modulus;
        public string P;
        public string Q;
        public string DP;
        public string DQ;
        public string InverseQ;
        public string D;

        public RSAParametersData(RSAParameters parameters)
        {
            Exponent = parameters.Exponent != null ? Convert.ToBase64String(parameters.Exponent) : null;
            Modulus = parameters.Modulus != null ? Convert.ToBase64String(parameters.Modulus) : null;
            P = parameters.P != null ? Convert.ToBase64String(parameters.P) : null;
            Q = parameters.Q != null ? Convert.ToBase64String(parameters.Q) : null;
            DP = parameters.DP != null ? Convert.ToBase64String(parameters.DP) : null;
            DQ = parameters.DQ != null ? Convert.ToBase64String(parameters.DQ) : null;
            InverseQ = parameters.InverseQ != null ? Convert.ToBase64String(parameters.InverseQ) : null;
            D = parameters.D != null ? Convert.ToBase64String(parameters.D) : null;
        }

        public RSAParameters ToRSAParameters()
        {
            return new RSAParameters
            {
                Exponent = Exponent != null ? Convert.FromBase64String(Exponent) : null,
                Modulus = Modulus != null ? Convert.FromBase64String(Modulus) : null,
                P = P != null ? Convert.FromBase64String(P) : null,
                Q = Q != null ? Convert.FromBase64String(Q) : null,
                DP = DP != null ? Convert.FromBase64String(DP) : null,
                DQ = DQ != null ? Convert.FromBase64String(DQ) : null,
                InverseQ = InverseQ != null ? Convert.FromBase64String(InverseQ) : null,
                D = D != null ? Convert.FromBase64String(D) : null
            };
        }
    }
}
