import os

def find_tabs_in_yaml(directory):
    yaml_files = [os.path.join(root, file)
                  for root, dirs, files in os.walk(directory)
                  for file in files
                  if file.endswith('.yaml') or file.endswith('.yml')]

    for yaml_file in yaml_files:
        with open(yaml_file, 'r', encoding='utf-8') as file:
            lines = file.readlines()
        
        for i, line in enumerate(lines):
            if '\t' in line:
                print(f'Tab character found in file {yaml_file}, line {i + 1}: {line.strip()}')

# Укажите директорию, в которой находятся ваши YAML файлы
directory_path = 'D:/OtherGames/SpaceStation14/перевод/corvax-frontier-14/Resources/Prototypes/'
find_tabs_in_yaml(directory_path)
