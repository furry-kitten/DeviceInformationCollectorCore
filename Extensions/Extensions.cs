using System;
using System.Threading;
using Newtonsoft.Json;

namespace UsbDeviceInformationCollectorCore.Extensions
{
    internal static class Extensions
    {
        internal static string ToJson<T>(this T obj) => JsonConvert.SerializeObject(obj);

        internal static T ExecInReadLock<T>(this ReaderWriterLockSlim locker, Func<T> func)
        {
            locker.EnterReadLock();
            try
            {
                return func();
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
            finally
            {
                locker.ExitWriteLock();
            }
        }
    }
}