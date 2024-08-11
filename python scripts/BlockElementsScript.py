import os
import xml.etree.ElementTree as ET
import csv

# Set the path to the Space Engineers Content\Data directory
data_dir = r'C:\Program Files (x86)\Steam\steamapps\common\SpaceEngineers\Content\Data\CubeBlocks'

# Define the output CSV file
output_csv = 'block_definitionsTest.csv'

# Dictionary to store unique XML fields per TypeId
block_data = {}

# Loop through each .sbc file in the directory
for filename in os.listdir(data_dir):
    if filename.endswith('.sbc'):
        filepath = os.path.join(data_dir, filename)

        # Parse the XML file
        tree = ET.parse(filepath)
        root = tree.getroot()

        # Extract block definitions and their XML elements
        for block in root.findall('.//Definition'):
            block_type = block.find('Id').find('TypeId').text
            elements = [elem.tag for elem in block]

            # Merge unique XML fields for each TypeId
            if block_type in block_data:
                block_data[block_type].update(elements)
            else:
                block_data[block_type] = set(elements)

# Write the data to a CSV file
with open(output_csv, 'w', newline='') as csvfile:
    csvwriter = csv.writer(csvfile)

    # Write the header
    header = ['BlockType'] + ['XMLFields']
    csvwriter.writerow(header)

    # Write the block data
    for block_type, elements in block_data.items():
        csvwriter.writerow([block_type] + list(elements))

print(f"CSV file created: {output_csv}")
