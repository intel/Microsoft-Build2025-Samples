using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntimeGenAI;
using Microsoft.Windows.AI.MachineLearning;
internal class Program
{
    public static async Task<int> Main(string[] arguments)
    {
        try
        {
            if (arguments.Length < 1)
            {
                throw new ArgumentException("Specify the model folder!");
            }

            EnvironmentCreationOptions envOptions = new()
            {
                logId = "GenAIApplication",
                logLevel = OrtLoggingLevel.ORT_LOGGING_LEVEL_WARNING
            };
            using OrtEnv ortEnv = OrtEnv.CreateInstanceWithOptions(ref envOptions);

            await InitializeProvidersAsync();
            string modelPath = arguments[0];
            Console.WriteLine("Model path is " + modelPath);

            using Config config = new Config(modelPath);
            using Model model = new Model(config);
            using Tokenizer tokenizer = new Tokenizer(model);
            int minLength = 50;
            int maxLength = 4096;

            while (true)
             {
                Console.WriteLine("\n------------------------------------------------------------\n");
                Console.WriteLine("Please enter your prompt or type exit to terminate the program:");
                Console.Write("User Input : ");
                var user_input = "";
                user_input = Console.ReadLine();
                if(user_input == "exit")
                {
                    break;
                }
                var messages = new[]
                    {
                    new { role = "system", content = "You are Qwen, created by Alibaba Cloud. You are a helpful assistant." },
                    new { role = "user", content = user_input }
                };

                // Convert to JSON string for tokenizer
                string messagesJson = System.Text.Json.JsonSerializer.Serialize(messages);
                var sequences = tokenizer.Encode(tokenizer.ApplyChatTemplate("", messagesJson, "", true));
                using GeneratorParams generatorParams = new GeneratorParams(model);
                generatorParams.SetSearchOption("min_length", minLength);
                generatorParams.SetSearchOption("max_length", maxLength);
                using var tokenizerStream = tokenizer.CreateStream();
                using var generator = new Generator(model, generatorParams);
                generator.AppendTokenSequences(sequences);
                var watch = System.Diagnostics.Stopwatch.StartNew();
                Console.Write("Answer:");

                while (!generator.IsDone())
                {
                    generator.GenerateNextToken();
                    Console.Write(tokenizerStream.Decode(generator.GetSequence(0)[^1]));
                }
                Console.WriteLine();
                watch.Stop();
                var runTimeInSeconds = watch.Elapsed.TotalSeconds;
                var outputSequence = generator.GetSequence(0);
                var totalTokens = outputSequence.Length;
                Console.WriteLine();
                Console.WriteLine($"Total Tokens: {totalTokens} Time: {runTimeInSeconds:0.00} Tokens per second: {totalTokens / runTimeInSeconds:0.00}");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Exception: {ex}");
        }
        //Environment.Exit(0);
        return -1;
    }

    public static async Task InitializeProvidersAsync()
    {
        Console.WriteLine("Getting available providers...");
        var catalog = ExecutionProviderCatalog.GetDefault();
        var providers = catalog.FindAllProviders();

        foreach (var provider in providers)
        {
            Console.WriteLine($"Provider: {provider.Name}");
            try
            {
                var providerState = provider.ReadyState;
                Console.WriteLine($"Provider state: {providerState}");
                await provider.EnsureReadyAsync();
                Console.WriteLine($"Provider state: {provider.ReadyState}");
                provider.TryRegister();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize provider {provider.Name}: {ex.Message}");
                // Continue with other providers
            }
        }
    }


}



