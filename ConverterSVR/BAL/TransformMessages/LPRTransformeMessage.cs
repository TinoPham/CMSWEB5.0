using ConvertMessage;
using ConvertMessage.PACDMObjects.LPR;
using PACDMModel.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConverterSVR.BAL.TransformMessages
{
    internal class LPRTransformeMessage : Commons.SingletonClassBase<LPRTransformeMessage>, ITransformMessage<Info, tbl_LPR_Info>
    {
        const string LPR_NAME_IMAGE_FORMAT = "yyyyMMddHHmmss";
        const string prefix = "x";
        const string imageExtension = ".jpg";
        public tbl_LPR_Info TransForm(Info input, MessageDVRInfo DVRInfo)
        {

            tbl_LPR_Info lpr = new tbl_LPR_Info();
            string imagePath = "";
            if (!string.IsNullOrEmpty(input.LPR_ImageBase64)) 
            {
                string LPR_path = System.IO.Path.Combine(AppSettings.AppSettings.Instance.DvrPath, "LPR", DVRInfo.KDVR.ToString());
                if (!System.IO.Directory.Exists(LPR_path))
                {
                    System.IO.Directory.CreateDirectory(LPR_path);
                }
                string filename = prefix + input.DVRDate.Value.ToString(LPR_NAME_IMAGE_FORMAT) + "_" + System.Guid.NewGuid() + imageExtension;
                imagePath = System.IO.Path.Combine(LPR_path, filename);

                using (Image image = Image.FromStream(new MemoryStream(ConvertBase64ToBytes(input.LPR_ImageBase64))))
                {
                    image.Save(imagePath, ImageFormat.Jpeg);  
                }
            lpr.LPR_ImageName = filename;
            }
            lpr.CamNo = input.CamNo;
            lpr.DVRDate = input.DVRDate;
            lpr.LPR_ID = input.LPR_ID;
            //lpr.LPR_Image = ConvertBase64ToBytes(input.LPR_ImageBase64);
            lpr.LPR_isMatch = input.LPR_isMatch;
            lpr.LPR_Num = input.LPR_Num;
            lpr.LPR_PACID = DVRInfo.KDVR;
            lpr.LPR_Possibility = input.LPR_Possibility;
            return lpr; 
        
        }
        private byte[] ConvertBase64ToBytes(string base64String) {

            if (string.IsNullOrEmpty(base64String)) return null;
            try
            {
                byte[] image = Convert.FromBase64String(base64String);
                return image;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
