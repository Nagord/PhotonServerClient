using HarmonyLib;
using PulsarModLoader.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Web.Caching;
using static PulsarModLoader.Patches.HarmonyHelpers;

namespace PhotonServerClient
{
    [HarmonyPatch(typeof(NetworkingPeer),nameof(NetworkingPeer.OnOperationResponse))]
    internal class GameServerAddressPatch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var targetSequence = new CodeMatch[]
            {
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldarg_1),
                new CodeMatch(OpCodes.Ldc_I4, 230),
                new CodeMatch(OpCodes.Callvirt),
                new CodeMatch(OpCodes.Castclass),
                new CodeMatch(OpCodes.Call, AccessTools.PropertySetter(typeof(NetworkingPeer), nameof(NetworkingPeer.GameServerAddress)))
            };
            var AddedSequence = new CodeInstruction[]
            {
                new CodeInstruction (OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GameServerAddressPatch), nameof(GameServerAddressPatch.PatchMethod)))
            };

            CodeMatcher match = new CodeMatcher(instructions);
            for (int i= 0; i < 3; i++)
            {
                UnityEngine.Debug.Log($"Inserting instance {i}");
                match.MatchEndForward(targetSequence).Advance(1).Insert(AddedSequence);
            }
            return match.Instructions();
            //List<CodeInstruction> list = instructions.ToList();
            //CodeInstruction codeInstruction = targetSequence.ElementAt(0);
            //int num = targetSequence.Count();
            //for (int i = 0; i < list.Count; i++)
            //{
            //    if (i + num <= list.Count)
            //    {
            //        bool flag = true;
            //        for (int j = 0; j < num && flag; j++)
            //        {
            //            flag = list[i + j].opcode.Equals(targetSequence.ElementAt(j).opcode);
            //            flag = flag && ((targetSequence.ElementAt(j).operand != null && list[i + j].operand.Equals(targetSequence.ElementAt(j).operand)) || targetSequence.ElementAt(j).operand == null || list[i+j].operand == null);

            //            if (flag)
            //            {
            //                UnityEngine.Debug.Log($"Found {targetSequence.ElementAt(j).opcode} at {i + j}");
            //            }

            //        }
            //        if (!flag)
            //        {
            //            continue;
            //        }
            //        int index = i + num;
            //        list.InsertRange(index, AddedSequence.Select((CodeInstruction c) => c.FullClone()));

            //    }
            //}
            //return list;
        }
#if DEBUG
        static void Postfix()
        {
            UnityEngine.Debug.Log("PostFix Check");
            UnityEngine.Debug.Log($"Game Server Stored address:{PhotonNetwork.networkingPeer.GameServerAddress}");
        }
#endif
        static void PatchMethod(ExitGames.Client.Photon.OperationResponse response)
        {
#if DEBUG
            UnityEngine.Debug.Log($"Operation Code: {response.OperationCode}");
            UnityEngine.Debug.Log($"Server Address: {PhotonNetwork.PhotonServerSettings.ServerAddress}");
#endif

            if (ClientInterface.IsPrivateConnection)
            {
                string ipResponse = (string)response[230];
#if DEBUG
                UnityEngine.Debug.Log($"Game Server Response address:{ipResponse}");
#endif
                string[] ipPortSplit = ipResponse.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                //It forces the Client interface set ip instead of using the servers responded ip as the response ip can be incorrect because this ancienct server version is questionable at times
                string selectedIp = ClientInterface.IP.Value + ":";
                if (ipPortSplit.Length > 1)
                {
                    //Length - 1 instead of 1 because this accounts for potential ipv4 and ipv6 address usage and ipv6 divides using : instead of .
                    selectedIp += ipPortSplit[ipPortSplit.Length - 1];
                }
                else
                {
                    selectedIp += "5056";
                }
                
                PhotonNetwork.networkingPeer.GameServerAddress = selectedIp;
#if DEBUG
                UnityEngine.Debug.Log($"Game Server Set address:{selectedIp}");
                UnityEngine.Debug.Log($"Game Server Stored address:{PhotonNetwork.networkingPeer.GameServerAddress}");
#endif
            }
        }
    }
}
