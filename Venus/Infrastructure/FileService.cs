using System;
using System.IO;
using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace Venus.Infrastructure
{
    public class FileService : IFileService
    {
        public FileService()
        {

        }

        public void WriteData(string filePath, byte[] data)
        {
            try
            {
                var entropy = GetEntropy();
                var fStream = new FileStream(filePath, FileMode.OpenOrCreate);
                
                EncryptDataToStream(
                    data, 
                    entropy, 
                    DataProtectionScope.LocalMachine, 
                    fStream);

                fStream.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
        }

        public byte[] ReadData(string filePath)
        {
            if (!File.Exists(filePath)) { return null; }

            var fStream = new FileStream(filePath, FileMode.Open);

            var arrBytes = DecryptDataFromStream(
                    GetEntropy(),
                    DataProtectionScope.LocalMachine,
                    fStream,
                    (int) fStream.Length);

            fStream.Close();

            return arrBytes;
        }

        private byte[] GetEntropy()
        {
            return Encoding.ASCII.GetBytes(GetMachineId());
        }

        private void EncryptDataToStream(
            byte[] buffer, 
            byte[] entropy, 
            DataProtectionScope scope, 
            Stream s)
        {
            if (buffer == null){throw new ArgumentNullException("buffer");}
            if (buffer.Length <= 0){throw new ArgumentException("Buffer");}
            if (entropy == null){throw new ArgumentNullException("Entropy");}
            if (entropy.Length <= 0){throw new ArgumentException("Entropy");}
            if (s == null){throw new ArgumentNullException("s");}

            var encryptedData = ProtectedData.Protect(buffer, entropy, scope);

            // Write the encrypted data to a stream.
            if (s.CanWrite)
            {
                s.Write(encryptedData, 0, encryptedData.Length);
            }
        }

        private byte[] DecryptDataFromStream(
            byte[] entropy, 
            DataProtectionScope scope, 
            Stream s, 
            int length)
        {
            if (s == null){throw new ArgumentNullException("s");}
            if (entropy == null){throw new ArgumentNullException("Entropy");}
            if (entropy.Length <= 0){throw new ArgumentException("Entropy");}
            
            byte[] inBuffer = new byte[length];
            byte[] outBuffer;

            // Read the encrypted data from a stream.
            if (s.CanRead)
            {
                s.Read(inBuffer, 0, length);

                outBuffer = ProtectedData.Unprotect(inBuffer, entropy, scope);
            }
            else
            {
                throw new IOException("Could not read the stream.");
            }

            // Return the length that was written to the stream. 
            return outBuffer;
        }

        private string GetMachineId()
        {
            try
            {
                var searcher = new ManagementObjectSearcher(
                    "root\\CIMV2", "SELECT * FROM Win32_Processor");

                string retval = "";

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    retval += queryObj["Architecture"];
                    retval += queryObj["Family"];
                    retval += queryObj["ProcessorId"];
                }

                return retval;
            }
            catch (ManagementException e)
            {
                // TODO: Handle error for UX

                throw;
            }
        }
    }
}