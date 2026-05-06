
using Microsoft.AI.Foundry.Local;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;



class Program
{
    static async Task Main(string[] args) {

        var service= new FoundryLocalService();
        await service.InitializeAsync();
        var modelId =  await service.SelectandDownloadModel();
        
        ApiKeyCredential key = new ApiKeyCredential("not-needed-for-local");

        OpenAIClient client = new OpenAIClient(key, new OpenAIClientOptions {
                              Endpoint = new Uri(service.Endpoint + "/v1")
        });
        var options = new ChatCompletionOptions
        {
            MaxOutputTokenCount = 5000
        };

        var prompt = "What is artificial intelligence?";
        ChatClient chatClient = client.GetChatClient(modelId);
        Console.WriteLine("Request sent to Foundry Local\n ");
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("You are a helpful assistant."),
            new UserChatMessage(prompt),
        };
        CollectionResult<StreamingChatCompletionUpdate> completionUpdates = chatClient.CompleteChatStreaming(messages, options);

        Console.WriteLine($"[ASSISTANT]: {prompt}");
        Console.WriteLine("\n------------------------------------------------------------");
        foreach (StreamingChatCompletionUpdate completionUpdate in completionUpdates)
        {
            if (completionUpdate.ContentUpdate.Count > 0)
            {
                Console.Write(completionUpdate.ContentUpdate[0].Text);
            }
        }
        Console.WriteLine("\n------------------------------------------------------------\n");
        Console.WriteLine("Please hit enter to exit the program.");
        Console.ReadLine();
        await service.ShutdownAsync();
        Environment.Exit(0);

    }
}
   