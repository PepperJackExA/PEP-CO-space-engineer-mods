import os

def pull_lines_with_word(file_path, target_words):
    lines_with_word = []
    try:
        with open(file_path, 'r', encoding='utf-8', errors='ignore') as file:
            for line in file:
                if any(word in line for word in target_words):
                    lines_with_word.append(line.strip())
    except FileNotFoundError:
        print(f"File '{file_path}' not found.")
    except Exception as e:
        print(f"Error occurred while processing file '{file_path}': {e}")
    return lines_with_word

def add_line_after_tag(lines, tag, new_line):
    modified_lines = []
    material_names = set()  # Keep track of material names
    for line in lines:
        if tag in line:
            modified_lines.append(new_line)
        if '<Component Subtype="' in line:  # Check for Component tag start
            # Extract the material name
            material_name = line.split('Component Subtype="')[1].split('"')[0]
            material_names.add(material_name)  # Add material name to set
            # Extract the Count attribute
            count_attribute = line.split('Count="')[1].split('"')[0]
            # Append the material name and Count attribute to the new Component tag
            modified_lines[-1] = f'<Component Subtype="{material_name}" Count="{count_attribute}">'
            modified_lines.append('<DeconstructId>')
            modified_lines.append('<TypeId>Ore</TypeId>')
            modified_lines.append(f'<SubtypeId>scrap{material_name}</SubtypeId>')
            modified_lines.append('</DeconstructId>')
            modified_lines.append('</Component>')
        else:         
            modified_lines.append(line)
    return modified_lines, material_names  # Return modified lines and material names

def write_sbc_file(output_file_path, material_name):
    with open(output_file_path, 'w', encoding='utf-8') as output_file:
        output_file.write('<Definitions xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">\n')
        output_file.write('    <Components>\n')
        output_file.write('        <Component>\n')
        output_file.write('            <Id>\n')
        output_file.write('                <TypeId>Ore</TypeId>\n')
        output_file.write(f'                <SubtypeId>scrap{material_name}</SubtypeId>\n')
        output_file.write('            </Id>\n')
        output_file.write(f'            <DisplayName>scrap{material_name}</DisplayName>\n')
        output_file.write(f'            <Icon>Textures\\GUI\\Icons\\component\\construction_components_component.dds</Icon>\n')
        output_file.write('            <Size>\n')
        output_file.write('                <X>0.2</X>\n')
        output_file.write('                <Y>0.1</Y>\n')
        output_file.write('                <Z>0.1</Z>\n')
        output_file.write('            </Size>\n')
        output_file.write('            <Mass>8</Mass>\n')
        output_file.write('            <Volume>2</Volume>\n')
        output_file.write(f'            <Model>Models\\Components\\construction_components_component.mwm</Model>\n')
        output_file.write('            <PhysicalMaterial>Metal</PhysicalMaterial>\n')
        output_file.write('            <MaxIntegrity>30</MaxIntegrity>\n')
        output_file.write('            <Health>18</Health>\n')
        output_file.write('        </Component>\n')
        output_file.write('    </Components>\n')
        output_file.write('</Definitions>\n')


def output_file_path(input_file, output_directory, directory):
    # Get the relative path of the input file relative to the directory
    relative_path = os.path.relpath(input_file, directory)
    # Construct the output file path by joining the output directory with the relative path
    output_path = os.path.join(output_directory, os.path.splitext(relative_path)[0] + '.xml')
    # Ensure all parent directories of the output file path are created
    output_dir = os.path.dirname(output_path)
    os.makedirs(output_dir, exist_ok=True)
    return output_path

def process_files(directory, output_directory, target_words, material_names):
    try:
        for root, _, files in os.walk(directory):
            for file_name in files:
                if file_name.endswith('.sbc'):
                    input_file_path = os.path.join(root, file_name)
                    output_file_path_name = output_file_path(input_file_path, output_directory, directory)
                    os.makedirs(os.path.dirname(output_file_path_name), exist_ok=True)
                    lines = pull_lines_with_word(input_file_path, target_words)

                    with open(output_file_path_name, 'w', encoding='utf-8') as output_file:
                        for line in custom_lines_at_beginning:
                            output_file.write(line + '\n')

                        lines, material_names = add_line_after_tag(lines, '', '')  # Capture the returned material_names

                        for line in lines:
                            if line.strip():  # Check if the line is not empty
                                output_file.write(line + '\n')

                        for line in custom_lines_at_end:
                            output_file.write(line + '\n')

                       # Write the corresponding .sbc file for each material_name
                        for material_name in material_names:
                            sbc_file_path = os.path.join(f'{output_directory}\scrap', f'scrap_{material_name}_ore.sbc')
                            os.makedirs(f'{output_directory}\scrap', exist_ok=True)
                            write_sbc_file(sbc_file_path, material_name)
                            print("Created .sbc file:", sbc_file_path)
    except Exception as e:
        print("An error occurred:", e)

directory = r'C:\Program Files (x86)\Steam\steamapps\common\SpaceEngineers\Content'  # Replace with the actual directory path
output_directory = r'C:\Users\ljdug\AppData\Roaming\SpaceEngineers\Mods\modified_files'  # Replace with the desired output directory path
os.makedirs(output_directory, exist_ok=True)

custom_lines_at_beginning = ['<?xml version="1.0" encoding="utf-8"?>']
custom_lines_at_end = []
target_words = ['CubeBlock','Definitions','<TypeId>','<SubtypeId','<DisplayName>','Definition','<Id','</Id>','<Component','</Component','<CriticalComponent']


material_names = set()  # Initialize material_names outside the loop

process_files(directory, output_directory, target_words, material_names)

