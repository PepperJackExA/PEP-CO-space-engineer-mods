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

def output_file_path(input_file, output_directory, directory):
    # Get the relative path of the input file relative to the directory
    relative_path = os.path.relpath(input_file, directory)
    # Construct the output file path by joining the output directory with the relative path
    output_path = os.path.join(output_directory, os.path.splitext(relative_path)[0] + '.xml')
    # Ensure all parent directories of the output file path are created
    output_dir = os.path.dirname(output_path)
    os.makedirs(output_dir, exist_ok=True)
    return output_path

def process_files(directory, output_directory):
    for root, _, files in os.walk(directory):
        for file_name in files:
            if file_name.endswith('.sbc'):
                input_file_path = os.path.join(root, file_name)
                output_file_path_name = output_file_path(input_file_path, output_directory, directory)

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


directory = r'C:\Program Files (x86)\Steam\steamapps\workshop\content\244850\3179652888'  # Replace with the actual directory path
output_directory = r'C:\Users\ljdug\AppData\Roaming\SpaceEngineers\Mods\modified_files\3179652888'  # Replace with the desired output directory path
os.makedirs(output_directory, exist_ok=True)

custom_lines_at_beginning = ['<?xml version="1.0" encoding="utf-8"?>']
custom_lines_at_end = [' ']
target_words = ['</CubeBlocks>','</Definitions>','<TypeId>','<SubtypeId','<DisplayName>','<Definition','</Definition>','<Id','</Id>','CubeBlocks>','<BlockPairName>','<Component','</Component','<CriticalComponent',
                'ProducedGases','GasInfo','IceToGasRatio','<PCU>','IceConsumptionPerSecond','PowerConsumption','BlueprintClasses>','Class>',
                'ForceMagnitude','SlowdownFactor','BuildTimeSeconds','MinPlanetaryInfluence','EffectivenessAtMinInfluence','ThrusterType','FuelConverter','FuelId','Efficiency>',
                'AmmoMagazine>','<Mass>','<Volume>','<PhysicalMaterial>','<Capacity>','AmmoDefinitionId','OfferAmount>','OrderAmount>','<CanPlayerOrder>',
                'Ammos>','<Ammo','</Ammo','BasicProperties>','<DesiredSpeed>','<SpeedVariance>','<MaxTrajectory>','<BackkickForce>','<PhysicalMaterial>','<ExplosiveDamageMultiplier>',
                'ProjectileProperties>','<Projectile','<HeadShot>','<Explosion','<EndOfLife','Description>',
                'MissileProperties','<Missile','</Missile','Sounds>','Sound>','<MaxDistance','<Loopable','<UpdateDistance','Waves>','<Wave','</Wave','<Loop>','<Start>','<End>',
                'CategoryClasses','<Category','</Category>','<StrictSearch>','ItemIds>','<string>','Public>','<Name>','<SearchBlocks','<IsToolCategory',
                'BlockVariantGroup','<Blocks>','</Blocks>','<Block Type=','BlueprintClassEntries>','<Entry Class=','Blueprints>','Blueprint>','Prerequisites>','<Item Amount=','Results>',
                'BaseProductionTimeInSeconds>','<Bots>','</Bots>','<Bot xsi:type=','<Bot>','</Bot>','<RemoveAfterDeath','<AvailableInSurvival','<FactionTag>','InventoryContainerTypeId','<AttackLength>',
                '<AttackRadius>','<CharacterDamage>','<GridDamage>','<TargetGrids>','<TargetCharacters>','SpawnGroups>','SpawnGroup>',' <IsEncounter','<IsPirate','<IsCargoShip','<Speed>','<Frequency>',
                'Characters>','Character>','Inventory>','InventorySpawnContainerId>','<UseOnlyWalking>','<MaxSlope>','<MaxSprintSpeed>','<MaxRunSpeed>','<MaxBackrunSpeed>','<MaxRunStrafingSpeed>',
                '<MaxWalkSpeed>','<MaxBackwalkSpeed>','<MaxWalkStrafingSpeed>','<MaxCrouchWalkSpeed>','<MaxCrouchBackwalkSpeed>','<MaxCrouchStrafingSpeed>','<JumpForce>','<CharacterHeadSize>',
                '<OxygenConsumption>','<OxygenConsumptionMultiplier>','<OxygenSuitRefillTime>','<MinOxygenLevelForSuitRefill>','<PressureLevelForLowDamage>','<DamageAmountAtZeroPressure>','<NeedsOxygen>',
                '<MaxCapacity>','<Throughput>','SuitResourceStorage>','<Resource>','</Resource>','<MaxCapacity>','<Throughput>','<SuitConsumptionInTemperatureExtreme>','Jetpack>','Thrust>','<SideFlameOffset>',
                '<FrontFlameOffset>','ThrustProperties>','<LootingTime>','<MaxIntegrity>','<DropProbability>','<Health>','AcquisitionAmount>','<MinimalPricePerUnit>','</ContainerType','<ContainerType','<Item A',
                '<Items>','</Items>','</Item>','ContractTypes>','<ContractType xsi','</ContractType>','<MinimumReputation>','<FailReputationPrice>','<MinimumMoney>','<MinStartingDeposit>','<MaxStartingDeposit>',
                '<DurationMultiplier>','<Duration_BaseTime>','<Duration_TimePerJumpDist>','<Duration_TimePerMeter>','ChancesPerFactionType>','ContractChance>','DefinitionId>','<Value>','AvailableItems>','<RewardRadius>',
                '<TriggerRadius>','<TravelDistanceMin>','<TravelDistanceMax>','<DroneFirstDelayInS>','<InitialDelayInS>','<DroneAttackPeriodInS>','<DronesPerWave>','<Duration_BaseTime>','<Duration_FlightTimeMultiplier>',
                'PrefabsAttackDrones>','PrefabsEscortShips>','DroneBehaviours>','<Duration_TimePerCubicKm>','PrefabsSearchableGrids>','<RemarkPeriodInS>','<RemarkVariance>','<KillRange>','<KillRangeMultiplier>','<ReputationLossForTarget>',
                '<RewardRadius>','<PriceToRewardCoeficient>','<PriceSpread>','<MaxGridDistance>','<MinGridDistance>','PrefabNames>', 'Prefabs>','<Prefab SubtypeId','</Prefab>',
                'DropContainers>','DropContainer>','<Prefab>','<Priority>','SpawnRules>','<CanSpawnInSpace>','<CanSpawnInAtmosphere>','<CanSpawnOnMoon>','<CanBePersonal>','<CanBeCompetetive>',
                'EntityComponents>','<EntityComponent xsi','</EntityComponent>','<TimeToRemoveMin>','Container>','DefaultComponents>','</Faction>','<Flags>','Factions>','<Faction Tag','<StartingBalance>','<DiscoveredByDefault>','<FactionIcon>',
                '<IsDefault>','<AcceptHumans>','<AutoAcceptMember>','<EnableFriendlyFire>','OffersList>','<ItemId Type','<CanSellOxygen>','OrdersList>','<CanSellHydrogen>','<MaxContractCount>','GasProperties>','<Gas>','</Gas>','<EnergyDensity>','GlobalEvents>',
                'GlobalEvent>','<MinActivationTimeMs>','<MaxActivationTimeMs>','<FirstActivationTimeMs>','HandItems>','<HandItem xsi','</HandItem>','<RunMultiplier>','AmplitudeOffset','AmplitudeScale','PhysicalItems>',
                'PhysicalItem>','<PhysicalItem xsi:','PhysicalMaterials>','<Density>','<CollisionMultiplier>','</PhysicalMaterial>','</ResearchBlock>','<ResearchBlock xsi:type','UnlockedByGroups>','<GroupSubtype>','ResearchGroups>','<ResearchGroup xsi',
                'Members>','<BlockId Type','RespawnShips>','<Ship>','</Ship>','<CooldownSeconds>','<DisplayName>','<UseForPlanetsWithAtmosphere>','<UseForSpace>','StatDefinitions>','<Stat xsi:typ','</Stat>','<MinValue>','<DefaultValue>','<MaxValue>','Weapons>','Weapon>',
                '<RangeMultiplier>','<DamageMultiplier>','<ReleaseTimeAfterFire>','<DeviateShotAngleAiming>','<DeviateShotAngle>','<ReloadTime>','<Recoil','</Recoil','<Weapon>','<CubeSize>','<IsAirTight>','BlockPositions>','BlockPosition>','<ShowEdges>','<RequiredPowerInput>',
                '<ResourceSinkGroup>','<MaxBroadcastRadius>','<MaxBroadcastPowerDrainkW>','<EnableFirstPerson>','<PowerInputIdle>','<PowerInputTurning>','<PowerInputLasing>','<MinElevationDegrees>','<MaxElevationDegrees>','<MinAzimuthDegrees>','<MaxAzimuthDegrees>',
                '<RotationRate>','<MaxRange>','<RequireLineOfSight>','DeconstructId>','<MaxFieldSize','<MinFieldSize','GravityAcceleration>','VirtualMass>','InventoryMaxVolume>','StoredGasId>','<LeakPercent>','<GasExplosion','ResearchGroup']
lines_to_add_after_tags = {
    '</Definistion>': ' ',
}

process_files(directory, output_directory)