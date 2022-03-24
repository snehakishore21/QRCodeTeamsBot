using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXing;

namespace QRCodeTeamsBot.Bots
{
    public partial class DecodeQR 
    {
        public string Urlvalue;
        public string websiteLink;
        public bool isPaymentLink;
        public string merchantName;
        
        public void getIntentOfQrCode(Bitmap bitmapImage)
        {
            try
            {
                BarcodeReader barcodeReader = new BarcodeReader();
                Result result = barcodeReader.Decode((Bitmap)bitmapImage);
                if (result != null)
                {
                    this.Urlvalue = result.ToString();
                    parseUrlIntent(this.Urlvalue);
                }
            }
            catch
            {

            }
        }
        public void parseUrlIntent(string url)
        {
            try
            {
                this.isPaymentLink = url.Contains("upi://");
                if (this.isPaymentLink)
                {
                    int index = url.IndexOf("pa=") + 3;
                    int endIndex = url.IndexOf("@", index);
                    this.merchantName = url.Substring(index, endIndex);
                }
                this.websiteLink = url;
            }
            catch { }
        }
    }
}

