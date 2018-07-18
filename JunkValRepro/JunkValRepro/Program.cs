using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using ERND.Framework.Shared.Cache;
using NLog;
using ServiceStack.Redis;
using Timer = System.Timers.Timer;

namespace JunkValRepro
{
    class Program
    {
        private const string statusHash = "statusHash";

        private const string locationHash = "locationHash";

        private const string ststusval =
                "<ExternalDeviceDetails i:type=\"VmsDeviceStaus\" xmlns=\"http://schemas.datacontract.org/2004/07/ESM.Shared.Device.Common\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\"><ExternalDeviceFunctionalStatus>WORKINGPROPERLY</ExternalDeviceFunctionalStatus><ExternalDeviceOperationalStatus>ON</ExternalDeviceOperationalStatus><VmsStreamingStatus>Recording</VmsStreamingStatus></ExternalDeviceDetails>"
            ;

        private const string locationval =
                "{\"DeviceId\":\"752\",\"PhysicalId\":\"378e5e3a-ebb7-4d64-bdc3-c5ecad63f105\",\"Azimuth\":280.513,\"Tilt\":5.535,\"Zoom\":1}"
            ;
        //private static RedisManager m_manager = new RedisManager();
        private static Timer timer1 = new Timer(2000);//Read Write
        private static Timer readTimer = new Timer(2000);
        private static Timer writeTimer = new Timer(2000);
        private static Timer timer11 = new Timer(2000);//Read Write

        private static Timer timer2 = new Timer(2000);//Read Write
        private static Timer readTimer2 = new Timer(2000);
        private static Timer writeTimer2 = new Timer(2000);
        private static Timer timer22 = new Timer(2000);//Read Write

        private static NLog.Logger m_log = LogManager.GetCurrentClassLogger();
        static void Main(string[] args)
        {
            var th = new System.Threading.Thread(async () =>
            {
                while (true)
                {
                    try
                    {
                        onHandleTimer1(null,null);
                        await Task.Delay(TimeSpan.FromSeconds(2));
                    }
                    catch (Exception ex)
                    {
                       
                    }
                }
            });

            th.Start();

            var th1 = new System.Threading.Thread(async () =>
            {
                while (true)
                {
                    try
                    {
                        onHandleTimer11(null, null);
                        await Task.Delay(TimeSpan.FromSeconds(2));
                    }
                    catch (Exception ex)
                    {

                    }
                }
            });

            th1.Start();


            var th2 = new System.Threading.Thread(async () =>
            {
                while (true)
                {
                    try
                    {
                        onHandleReadTimer(null, null);
                        await Task.Delay(TimeSpan.FromSeconds(2));
                    }
                    catch (Exception ex)
                    {

                    }
                }
            });

            th2.Start();

            var th3 = new System.Threading.Thread(async () =>
            {
                while (true)
                {
                    try
                    {
                        onHandleWriteTimer(null, null);
                        await Task.Delay(TimeSpan.FromSeconds(2));
                    }
                    catch (Exception ex)
                    {

                    }
                }
            });

            th3.Start();

            var th4 = new System.Threading.Thread(async () =>
            {
                while (true)
                {
                    try
                    {
                        onHandleTimer2(null, null);
                        await Task.Delay(TimeSpan.FromSeconds(2));
                    }
                    catch (Exception ex)
                    {

                    }
                }
            });

            th4.Start();

            var th5 = new System.Threading.Thread(async () =>
            {
                while (true)
                {
                    try
                    {
                        onHandleReadTimer2(null, null);
                        await Task.Delay(TimeSpan.FromSeconds(2));
                    }
                    catch (Exception ex)
                    {

                    }
                }
            });

            th5.Start();

            var th6 = new System.Threading.Thread(async () =>
            {
                while (true)
                {
                    try
                    {
                        onHandleWriteTimer2(null, null);
                        await Task.Delay(TimeSpan.FromSeconds(2));
                    }
                    catch (Exception ex)
                    {

                    }
                }
            });

            th6.Start();

            var th7 = new System.Threading.Thread(async () =>
            {
                while (true)
                {
                    try
                    {
                        onHandleTimer22(null, null);
                        await Task.Delay(TimeSpan.FromSeconds(2));
                    }
                    catch (Exception ex)
                    {

                    }
                }
            });

            th7.Start();

            //timer1.Elapsed += onHandleTimer1;
            //timer1.Start();

            //readTimer.Elapsed += onHandleReadTimer;
            //readTimer.Start();

            //writeTimer.Elapsed += onHandleWriteTimer;
            //writeTimer.Start();

            //timer11.Elapsed += onHandleTimer11;
            //timer11.Start();

            //timer2.Elapsed += onHandleTimer2;
            //timer2.Start();

            //readTimer2.Elapsed += onHandleReadTimer2;
            //readTimer2.Start();

            //writeTimer2.Elapsed += onHandleWriteTimer2;
            //writeTimer2.Start();

            //timer22.Elapsed += onHandleTimer22;
            //timer22.Start();

            Console.WriteLine("Press any key to exit");
            Console.ReadLine();
            //timer1.Stop();
            //timer1.Dispose();

            //readTimer.Stop();
            //readTimer.Dispose();

            //writeTimer.Stop();
            //writeTimer.Dispose();

            //timer2.Stop();
            //timer2.Dispose();

            //readTimer2.Stop();
            //readTimer2.Dispose();

            //writeTimer2.Stop();
            //writeTimer2.Dispose();

            //timer11.Stop();
            //timer11.Dispose();

            //timer22.Stop();
            //timer22.Dispose();
        }

