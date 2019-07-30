using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Service;
using System;

namespace InvoiceApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        private readonly ILogger<InvoiceController> logger;
        IMessageProcessor messageProcessor;

        public InvoiceController(IMessageProcessor _messageProcessor)
        {
            messageProcessor = _messageProcessor;
        }

        /// <summary>
        /// Just for dev purpose, ping end point, to check whether API is available. 
        /// </summary>
        /// <returns></returns>
        [HttpGet("get")]
        public IActionResult Get()
        {
            return Ok("Ok");
        }
        
        /// <summary>
        /// To make changeable gst amount, I am saving gst into a Xml file. Better approach is to use database to set master tables
        /// </summary>
        /// <param name="gstPrice"></param>
        /// <returns></returns>
        [HttpPost("setgst")]
        public IActionResult SetGstPrice(decimal gstPrice)
        {
            try
            {
                if (gstPrice < 0)
                {
                    return BadRequest("Amount should be greater than 0");
                }

                var returnValue = messageProcessor.SetGstPrice(gstPrice);
                if (!returnValue)
                {
                    return BadRequest("Cannot process this request");
                }
                else
                {
                    return Ok(true);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception on SetGstPrice");
                return BadRequest(ex.Message);
            }
        }
        
        /// <summary>
        /// To test Gst value saved in Xml file
        /// </summary>
        /// <returns></returns>
        [HttpGet("getgst")]
        public IActionResult GetGstPrice()
        {
            try
            {
                var amount = messageProcessor.GetGstPrice();
                return Ok(amount);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception on GetGstPrice");
                return BadRequest(ex.Message);
            }
        }
        
        /// <summary>
        /// This is a main Processor endpoint, any validation error will return error collection, successed message will return parsed and tax calculated object
        /// </summary>
        /// <param name="inputMessage"></param>
        /// <returns></returns>
        [HttpPost("processmessage")]
        public IActionResult ProcessMessage(string inputMessage)
        {
            try
            {
                var validMessage = messageProcessor.ValidateMessage(inputMessage);
                if (!validMessage.IsValid)
                {
                    return BadRequest(validMessage);
                }
                else
                {
                    var objProcessed = messageProcessor.ProcessMessage(inputMessage);
                    return Ok(objProcessed);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception on ProcessMessge");
                return BadRequest(ex.Message);
            }
        }
    }
}
