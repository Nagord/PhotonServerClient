using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PhotonServerClient
{
    internal static class HostResolverStateMachine
    {
        internal static bool ResolveHost(string hostAddress, out IPAddress address)
        {
            if (workingTask == null || workingTask.IsCompleted)
            {
                workingTask = InternalResolveHost(hostAddress);
            }
            lock (_sync)
            {
                address = _LastResolveHostIp;
                return _LastResult;
            }
        }
        private static readonly object _sync = new object();
        private static bool _LastResult;
        private static bool LastResult
        {
            get
            {
                lock (_sync)
                {
                    return _LastResult;
                }
            }
            set
            {
                lock (_sync)
                {
                    _LastResult = value;
                }
            }
        }
        private static IPAddress _LastResolveHostIp = null;
        private static IPAddress LastResolveHostIp
        {
            get
            {
                lock (_sync)
                {
                    return _LastResolveHostIp;
                }
            }
            set
            {
                lock (_sync)
                {
                    _LastResolveHostIp = value;
                }
            }
        }
        private static Task workingTask;
        private static async Task InternalResolveHost(string hostAddress)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            IPAddress[] iPAddresses = new IPAddress[0];
            try
            {
                iPAddresses = await Dns.GetHostAddressesAsync(hostAddress);
            }
            catch
            {
                try
                {
                    IPHostEntry hostByName = await Dns.GetHostEntryAsync(hostAddress);
                    iPAddresses = hostByName.AddressList;
                }
                catch
                {
                    //UnityEngine.Debug.Log($"Error resolving Ip Address found for {hostAddress}");
                    LastResult = false;
                    await Task.Delay(Math.Max(0, 500 - (int)watch.ElapsedMilliseconds));
                    return;
                }
            }
            Array.Sort<IPAddress>(iPAddresses, new Comparison<IPAddress>(AddressSortComparer));
            if (iPAddresses.Length > 0)
            {
                LastResolveHostIp = iPAddresses[0];
                LastResult = true;
                await Task.Delay(Math.Max(0, 500 - (int)watch.ElapsedMilliseconds));
                return;
            }
            LastResult = false;
            await Task.Delay(Math.Max(0, 500 - (int)watch.ElapsedMilliseconds));
            return;
        }
        private static int AddressSortComparer(IPAddress x, IPAddress y)
        {
            bool flag = x.AddressFamily == y.AddressFamily;
            int num;
            if (flag)
            {
                num = 0;
            }
            else
            {
                num = ((x.AddressFamily == AddressFamily.InterNetworkV6) ? (-1) : 1);
            }
            return num;
        }
    }
}
