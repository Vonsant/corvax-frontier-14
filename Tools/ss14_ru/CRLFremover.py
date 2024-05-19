import os

def convert_crlf_to_lf_in_file(file_path):
    with open(file_path, 'rb') as file:
        content = file.read()

    new_content = content.replace(b'\r\n', b'\n')

    with open(file_path, 'wb') as file:
        file.write(new_content)
    print(f"Converted CRLF to LF in: {file_path}")

def process_files_in_directory(directory):
    for root, _, files in os.walk(directory):
        for file in files:
            if file.endswith(('.py', '.txt', '.ftl')):
                file_path = os.path.join(root, file)
                convert_crlf_to_lf_in_file(file_path)

# Укажите путь к директории, в которой находятся ваши файлы
directory_path = r'D:\OtherGames\SpaceStation14\перевод\corvax-frontier-14\Resources\Locale'
process_files_in_directory(directory_path)
