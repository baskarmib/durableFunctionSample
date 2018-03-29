# durableFunctionSample
This sample uses a example to explain Function Chaining pattern

The samples here include three different activities.

Accept Order activity receives an Order and responds with Order Number to the input Order.
ProcessOrder activity will be triggered as second activity which processes the previous received Order.
SendOrderConfirmation activity sends an order confirmation email using SendGrid API for previous processed order.

Configuration for storage account and Send Grid API key, SendGridFromAddress, SendGridSubject has to be updated in local.settings.json file after downloading the solution.

They are currently not included in the solution.

Use the azure function request to post to the orchestration url. You need to use an valid email address to receive the email from Azure Function using SendGrid.

Below is the content to go inside local.settings.json { "IsEncrypted": false, "Values": { "AzureWebJobsStorage": "", "AzureWebJobsDashboard": "", "SendGrid": "", "SendGridFromAddress": "", "SendGridSubject" : "" } }

Below is the Orchestration Url which needs to be used in PostMan to test the code.
Http Method - Post - 
Url - Replace the {PortNumber} with port number displayed Azure Function Debug Console.
http://localhost:{PortNumber}/api/orchestrators/OrderProcessingSequence

If everything is running the response should be something as below in json format.

{
    "id": "ccc8a24a9a3b4d1c8ba2797ccf7f2f80",
    "statusQueryGetUri": "http://localhost:{PortNumber}/admin/extensions/DurableTaskExtension/instances/ccc8a24a9a3b4d1c8ba2797ccf7f2f80?taskHub=DurableFunctionsHub&connection=Storage&code=nlpquere",
    "sendEventPostUri": "http://localhost:{PortNumber}/admin/extensions/DurableTaskExtension/instances/ccc8a24a9a3b4d1c8ba2797ccf7f2f80/raiseEvent/{eventName}?taskHub=DurableFunctionsHub&connection=Storage&code=nlssJY",
    "terminatePostUri": "http://localhost:{PortNumber}/admin/extensions/DurableTaskExtension/instances/ccc8a24a9a3b4d1c8ba2797ccf7f2f80/terminate?reason={text}&taskHub=DurableFunctionsHub&connection=Storage&code=adsJY"
}
