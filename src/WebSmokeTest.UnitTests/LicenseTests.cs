
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SmokeTest.Classes;
using SmokeTest.Shared;

namespace SmokeTest.UnitTests
{
    [TestClass]
    public class LicenseTests
    {
        [TestMethod]
        public void ValidLicense_Successfully_Decrypts()
        {
            LicenseFactory licenseFactory = new LicenseFactory();
            DateTime expire = DateTime.UtcNow;
            ILicense lic = licenseFactory.LoadLicense(CreateLicense(1, "Jim Bob", expire, 10, 9, 8, 7, 6, 5));
            string license = licenseFactory.SaveLicense(lic);

            ILicense sut = licenseFactory.LoadLicense(license);

            Assert.AreEqual(2, sut.MaximumConfigurations);
            Assert.AreEqual(expire, sut.Expires);
        }

        [TestMethod]
        public void InvalidLicenseClass_ReturnsStringEmpty()
        {
            LicenseFactory licenseFactory = new LicenseFactory();
            ILicense lic = new Mocks.License();
            string license = licenseFactory.SaveLicense(lic);

            Assert.AreEqual(String.Empty, license);
            Assert.IsNull(lic as Classes.License);
            Assert.IsNotNull(lic as Mocks.License);
        }

        [Ignore]
        [TestMethod]
        public void CreateTestLicense()
        {
            string contents = CreateLicense(1, "Simon Carter", DateTime.Now.AddDays(100), 6, 10, 10000, 300, 5, 10);
            File.WriteAllText("C:\\ProgramData\\SmokeTest\\Data\\newlic.lic", contents);
        }

        private const string key = "vTL9YkYt7jZduVWOB/JiumshaubM6YzdVjsZfmN3hT8=";
        private static readonly byte[] Header = new byte[9] { 83, 109, 111, 107, 101, 84, 101, 115, 116 };

        private string CreateLicense(in byte version, in string name, in DateTime expires,
            in int maximumRunningTests, in int maximumConfigurations, in int maximumPageScans,
            in int maximumOpenEndpoints, in int maximumTestsToRun, in int maximumTestSchedules)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(ms))
                {
                    binaryWriter.Write(Header);
                    binaryWriter.Write(version);
                    binaryWriter.Write(name.Length);
                    binaryWriter.Write(Encoding.UTF8.GetBytes(name));
                    binaryWriter.Write(expires.Ticks);
                    binaryWriter.Write(maximumRunningTests);
                    binaryWriter.Write(maximumConfigurations);
                    binaryWriter.Write(maximumPageScans);
                    binaryWriter.Write(maximumOpenEndpoints);
                    binaryWriter.Write(maximumTestsToRun);
                    binaryWriter.Write(maximumTestSchedules);

                    ms.Position = 0;
                    byte[] licenseData = new byte[ms.Length];
                    int read = ms.Read(licenseData, 0, licenseData.Length);

                    return EncryptString(licenseData, Convert.FromBase64String(key));
                }
            }
        }

        private static string EncryptString(byte[] message, byte[] key)
        {
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            byte[] iv = aes.IV;
            using (MemoryStream memStream = new MemoryStream())
            {
                memStream.Write(iv, 0, iv.Length);

                using (CryptoStream cryptStream = new CryptoStream(memStream, aes.CreateEncryptor(key, aes.IV), CryptoStreamMode.Write))
                {
                    using (StreamWriter writer = new StreamWriter(cryptStream))
                    {
                        writer.Write(Convert.ToBase64String(message));
                    }
                }

                byte[] buf = memStream.ToArray();
                return Convert.ToBase64String(buf, 0, buf.Length);
            }
        }
    }
}
