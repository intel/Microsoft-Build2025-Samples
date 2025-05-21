using Microsoft.Windows.AI.Imaging;
using Microsoft.Graphics.Imaging;
using Windows.Graphics.Imaging;
using System.Text;
using Microsoft.Windows.AI;
public static class OCRRecognition
{
    public async static Task<string> RecognizeText()
    {
        SoftwareBitmap inputBitmap = await Helper.LoadImageBufferFromFileAsync("ocr-text.jpg");
        SoftwareBitmap convertedImage = SoftwareBitmap.Convert(inputBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
        using var imageBuffer = ImageBuffer.CreateForSoftwareBitmap(convertedImage);

        var text = await RecognizeTextFromSoftwareBitmap(imageBuffer);
        Console.Write(text);
        return text;
    }
    
    private static async Task<string> RecognizeTextFromSoftwareBitmap(ImageBuffer imageBuffer)
    {
        if (TextRecognizer.GetReadyState() == AIFeatureReadyState.NotReady)
        { 
            await TextRecognizer.EnsureReadyAsync();
        }
        TextRecognizer textRecognizer = await TextRecognizer.CreateAsync();

        Console.WriteLine($"Recognizing text in image at : {Helper.ImagesFolder}\\Assets\\ocr-text.jpg \n");

        RecognizedText recognizedText = textRecognizer.RecognizeTextFromImage(imageBuffer);
        StringBuilder stringBuilder = new StringBuilder();

        foreach (var line in recognizedText.Lines)
        {
            stringBuilder.AppendLine(line.Text);
        }
        return stringBuilder.ToString();
    }

}