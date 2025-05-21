class Program
{
    static async Task Main(string[] args)
    {
        //Default values in case input arguments are not provided.
        string wcrApi = Constants.Phi3;
        Helper.ImagesFolder = "c:\\images";
        if (args.Length == 2)
        {
            wcrApi = args[0].ToLower();
            Helper.ImagesFolder = args[1].ToLower();
            Console.WriteLine("The WCR API argument is " + args[0].ToLower());
            Console.WriteLine("The images folder argument is " + args[1].ToLower());
        }
        else
        {
            Console.WriteLine("Defaulting to run phi3 since both params were not provided\n");

        }

        switch (wcrApi)
            {
                case Constants.Phi3:
                    await Phi3Silica.run_prompt("Can you tell me something about the solar system?\n");
                    break;

                case Constants.Ocr:
                    await OCRRecognition.RecognizeText();
                    break;

                case Constants.ImageScaler:
                    await Imaging.ScaleImage();
                    break;

                case Constants.ImageExtractor:
                    await Imaging.ImageExtraction();
                    break;
                case Constants.ImageErase:
                    await Imaging.ImageObjectErase();
                    break;
                default:
                    Console.WriteLine("Option is not recognized. Please specify one of \n phi3 \n ocr \n image_scaler\n image_extractor\n image_erase ");
                    break;
            }
        Console.WriteLine("Press enter to exit the program");
        Console.ReadLine();
    }

}
