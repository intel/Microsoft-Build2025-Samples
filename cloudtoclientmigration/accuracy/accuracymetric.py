from nltk.translate.meteor_score import meteor_score
from nltk import word_tokenize
import json
import numpy as np
import argparse

def parse_jsonl(file_path: str):
    json_list = []
    with open(file_path, 'r') as file:
        for line in file:
            json_object = json.loads(line.strip())
            json_list.append(json_object)
    return json_list

parser = argparse.ArgumentParser(description="Help command")
parser.add_argument("-r", "--results", type=str, required=True, help="Path to results json file for evaluation.")

#Parse the input args.
args = parser.parse_args()

results_file = args.results

print(f"Location of results file is {results_file}")

#METEOR (Metric for Evaluation of Translation with Explicit ORdering) is a metric for the evaluation of machine translation output.
# Calculate METEOR Score
json_list = parse_jsonl(results_file)
scores = []
print("Computing METEOR Score")

for json_obj in json_list:
    scores.append(meteor_score([word_tokenize(json_obj['reference_text'])], word_tokenize( json_obj['generated_text'])))

print(f"METEOR Score: {np.mean(scores)}")