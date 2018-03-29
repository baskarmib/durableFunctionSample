using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using System;
using System.Linq;
using Microsoft.Azure.WebJobs.Host;
using System.Text;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace DurableSample
{
    public static class OrderProcessing
    {
        [FunctionName("OrderProcessingSequence")]
        public static async Task<Order> Run([OrchestrationTrigger] DurableOrchestrationContext context, TraceWriter log)
        {

            string inputOrder = context.GetRawInput();
            log.Info(inputOrder);
            Order orderDetails = JsonConvert.DeserializeObject<Order>(inputOrder);
            //Assigns an Order Number
            orderDetails = await context.CallActivityAsync<Order>(
                "AcceptOrder",
                orderDetails);

            //Process Order
            orderDetails = await context.CallActivityAsync<Order>(
               "ProcessOrder",
               orderDetails);


            //SendOrderConfirmationEmail
            await context.CallActivityAsync<string>(
               "SendOrderConfirmation",
               orderDetails);

            return orderDetails;            
        }

        [FunctionName("AcceptOrder")]
        public static Order AcceptOrder([ActivityTrigger] Order orderDetails)
        {
            Order orderinput = orderDetails;
            if (orderinput == null || orderinput.ItemsList == null)
            {
                throw new ArgumentNullException(
                    nameof(orderinput.ItemsList),
                    "Order Items are not provided");
            }

            orderinput.OrderNumber = System.Guid.NewGuid().ToString();
            orderinput.OrderStatus = "New";
            return orderinput;
        }

        [FunctionName("ProcessOrder")]
        public static Order ProcessOrder([ActivityTrigger] Order orderDetails)
        {

            if(orderDetails == null)
            {
                throw new ArgumentNullException(
                   nameof(orderDetails),
                   "Order Items are not provided");
            }
            
            //Apply some business logic to check Stock.
            int productOnStock = 100;
            decimal OrderTotal = 0;
            foreach (var order in orderDetails.ItemsList)
            {
                if (!string.IsNullOrWhiteSpace(order.Qty) && Convert.ToInt32(order.Qty) > 0)
                {
                    int orderQuantity = Convert.ToInt32(order.Qty);
                    if (orderQuantity > productOnStock)
                    {
                        order.LineStatus = "NotFulfilled";
                    }
                    else
                    {
                        order.LineStatus = "Fulfilled";
                        OrderTotal = OrderTotal + ((orderQuantity) * (order.Price));
                    }
                }
                else
                {
                    order.LineStatus = "NotFulfilled";
                }
            }

            orderDetails.OrderTotal = OrderTotal;
            var checkstatus = orderDetails.ItemsList.Where(x => x.LineStatus.Equals("NotFulfilled"));
            if (checkstatus.Count() > 0)
            {
                orderDetails.OrderStatus = "NotFulfilled";
            }
            else
            {
                orderDetails.OrderStatus = "Completed";
            }
            return orderDetails;
        }

        [FunctionName("SendOrderConfirmation")]
        public static string SendOrderConfirmation([ActivityTrigger] Order orderDetails, TraceWriter log)
        {
            
            StringBuilder htmlContent = new StringBuilder();
            if (string.IsNullOrWhiteSpace(orderDetails.EmailAddress))
            {
                log.Info($"C# Queue Timer trigger function Email Address Not Found for Order: {orderDetails.OrderNumber}");
                throw new ArgumentNullException(
                  nameof(orderDetails.EmailAddress),
                  "EmailAddress Is Not Provided");
            }
            if (orderDetails.OrderStatus.Equals("Completed"))
            {

                htmlContent.Append(string.Format("<p>Your Order with Order Number - {0} has been received and Shipped</p></br>", orderDetails.OrderNumber));

                htmlContent.Append("<table>");
                htmlContent.Append("<tr>");
                htmlContent.Append(string.Format("<th>{0}</th><th>{1}</th><th>{2}</th><th>{3}</th><th>{4}</th></tr>", "ItemId", "Name", "Qty", "Price", "Status"));
                foreach (var itemdetails in orderDetails.ItemsList)
                {
                    htmlContent.Append("<tr>");
                    htmlContent.Append(string.Format("<th>{0}</th><th>{1}</th><th>{2}</th><th>{3}</th><th>{4}</th></tr>", itemdetails.ItemId.ToString(), itemdetails.ItemName
                        , itemdetails.Qty, itemdetails.Price, itemdetails.LineStatus));
                }
                htmlContent.Append("</table>");
                SendOrderConfirmationEmail(orderDetails.EmailAddress, htmlContent.ToString()).Wait();
            }
            else
            {
                string htmlMessage = string.Format("<p>Please contact customer care for your order number-{0}</p>", orderDetails.OrderNumber);
                SendOrderConfirmationEmail(orderDetails.EmailAddress, htmlMessage).Wait();
            }
            return "Email Processing Completed";
        }

        static async Task SendOrderConfirmationEmail(string toEmailAddress, string htmlContent)
        {
            var apiKey = System.Configuration.ConfigurationManager.AppSettings["SendGrid"].ToString();
            var fromaddress = System.Configuration.ConfigurationManager.AppSettings["SendGridFromAddress"].ToString();
            var subject = System.Configuration.ConfigurationManager.AppSettings["SendGridSubject"].ToString();
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(fromaddress); //Update the from address before publishing to azure.
            var to = new EmailAddress(toEmailAddress);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, string.Empty, htmlContent);
            var response = await client.SendEmailAsync(msg);
        }

    }
}
