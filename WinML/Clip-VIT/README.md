# Bring Your Own Model (BYOM)

## Software Prerequisites

1. **Install Visual Studio Code**  
   [Visual Studio Code](https://code.visualstudio.com/)

2. **Install AI Toolkit Extension in Visual Studio Code**  
   [AI Toolkit for VS Code](https://code.visualstudio.com/docs/intelligentapps/overview)

3. **Install latest Windows App SDK Stable Release**
   [Windows App SDK](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/downloads)  
    
## Instructions for Running the Clip-VIT Sample

Running the sample involves the model preparation phase that converts and quantizes the model using AI Toolkit, followed by inferencing using Windows ML Runtime and Windows ML Runtime Intel OpenVINO EP.

### Model Preparation

1. **Set Up Huggingface Token**  
   - Create a hugging face account if you don't have one by going to [Hugging Face](https://huggingface.co)
   - Create an access token if you don't have one by logging into your Hugging Face account. Follow these instructions:  
     [User Access Tokens](https://huggingface.co/docs/hub/en/security-tokens)
   - Set the Huggingface token as a system environment variable named `HUGGINGFACE_HUB_TOKEN`.
 
2. **Create New Model Project**  
   - Go to the AI Toolkit Extension and click on `Conversion (Preview)` > `New Model Project`.  

     ![Create Project](images/create-project.png)

3. **Select Model**  
   - Choose the model `openai/clip-vit-vbase-patch32` and click the `Next` button.  

     ![Select Model](images/select-model.png)

4. **Enter Project Details**  
   - Specify the folder name and project name, then click the `Next` button.  

     ![Enter Project Name](images/project-name.png)

5. **Go back to AI Toolkit Extension**  
   - Go to the AI Toolkit Extension and click on `Conversion (Preview)`

     ![Enter Project Name](images/aitoolkit-extension.png)

6. **Select Conversion Workflow**  
   - Click on "Convert to Intel NPU" workflow.  

     ![Convert to Intel NPU](images/Select-conversion.png)

7. **Run Conversion**  
   - Accept the default options for conversion to ONNX format and quantization, then hit the `Run` button.  

     ![Run Conversion Workflow](images/convert.png)

8. **Copy Model Path**  
   - Once conversion completes, copy the model path by going to `Actions` > `Copy Model Path`.  

     ![Copy Model Path](images/copy-model-path.png)

9. **Access Model Files**  
   - Remove `model.onnx` from the end of the model path and enter the path in File Explorer.

10. **Copy Model Files**  
   - Copy files ending with extensions `openvino_model_quant_st.bin`, `openvino_model_quant_st.onnx`, and `openvino_model_quant_st.xml` into the root folder of the sample application, i.e., `Microsoft-Build2025-Samples\WinML\Clip-VIT`.

### Model Inferencing

1. **Load the Solution**  
   Open `Clip-VIT.sln` located in the `Clip-VIT` directory.

2. **Build the Solution**  
   Run "Clean Solution" followed by "Build Solution".

3. **Prepare Image Directory**  
   Create a new folder named `images` under `C:\`, resulting in the path `C:\images`, and copy `bird.jpg` from the root folder into it.

4. **Set Command Line Arguments**  
   - Right-click on `Clip-VIT` > `Properties` > `Debug` > `Command line arguments`.
   - Specify `c:\images\bird.jpg NPU` to target NPU as the execution device for the model.

   ![Properties](images/command-line-arguments.png)
   
   You can also target CPU and GPU as the execution devices with the following arguments:  
   - `c:\images\bird.jpg CPU`  
   - `c:\images\bird.jpg GPU`


5. **Run the Project**  
   - Execute the `Clip-VIT` project, and you should see a result similar to below.  

   ![Inference Result](images/inference-result.png)