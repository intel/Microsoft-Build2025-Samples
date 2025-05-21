using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Graphics;
using Microsoft.Windows.AI.Imaging;
using Microsoft.Windows.AI;
public static class Imaging
{
    public static async Task<SoftwareBitmap> ScaleImage()
    {
        if (ImageScaler.GetReadyState() == AIFeatureReadyState.NotReady)
        {
            await ImageScaler.EnsureReadyAsync();
        }
        ImageScaler imageScaler = await ImageScaler.CreateAsync();

        SoftwareBitmap softwareBitmap = await Helper.LoadImageBufferFromFileAsync("horses.jpg");

        Console.WriteLine($"Scaling image at : {Helper.ImagesFolder}\\Assets\\horses.jpg to  a 1000 x 1000 image. \n");

        SoftwareBitmap finalImage = imageScaler.ScaleSoftwareBitmap(softwareBitmap, 224, 224);
        StorageFile file = await Helper.GetStorageFile("horses-scaled.jpg", true);

        await SaveSoftwareBitmapToFileAsync(finalImage, file);

        Console.WriteLine($"Scaled image stored at : {Helper.ImagesFolder}\\Results\\horses-scaled.jpg.\n");

        return finalImage;
    }

    public static async Task<SoftwareBitmap> ImageObjectErase()
    {
        if (ImageObjectRemover.GetReadyState() == AIFeatureReadyState.NotReady)
        {
            await ImageObjectRemover.EnsureReadyAsync();
        }
        ImageObjectRemover imageObjectRemover = await ImageObjectRemover.CreateAsync();
        Console.WriteLine($"Image for object erase at : {Helper.ImagesFolder}\\Assets\\horses.jpg. Image mask for erasure at : {Helper.ImagesFolder}\\Assets\\horsesMask.jpg. \n");

        SoftwareBitmap softwareBitmap = await Helper.LoadImageBufferFromFileAsync("horses.jpg");
        SoftwareBitmap maskBitmap = await Helper.LoadImageBufferFromFileAsync("horsesMask.jpg");

        if (maskBitmap.BitmapPixelFormat != BitmapPixelFormat.Gray8 || maskBitmap.BitmapAlphaMode != BitmapAlphaMode.Premultiplied)
        {
            maskBitmap = SoftwareBitmap.Convert(maskBitmap, BitmapPixelFormat.Gray8, BitmapAlphaMode.Ignore);
        }
        SoftwareBitmap finalImage = imageObjectRemover.RemoveFromSoftwareBitmap(softwareBitmap, maskBitmap);
        Console.WriteLine($"Image after erasure at : {Helper.ImagesFolder}\\Results\\horses-masked.jpg\n");

        StorageFile file = await Helper.GetStorageFile("horses-masked.jpg", true);
        await SaveSoftwareBitmapToFileAsync(finalImage, file);
        return finalImage;
    }

    public static async Task<SoftwareBitmap> ImageExtraction()
    {
        if (ImageObjectExtractor.GetReadyState() == AIFeatureReadyState.NotReady)
        {
            await ImageObjectExtractor.EnsureReadyAsync();
        }
        SoftwareBitmap softwareBitmap = await Helper.LoadImageBufferFromFileAsync("dog.jpg");
        Console.WriteLine($"Image for extraction at : {Helper.ImagesFolder}\\Assets\\dog.jpg\n");

        ImageObjectExtractor imageObjectExtractor = await ImageObjectExtractor.CreateWithSoftwareBitmapAsync(softwareBitmap);

        var points = new List<PointInt32>
        {
                 new PointInt32(256, 256)
        };

        ImageObjectExtractorHint hint = new ImageObjectExtractorHint(null, points, null);
        SoftwareBitmap finalImage = imageObjectExtractor.GetSoftwareBitmapObjectMask(hint);
        StorageFile file = await Helper.GetStorageFile("dog-extracted.jpg", true);
        Console.WriteLine($"Image after extracting at : {Helper.ImagesFolder}\\Results\\dog-extracted.jpg.\n");

        await SaveSoftwareBitmapToFileAsync(finalImage, file);
        return finalImage;
    }

    public static async Task SaveSoftwareBitmapToFileAsync(SoftwareBitmap bitmap, StorageFile file)
    {
        using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
        {
            //Convert to a format compatible with encoders (e.g., BGRA8)
            SoftwareBitmap outputBitmap = SoftwareBitmap.Convert(bitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
            encoder.SetSoftwareBitmap(outputBitmap);
            await encoder.FlushAsync();
        }
    }

}