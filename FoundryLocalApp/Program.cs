
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

class Program
{
    static void Main(string[] args)
    {
        ApiKeyCredential key = new ApiKeyCredential("not-needed-for-local");
        OpenAIClient client = new OpenAIClient(key, new OpenAIClientOptions
        {
            Endpoint = new Uri("http://localhost:5272/v1")
        });
        var chatClient = client.GetChatClient("mistral-7b-v02-int4-gpu");
        var prompt = "Can you tell me something about the solar system?";
        Console.WriteLine("Request sent to Foundry Local\n ");
        CollectionResult<StreamingChatCompletionUpdate> completionUpdates = chatClient.CompleteChatStreaming(prompt);
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
}