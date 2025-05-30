# Copyright (C) 2023-2025 Intel Corporation
# SPDX-License-Identifier: Apache-2.0

import argparse
from pathlib import Path
from time import sleep
import json
from utils import parse_jsonl
import os
import openvino_genai as ov_genai
from openvino_genai import PerfMetrics, EncodedResults, DecodedResults

def convert_ov_tokenizer(tokenizer_path):
    from optimum.exporters.openvino.convert import export_tokenizer
    from transformers.models.auto.tokenization_auto import AutoTokenizer

    hf_tokenizer = AutoTokenizer.from_pretrained(tokenizer_path, trust_remote_code=True)

    export_tokenizer(hf_tokenizer, tokenizer_path)

def write_perf_metrics(perf_metrics: PerfMetrics):
    print(f"\nPerformance Metrics")
    print(f"-------------------------")
    print(f"Load time: {perf_metrics.get_load_time():.2f} ms")
    print(f"Generate time: {perf_metrics.get_generate_duration().mean:.2f} ± {perf_metrics.get_generate_duration().std:.2f} ms")
    print(f"Tokenization time: {perf_metrics.get_tokenization_duration().mean:.2f} ± {perf_metrics.get_tokenization_duration().std:.2f} ms")
    print(f"Detokenization time: {perf_metrics.get_detokenization_duration().mean:.2f} ± {perf_metrics.get_detokenization_duration().std:.2f} ms")
    print(f"TTFT: {perf_metrics.get_ttft().mean:.2f} ± {perf_metrics.get_ttft().std:.2f} ms")
    print(f"TPOT: {perf_metrics.get_tpot().mean:.2f} ± {perf_metrics.get_tpot().std:.2f} ms")
    print(f"Throughput : {perf_metrics.get_throughput().mean:.2f} ± {perf_metrics.get_throughput().std:.2f} tokens/s")

def get_model_name(model_path: str) -> str:
    path_segments = model_path.split("\\")
    return f'{path_segments[2]}\\{path_segments[3]}'

def main():
    parser = argparse.ArgumentParser(description="Help command")
    parser.add_argument("-m", "--model", type=str, required=True, help="Path to model and tokenizers base directory")
    parser.add_argument("-p", "--prompt", type=str, default="Please translate the following string to english. Please print out only the translated text: आज मौसम अच्छा है।", help="Prompt")
    parser.add_argument("-nw", "--num_warmup", type=int, default=1, help="Number of warmup iterations")
    parser.add_argument("-pf", "--prompt_file", type=str, default="./prompts.jsonl", help="Prompt file")
    parser.add_argument("-n", "--num_iter", type=int, default=1, help="Number of iterations")
    parser.add_argument("-mt", "--max_new_tokens", type=int, default=250, help="Maximum number of new tokens")
    parser.add_argument("-d", "--device", type=str, default="GPU", help="Device")
    
    #Parse the input args.
    args = parser.parse_args()

    default_prompt = [args.prompt]
    models_path = args.model
    model_name = get_model_name(models_path)
    #print(f'Model name is {model_name}')
    device = args.device
    num_iter = args.num_iter
    prompt_file = args.prompt_file
    
    config = ov_genai.GenerationConfig()
    config.max_new_tokens = args.max_new_tokens
    mod_path = Path(models_path)
    
    #Create tokenizers/detokeinzers if they don't exist.
    if (not (mod_path/"openvino_tokenizer.xml").exists()) or not ((mod_path/"openvino_detokenizer.xml").exists()):
        convert_ov_tokenizer(mod_path)

    #Generate OV Gen AI Pipeline.
    pipe = ov_genai.LLMPipeline(models_path, device)
    res = pipe.generate(default_prompt, config)
    perf_metrics = res.perf_metrics
    json_list = parse_jsonl(prompt_file)

    json_response_list = []
    accuracy_file_dir = f'..\\results\\{model_name}'
    os.makedirs(accuracy_file_dir, exist_ok=True)
    accuracy_file_path = f'.\\{accuracy_file_dir}\\results.json'
    if os.path.exists(accuracy_file_path):
        os.remove(accuracy_file_path)
    for _ in range(num_iter):
        for json_obj in json_list:
            sleep(2)
            prompt = [f'Please translate the following strings to English. Please print out only the translated text: {json_obj["prompt"]}']
            
            #Perform inferencing.
            res: EncodedResults| DecodedResults = pipe.generate(prompt, config)
            print(f'\nText to be translated : {json_obj["prompt"]}\n\nTranslated text is: {res.texts[0]}')
            perf_metrics += res.perf_metrics
            json_object_clone = json.loads(json.dumps(json_obj))
            json_object_clone['generated_text'] = res.texts[0]
            json_response_list.append(json_object_clone)
            with open(accuracy_file_path, 'a') as outfile:
                json_string = json.dumps(json_object_clone)
                outfile.write(json_string + '\n')
                #json.dumps(json_object_clone + '\n', outfile)
    write_perf_metrics(perf_metrics)

if __name__ == "__main__":
    main()