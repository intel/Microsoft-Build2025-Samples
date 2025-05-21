# Windows AI APIs

## Software Prerequisites

1. **Join the Windows Insider Program**  
   The Windows AI APIs currently require you to join the Windows Insider program. For more information, visit:  
   [Windows AI APIs Documentation](https://learn.microsoft.com/en-us/windows/ai/apis/phi-silica#:~:text=windows.ai.text-,Important,so%20until%20the%20app%20has%20been%20granted%20package%20identity%20at%20runtime,-.)

2. **Instructions to Join**  
   Follow the instructions to join the Windows Insider program:  
   [Join the Windows Insider Program](https://support.microsoft.com/en-us/windows/join-the-windows-insider-program-and-manage-insider-settings-ef20bb3d-40f4-20cc-ba3c-a72c844b563c)

3. **Enable Developer Mode**  
   Turn on Developer Mode from Windows Settings.

## Instructions for Running the Windows AI APIs

1. **Load the Solution**  
   Open `WindowsAI-Apis.sln` located in the `WindowsAI-Apis` directory.

2. **Build the Solution**  
   Run "Clean Solution" followed by "Build Solution".

3. **Prepare Image Directory**  
   - Create a new folder named `images` under `C:\`, resulting in the path `C:\images`.
   - Copy the `Assets` folder containing images from `WindowsAI-Apis` into `C:\images`. The images should now be available at `C:\images\Assets`.

4. **Run the Project**  
   Execute the `WindowsAI-APIs-Packaged` project with the target set to "Local machine".

5. **Command Line Arguments**  
   - By default, Phi Silica runs if no command line arguments are provided.
   - To specify command line arguments, right-click on `WindowsAI-APIs-Packaged` > `Properties` > `Debug` > `Command line arguments`.

6. **Command Line Arguments for each API**  
   - **Phi Silica**: No command line arguments needed.
   - **Text Recognition**: `ocr C:\images`
   - **Image Super Resolution**: `image_scaler C:\images`
   - **Image Erase**: `image_erase C:\images`
   - **Image Segmentation**: `image_extractor C:\images`

**Note:** For imaging tasks, images will be read from the `C:\images\Assets` folder, and results will be written to `C:\images\Results`. The command prompt will indicate the location of the newly generated image.

