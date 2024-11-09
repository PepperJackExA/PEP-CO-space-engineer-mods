using Sandbox.Game;
using VRage.Game.ModAPI;
using Sandbox.Game.Entities.Character.Components;
using VRageMath;

namespace PEPCO.iSurvival.factors
{
    public static class EnvironmentalFactors
    {
        public static float GetEnvironmentalFactor(IMyPlayer player)
        {
            Vector3D playerPosition = player.GetPosition();
            string currentWeather = MyVisualScriptLogicProvider.GetWeather(playerPosition);
            //MyAPIGateway.Utilities.ShowMessage("Weather", $"{currentWeather}");

            if (currentWeather.ToLower().Contains("clear"))
            {
                return 1f; // No effect
            }
            else if (currentWeather.ToLower().Contains("rain"))
            {
                return 1.1f; // Slightly increased drain due to discomfort
            }
            else if (currentWeather.ToLower().Contains("storm"))
            {
                return 1.3f; // Higher drain due to harsh conditions
            }
            else if (currentWeather.ToLower().Contains("sand"))
            {
                return 1.5f; // Much higher drain, especially for water
            }
            else if (currentWeather.ToLower().Contains("snow"))
            {
                return 1.2f; // Increased drain due to cold stress
            }
            else
            {
                return 1.0f; // Default effect
            }
        }
        public static float OxygenLevelEnvironmentalFactor(IMyPlayer player)
        {
            // Ensure player and its character are valid
            if (player == null || player.Character == null) return 1.0f;

            // Ensure character has oxygen properties
            var character = player.Character;
            var oxygenComponent = character.Components?.Get<MyCharacterOxygenComponent>();

            // Return the oxygen level if available, otherwise return a default factor
            return oxygenComponent?.EnvironmentOxygenLevel ?? 1.0f;
        }
    }
}