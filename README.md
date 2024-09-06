# Sample AI Customer Service Application using Azure Communication Services (WhatsApp) and Azure OpenAI

This demo application integrates Azure Communication Services, Azure Event Grid, and Azure OpenAI to create a customer service chatbot for Contoso Electronics. The application processes customer messages received via platforms like WhatsApp, generates intelligent responses using Azure OpenAI, and sends replies back through Azure Communication Services Advanced Messaging.

## Features
This project provides the following features:
- **Webhook Integration**: Listens for incoming Event Grid events at `/webhook` to handle subscription validations and notifications.
- **AI-Powered Responses**: Uses Azure OpenAI to generate responses based on predefined prompts tailored for Contoso Electronics' customer service, specifically assisting with calculator errors.
- **Azure Communication Services**: Sends responses back to customers through Azure Communication Services Advanced Messaging on WhatsApp.

## Prerequisites
Before setting up the application, ensure you have the following:
- An Azure account with an active subscription. For details, see [Create an account for free](https://aka.ms/Mech-Azureaccount).
- [.NET SDK 7.0 or later](https://dotnet.microsoft.com/download).
- Azure Communication Services resource. For details, see [Create and manage Communication Services resources](https://learn.microsoft.com/en-us/azure/communication-services/quickstarts/create-communication-resource).
- WhatsApp Channel under Azure Communication Services resource. For details, see [Register WhatsApp business account](https://learn.microsoft.com/en-us/azure/communication-services/quickstarts/advanced-messaging/whatsapp/connect-whatsapp-business-account).
- Azure OpenAI resource. For details, see [Create and deploy an Azure OpenAI Service resource](https://learn.microsoft.com/en-us/azure/ai-services/openai/how-to/create-resource).

## Setup
1. **Clone the Repository**:
   ```bash
   git clone https://github.com/MilanKaur-01/RAG-with-WhatsApp.git
   ```

2. **Configure Settings**:

   Update the application settings with your Azure service credentials:
   - Obtain the Connection String and Channel Registration ID from your Azure Communication Services resource via the Azure Portal.
   - Obtain the Endpoint, API Key, and Deployment Name for your Azure OpenAI resource from the Azure Portal.
  
    Add the above information to the appsettings.json file located in the CpmDemoApp folder:  
    ```Json
    {
        "NotificationMessagesClientOptions": {
            "ConnectionString": "your-acs-connection-string",
            "ChannelRegistrationId": "your-channel-registration-id"
        },
        "OpenAIClientOptions": {
            "Endpoint": "https://your-openai-endpoint",
            "Key": "your-openai-api-key",
            "DeploymentName": "your-deployment-name"
        }
    }
    ```

3. **Deploy the sample app:**

    To deploy the web app, follow the quickstart guide: [Quickstart: Publish an ASP.NET web app](https://learn.microsoft.com/en-us/visualstudio/deployment/quickstart-deploy-aspnet-web-app?view=vs-2022&tabs=azure).

4. **Set Up Event Grid Subscription:**

   To receive and handle incoming messages, set up an Event Grid subscription:
    - Navigate to the Events tab of your Azure Communication Services resource in the Azure Portal.
    - Create a new Event Subscription for the `advancedmessagereceived` event type.
    - Choose **Web Hook**  as the endpoint type.
    - Use your deployed web app URL appended with `/webhook` as the subscriber endpoint (e.g., https://yourapp.azurewebsites.net/webhook).
    - Complete the setup and create the subscription. Once the subscription is active, the application will start receiving and responding to WhatsApp messages.
   
## Usage

- **Receive Messages**: The application receives customer messages via WhatsApp and processes them through Event Grid.
- **Generate AI Responses**: Azure OpenAI generates responses based on a predefined prompt, which guides the AI to assist with specific calculator error codes.
- **Send Responses**: Responses are sent back to the customer using Azure Communication Services, providing real-time support through WhatsApp.

## Troubleshooting
- **No AI Response**: If no response is generated from Azure OpenAI, ensure that your API keys and endpoint configurations are correct.
- **Message Delivery Issues**: If responses are not reaching customers, verify the configuration of your Azure Communication Services resource and the Event Grid subscription.
- **Not Receiving Messages from WhatsApp Customers**: Check that the Event Grid subscription is set up correctly with the appropriate event types and endpoint configuration.
