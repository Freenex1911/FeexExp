using Rocket.API;

namespace Freenex.FeexExp
{
    public class FeexExpConfiguration : IRocketPluginConfiguration
    {
        public bool EnableCustomLosePercentage;
        public decimal CustomLosePercentage;

        public void LoadDefaults()
        {
            EnableCustomLosePercentage = false;
            CustomLosePercentage = 20;
        }
    }
}
