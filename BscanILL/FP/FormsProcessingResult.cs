#define TransNumber_LONG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BscanILL.FP
{
	public class FormsProcessingResult
	{
		public string IllNumber;

#if TransNumber_LONG
        public long? TN;
#else
		public int? TN ;
#endif        
		public string Address ;
		public string PatronName ;
		public BscanILL.Export.ExportType DeliveryMethod;
		public bool AddressFlag;

		public FormsProcessingResult(string illNumber, string tn, string address, string patronName, string deliveryMethod, bool addressFlg)
		{
            this.IllNumber = illNumber;

#if TransNumber_LONG
            long temp;
            this.TN = (long.TryParse(tn, out temp)) ? (long?)temp : null;
#else
            int temp;
            this.TN = (int.TryParse(tn, out temp)) ? (int?)temp : null;
#endif
			this.Address = address;
			this.PatronName = patronName;
			this.AddressFlag = addressFlg;

			deliveryMethod = deliveryMethod.ToLower();

            if (deliveryMethod.Contains("ftp directory"))
                this.DeliveryMethod = Export.ExportType.FtpDir;
			else if (deliveryMethod.Contains("ftp"))
				this.DeliveryMethod = Export.ExportType.Ftp;
			else if (deliveryMethod.Contains("email") || deliveryMethod.Contains("e-mail"))
				this.DeliveryMethod = Export.ExportType.Email;
			else if (deliveryMethod.Contains("ariel"))
				this.DeliveryMethod = Export.ExportType.Ariel;
			else if (deliveryMethod.Contains("illiad"))
				this.DeliveryMethod = Export.ExportType.ILLiad;
			else if (deliveryMethod.Contains("odyssey"))
				this.DeliveryMethod = Export.ExportType.Odyssey;
			else if (deliveryMethod.Contains("exchange"))
				this.DeliveryMethod = Export.ExportType.ArticleExchange;
            else if (deliveryMethod.Contains("tipasa"))
                this.DeliveryMethod = Export.ExportType.Tipasa;
            else if (deliveryMethod.Contains("worldshare"))
                this.DeliveryMethod = Export.ExportType.WorldShareILL;
			else if (deliveryMethod.Contains("rapido"))
				this.DeliveryMethod = Export.ExportType.Rapido;
			else 
				this.DeliveryMethod = Export.ExportType.SaveOnDisk;
		}

#if TransNumber_LONG
        public FormsProcessingResult(string illNumber, long? tn, string address, string patronName, BscanILL.Export.ExportType deliveryMethod, bool addressFlg)
#else
		public FormsProcessingResult(string illNumber, int? tn, string address, string patronName, BscanILL.Export.ExportType deliveryMethod, bool addressFlg)
#endif        
		{
			this.IllNumber = illNumber;
			this.TN = tn;
			this.Address = address;
			this.PatronName = patronName;
			this.DeliveryMethod = deliveryMethod;
			this.AddressFlag = addressFlg;
		}

		public override string ToString()
		{
			string message = "ILL Number: " +this.IllNumber + Environment.NewLine;
			message += "TN: " + this.TN + Environment.NewLine;
			message += "Address: " + this.Address + Environment.NewLine;
			message += "Patron: " + this.PatronName + Environment.NewLine;
			message += "Delivery: " + this.DeliveryMethod.ToString() + Environment.NewLine;
			message += "Flag: " + this.AddressFlag.ToString() + Environment.NewLine;

			return message;
		}


	}
}
