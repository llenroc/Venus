using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Venus.Infrastructure.Exchanges;

namespace Venus.Infrastructure
{
    public class ApiKeyData : IDisposable
    {
        public SupportedExchanges Exchange { get; }
        private readonly byte[] _apiKey;
        private readonly byte[] _keyEntropy;
        private readonly byte[] _apiSecret;
        private readonly byte[] _secretEntropy;
        private readonly byte[] _apiPasssword;
        private readonly byte[] _pwdEntropy;


        public ApiKeyData(
            SupportedExchanges exchange, 
            string key, 
            string secret, 
            string password)
        {
            Exchange = exchange;

            _keyEntropy = GetNewEntropy();
            _apiKey = ProtectData(key, _keyEntropy);

            _secretEntropy = GetNewEntropy();
            _apiSecret = ProtectData(secret, _secretEntropy);
            

            _pwdEntropy = GetNewEntropy();
            _apiPasssword = ProtectData(password, _pwdEntropy);
        }

        /// <summary>
        /// Used for deserialization
        /// </summary>
        /// <param name="exchange"></param>
        /// <param name="key"></param>
        /// <param name="keyEntropy"></param>
        /// <param name="secret"></param>
        /// <param name="secretEntropy"></param>
        /// <param name="password"></param>
        /// <param name="passwordEntropy"></param>
        public ApiKeyData(
            SupportedExchanges exchange,
            byte[] key, 
            byte[] keyEntropy,
            byte[] secret, 
            byte[] secretEntropy,
            byte[] password, 
            byte[] passwordEntropy)
        {
            Exchange = exchange;

            _keyEntropy = keyEntropy;
            _apiKey = key;

            _secretEntropy = secretEntropy;
            _apiSecret = secret;


            _pwdEntropy = passwordEntropy;
            _apiPasssword = password;
        }

        private byte[] ProtectData(string data, byte[] entropy)
        {
            if (string.IsNullOrWhiteSpace(data)) { return new byte[0]; }

            var byteText = Encoding.UTF8.GetBytes(data);

            return ProtectedData.Protect(byteText, entropy, DataProtectionScope.CurrentUser);
        }

        private byte[] GetNewEntropy()
        {
            byte[] entropy = new byte[20];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(entropy);
            }

            return entropy;
        }

        public string GetRawApiKey()
        {
            return _apiKey.Length <= 0 ? "" : Encoding.Default.GetString(ProtectedData.Unprotect(_apiKey, _keyEntropy, DataProtectionScope.CurrentUser));
        }

        public string GetRawApiSecret()
        {
            return _apiSecret.Length <= 0 ? "" : Encoding.Default.GetString(ProtectedData.Unprotect(_apiSecret, _secretEntropy, DataProtectionScope.CurrentUser));
        }

        public string GetRawApiPassword()
        {
            return _apiPasssword.Length <= 0 ? "" : Encoding.Default.GetString(ProtectedData.Unprotect(_apiPasssword, _pwdEntropy, DataProtectionScope.CurrentUser));
        }

        public byte[] Serialize()
        {
            using (var m = new MemoryStream())
            {
                using (var writer = new BinaryWriter(m))
                {
                    writer.Write((int)Exchange);

                    writer.Write(_apiKey.Length);
                    writer.Write(_apiKey);

                    writer.Write(_keyEntropy.Length);
                    writer.Write(_keyEntropy);

                    writer.Write(_apiSecret.Length);
                    writer.Write(_apiSecret);

                    writer.Write(_secretEntropy.Length);
                    writer.Write(_secretEntropy);

                    writer.Write(_apiPasssword.Length);
                    writer.Write(_apiPasssword);

                    writer.Write(_pwdEntropy.Length);
                    writer.Write(_pwdEntropy);
                }
                return m.ToArray();
            }
        }

        public static ApiKeyData Desserialize(byte[] data)
        {
            ApiKeyData result;
            using (var m = new MemoryStream(data))
            {
                using (var reader = new BinaryReader(m))
                {
                    var exchange = reader.ReadInt32();

                    var apiLength = reader.ReadInt32();
                    var apikey = reader.ReadBytes(apiLength);

                    var keyEntropyLength = reader.ReadInt32();
                    var keyEntropy = reader.ReadBytes(keyEntropyLength);

                    var secretLength = reader.ReadInt32();
                    var secret = reader.ReadBytes(secretLength);

                    var secretEntropyLength = reader.ReadInt32();
                    var secretEntropy = reader.ReadBytes(secretEntropyLength);

                    var apiPwdLength = reader.ReadInt32();
                    var apiPwd = reader.ReadBytes(apiPwdLength);

                    var pwdEntropyLength = reader.ReadInt32();
                    var pwdEntropy = reader.ReadBytes(pwdEntropyLength);

                    result = new ApiKeyData((SupportedExchanges)exchange, apikey, keyEntropy, secret, secretEntropy, apiPwd, pwdEntropy);
                }
            }
            return result;
        }

        public void Dispose()
        {
            GC.Collect();
        }
    }
}