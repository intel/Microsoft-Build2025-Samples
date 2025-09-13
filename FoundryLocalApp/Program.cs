
using Microsoft.AI.Foundry.Local;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

class Program
{
    static async Task Main(string[] args) { 

        var manager = new FoundryLocalManager();
        await manager.StartServiceAsync(CancellationToken.None);
        if (!manager.IsServiceRunning)
        {
            Console.WriteLine($"Foundry Service failed to start. Exiting!\n ");
            Environment.Exit(0);
        }
        else
        {
            Console.WriteLine($"Foundry Service running at {manager.Endpoint}\n ");
        }
        List<ModelInfo> models = await manager.ListCatalogModelsAsync();
        Console.WriteLine($"List of supported models in Foundry Local:\n");
        for (int i = 0; i < models.Count; i++)
        {
            Console.WriteLine($"{models[i].ModelId}");
        }

        Console.WriteLine("\nPlease copy the model name from the above list which you would like to try out :");
        var modelId = "";
        while (modelId.Equals("")) {
            modelId = Console.ReadLine()?.Trim();
        }
        var modelLoaded = await isModelLoaded(modelId, manager);
        if(!modelLoaded)
        {
            Console.WriteLine("\nDownloading Model since its not found in cache. This can take several mins depending on network speed and model size.\n");
        }
        else
        {
            Console.WriteLine("\nModel found in cache.\n");
        }
        await FoundryLocalManager.StartModelAsync(modelId);

        ApiKeyCredential key = new ApiKeyCredential("not-needed-for-local");

        OpenAIClient client = new OpenAIClient(key, new OpenAIClientOptions
        {
            Endpoint = manager?.Endpoint


        });
        var options = new ChatCompletionOptions
        {
            MaxOutputTokenCount = 5000
        };
       
        ChatClient chatClient = client.GetChatClient(modelId);
        Console.WriteLine("Request sent to Foundry Local\n ");
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("You are a helpful assistant."),
            new UserChatMessage("What is artificial intelligence?"),  
        };
        CollectionResult<StreamingChatCompletionUpdate> completionUpdates = chatClient.CompleteChatStreaming(messages, options);

        Console.WriteLine($"[ASSISTANT]: ");
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
        Environment.Exit(0);

    }

    private static async Task<bool> isModelLoaded(string modelId, FoundryLocalManager manager)
    {
        List<ModelInfo> loadedModels = await manager.ListCachedModelsAsync();
        for ( int i=0; i < loadedModels.Count; i++)
        {
            if (loadedModels[i].ModelId.Equals(modelId))
            {
                return true;
            }
        }
        return false;
    }
}
   