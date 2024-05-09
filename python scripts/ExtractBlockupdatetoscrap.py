import os

def pull_lines_with_word(file_path, target_words):
    lines_with_word = []
    with open(file_path, 'r', encoding='utf-8', errors='ignore') as file:
        for line in file:
            if any(word in line for word in target_words):
                lines_with_word.append(line.strip())
    return lines_with_word

def add_line_after_tag(lines, tag, new_line):
    modified_lines = []
    for line in lines:
        modified_lines.append(line)
        if tag in line:
            modified_lines.append(new_line)
        if '<Component Subtype="' in line:  # Check for Component tag start
            # Extract the material name
            material_name = line.split('Component Subtype="')[1].split('"')[0]
            # Append the material name to the SubtypeId in the DeconstructId section
            modified_lines.append('<DeconstructId>')
            modified_lines.append('<TypeId>Ore</TypeId>')
            modified_lines.append('<SubtypeId>Scrap' + material_name + '</SubtypeId>')
            modified_lines.append('</DeconstructId>')
    return modified_lines

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

def process_files(directory, output_directory, target_words):
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

                    lines = add_line_after_tag(lines, '', '')

                    for line in lines:
                        if line.strip():  # Check if the line is not empty
                            output_file.write(line + '\n')

                    for line in custom_lines_at_end:
                        output_file.write(line + '\n')

directory = r'C:\Program Files (x86)\Steam\steamapps\common\SpaceEngineers\Content'  # Replace with the actual directory path
output_directory = r'C:\Users\ljdug\AppData\Roaming\SpaceEngineers\Mods\modified_files'  # Replace with the desired output directory path
os.makedirs(output_directory, exist_ok=True)

custom_lines_at_beginning = ['<?xml version="1.0" encoding="utf-8"?>']
custom_lines_at_end = []
target_words = ['CubeBlocks>','</Definitions>','<TypeId>','<SubtypeId','<DisplayName>','<Definition','</Definition>','<Id','</Id>','<Component','</Component','<CriticalComponent']

process_files(directory, output_directory, target_words)
