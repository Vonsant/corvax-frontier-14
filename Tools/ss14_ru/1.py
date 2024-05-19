def process_file(file_name):
    # Чтение строк из файла
    with open(file_name, 'r', encoding='utf-8') as file:
        lines = file.readlines()

    # Удаление повторяющихся строк и сортировка
    unique_lines = sorted(set(line.strip() for line in lines))

    # Запись уникальных строк обратно в файл
    with open(file_name, 'w', encoding='utf-8') as file:
        for line in unique_lines:
            file.write(line + '\n')

    print(f"Processed file '{file_name}' - duplicates removed, lines sorted.")

# Укажите имя файла
file_name = 'yamlextractor.txt'
process_file(file_name)
