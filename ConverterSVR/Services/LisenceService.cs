using ConverterSVR.IServices;
using LicenseInfo.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConverterSVR.Services
{
    public class LisenceService : ILicenseService
    {
        class LicenseViewModel : CMSWebModule
        {
            public string ModuleText { get; set; }
        }
        public void GenerateLicense(LicenseModel _Licencense)
        {
            string xmldata = LicenseInfo.LicenseInfo.Instance.ModelToXmlString(_Licencense);
            if (string.IsNullOrEmpty(xmldata))
            {
                return;
            }
            byte[] buff = Cryptography.Rijndael.DefaultEncryptStringToBytes(xmldata);
            SaveBuffer2File(buff, Path.Combine(AppSettings.AppSettings.Instance.AppData, AppSettings.AppSettings.License_Info_File));
        }

        private bool SaveBuffer2File(byte[] buff, string path)
        {
            if (buff == null || string.IsNullOrEmpty(path))
                return false;
            try
            {
                using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Create)))
                {
                    writer.Write(buff);
                }
                return true;
            }
            catch (Exception ex)
            {

                return false;
            }
        }
    }
}
