# durableFunctionSample
This sample uses a example to explain Function Chaining pattern

The samples here include three different activities.

Accept Order activity receives an Order and responds with Order Number to the input Order.
ProcessOrder activity will be triggered as second activity which processes the previous received Order.
SendOrderConfirmation activity sends an order confirmation email using SendGrid API for previous processed order.

Configuration for storage account and Send Grid API key, SendGridFromAddress, SendGridSubject has to be updated in local.settings.json file after downloading the solution.

They are currently not included in the solution.

Use the azure function request to post to the http function. You need to use an valid email address to receive the email from Azure Function using SendGrid.

Below is the content to go inside local.settings.json { "IsEncrypted": false, "Values": { "AzureWebJobsStorage": "", "AzureWebJobsDashboard": "", "SendGrid": "", "SendGridFromAddress": "", "SendGridSubject" : "" } }
