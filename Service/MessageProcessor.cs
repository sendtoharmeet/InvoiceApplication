using Service.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

namespace Service
{
    public class MessageProcessor : IMessageProcessor
    {
        Dictionary<string, string> keyValidatorElements;
        Dictionary<string, string> keyProcessingElements;
        decimal GstDefaultAmount = 12M;

        public MessageProcessor()
        {
            keyValidatorElements = new Dictionary<string, string>();
            keyValidatorElements.Add("Vendor", "vendor");
            keyValidatorElements.Add("Description", "description");
            keyValidatorElements.Add("Date", "date");
            keyValidatorElements.Add("ExpenseDetail", "expense");
            keyValidatorElements.Add("CostCentre", "cost_centre");
            keyValidatorElements.Add("TotalIncludingTax", "total");
            keyValidatorElements.Add("PaymentMethod", "payment_method");

            keyProcessingElements = new Dictionary<string, string>();
            keyProcessingElements.Add("Vendor", "vendor");
            keyProcessingElements.Add("Description", "description");
            keyProcessingElements.Add("Date", "date");
            keyProcessingElements.Add("CostCentre", "cost_centre");
            keyProcessingElements.Add("TotalIncludingTax", "total");
            keyProcessingElements.Add("PaymentMethod", "payment_method");
        }

        public bool SetGstPrice(decimal gstAmount)
        {
            if (gstAmount < 0)
                return false;

            string appDataFolder = Directory.GetCurrentDirectory();
            string gstDataPath = "GstRate.xml";
            string fullGstDataPath = Path.Combine(appDataFolder, gstDataPath);
            using (XmlWriter writer = XmlWriter.Create(fullGstDataPath))
            {
                writer.WriteStartElement("Gst");
                writer.WriteElementString("Rate", gstAmount.ToString());
                writer.WriteEndElement();
                writer.Flush();
            }

            return true;
        }

        public decimal GetGstPrice()
        {
            try
            {
                string appDataFolder = Directory.GetCurrentDirectory();
                string gstDataPath = "GstRate.xml";
                string fullGstDataPath = Path.Combine(appDataFolder, gstDataPath);

                if (!File.Exists(fullGstDataPath))
                {
                    return GstDefaultAmount;
                }

                XmlDocument doc = new XmlDocument();
                doc.Load(fullGstDataPath);
                XmlNode node = doc.DocumentElement.SelectSingleNode("/Gst/Rate");
                string gstValue = node.InnerText;

                decimal dOut = 0M;
                var isValid = decimal.TryParse(gstValue, out dOut);

                return dOut;
            }
            catch (Exception ex)
            {
                return GstDefaultAmount;
            }
        }

        public Invoice ProcessMessage(string inputMessage)
        {
            var objInvoice = new Invoice();
            Expense expense = new Expense();

            foreach (var item in keyProcessingElements)
            {
                var beginingTagIndex = inputMessage.IndexOf(string.Format("<{0}>", item.Value)) + item.Value.Length + 2;
                var closingTagIndex = inputMessage.IndexOf(string.Format("</{0}>", item.Value));

                if (closingTagIndex == -1)
                    continue;

                var tagValue = inputMessage.Substring(beginingTagIndex, (closingTagIndex - beginingTagIndex));
                switch (item.Key)
                {
                    case "Vendor":
                        objInvoice.Vendor = tagValue;
                        break;
                    case "Description":
                        objInvoice.Description = tagValue;
                        break;
                    case "Date":
                        objInvoice.DateText = tagValue;
                        DateTime oDate = default(DateTime);
                        var isValid = DateTime.TryParse(tagValue, out oDate);
                        objInvoice.Date = isValid ? oDate : default(DateTime?);
                        break;
                    case "CostCentre":
                        expense.CostCentre = tagValue;
                        break;
                    case "TotalIncludingTax":
                        var total = tagValue;
                        decimal totalInc;
                        expense.TotalIncludingTax = decimal.TryParse(total, out totalInc) ? totalInc : 0;
                        expense.TotalIncludingTax = decimal.Round(expense.TotalIncludingTax, 2);
                        break;
                    case "PaymentMethod":
                        expense.PaymentMethod = tagValue;
                        break;
                }
            }

            objInvoice.ExpenseDetail = expense;

            if (string.IsNullOrWhiteSpace(objInvoice.ExpenseDetail.CostCentre))
            {
                objInvoice.ExpenseDetail.CostCentre = "UNKNOWN";
            }
            if (objInvoice.ExpenseDetail.TotalIncludingTax > 0)
            {
                decimal gstRate = GetGstPrice();
                objInvoice.ExpenseDetail.TotalExcludingGst = decimal.Round(100 / (100 + gstRate) * objInvoice.ExpenseDetail.TotalIncludingTax, 2);
                objInvoice.ExpenseDetail.GstTax = decimal.Round(objInvoice.ExpenseDetail.TotalIncludingTax - objInvoice.ExpenseDetail.TotalExcludingGst, 2);
            }

            return objInvoice;
        }

        public ValidateResponse ValidateMessage(string message)
        {
            if(string.IsNullOrWhiteSpace(message))
            {
                var list = new List<string>();
                list.Add("Empty message");
                return new ValidateResponse() { IsValid = false, ErrorList = list };
            }

            var objValidate = new ValidateResponse();
            objValidate.IsValid = true;
            foreach (var item in keyValidatorElements)
            {
                var beginingTag = Regex.Matches(message, string.Format("<{0}>", item.Value)).Count;
                var closingTag = Regex.Matches(message, string.Format("</{0}>", item.Value)).Count;
                if (beginingTag != closingTag)
                {
                    objValidate.ErrorList.Add(string.Format("Missing {0} tag", item.Value));
                    objValidate.IsValid = false;
                }
            }

            if (Regex.Matches(message, "<total>").Count == 0)
            {
                objValidate.ErrorList.Add("Missing Total tag");
                objValidate.IsValid = false;
            }

            return objValidate;
        }
    }
}
