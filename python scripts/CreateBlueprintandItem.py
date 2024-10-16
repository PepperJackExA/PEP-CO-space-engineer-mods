def generate_sbc_file(item_name):
    xml_content = f'''<?xml version="1.0"?>
<Definitions xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">

	<!-- Containers -->

	<ContainerTypes>

		<ContainerType CountMin="1" CountMax="1">
			<Id>
				<TypeId>ContainerTypeDefinition</TypeId>
				<SubtypeId>SpiderLoot</SubtypeId>
			</Id>
			<Items>
				<Item AmountMin="3" AmountMax="6">
					<Frequency>1.0</Frequency>
					<Id>
						<TypeId>ConsumableItem</TypeId>
						<SubtypeId>{item_name}</SubtypeId>
					</Id>
				</Item>
			</Items>
		</ContainerType>


	</ContainerTypes>

	<!-- Blueprint Classes -->

	<BlueprintClasses>

		<Class>
			<Id>
				<TypeId>BlueprintClassDefinition</TypeId>
				<SubtypeId>Farming</SubtypeId>
			</Id>
			<DisplayName>Farming</DisplayName>
			<Description>Farming</Description>
			<Icon>Textures\\GUI\\Icons\\buttons\\component.dds</Icon>
			<HighlightIcon>Textures\\GUI\\Icons\\buttons\\component.dds</HighlightIcon>
			<FocusIcon>Textures\\GUI\\Icons\\buttons\\component_focus.dds</FocusIcon>
			<InputConstraintIcon>Textures\\GUI\\Icons\\filter_ingot.dds</InputConstraintIcon>
			<OutputConstraintIcon>Textures\\GUI\\Icons\\filter_ingot.dds</OutputConstraintIcon>
		</Class>

	</BlueprintClasses>

	<!-- Blueprint Class Entries -->

	<BlueprintClassEntries>
		<Entry Class="Farming" BlueprintSubtypeId="{item_name}" />
	</BlueprintClassEntries>

	<!-- Blueprints -->

	<Blueprints>

		<Blueprint>
			<Id>
				<TypeId>BlueprintDefinition</TypeId>
				<SubtypeId>{item_name}</SubtypeId>
			</Id>
			<DisplayName>{item_name}</DisplayName>
			<Description>a space-grown version of the red fruit</Description>
			<Icon>Textures\\GUI\\Icons\\{item_name}.png</Icon>
			<Prerequisites>
				<Item Amount="0.01" TypeId="Ingot" SubtypeId="Stone" />
				<Item Amount="0.001" TypeId="Ore" SubtypeId="Sapling" />
				<Item Amount="0.1" TypeId="Ore" SubtypeId="Organic" />
				<Item Amount="0.6" TypeId="Ore" SubtypeId="Ice" />
			</Prerequisites>
			<Result Amount="1" TypeId="ConsumableItem" SubtypeId="{item_name}" />
			<BaseProductionTimeInSeconds>6000</BaseProductionTimeInSeconds>
		</Blueprint>

	</Blueprints>

	<!-- Physical Items -->

	<PhysicalItems>

		<PhysicalItem xsi:type="MyObjectBuilder_ConsumableItemDefinition">
			<Id>
				<TypeId>ConsumableItem</TypeId>
				<SubtypeId>{item_name}</SubtypeId>
			</Id>
			<DisplayName>{item_name}</DisplayName>
			<Description>a space-grown version of the red fruit</Description>
			<Icon>Textures\\GUI\\Icons\\{item_name}.png</Icon>
			<Size>
				<X>0.03</X>
				<Y>0.03</Y>
				<Z>0.03</Z>
			</Size>
			<Mass>0.05</Mass>
			<Volume>0.05</Volume>
			<Model>Models\\{item_name}.mwm</Model>
			<PhysicalMaterial>None</PhysicalMaterial>
			<Stats>
				<Stat Name="Health" Value="0.01" Time="10"/>
				<Stat Name="Fatigue" Value="0.05" Time="2"/>
				<Stat Name="Hunger" Value="0.05" Time="2"/>
			</Stats>
			<MinimalPricePerUnit>50</MinimalPricePerUnit>
			<MinimumOfferAmount>1000</MinimumOfferAmount>
			<MaximumOfferAmount>10000</MaximumOfferAmount>
			<CanPlayerOrder>true</CanPlayerOrder>
		</PhysicalItem>

	</PhysicalItems>

</Definitions>
'''
    with open(f'{item_name}.sbc', 'w') as file:
        file.write(xml_content)

# Example usage:
generate_sbc_file("Banana","2")
