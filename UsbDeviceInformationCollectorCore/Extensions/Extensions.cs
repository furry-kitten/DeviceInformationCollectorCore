using System;
using System.Threading;
using Newtonsoft.Json;
using NLog;

namespace UsbDeviceInformationCollectorCore.Extensions
{
    internal static class Extensions
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        internal static string ToJson<T>(this T obj) => JsonConvert.SerializeObject(obj);

        internal static T ExecInReadLock<T>(this ReaderWriterLockSlim locker, Func<T> func)
        {
            locker.EnterReadLock();
            try
            {
                return func();
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return default;
            }
            finally
            {
                locker.ExitReadLock();
            }
        }

        internal static T ExecInWriteLock<T>(this ReaderWriterLockSlim locker, Func<T> func)
        {
            locker.EnterWriteLock();
            try
            {
                return func();
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return default;
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        internal static void ExecInReadLock(this ReaderWriterLockSlim locker, Action act)
        {
            locker.EnterReadLock();
            try
            {
                act();
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            finally
            {
                locker.ExitReadLock();
            }
        }

        internal static void ExecInWriteLock(this ReaderWriterLockSlim locker, Action act)
        {
            locker.EnterWriteLock();
            try
            {
                act();
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }
    }
}