        private static void onHandleTimer1(Object source,ElapsedEventArgs e)
        {
            CacheManager m_manager = new CacheManager();

            var val = m_manager.GetValueFromHash<string>(statusHash, "1001"); //"<Status><Enable>1</Enable></Status>"
            m_log.Debug($"{MethodBase.GetCurrentMethod().Name} Setting entry in {statusHash}, Key: 1001, value: {val}");

            if (val != null)
            {
                m_manager.SetEntryInHash<string>(statusHash, "1001", val);
                m_log.Debug(
                    $"{MethodBase.GetCurrentMethod().Name} Setting entry in {statusHash}, Key: 1001, value: {val}");
            }
            else
            {
                m_manager.SetEntryInHash<string>(statusHash, "1001", ststusval);
                m_log.Debug(
                    $"{MethodBase.GetCurrentMethod().Name} Setting entry in {statusHash}, Key: 1001, value: {ststusval}");

            }

        }

        private static void onHandleTimer11(Object source, ElapsedEventArgs e)
        {
            CacheManager m_manager = new CacheManager();

            var val = m_manager.GetValueFromHash<string>(statusHash, "1002"); //"<Status><Enable>1</Enable></Status>"
            m_log.Debug($"{MethodBase.GetCurrentMethod().Name} Setting entry in {statusHash}, Key: 1002, value: {val}");

            if (val != null)
            {
                m_manager.SetEntryInHash<string>(statusHash, "1002", val);
                m_log.Debug(
                    $"{MethodBase.GetCurrentMethod().Name} Setting entry in {statusHash}, Key: 1002, value: {val}");
            }
            else
            {
                m_manager.SetEntryInHash<string>(statusHash, "1002", ststusval);
                m_log.Debug(
                    $"{MethodBase.GetCurrentMethod().Name} Setting entry in {statusHash}, Key: 1002, value: {ststusval}");

            }

            var val2 = m_manager.GetValueFromHash<string>(statusHash, "1002"); //"<Status><Enable>1</Enable></Status>"
            m_log.Debug($"{MethodBase.GetCurrentMethod().Name} Setting entry in {statusHash}, Key: 1002, value: {val2}");

            if (val != null)
            {
                m_manager.SetEntryInHash<string>(statusHash, "1002", val2);
                m_log.Debug(
                    $"{MethodBase.GetCurrentMethod().Name} Setting entry in {statusHash}, Key: 1002, value: {val2}");
            }
            else
            {
                m_manager.SetEntryInHash<string>(statusHash, "1002", ststusval);
                m_log.Debug(
                    $"{MethodBase.GetCurrentMethod().Name} Setting entry in {statusHash}, Key: 1002, value: {ststusval}");

            }

            var val3 = m_manager.GetValueFromHash<string>(statusHash, "1003"); //"<Status><Enable>1</Enable></Status>"
            m_log.Debug($"{MethodBase.GetCurrentMethod().Name} Setting entry in {statusHash}, Key: 1002, value: {val3}");
            if (val3 != null)
            {
                m_manager.SetEntryInHash<string>(statusHash, "1003", val3);
            }
        }
        private static void onHandleReadTimer(Object source, ElapsedEventArgs e)
        {
            CacheManager m_manager = new CacheManager();
            var val = m_manager.GetValueFromHash<string>(statusHash, "1001"); //"<Status><Enable>1</Enable></Status>"
            m_log.Debug($"{MethodBase.GetCurrentMethod().Name} Setting entry in {statusHash}, Key: 1001, value: {val}");

        }
        private static void onHandleWriteTimer(Object source, ElapsedEventArgs e)
        {
            CacheManager m_manager = new CacheManager();
            m_manager.SetEntryInHash<string>(statusHash, "1001", ststusval);
            m_log.Debug($"{MethodBase.GetCurrentMethod().Name} Setting entry in {statusHash}, Key: 1001, value: <Status><Enable>1</Enable></Status>");

        }

