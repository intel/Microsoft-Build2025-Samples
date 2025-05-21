using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;

public class Helper {

    public static string ImagesFolder ="";
   public static async Task<StorageFile> GetStorageFile(string fileName, bool createFile) {         
        string subfolder = Constants.Assets;
        if(createFile) {
            subfolder = Constants.Results;
        }
        string fullPath = Path.Combine(ImagesFolder, subfolder);
        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
            Console.WriteLine($"Directory created at: {fullPath}");
        }
       
        StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(fullPath);

        StorageFile file;
        if(createFile) {
            file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
        } else{
            file = await folder.GetFileAsync(fileName);
        }
        return file;
   }

    public static async Task<SoftwareBitmap> LoadImageBufferFromFileAsync(string fileName)
    {
        StorageFile file = await GetStorageFile(fileName, false);
        IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read);
        BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
        SoftwareBitmap bitmap = await decoder.GetSoftwareBitmapAsync();

        if (bitmap == null)
        {
            return null;
        }
        return bitmap;
    }


}