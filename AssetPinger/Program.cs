using Microsoft.EntityFrameworkCore;
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
            try
            {
                List<Assets> ActiveAssets = new List<Assets>(Context.Assets.Where(item => item.IsArchive == false).ToList());
                var tasks = new List<Task>();

                for (int i = 0; i < ActiveAssets.Count(); i++)
                {
                    if (string.IsNullOrWhiteSpace(ActiveAssets[i].Ip) || !ActiveAssets[i].Ip.StartsWith("106.114."))
                    {
                        continue;
                    }

                    string IPAddressOfCurrentAsset = ActiveAssets[i].Ip ?? "";
                    var parts = IPAddressOfCurrentAsset.Split('.');
                    bool isValidIPAddress = parts.Length == 4 && !parts.Any(
                       x =>
                       {
                           return Int32.TryParse(x, out int y) && y > 255 || y < 1;
                       });
                    if (!isValidIPAddress)
                    {
                        continue;
                    }

                    Ping p = new Ping();
                    var task = PingAndUpdateAsync(p, ActiveAssets[i]);
                    tasks.Add(task);
                }

                await Task.WhenAll(tasks).ContinueWith(t =>
                {
                    ActiveAssets.Clear();
                });
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine(ex.Message);
                Logger.Error(ex.Message, "NullReferenceException");
                throw new Exception(ex.Message);
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine(ex.Message);
                Logger.Error(ex.Message, "DbUpdateException");
                throw new Exception(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Logger.Error(ex.Message, "Exception");
                throw new Exception(ex.Message);
            }
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
            catch (NullReferenceException ex)
            {
                Console.WriteLine(ex.Message);
                Logger.Error(ex.Message, "NullReferenceException");
                throw new Exception(ex.Message);
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine(ex.Message);
                Logger.Error(ex.Message, "DbUpdateException");
                throw new Exception(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Logger.Error(ex.Message, "Exception");
                throw new Exception(ex.Message);
            }
        }
    }
}
