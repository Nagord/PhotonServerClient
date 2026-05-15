using ExitGames.Client.Photon;
using HarmonyLib;
using PulsarModLoader;
using System;
using System.Collections;
using UnityEngine;
using Logger = PulsarModLoader.Utilities.Logger;

namespace PhotonServerClient
{
    public class ClientInterface
    {
        //Replace Normal Connection
        [HarmonyPatch(typeof(PLGlobal), "ConnectToPhotonUsingSavedRegion")]
        internal class ConnectToPhotonPatch
        {
            static bool Prefix()
            {
                Connect();
                return false;
            }
        }

        //Largely copied from vanilla, but effecively replaces vanilla.
        private static IEnumerator SafeConnect()
        {
            Logger.Info("Trying to safe connect");
            PhotonNetwork.offlineMode = true;
            yield return 0;
            PhotonNetwork.offlineMode = false;
            yield return 0;

            if (PhotonNetwork.connected)
            {
                try
                {
                    PhotonNetwork.Disconnect();
                }
                catch (Exception ex)
                {
                    Logger.Info("WaitToConnect: " + ex.Message);
                }
            }
            yield return 0;

            int attempts = 0;
            while (PhotonNetwork.connectionState != ConnectionState.Disconnected)
            {
                yield return new WaitForSecondsRealtime(0.5f);
                int num = attempts;
                attempts = num + 1;
                if (attempts == 3)
                {
                    try
                    {
                        PhotonNetwork.Disconnect();
                    }
                    catch (Exception ex)
                    {
                        Logger.Info("WaitToConnect: " + ex.Message);
                    }
                }
                if (attempts > 10)
                {
                    break;
                }
            }

            Logger.Info("Attempting ConnectUsingSettings");
            if (IsPrivateConnection)
            {
                PhotonNetwork.ConnectUsingSettings(PLNetworkManager.Instance.VersionString);
            }
            else
            {
                PhotonNetwork.ConnectToRegion(PLRegionSelect.GetRegionCode(PLXMLOptionsIO.Instance.CurrentOptions.GetStringValue("PhotonRegion")), PLNetworkManager.Instance.VersionString, null);
            }
            yield break;
        }

        static readonly string AppID = "6246645a-91a6-4580-b5bd-64d6b8fc6875";
        public static SaveValue<string> IP = new SaveValue<string>("SavedIP", "127.0.0.1");
        public static SaveValue<int> Port = new SaveValue<int>("SavedPort", 5055); 
        public static SaveValue<bool> AlwaysConnectToPrivate = new SaveValue<bool>("AlwaysConnectToPrivate", false);

        private static bool PrivateConnection = false;
        private static bool UdpPortConfigurationPublic = false;
        private static bool FirstConnection = true;

        public static bool IsPrivateConnection
        {
            get
            {
                return PrivateConnection;
            }
            set
            {
                if (FirstConnection)
                {
                    UdpPortConfigurationPublic = PhotonNetwork.UseAlternativeUdpPorts;
                    FirstConnection = false;
                }
                if (value)
                {
                    PhotonNetwork.UseAlternativeUdpPorts = false;
                    PrivateConnection = true;
                    PhotonNetwork.PhotonServerSettings.UseMyServer(IP, Port, AppID);
                }
                else
                {
                    PhotonNetwork.UseAlternativeUdpPorts = UdpPortConfigurationPublic;
                    PrivateConnection = false;
                    PhotonNetwork.PhotonServerSettings.UseCloud(AppID);
                }
            }
        }

        public static void Connect()
        {
            PLGlobal.Instance.StartCoroutine(SafeConnect());
        }

        static void DebugOutput(bool EnablePhotonStats)
        {
            ServerSettings settings = PhotonNetwork.PhotonServerSettings;
            Logger.Info($"AppID: {settings.AppID} Address: {settings.ServerAddress}:{settings.ServerPort}, Protocol {settings.Protocol}, HostType: {settings.HostType}, NetworkLogging: {settings.NetworkLogging.ToString()}, PUNLogging: {settings.PunLogging.ToString()}, PreferedRegion: {settings.PreferredRegion}");
            
            if (EnablePhotonStats)
            {
                settings.EnableLobbyStatistics = true;
                settings.NetworkLogging = DebugLevel.ALL;
                settings.PunLogging = PhotonLogLevel.Full;
            }
            else
            {
                settings.EnableLobbyStatistics = false;
                settings.NetworkLogging = DebugLevel.ERROR;
                settings.PunLogging = PhotonLogLevel.ErrorsOnly;

            }
        }
    }
}
