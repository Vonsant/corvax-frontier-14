import os

# Путь к директории, в которой нужно искать файлы
directory_path = r"D:\OtherGames\SpaceStation14\перевод\corvax-frontier-14\Resources\Locale"

# Имя файла для сохранения результатов
output_file = "error_files_list.txt"

# Функция для проверки наличия ошибки в файле
def has_error(line):
    return "ent-['" in line

# Список для хранения путей к файлам с ошибками
error_files = []

# Обход всех файлов в указанной директории
for root, _, files in os.walk(directory_path):
    for file in files:
        if file.endswith(".ftl") or file.endswith(".json") or file.endswith(".yaml"):  # добавлены возможные расширения файлов
            file_path = os.path.join(root, file)
            with open(file_path, 'r', encoding='utf-8') as f:
                for line in f:
                    if has_error(line):
                        error_files.append(file_path)
                        break  # достаточно найти одну ошибку, чтобы записать файл в список

# Удаление дубликатов путем преобразования списка в множество и обратно
error_files = list(set(error_files))

# Сохранение списка файлов с ошибками в текстовый файл
with open(output_file, 'w', encoding='utf-8') as f:
    for file_path in error_files:
        f.write(file_path + "\n")

print(f"Список файлов с ошибками сохранен в {output_file}")
