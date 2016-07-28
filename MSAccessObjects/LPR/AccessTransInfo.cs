using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using System.Drawing;

namespace MSAccessObjects.LPR
{
	[Serializable]
	[XmlRoot(ConstEnums.Info)]
	public class AccessTransInfo : AccessTransBase
	{
		[XmlElement(ConstEnums.LPR_ID)]
		public int LPR_ID { get; set;}

		[XmlElement(ConstEnums.LPR_CamNo)]
		public int LPR_CamNo { get; set;}


        string _DVRTime;

        [XmlElement(ConstEnums.DVRTime)]
        public string DVRTime
        {
            get { return _DVRTime; }
            set { _DVRTime = value; }

        }

        string _DVRDate;
		[XmlElement(ConstEnums.DVRDate)]
        public string DVRDate { get {
            if (string.IsNullOrEmpty(_DVRTime)) return _DVRDate;
            return _DVRDate + " " + _DVRTime.Replace(":000",""); 
        } set{ _DVRDate = value;} }

		[XmlElement(ConstEnums.LPR_NUM)]
		public string LPR_NUM { get; set;}



        string _LPR_JPEG;
		[XmlElement(ConstEnums.LPR_JPEG_FILE_NAME)]
		public string LPR_JPEG_FILE_NAME
        { get { return ConvertToBase64String(_LPR_JPEG); } set { _LPR_JPEG = value; } }

		[XmlElement(ConstEnums.LPR_Possibility)]
		public string LPR_Possibility { get; set;}

		[XmlElement(ConstEnums.LPR_PACID)]
		public string LPR_PACID { get; set;}

		[XmlElement(ConstEnums.LPR_isMatch)]
		public string LPR_isMatch { get; set;}


        string ConvertToBase64String(string filepath)
        {
            if (string.IsNullOrEmpty(filepath)) return "";
            if (File.Exists(filepath))
            {
                using (Image image = Image.FromFile(filepath))
                {
                    using (MemoryStream m = new MemoryStream())
                    {
                        image.Save(m, image.RawFormat);
                        byte[] imageBytes = m.ToArray();
                        string base64String = Convert.ToBase64String(imageBytes);
                        return base64String;
                    }
                }
            }
            else return "";
        }
	}
}
