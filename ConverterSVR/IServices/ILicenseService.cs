using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConverterSVR.IServices
{
    public interface ILicenseService
    {
        void GenerateLicense(LicenseInfo.Models.LicenseModel model);
    }
}
