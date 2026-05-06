using System;
using Microsoft.AI.Foundry.Local;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

public class FoundryLocalService
{
    private FoundryLocalManager? _foundryManager;
    private string? _endpoint = "http://127.0.0.1:5000";
    private string _modelId;
    private IModel? _model;
    private string? _judgeModelId;
    private object? _judgeModelVariant;
    private bool _isInitialized;
    private ILoggerFactory? _loggerFactory;

    public FoundryLocalService()
    {
    }

    private void Log(string message) => Console.WriteLine(message);
    public FoundryLocalManager? Manager => _foundryManager;
    public string Endpoint => _endpoint ?? "http://127.0.0.1:5000";

    private async Task<string?> LoadModelAsync(string modelId)
    {
        if (_foundryManager == null) return null;
        try
        {
            Console.WriteLine($"Loading model: {modelId}...");
            var catalog = await _foundryManager.GetCatalogAsync();
            _model = await catalog.GetModelVariantAsync(modelId);
            if (_model == null)
            {
                Console.WriteLine($"Model '{modelId}' not found in catalog.");
                return null;
            }
            var modelLoaded = await isModelLoaded(modelId, _foundryManager);

            if (!modelLoaded)
            {
                Console.WriteLine("\nDownloading Model since its not found in cache. This can take several mins depending on network speed and model size.\n");
                await _model.DownloadAsync(null);
            }
            else
            {
                Console.WriteLine("\nModel found in cache.\n");
            }
          
            await _model.LoadAsync();
            Console.WriteLine($"Model '{modelId}' loaded.");
            return modelId;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading model '{modelId}': {ex.Message}");
            return null;
        }
    }

    private async Task<bool> downloadEPs()
    {
        try
        {
            Console.WriteLine($"Downloading Eps");

            if (_foundryManager == null)
            {
                Console.WriteLine("Foundry Manager not initialized.");
                return false;
            }
            // Discover available execution providers and their registration status.
            var eps = _foundryManager.DiscoverEps();
            int maxNameLen = 30;
            Console.WriteLine("Available execution providers:");
            Console.WriteLine($"  {"Name".PadRight(maxNameLen)}  Registered");
            Console.WriteLine($"  {new string('─', maxNameLen)}  {"──────────"}");
            foreach (var ep in eps)
            {
                Console.WriteLine($"  {ep.Name.PadRight(maxNameLen)}  {ep.IsRegistered}");
            }

            // Download and register all execution providers with per-EP progress.
            // EP packages include dependencies and may be large.
            // Download is only required again if a new version of the EP is released.
            Console.WriteLine("\nDownloading execution providers:");
            if (eps.Length > 0)
            {
                string currentEp = "";
                await _foundryManager.DownloadAndRegisterEpsAsync((epName, percent) =>
                {
                    if (epName != currentEp)
                    {
                        if (currentEp != "")
                        {
                            Console.WriteLine();
                        }
                        currentEp = epName;
                    }
                    Console.WriteLine($"{epName.PadRight(maxNameLen)}  {percent,6:F1}%");
                });
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("No execution providers to download.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading model: {ex.Message}");
            return false;

        }
        return true;
    }

    public async Task InitializeAsync()
    {
        try
        {
            Console.WriteLine("Initializing Foundry Local...");
            var config = new Configuration
            {
                AppName = "Foundry Local Sample App",
                LogLevel = Microsoft.AI.Foundry.Local.LogLevel.Debug,
                Web = new Configuration.WebService
                {
                    Urls = _endpoint
                }
            };

            _loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);
            });

            var logger = _loggerFactory.CreateLogger("Foundry Local Sample App");

            // Initialize the singleton instance
            await FoundryLocalManager.CreateAsync(config, logger);
            _foundryManager = FoundryLocalManager.Instance;
            await downloadEPs();
            await _foundryManager.StartWebServiceAsync();
            _isInitialized = true;

            Console.WriteLine($"Started web server successfully");
            Console.WriteLine($"Foundry Local initialized successfully");
            Console.WriteLine($"Endpoint: {_endpoint}");
            Console.WriteLine();

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during Foundry Local initialization: {ex.Message}");
            Console.WriteLine("Using fallback endpoint: http://localhost:8080");
            _endpoint = "http://localhost:8080";
        _isInitialized = true;
        }

    }

    public async Task<string?> SelectandDownloadModel()
    {
        var  modelCatalog = await _foundryManager.GetCatalogAsync();
        Console.WriteLine($"List of supported models in Foundry Local:\n");
        var models = await modelCatalog.ListModelsAsync();
        foreach (var availableModel in models)
        {
            foreach (var variant in availableModel.Variants)
            {
                Log($"{variant.Id}");
            }
        }

        Console.WriteLine("\nPlease copy the model name from the above list which you would like to try out :");
        var modelId = "";
        while (modelId.Equals(""))
        {
            modelId = Console.ReadLine()?.Trim();
        }
        return await LoadModelAsync(modelId);
 
    }

    private static async Task<bool> isModelLoaded(string modelId, FoundryLocalManager manager)
    {
        var catalog = await manager.GetCatalogAsync();
        var cachedModels = await catalog.GetCachedModelsAsync();
        return cachedModels.Any(m => {
            return m.Info.Name.Contains(modelId.Split(":")[0], StringComparison.OrdinalIgnoreCase);
        });
    }

    public async Task ShutdownAsync()
    {
        if (_foundryManager != null)
        {
            try
            {
                Console.WriteLine("Shutting down Foundry Local...");

                // Unload model if loaded
                if (_model != null)
                {
                    await _model.UnloadAsync();
                    Console.WriteLine("Model unloaded");
                }
              
                // Stop the web service
                await _foundryManager.StopWebServiceAsync();
                Console.WriteLine("Foundry Local shutdown complete");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during shutdown: {ex.Message}");
            }
        }

        _loggerFactory?.Dispose();
        await Task.CompletedTask;
    }


}
