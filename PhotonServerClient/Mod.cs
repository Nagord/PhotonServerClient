using PulsarModLoader;

namespace PhotonServerClient
{
    public class Mod : PulsarMod
    {
        public Mod()
        {
            ClientInterface.IsPrivateConnection = ClientInterface.AlwaysConnectToPrivate.Value;
        }

        public override string Author => "Dragon, OnHyex";

        public override string Name => "Photon Server Client";

        public override string LongDescription => "Provides a GUI for connecting to custom/private photon servers.";
        public override string Version => "1.1.0";
        

        public override string HarmonyIdentifier()
        {
            return $"{Author}.{Name}";
        }
    }
}
