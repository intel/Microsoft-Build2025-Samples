using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.VisualBasic;
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Storage;
using Windows.Storage.Streams;

public class ResNetHelper
{
    //Return ExecutionProviderDevicePolicy based on the device passed
    public static ExecutionProviderDevicePolicy GetExecutionProviderDevicePolicy(string device)
    {
        switch (device)
        {
            case Constants.CPU:
                return ExecutionProviderDevicePolicy.PREFER_CPU;

            case Constants.GPU:
                return ExecutionProviderDevicePolicy.PREFER_GPU;

            case Constants.NPU:
                return ExecutionProviderDevicePolicy.PREFER_NPU;

            default:
                return ExecutionProviderDevicePolicy.PREFER_CPU;
        }
    }

    public static async Task<VideoFrame> LoadImageFileAsync(string filePath)
    {
        StorageFile file = await StorageFile.GetFileFromPathAsync(filePath);
        var stream = await file.OpenAsync(FileAccessMode.Read);
        BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
        SoftwareBitmap softwareBitmap = await decoder.GetSoftwareBitmapAsync();
        VideoFrame inputImage = VideoFrame.CreateWithSoftwareBitmap(softwareBitmap);

        return inputImage;
    }

    public static IList<string> LoadLabels(string labelsPath)
    {
        return File.ReadAllLines(labelsPath)
            .Select(line => line.Split(',', 2)[1])
            .ToList();
    }

    public static async Task<DenseTensor<float>> PreprocessImageAsync(VideoFrame videoFrame)
    {
        var softwareBitmap = videoFrame.SoftwareBitmap;
        const int targetWidth = 224;
        const int targetHeight = 224;

        float[] mean = new float[] { 0.485f, 0.456f, 0.406f };
        float[] std = new float[] { 0.229f, 0.224f, 0.225f };

        // Convert to BGRA8
        if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 ||
            softwareBitmap.BitmapAlphaMode != BitmapAlphaMode.Premultiplied)
        {
            softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
        }

        // Resize
        softwareBitmap = await ResizeSoftwareBitmapAsync(softwareBitmap, targetWidth, targetHeight);

        // Get pixel data
        uint bufferSize = (uint)(targetWidth * targetHeight * 4);
        var buffer = new Windows.Storage.Streams.Buffer(bufferSize);
        softwareBitmap.CopyToBuffer(buffer);
        byte[] pixels = buffer.ToArray();

        // Output Tensor shape: [1, 3, 224, 224]
        var tensorData = new DenseTensor<float>(new[] { 1, 3, targetHeight, targetWidth });

        for (int y = 0; y < targetHeight; y++)
        {
            for (int x = 0; x < targetWidth; x++)
            {
                int pixelIndex = (y * targetWidth + x) * 4;
                float r = pixels[pixelIndex + 2] / 255f;
                float g = pixels[pixelIndex + 1] / 255f;
                float b = pixels[pixelIndex + 0] / 255f;

                // Normalize using mean/stddev
                r = (r - mean[0]) / std[0];
                g = (g - mean[1]) / std[1];
                b = (b - mean[2]) / std[2];

                int baseIndex = y * targetWidth + x;
                tensorData[0, 0, y, x] = r; // R
                tensorData[0, 1, y, x] = g; // G
                tensorData[0, 2, y, x] = b; // B
            }
        }

        return tensorData;
    }

    private static async Task<SoftwareBitmap> ResizeSoftwareBitmapAsync(SoftwareBitmap bitmap, int width, int height)
    {
        using (var stream = new InMemoryRandomAccessStream())
        {
            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
            encoder.SetSoftwareBitmap(bitmap);
            encoder.IsThumbnailGenerated = false;
            await encoder.FlushAsync();
            stream.Seek(0);

            var decoder = await BitmapDecoder.CreateAsync(stream);
            var transform = new BitmapTransform()
            {
                ScaledWidth = (uint)width,
                ScaledHeight = (uint)height,
                InterpolationMode = BitmapInterpolationMode.Fant
            };
            var resized = await decoder.GetSoftwareBitmapAsync(
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Premultiplied,
                transform,
                ExifOrientationMode.IgnoreExifOrientation,
                ColorManagementMode.DoNotColorManage);

            return resized;
        }
    }
}
