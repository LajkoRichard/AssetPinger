using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace AssetPinger
{
    class Program
    {
        static public Timer timer = new Timer();
        static public AssetDBContext Context = new AssetDBContext();
        static public Object LockObject = new Object();
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            timer.Elapsed += new ElapsedEventHandler(StartEvent);
            //timer.Interval = 5 * 60 * 1000; //number in milliseconds  
            timer.Interval = TimeSpan.FromMinutes(5).TotalMilliseconds;
            timer.Enabled = true;
            Console.ReadKey();
        }

        static private void StartEvent(object sender, ElapsedEventArgs e)
        {
            Logger.Info("Pinging started!");
            StartPing();
            Logger.Info("Pinging finished!");
        }

        static public async void StartPing()
        {
            List<Assets> ActiveAssets = new List<Assets>(Context.Assets.Where(item => item.IsArchive == false).ToList());
            var tasks = new List<Task>();

            for (int i = 0; i < ActiveAssets.Count(); i++)
            {
                Ping p = new Ping();
                var task = PingAndUpdateAsync(p, ActiveAssets[i]);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks).ContinueWith(t => 
            {
                ActiveAssets.Clear();
            });
        }

        static private async Task PingAndUpdateAsync(Ping p, Assets asset)
        {
            try
            {
                var reply = await p.SendPingAsync(asset.Ip, 100);

                if (reply.Status == IPStatus.Success)
                {
                    lock (LockObject)
                    {
                        asset.IsActive = true;
                        asset.LastActiveTime = DateTime.Now;
                        Context.SaveChanges();
                    }
                }
                else
                {
                    lock (LockObject)
                    {
                        asset.IsActive = false;
                        Context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
        }
    }
}
