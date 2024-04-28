from xml.etree.ElementTree import ElementTree
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
    return modified_lines

def output_file_path(input_file, output_directory):
    subfolder_name = os.path.basename(os.path.dirname(input_file))
    return os.path.join(output_directory, subfolder_name, os.path.splitext(os.path.basename(input_file))[0] + '.xml')

def process_files(directory, output_directory):
    for root, _, files in os.walk(directory):
        for file_name in files:
            if file_name.endswith('.sbc'):
                input_file_path = os.path.join(root, file_name)
                output_file_path_name = output_file_path(input_file_path, output_directory)

                # Ensure the output folder exists
                os.makedirs(os.path.dirname(output_file_path_name), exist_ok=True)

                lines = pull_lines_with_word(input_file_path, target_words)

                with open(output_file_path_name, 'w', encoding='utf-8') as output_file:
                    # Add custom lines at the beginning of the file
                    for line in custom_lines_at_beginning:
                        output_file.write(line + '\n')

                    for tag, new_line in lines_to_add_after_tags.items():
                        lines = add_line_after_tag(lines, tag, new_line)

                    for line in lines:
                        output_file.write(line + '\n')

                    # Add custom lines at the end of the file
                    for line in custom_lines_at_end:
                        output_file.write(line + '\n')


directory = r'C:\Program Files (x86)\Steam\steamapps\common\SpaceEngineers\Content\Data'  # Replace with the actual directory path
output_directory = r'C:\Users\ljdug\AppData\Roaming\SpaceEngineers\Mods\modified_files'  # Replace with the desired output directory path
os.makedirs(output_directory, exist_ok=True)

custom_lines_at_beginning = ['<?xml version="1.0" encoding="utf-8"?>']
custom_lines_at_end = [' ']
target_words = ['</CubeBlocks>','</Definitions>','<TypeId>','<SubtypeId>','<DisplayName>','<Definition','</Definition>','<Id>','</Id>','CubeBlocks>','<BlockPairName>','<Component','</Component','<CriticalComponent',
                'ProducedGases','GasInfo','IceToGasRatio','<PCU>','IceConsumptionPerSecond','PowerConsumption','BlueprintClasses>','Class>',
                'ForceMagnitude','SlowdownFactor','BuildTimeSeconds','MinPlanetaryInfluence','EffectivenessAtMinInfluence','ThrusterType','FuelConverter','FuelId','Efficiency>',
                'AmmoMagazine>','<Mass>','<Volume>','<PhysicalMaterial>','<Capacity>','AmmoDefinitionId','OfferAmount>','OrderAmount>','<CanPlayerOrder>',
                'Ammos>','<Ammo','</Ammo','BasicProperties>','<DesiredSpeed>','<SpeedVariance>','<MaxTrajectory>','<BackkickForce>','<PhysicalMaterial>','<ExplosiveDamageMultiplier>',
                'ProjectileProperties>','<Projectile','<HeadShot>','<Explosion','<EndOfLife'
                'MissileProperties','<Missile','</Missile','Sounds>','Sound>','<MaxDistance','<Loopable','<UpdateDistance','Waves>','<Wave','</Wave','<Loop>','<Start>','<End>']
lines_to_add_after_tags = {
    '</Definistion>': ' ',
}

process_files(directory, output_directory)