using PulsarModLoader.CustomGUI;
using System;
using System.Net;
using System.Net.Sockets;
using static UnityEngine.GUILayout;

namespace PhotonServerClient
{
    internal class GUI : ModSettingsMenu
    {
        public override string Name()
        {
            return $"Photon Server Client: {(ClientInterface.IsPrivateConnection ? "Private" : "Public")} Connection";
        }

        string IPString;
        string PortString;
        public override void Draw()
        {
            //PrivateModeLabel
            Label($"Photon Server Client: {(ClientInterface.IsPrivateConnection ? "Private" : "Public")} Connection");


            //Funny broken IP textfields
            BeginHorizontal();
            BeginVertical();

            Label("IP");
            IPString = TextField(IPString);

            EndVertical();
            BeginVertical();

            Label("Port");
            PortString = TextField(PortString);

            EndVertical();
            EndHorizontal();


            //Saved Address label
            Label($"Saved Address: {ClientInterface.IP.Value}:{ClientInterface.Port.Value}");


            //Connected Address Label
            string ConnectedAddressDisplay;
            if(ClientInterface.IsPrivateConnection)
            {
                if (PhotonNetwork.connectionState == ConnectionState.Disconnected)
                {
                    ConnectedAddressDisplay = "Disconnected";
                }
                else
                {
                    ConnectedAddressDisplay = PhotonNetwork.ServerAddress;
                }
            }
            else
            {
                ConnectedAddressDisplay = "PhotonCloud";
            }
            Label($"Connected to: {ConnectedAddressDisplay}");


            //Connection Status Label
            Label("Connection status: " + PhotonNetwork.connectionStateDetailed.ToString());


            //PrivateConnectionButton
            if (Button($"Switch to {(ClientInterface.IsPrivateConnection ? "Public" : "Private")} Connection"))
            {
                ClientInterface.IsPrivateConnection = !ClientInterface.IsPrivateConnection;
                ClientInterface.Connect();
            }


            //DefaultConnectionButton
            if (Button($"Default Connection Type: {(ClientInterface.AlwaysConnectToPrivate ? "Private" : "Public")}"))
            {
                ClientInterface.AlwaysConnectToPrivate.Value = !ClientInterface.AlwaysConnectToPrivate.Value;
            }


            //ConnectBtn
            if (Button("Connect"))
            {
                ClientInterface.Connect();
            }


            //IP parsing + warning
            if (!IPAddress.TryParse(IPString, out IPAddress addressObject) && !HostResolverStateMachine.ResolveHost(IPString, out addressObject))
            {
                //Photon Connections cannot take hostnames
                Label("IP failed to parse");
            }
            else
            {
                string address = addressObject.ToString();
                if (address != ClientInterface.IP.Value && address != "0.0.0.0" && address != "::" && address != "255.255.255.255")
                {
                    ClientInterface.IP.Value = address.ToString();

                    if (ClientInterface.IsPrivateConnection)
                    {
                        ClientInterface.IsPrivateConnection = true;
                    }
                }
            }


            //Port Parsing + warning
            if (ushort.TryParse(PortString, out ushort Port) && Port > 0)
            {
                if (Port != ClientInterface.Port.Value)
                {
                    ClientInterface.Port.Value = Port;

                    if (ClientInterface.IsPrivateConnection)
                    {
                        ClientInterface.IsPrivateConnection = true;
                    }
                }
            }
            else
            {
                Label("Port value not a valid port number");
            }
        }

        public override void OnOpen()
        {
            IPString = ClientInterface.IP.ToString();
            PortString = ClientInterface.Port.ToString();
        }


       
    }
}