        /// <summary>
        /// /////////////////////////////////////////////////////////////////////////////////////////
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private static void onHandleTimer2(Object source, ElapsedEventArgs e)
        {
            CacheManager m_manager = new CacheManager();

            var val = m_manager.GetValueFromHash<string>(locationHash, "378e5e3a-ebb7-4d64-bdc3-c5ecad63f105"); //"<Status><Enable>1</Enable></Status>"
            m_log.Debug($"{MethodBase.GetCurrentMethod().Name} Setting entry in {locationHash}, Key: 1001, value: {val}");

            if (val != null)
            {
                m_manager.SetEntryInHash<string>(locationHash, "378e5e3a-ebb7-4d64-bdc3-c5ecad63f105", val);
                m_log.Debug(
                    $"{MethodBase.GetCurrentMethod().Name} Setting entry in {locationHash}, Key: 1001, value: <Status><Enable>1</Enable></Status>");
            }
            else
            {
                m_manager.SetEntryInHash<string>(locationHash, "378e5e3a-ebb7-4d64-bdc3-c5ecad63f105", locationval);
                m_log.Debug(
                    $"{MethodBase.GetCurrentMethod().Name} Setting entry in {locationHash}, Key: 1001, value: <Status><Enable>1</Enable></Status>");

            }
        }

        private static void onHandleReadTimer2(Object source, ElapsedEventArgs e)
        {
            CacheManager m_manager = new CacheManager();
            var val = m_manager.GetValueFromHash<string>(locationHash, "378e5e3a-ebb7-4d64-bdc3-c5ecad63f105"); //"<Status><Enable>1</Enable></Status>"
            m_log.Debug($"{MethodBase.GetCurrentMethod().Name} Setting entry in {locationHash}, Key: 1001, value: {val}");

        }

        private static void onHandleWriteTimer2(Object source, ElapsedEventArgs e)
        {
            CacheManager m_manager = new CacheManager();
            m_manager.SetEntryInHash<string>(locationHash, "378e5e3a-ebb7-4d64-bdc3-c5ecad63f105", locationval);
            m_log.Debug($"{MethodBase.GetCurrentMethod().Name} Setting entry in {locationHash}, Key: 1001, value: <Status><Enable>1</Enable></Status>");

        }

        private static void onHandleTimer22(Object source, ElapsedEventArgs e)
        {
            CacheManager m_manager = new CacheManager();

            var val = m_manager.GetValueFromHash<string>(locationHash, "378e5e3a-ebb7-4d64-bdc3-c5ecad63f106"); //"<Status><Enable>1</Enable></Status>"
            m_log.Debug($"{MethodBase.GetCurrentMethod().Name} Setting entry in {locationHash}, Key: 1001, value: {val}");

            if (val != null)
            {
                m_manager.SetEntryInHash<string>(locationHash, "378e5e3a-ebb7-4d64-bdc3-c5ecad63f106", val);
                m_log.Debug(
                    $"{MethodBase.GetCurrentMethod().Name} Setting entry in {locationHash}, Key: 1001, value: {val}");
            }
            else
            {
                m_manager.SetEntryInHash<string>(locationHash, "378e5e3a-ebb7-4d64-bdc3-c5ecad63f106", locationval);
                m_log.Debug(
                    $"{MethodBase.GetCurrentMethod().Name} Setting entry in {locationHash}, Key: 1001, value: {locationval}");

            }

            var val2 = m_manager.GetValueFromHash<string>(locationHash, "378e5e3a-ebb7-4d64-bdc3-c5ecad63f107"); //"<Status><Enable>1</Enable></Status>"
            m_log.Debug($"{MethodBase.GetCurrentMethod().Name} Setting entry in {locationHash}, Key: 1001, value: {val2}");
            if (val2 != null)
            {
                m_manager.SetEntryInHash<string>(locationHash, "378e5e3a-ebb7-4d64-bdc3-c5ecad63f107", val2);
            }
        }
    }
}
