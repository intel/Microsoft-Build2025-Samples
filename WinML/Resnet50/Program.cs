using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.Windows.AI.MachineLearning;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml;

internal class Program
{
    public static async Task<int> Main(string[] arguments)
    {
        try
        {
            if (arguments.Length < 1)
            {
                throw new ArgumentException("Pass an image!");
            }

            // Create a new instance of EnvironmentCreationOptions
            EnvironmentCreationOptions envOptions = new()
            {
                logId = "ResnetApplication",
                logLevel = OrtLoggingLevel.ORT_LOGGING_LEVEL_WARNING
            };

            using OrtEnv ortEnv = OrtEnv.CreateInstanceWithOptions(ref envOptions);

            await InitializeProvidersAsync();

            string executableFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;
           
            Console.WriteLine($"Executable folder is {executableFolder}");
            string modelPath = Path.Combine(executableFolder, "ov_model_st_quant.onnx");
            string labelsPath = Path.Combine(executableFolder, "ResNet50Labels.txt");
            string device = arguments.Length > 1 ? arguments[1] : Constants.GPU;
           
            string imagePath = arguments[0];
            Console.WriteLine($"Device selected is {device}");
            Console.WriteLine($"The image path is {imagePath}");

            Console.WriteLine("Creating session ...");
            SessionOptions sessionOptions = new();

            sessionOptions.SetEpSelectionPolicy(ResNetHelper.GetExecutionProviderDevicePolicy(device));
            using var session = new InferenceSession(modelPath, sessionOptions);

            Console.WriteLine("Preparing input ...");
            var input = await ResNetHelper.PreprocessImageAsync(await ResNetHelper.LoadImageFileAsync(imagePath));

            // Prepare input tensor
            var inputName = session.InputMetadata.First().Key;
            Console.WriteLine("Input name is " + inputName);

            var inputTensor = new DenseTensor<float>(
                input.ToArray(),          // Use the DenseTensor<float> directly
                new[] { 1, 3, 224, 224 }, // Shape of the tensor
                false                     // isReversedStride should be explicitly set to false
            );

            // Bind inputs and run inference
            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor(inputName, inputTensor)
            };

            Console.WriteLine("Running inference ...");
            using var results = session.Run(inputs);

            // Uncomment the below code to view the device activity in task manager
            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();
            //for (int i = 0; i < 1000; i++)
            //{
            //    Console.WriteLine($"Iteration {i} ");
            //    var results1 = session.Run(inputs);
            //}
            //stopwatch.Stop();
            //Console.WriteLine($"Execution Time for 1000 requests: {stopwatch.Elapsed.TotalMilliseconds/1000} secs");

            // Extract output tensor
            var outputName = session.OutputMetadata.First().Key;
            Console.WriteLine("Output name is " + outputName);

            var resultTensor = results.First(r => r.Name == outputName).AsEnumerable<float>().ToArray();

            // Load labels and print results
            var labels = ResNetHelper.LoadLabels(labelsPath);
            PrintResults(labels, resultTensor);

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Exception: {ex}");
        }

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
    public static IReadOnlyList<float> Softmax(IReadOnlyList<float> logits)
    {
        List<float> exps = new List<float>(logits.Count);
        float maxLogit = logits.Max();

        float sum = 0.0f;
        for (int i = 0; i < logits.Count; ++i)
        {
            float expValue = (float)Math.Exp(logits[i] - maxLogit); // stability trick
            exps.Add(expValue);
            sum += expValue;
        }

        for (int i = 0; i < exps.Count; ++i)
        {
            exps[i] /= sum;
        }

        return exps;
    }

    public static void PrintResults(IList<string> labels,  IReadOnlyList<float> results)
    {
        IReadOnlyList<float> probabilities = Softmax(results);

        List<KeyValuePair<float, int>> topResults = new List<KeyValuePair<float, int>>();

        for (int i = 0; i < probabilities.Count; ++i)
        {
            topResults.Add(new KeyValuePair<float, int>(probabilities[i], i));
        }

        topResults.Sort((a, b) => b.Key.CompareTo(a.Key));

        for (int i = 0; i < Math.Min(5, topResults.Count); ++i)
        {
            var (probability, index) = topResults[i];
            Console.WriteLine($"{labels[index]} with confidence of {Math.Round(probability*100, 2)}%");
        }
    }
}
