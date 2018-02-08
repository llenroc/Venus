using System.IO;
using Venus.Infrastructure;
using Venus.Infrastructure.Exchanges;

namespace Venus.Model
{
    public class Configuration
    {
        public double UpdateInterval { get; set; }
        public ApiKeyData BittrexApiKeyData { get; set; }
        public ApiKeyData GdaxApiKeyData { get; set; }
        public ApiKeyData BinanceApiKeyData { get; set; }

        public byte[] Serialize()
        {
            using (var m = new MemoryStream())
            {
                using (var writer = new BinaryWriter(m))
                {
                    writer.Write(UpdateInterval);

                    var serializeBittrexKey = BittrexApiKeyData.Serialize();
                    var serializeGdaxKey = GdaxApiKeyData.Serialize();
                    var serlalizeBinanceKey = BinanceApiKeyData.Serialize();

                    writer.Write(serializeBittrexKey.Length);
                    writer.Write(serializeBittrexKey);
                    writer.Write(serializeGdaxKey.Length);
                    writer.Write(serializeGdaxKey);
                    writer.Write(serlalizeBinanceKey.Length);
                    writer.Write(serlalizeBinanceKey);
                }
                return m.ToArray();
            }
        }

        public static Configuration Deserialize(byte[] data)
        {
            Configuration result;
            using (var m = new MemoryStream(data))
            {
                using (var reader = new BinaryReader(m))
                {
                    var updateinterval = reader.ReadDouble();

                    var bittrexkeylength = reader.ReadInt32();
                    var bittrexkeybytes = reader.ReadBytes(bittrexkeylength);

                    var gdaxkeylength = reader.ReadInt32();
                    var gdaxkeybytes = reader.ReadBytes(gdaxkeylength);

                    var binancekeylength = reader.ReadInt32();
                    var binancekeybytes = reader.ReadBytes(binancekeylength);

                    var bittrexkey = ApiKeyData.Desserialize(bittrexkeybytes);
                    var gdaxkey = ApiKeyData.Desserialize(gdaxkeybytes);
                    var binancekey = ApiKeyData.Desserialize(binancekeybytes);

                    result = new Configuration
                    {
                        UpdateInterval = updateinterval,
                        BittrexApiKeyData = bittrexkey,
                        GdaxApiKeyData = gdaxkey,
                        BinanceApiKeyData = binancekey
                    };
                }
            }
            return result;
        }
    }
}