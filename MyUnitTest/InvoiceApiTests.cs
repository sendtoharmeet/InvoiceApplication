using InvoiceApplication.Controllers;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using Service;
using Service.Model;
using System;

namespace Tests
{
    public class InvoiceApiTests
    {
        IMessageProcessor messageProcessor;

        [SetUp]
        public void Setup()
        {
            messageProcessor = new MessageProcessor();
        }
        
        [Test]
        public void ProcessEmbeddedXmlIncorrectDateTest()
        {
            var controller = new InvoiceController(messageProcessor);
            string inputMessage = @"Hi Yvaine, Please create an expense claim for the below. Relevant details are marked up as requested…  <expense><cost_centre>DEV002</cost_centre><total>112.0</total><payment_method>personal card</payment_method> </expense>From: Ivan Castle Sent: Friday, 16 February 2018 10:32 AM To: Antoine Lloyd <Antoine.Lloyd@example.com>Subject: test Hi Antoine, Please create a reservation at the <vendor>Viaduct Steakhouse</vendor> our <description>development team's project end celebration dinner</description> on <date>Tuesday 27 April 2017</date>. We expect to arrive around 7.15pm. Approximately 12 people but I’ll confirm exact numbers closer to the day. Regards, Ivan";

            IActionResult actionResult = controller.SetGstPrice(12M);
            var gstObject = actionResult as ObjectResult;

            actionResult = controller.ProcessMessage(inputMessage);
            var returnValue = actionResult as ObjectResult;
            var response = returnValue.Value as Invoice;
            
            Assert.AreEqual(response.ExpenseDetail.GstTax, 12M);
            Assert.AreEqual(response.ExpenseDetail.TotalExcludingGst.ToString(), "100.00");
            Assert.AreEqual(response.ExpenseDetail.TotalIncludingTax.ToString(), "112.0");
            Assert.AreEqual(response.ExpenseDetail.PaymentMethod, "personal card");
            Assert.AreEqual(response.ExpenseDetail.CostCentre, "DEV002");
            Assert.AreEqual(response.Description, "development team's project end celebration dinner");
            Assert.AreEqual(response.DateText, "Tuesday 27 April 2017");
            Assert.AreEqual(response.Date, default(DateTime?));
            Assert.AreEqual(response.Vendor, "Viaduct Steakhouse");
        }

        [Test]
        public void ProcessEmbeddedXmlWithCorrectDateTest()
        {
            var controller = new InvoiceController(messageProcessor);
            string inputMessage = @"Hi Yvaine, Please create an expense claim for the below. Relevant details are marked up as requested…  <expense><cost_centre>DEV002</cost_centre><total>112.0</total><payment_method>personal card</payment_method> </expense>From: Ivan Castle Sent: Friday, 16 February 2018 10:32 AM To: Antoine Lloyd <Antoine.Lloyd@example.com>Subject: test Hi Antoine, Please create a reservation at the <vendor>Viaduct Steakhouse</vendor> our <description>development team's project end celebration dinner</description> on <date>Wednesday 31 July 2019</date>. We expect to arrive around 7.15pm. Approximately 12 people but I’ll confirm exact numbers closer to the day. Regards, Ivan";

            IActionResult actionResult = controller.SetGstPrice(12M);
            var gstObject = actionResult as ObjectResult;

            actionResult = controller.ProcessMessage(inputMessage);
            var returnValue = actionResult as ObjectResult;
            var response = returnValue.Value as Invoice;

            Assert.AreEqual(response.ExpenseDetail.GstTax, 12M);
            Assert.AreEqual(response.ExpenseDetail.TotalExcludingGst.ToString(), "100.00");
            Assert.AreEqual(response.ExpenseDetail.TotalIncludingTax.ToString(), "112.0");
            Assert.AreEqual(response.ExpenseDetail.PaymentMethod, "personal card");
            Assert.AreEqual(response.ExpenseDetail.CostCentre, "DEV002");
            Assert.AreEqual(response.Description, "development team's project end celebration dinner");
            Assert.AreEqual(response.DateText, "Wednesday 31 July 2019");
            Assert.AreEqual(response.Date, new DateTime(2019, 07, 31));
            Assert.AreEqual(response.Vendor, "Viaduct Steakhouse");
        }

