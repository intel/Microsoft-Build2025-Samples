using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;

using Microsoft.Windows.AI.MachineLearning;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

internal class Program
{
    private static async Task<int> Main(string[] arguments)
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
                logId = "ClipVitApplication",
                logLevel = OrtLoggingLevel.ORT_LOGGING_LEVEL_WARNING
            };

            using OrtEnv ortEnv = OrtEnv.CreateInstanceWithOptions(ref envOptions);

            Infrastructure infrastructure = new();

            //Download the Windows ML Runtime Intel OpenVINO Execution Provider.
            await infrastructure.DownloadPackagesAsync();
            await infrastructure.RegisterExecutionProviderLibrariesAsync();

            string executableFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;
            Console.WriteLine($"Executable folder is {executableFolder}");
            //Please use AI Toolkit in Visual Studio code to convert and optimize the Clip-Vit model and Please e
            string modelPath = Path.Combine(executableFolder, "openvino_model_quant_st.onnx");
            string imagePath = arguments[0]; 
            string device = arguments.Length > 1 ? arguments[1] : Constants.NPU;
            Console.WriteLine($"\nImage path is {imagePath}");
            Console.WriteLine($"Device selected is {device}");

            Console.WriteLine("Creating session ...");
            SessionOptions sessionOptions = new();

            sessionOptions.SetEpSelectionPolicy(ImageHelper.GetExecutionProviderDevicePolicy(device));

            using var session = new InferenceSession(modelPath, sessionOptions);

            Console.WriteLine("Preparing input ...");
            
            //Fetch model input key names
            var firstInputKey = session.InputMetadata.First().Key;
            var secondInputKey = session.InputMetadata.ElementAt(1).Key;
            var thirdInputKey = session.InputMetadata.ElementAt(2).Key;
            var inputMeta = session.InputMetadata;
            
            //Write out the input metadata
            foreach (var input in inputMeta)
            {
                Console.WriteLine($"Input: {input.Key}, Type: {input.Value.ElementDataType}, Shape: {string.Join(", ", input.Value.Dimensions)}");
            }
            const int inputIdSize = 10 * 77;

            long[] flatInputIds = new long[inputIdSize];
            long[] attentionIds = new long[inputIdSize];

            for (int i = 0; i < flatInputIds.Length; i++)
            {
                flatInputIds[i] = 0;
                attentionIds[i] = 0;
            }

            IList<string> textDescriptions = ImageHelper.LoadTextDescriptions("imagedesc.txt");
            ImageHelper.GetEncodedIds(textDescriptions, flatInputIds, attentionIds);

            // Prepare input tensors
            var firstInputTensor = new DenseTensor<long>(flatInputIds, new[] { 10, 77 }); // Shape: [10, 77]
            var secondInputTensor = await ImageHelper.PreprocessImageAsync(await ImageHelper.LoadImageFileAsync(imagePath)); // Shape: [1, 3, 224, 224]
            var thirdInputTensor = new DenseTensor<long>(attentionIds, new[] { 10, 77 }); // Shape: [10, 77]

            // Bind inputs and run inference
            var inputs = new List<NamedOnnxValue>
                        {
                            NamedOnnxValue.CreateFromTensor(firstInputKey, firstInputTensor),
                            NamedOnnxValue.CreateFromTensor(secondInputKey, secondInputTensor),
                            NamedOnnxValue.CreateFromTensor(thirdInputKey, thirdInputTensor),
                        };

            Console.WriteLine("\nRunning inference ...");
            using var results = session.Run(inputs);

            // Uncomment the below code to view the device activity in task manager
            //for (int i = 0; i < 100; i++)
            //{
            //    session.Run(inputs);
            //}
            // Extract output tensor
            var outputName = session.OutputMetadata.ElementAt(0).Key;
            Console.WriteLine($"\nOutput tensor name is {outputName} \n");

            var resultTensor = results.First(r => r.Name == outputName).AsEnumerable<float>().ToArray();

            // Load labels and print results
            PrintLogits(resultTensor, textDescriptions);

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Exception: {ex}");
        }

        return -1;
    }

    public static float[] Softmax(float[] logits)
    {
        float maxLogit = logits.Max();
        float sumExp = logits.Sum(l => (float)Math.Exp(l - maxLogit));
        return logits.Select(l => (float)Math.Exp(l - maxLogit) / sumExp).ToArray();
    }

    public static void PrintLogits(float[] logits, IList<string> labels)
    {
        float[] probabilities = Softmax(logits);
        Console.WriteLine("Here is the probability score for each image desc: ");
        Console.WriteLine("-------------------------------------------------------------");

        for (int i = 0; i < probabilities.Length; i++)
        {
            Console.WriteLine($"{labels[i]}: {probabilities[i]:P2}");
        }
        Console.WriteLine("-------------------------------------------------------------");

    }

}
