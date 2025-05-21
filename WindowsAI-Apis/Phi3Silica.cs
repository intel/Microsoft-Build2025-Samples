
using Microsoft.Windows.AI;
using Microsoft.Windows.AI.Text;
public static class Phi3Silica
{
    public static async Task<string> run_prompt(string prompt)
    {
        if (LanguageModel.GetReadyState() == AIFeatureReadyState.NotReady)
            await LanguageModel.EnsureReadyAsync();
        using LanguageModel languageModel = await LanguageModel.CreateAsync();

        Console.WriteLine($"Question: {prompt}");

        var result = await languageModel.GenerateResponseAsync(prompt);

        Console.WriteLine($"Answer from Phi3 silica: {result.Text}");
        return result.Text;

    }
}