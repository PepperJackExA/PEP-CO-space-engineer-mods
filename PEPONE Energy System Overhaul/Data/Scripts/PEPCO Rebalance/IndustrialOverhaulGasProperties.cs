using Sandbox.Definitions;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ObjectBuilders.Definitions;

namespace PEPCO.IndustrialOverhaulGasPropertiesRebalance
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class PEPCO_Session : MySessionComponentBase
    {
        public override void LoadData()
        {
            // Gasoline
            IndustiralOverhaulGasolineGas(new MyDefinitionId(typeof(MyObjectBuilder_GasProperties), "Gasoline"));
            // Steam
            IndustiralOverhaulSteamGas(new MyDefinitionId(typeof(MyObjectBuilder_GasProperties), "Steam"));
            // Deuterium
            IndustiralOverhaulDeuteriumGas(new MyDefinitionId(typeof(MyObjectBuilder_GasProperties), "Deuterium"));

        }


        private void IndustiralOverhaulGasolineGas(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyGasProperties;
            //Gasoline
            definition.EnergyDensity *= 1; //0.001556
        }
        private void IndustiralOverhaulSteamGas(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyGasProperties;
            //Steam
            definition.EnergyDensity *= 1; //0
        }
        private void IndustiralOverhaulDeuteriumGas(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyGasProperties;
            //Deuterium
            definition.EnergyDensity *= 1; //0
        }
    }
}