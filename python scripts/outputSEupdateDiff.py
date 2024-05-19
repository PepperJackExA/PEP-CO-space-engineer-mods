import os
import difflib
import shutil

def read_file_with_encoding(filename, encodings):
    for encoding in encodings:
        try:
            with open(filename, 'r', encoding=encoding) as file:
                return file.readlines()
        except UnicodeDecodeError:
            continue
    raise UnicodeDecodeError(f"Failed to decode {filename} using encodings {encodings}")

def compare_and_show_diff(original_dir, updated_dir, output_dir):
    for root, _, files in os.walk(original_dir):
        for file in files:
            original_file = os.path.join(root, file)
            updated_file = os.path.join(updated_dir, os.path.relpath(original_file, original_dir))

            if not os.path.exists(updated_file):
                print(f"File {updated_file} not found in the updated directory.")
                continue

            try:
                original_content = read_file_with_encoding(original_file, ['utf-8', 'latin-1'])
                updated_content = read_file_with_encoding(updated_file, ['utf-8', 'latin-1'])
            except UnicodeDecodeError as e:
                print(f"Error decoding file {e.filename}: {e.reason}")
                continue

            if original_content != updated_content:
                diff = difflib.ndiff(original_content, updated_content)
                diff_lines = [line for line in diff if line.startswith('- ') or line.startswith('+ ')]
                if diff_lines:
                    print(f"Differences found in {original_file} and {updated_file}:")
                    for line in diff_lines:
                        print(line)

                    output_path = os.path.join(output_dir, os.path.relpath(root, original_dir))
                    os.makedirs(output_path, exist_ok=True)
                    shutil.copy(updated_file, output_path)

    # Check for files in the updated directory that are not present in the original directory
    for root, _, files in os.walk(updated_dir):
        for file in files:
            original_file = os.path.join(original_dir, os.path.relpath(os.path.join(root, file), updated_dir))
            updated_file = os.path.join(root, file)

            if not os.path.exists(original_file):
                print(f"New file {updated_file} found in the updated directory.")
                output_path = os.path.join(output_dir, os.path.relpath(root, updated_dir))
                os.makedirs(output_path, exist_ok=True)
                shutil.copy(updated_file, output_path)


    print("Comparison and copying completed.")

if __name__ == "__main__":
    original_directory = input("Enter path to the original directory (default: C:\\Users\\ljdug\\Desktop\\ts\\1): ").strip()
    original_directory = original_directory or r"C:\\Users\\ljdug\Desktop\\ts\\1"
    updated_directory = input("Enter path to the updated directory (default: C:\\Users\\ljdug\\Desktop\\ts\\2): ").strip()
    updated_directory = updated_directory or r"C:\\Users\\ljdug\Desktop\\ts\\2"
    output_directory = input("Enter path to the output directory (default: C:\\Users\\ljdug\\Desktop\\ts\\output): ").strip()
    output_directory = output_directory or r"C:\\Users\\ljdug\Desktop\\ts\\output"

    compare_and_show_diff(original_directory, updated_directory, output_directory)


