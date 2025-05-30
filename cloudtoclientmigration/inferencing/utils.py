import json

def parse_jsonl(file_path: str):
    json_list = []
    with open(file_path, 'r') as file:
        for line in file:
            json_object = json.loads(line.strip())
            json_list.append(json_object)
    return json_list


    
