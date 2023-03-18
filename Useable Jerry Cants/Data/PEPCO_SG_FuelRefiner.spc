<?xml version="1.0" encoding="utf-8"?>
<Definitions xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
    <CubeBlocks>
        <Definition xsi:type="MyObjectBuilder_OxygenGeneratorDefinition">
            <Id>
                <TypeId>OxygenGenerator</TypeId>
                <SubtypeId>PEPCO_SG_FuelRefiner</SubtypeId>
            </Id>
            <DisplayName>PEPCO_SG_FuelRefiner</DisplayName>
            <Icon>Textures\GUI\Icons\Cubes\container.dds</Icon>
            <Description>PEPCO_SG_FuelRefiner</Description>
            <CubeSize>Small</CubeSize>
            <BlockTopology>TriangleMesh</BlockTopology>
            <Size x="1" y="1" z="1" />
            <ModelOffset x="0" y="0" z="0" />
            <Model>Models\Cubes\Small\CargoContainerSmall.mwm</Model>
            <SilenceableByShipSoundSystem>true</SilenceableByShipSoundSystem>
            <Components>
                <Component Subtype="SteelPlate" Count="6" />
                <Component Subtype="Construction" Count="8" />
                <Component Subtype="LargeTube" Count="2" />
                <Component Subtype="Motor" Count="1" />
                <Component Subtype="Computer" Count="3" />
                <Component Subtype="SteelPlate" Count="2" />
            </Components>
            <CriticalComponent Subtype="Computer" Index="0" />
            <MountPoints>
                <MountPoint Side="Front" StartX="0" StartY="0" EndX="3" EndY="3" />
                <MountPoint Side="Right" StartX="1" StartY="0" EndX="2" EndY="3" />
                <MountPoint Side="Right" StartX="0" StartY="1" EndX="1" EndY="2" />
                <MountPoint Side="Left" StartX="0" StartY="0" EndX="1" EndY="3" />
                <MountPoint Side="Left" StartX="1" StartY="1" EndX="2" EndY="2" />
                <MountPoint Side="Top" StartX="0" StartY="1" EndX="3" EndY="2" />
                <MountPoint Side="Top" StartX="0" StartY="0" EndX="1" EndY="1" />
                <MountPoint Side="Top" StartX="2" StartY="0" EndX="3" EndY="1" />
                <MountPoint Side="Bottom" StartX="0" StartY="0" EndX="3" EndY="1" />
                <MountPoint Side="Bottom" StartX="0" StartY="1" EndX="1" EndY="2" />
                <MountPoint Side="Bottom" StartX="2" StartY="1" EndX="3" EndY="2" />
                <MountPoint Side="Back" StartX="0" StartY="1" EndX="3" EndY="2" />
            </MountPoints>
            <BuildProgressModels>
                <Model BuildPercentUpperBound="0.50" File="Models\Cubes\Small\OxygenGeneratorConstruction_1.mwm" />
                <Model BuildPercentUpperBound="1.00" File="Models\Cubes\Small\OxygenGeneratorConstruction_2.mwm" />
            </BuildProgressModels>
            <Center x="1" y="1" z="0" />
            <BuildTimeSeconds>14</BuildTimeSeconds>
            <EdgeType>Light</EdgeType>
            <ResourceSourceGroup>Reactors</ResourceSourceGroup>
            <ResourceSinkGroup>Factory</ResourceSinkGroup>
            <IceConsumptionPerSecond>5</IceConsumptionPerSecond>
            <InventoryMaxVolume>1</InventoryMaxVolume>
            <SilenceableByShipSoundSystem>true</SilenceableByShipSoundSystem>
            <InventorySize>
                <X>2</X>
                <Y>2</Y>
                <Z>2</Z>
            </InventorySize>

            <InventoryFillFactorMin>0.3</InventoryFillFactorMin>
            <InventoryFillFactorMax>0.6</InventoryFillFactorMax>

            <FuelPullAmountFromConveyorInMinutes>2.75</FuelPullAmountFromConveyorInMinutes>

            <StandbyPowerConsumption>0.001</StandbyPowerConsumption>
            <OperationalPowerConsumption>0.1</OperationalPowerConsumption>
            <ProducedGases>
                <GasInfo>
                    <Id>
                        <TypeId>GasProperties</TypeId>
                        <SubtypeId>Gasoline</SubtypeId>
                    </Id>
                    <IceToGasRatio>8</IceToGasRatio>
                </GasInfo>
            </ProducedGases>
		  <BlueprintClasses>
			  <Class>GasolineCantRefill</Class>
		  </BlueprintClasses>
			<BlockPairName>PEPCO_SG_FuelRefiner</BlockPairName>
            <DamageEffectName>Damage_WeapExpl_Damaged</DamageEffectName>
            <DamagedSound>ParticleWeapExpl</DamagedSound>
            <BlockPairName>OxygenGenerator</BlockPairName>
            <GenerateSound>BlockOxyGenProcess</GenerateSound>
            <IdleSound>BlockOxyGenIdle</IdleSound>
            <DestroyEffect>Explosion_Missile</DestroyEffect>
            <DestroySound>WepSmallMissileExpl</DestroySound>
            <EmissiveColorPreset>Extended</EmissiveColorPreset>
            <DestroyEffect>BlockDestroyedExplosion_Small</DestroyEffect>
            <DestroySound>WepSmallWarheadExpl</DestroySound>
            <PCU>50</PCU>
            <MirroringX>Z</MirroringX>
            <MirroringY>Z</MirroringY>
            <MirroringZ>Y</MirroringZ>
            <TieredUpdateTimes>
                <unsignedInt>300</unsignedInt>
                <unsignedInt>600</unsignedInt>
                <unsignedInt>1200</unsignedInt>
            </TieredUpdateTimes>
        </Definition>
    </CubeBlocks>
</Definitions>