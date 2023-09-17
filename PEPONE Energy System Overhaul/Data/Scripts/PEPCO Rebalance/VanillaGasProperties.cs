using Sandbox.Definitions;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ObjectBuilders.Definitions;

namespace PEPCO.VanillaGasPropertiesRebalance
{
    
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class PEPCO_Session : MySessionComponentBase
    {
        public override void LoadData()
        {

            VanillaHydrogenGas(new MyDefinitionId(typeof(MyObjectBuilder_GasProperties), "Hydrogen"));
            VanillaOxygenGas(new MyDefinitionId(typeof(MyObjectBuilder_GasProperties), "Oxygen"));

        }


        private void VanillaHydrogenGas(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyGasProperties;
            definition.EnergyDensity *= 1; //0.001556
            

        }
        private void VanillaOxygenGas(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(definitionId) as MyGasProperties;
            definition.EnergyDensity = 1; //0
        }
    }
}