        [Test]
        public void ProcessEmbeddedXmlMissingCostCentreTest()
        {
            var controller = new InvoiceController(messageProcessor);
            string inputMessage = @"Hi Yvaine, Please create an expense claim for the below. Relevant details are marked up as requested…  <expense><total>112.0</total><payment_method>personal card</payment_method> </expense>From: Ivan Castle Sent: Friday, 16 February 2018 10:32 AM To: Antoine Lloyd <Antoine.Lloyd@example.com>Subject: test Hi Antoine, Please create a reservation at the <vendor>Viaduct Steakhouse</vendor> our <description>development team's project end celebration dinner</description> on <date>Tuesday 27 April 2017</date>. We expect to arrive around 7.15pm. Approximately 12 people but I’ll confirm exact numbers closer to the day. Regards, Ivan";

            IActionResult actionResult = controller.SetGstPrice(12M);
            var gstObject = actionResult as ObjectResult;

            actionResult = controller.ProcessMessage(inputMessage);
            var returnValue = actionResult as ObjectResult;
            var response = returnValue.Value as Invoice;
            
            Assert.AreEqual(response.ExpenseDetail.GstTax.ToString(), "12.00");
            Assert.AreEqual(response.ExpenseDetail.TotalExcludingGst.ToString(), "100.00");
            Assert.AreEqual(response.ExpenseDetail.TotalIncludingTax.ToString(), "112.0");
            Assert.AreEqual(response.ExpenseDetail.PaymentMethod, "personal card");
            Assert.AreEqual(response.ExpenseDetail.CostCentre, "UNKNOWN");
            Assert.AreEqual(response.Description, "development team's project end celebration dinner");
            Assert.AreEqual(response.DateText, "Tuesday 27 April 2017");
            Assert.AreEqual(response.Date, default(DateTime?));
            Assert.AreEqual(response.Vendor, "Viaduct Steakhouse");
        }

        [Test]
        public void ProcessEmbeddedXmlMissingTotalAmountTest()
        {
            var controller = new InvoiceController(messageProcessor);
            string inputMessage = @"Hi Yvaine, Please create an expense claim for the below. Relevant details are marked up as requested…  <expense><payment_method>personal card</payment_method> </expense>From: Ivan Castle Sent: Friday, 16 February 2018 10:32 AM To: Antoine Lloyd <Antoine.Lloyd@example.com>Subject: test Hi Antoine, Please create a reservation at the <vendor>Viaduct Steakhouse</vendor> our <description>development team's project end celebration dinner on <date>Tuesday 27 April 2017</date>. We expect to arrive around 7.15pm. Approximately 12 people but I’ll confirm exact numbers closer to the day. Regards, Ivan";

            IActionResult actionResult = controller.ProcessMessage(inputMessage);
            var response = actionResult as ObjectResult;
            var validateResponse = response.Value as ValidateResponse;
            Assert.AreEqual(response.StatusCode, 400);
            Assert.True(validateResponse.ErrorList.Contains("Missing description tag"));
            Assert.True(validateResponse.ErrorList.Contains("Missing Total tag"));
        }

        [Test]
        public void ProcessEmbeddedXmlMissingClosingTagTest()
        {
            var controller = new InvoiceController(messageProcessor);
            string inputMessage = @"Hi Yvaine, Please create an expense claim for the below. Relevant details are marked up as requested…  <expense><total>112.0</total><payment_method>personal card</payment_method> </expense>From: Ivan Castle Sent: Friday, 16 February 2018 10:32 AM To: Antoine Lloyd <Antoine.Lloyd@example.com>Subject: test Hi Antoine, Please create a reservation at the <vendor>Viaduct Steakhouse</vendor> our <description>development team's project end celebration dinner on <date>Tuesday 27 April 2017</date>. We expect to arrive around 7.15pm. Approximately 12 people but I’ll confirm exact numbers closer to the day. Regards, Ivan";

            IActionResult actionResult = controller.ProcessMessage(inputMessage);
            var response = actionResult as ObjectResult;

            var validateResponse = response.Value as ValidateResponse;
            Assert.AreEqual(response.StatusCode, 400);
            Assert.True(validateResponse.ErrorList.Contains("Missing description tag"));
        }

        [Test]
        public void ProcessEmbeddedXmlMissingMessageTest()
        {
            var controller = new InvoiceController(messageProcessor);
            string inputMessage = @"";

            IActionResult actionResult = controller.ProcessMessage(inputMessage);
            var response = actionResult as ObjectResult;

            var validateResponse = response.Value as ValidateResponse;

            Assert.AreEqual(response.StatusCode, 400);
            Assert.True(validateResponse.ErrorList.Contains("Empty message"));
        }

        [Test]
        public void UpdateGstPriceTest()
        {
            var controller = new InvoiceController(messageProcessor);
            decimal setGstPrice = 14.5M;

            IActionResult actionResult = controller.SetGstPrice(setGstPrice);
            var responseUpdate = actionResult as ObjectResult;

            actionResult = controller.GetGstPrice();
            var responseSelect = actionResult as ObjectResult;

            Assert.AreEqual(responseUpdate.Value, true);
            Assert.AreEqual(responseSelect.Value, setGstPrice);
        }
    }
}