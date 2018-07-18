#region FileInfo

// *****************************************************************************************************
// Author : ERND
// Copyright (c) Etimad Research & Development LLC
// Description and Purpose : 
// *****************************************************************************************************

#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using ServiceStack;
using ServiceStack.Redis;
using ServiceStack.Redis.Generic;
using Cache.Interface;

namespace Adapters.Cache.Redis
{
    /// <summary>
    ///     This class implements the actual REDIS interaction thru ServiceStack.Redis.DLL component.
    ///     The implementation of REDIS is with REDIS and SENTINEL in paired way. One redis should have
    ///     one sentinel installation. Sentinel deals with the failover of REDIS. It automatically brings up one of the
    ///     slave to master.
    ///     This class is getting called from CacheManager class.
    ///     REDIS support three types of data types
    ///     String: Its a sequence of strings
    ///     List: Lists of strings
    ///     Hashes: Collection of key value pairs
    ///     The purpose of this class is:
    ///     It is an abstraction of In memory Cache implementation
    ///     The component which uses RedisAdapter need not be aware of which in-memory technology it uses
    /// </summary>
    [Export("REDIS", typeof(ICacheSentinelProvider))]
    public class RedisSentinelAdapter : ICacheSentinelProvider
    {
        #region Private Elements

        

        private string m_masterName;
        private string[] m_connectionString;

        private IRedisClientsManager m_redisManager;
        private object sm_synRoot = new object();

        private int m_retryCount;
        private int m_retrySeconds;
        private TimeSpan refreshSentinelHostAfter;

        private void GetProviderconfiguration()
        {
           
            m_masterName = "ESMmaster";

            if (m_masterName == null)
            {
                throw new Exception("ERNDCacheManagement mastername is null");
            }

            //It can have multiple sentinels configured. Put comma to seperate the sentinel configuration
            string constring = "10.1.5.45:16380,10.1.5.63:16380,10.1.5.64:16380";
            m_connectionString = constring.Split(',').ToArray();

            m_retryCount = 3;
            m_retrySeconds = 100;
            refreshSentinelHostAfter = TimeSpan.FromSeconds(1);

            if (m_connectionString == null)
            {
                throw new Exception("ERNDCacheManagement cache URL is null");
            }
        }

        #endregion

        #region Public Elements

        /// <summary>
        ///     when creating an object of RedisDataSource, this function is getting called.
        ///     It reads the [master name],[Sentinel configuration] from application config file.
        ///     Eg:-Master name : ERNDMaster
        ///     RedisConfig : 10.1.7.40:16381, 10.1.7.41:16381 [You can specify multiple sentinel configuration]
        /// </summary>
        public RedisSentinelAdapter()
        {
            //Licensing.RegisterLicense(
            //    "4918-e1JlZjo0OTE4LE5hbWU6RXRpbWFkIFImRCxUeXBlOkJ1c2luZXNzLEhhc2g6WmpodExKajdxZHB1eGE1Y3M4RW9lQkQwR21XVENjbitHTWxDZ1NYMVFwR2RmS2UxQm8rMXM2VHFzaUNCYXZNbTh6TEhLMEdWeG9TcFhBWWUvWmlEZmNOcEszU1FVazVoWUxjcDNwaS83YTN1VTVvOXpCWVJNNlJ3TVlzdnZGZ0oxSGd6OWhXaytselo3cG1TNGtOckIwd3dNcGh4dlBxMVhIU1dJUDZlYUE4PSxFeHBpcnk6MjAxOC0wNC0yNX0=");
            Licensing.RegisterLicense(
                "6238-e1JlZjo2MjM4LE5hbWU6RXRpbWFkIFImRCxUeXBlOkJ1c2luZXNzLEhhc2g6dU40dzNpaVZEZm1LVUQwMFNzOS9TTU5rcVZ1UmNzQjZIci80NU9Ma0ZhdEFSM0ZmWE5sbFJMcUh5cGVHZEc3Rkw5cGdJVWxDcmdzWTg5V1VLMmw2MU1DaWRuQmNnaWpxelNRR0tnSXBBUUdmY01LU0FzN0ZYeXkzc3RFcnNiNXFsaTBMOENpQlJ2aURDZmZ2NWZHaVFjUVlsOCtwalBValBlZCtsZGt4V2pBPSxFeHBpcnk6MjAxOS0wNC0yNn0=");

            GetProviderconfiguration();
            Initialize(m_connectionString, m_masterName);
        }

        /// <summary>
        ///     Initialize Redis connection
        ///     m_sentinelclient.GetMaster() gives the current master node name.
        /// </summary>
        /// <param name="connectionString">Redis connection string</param>
        /// <param name="mastername">Redis master node name</param>
        private void Initialize(string[] connectionString, string mastername)
        {
            try
            {
                RetryHelper.RetryOnException(m_retryCount, TimeSpan.FromSeconds(m_retrySeconds),
                                    () => { InitializeSentinel(connectionString, mastername); });
            }
            catch (Exception ex)
            {
                
                throw;
            }
        }

        private void InitializeSentinel(string[] connectionString, string mastername)
        {

            if (m_redisManager == null)
            {
                lock (sm_synRoot)
                {
                    if (m_redisManager == null)
                    {
                        var sentinel = new RedisSentinel(connectionString, mastername)
                        {
                            RefreshSentinelHostsAfter = refreshSentinelHostAfter
                        };
                        
                        sentinel.RedisManagerFactory +=
                            (master, slaves) => new RedisManagerPool(master);
                        m_redisManager = sentinel.Start();
                    }
                }
            }


        }

        /// <summary>
        ///     When a thread reads a key it can lock the key.
        ///     Acquirelock returns a IDisposable object. Lock clears when thread disposes IDisposable object.
        ///     Timeout specifies, how long to wait if the lock acquired by any ther thread.
        ///     timeout in seconds
        /// </summary>
        /// <param name="key">The unique cache key which hold the value</param>
        /// <param name="timeout">Timeout in seconds for acquiring the lock</param>
        /// <returns>Returns IDisposable object</returns>
        public IDisposable Acquirelock(string key, TimeSpan timeout)
        {
            IDisposable Lock = null;
            try
            {
                RetryHelper.RetryOnException(m_retryCount, TimeSpan.FromSeconds(m_retrySeconds),
                    () => { Lock = AcquirelockExtn(key, timeout); });
            }
            catch (Exception redisException)
            {
                
                throw;
            }

            return Lock;
        }

        private IDisposable AcquirelockExtn(string key, TimeSpan timeout)
        {
            IDisposable Lock;

            using (var client = GetClient(true))
            {
                Lock = client.AcquireLock(key + ".Lock", timeout);
            }

            return Lock;
        }

        /// <summary>
        ///     When a thread reads a key it can lock the key.
        ///     Acquirelock returns a IDisposable object. Lock clears when thread disposes IDisposable object.
        ///     Thread should wait indefinitly until the other thread releases the lock.
        /// </summary>
        /// <param name="key">The unique cache key which hold the value</param>
        /// <returns>Returns IDisposable object</returns>
        public IDisposable Acquirelock(string key)
        {
            IDisposable Lock = null;
            try
            {
                RetryHelper.RetryOnException(m_retryCount, TimeSpan.FromSeconds(m_retrySeconds),
                    () => { Lock = AcquirelockExtn(key); });
            }
            catch (Exception redisException)
            {
                

                throw;
            }

            return Lock;
        }

        private IDisposable AcquirelockExtn(string key)
        {
            IDisposable Lock;

            using (var client = GetClient(true))
            {
                Lock = client.AcquireLock(key + ".Lock");
            }

            return Lock;
        }

        /// <summary>
        ///     Returns all the keys as List of strings
        /// </summary>
        /// <returns>Returns List of keys</returns>
        public List<string> GetAllkeys()
        {
            List<string> keys = null;
            try
            {
                RetryHelper.RetryOnException(m_retryCount, TimeSpan.FromSeconds(m_retrySeconds),
                    () => { keys = GetAllkeysExtn(); });
            }
            catch (Exception redisException)
            {
                
                throw;
            }

            return keys;
        }

        private List<string> GetAllkeysExtn()
        {
            List<string> keys;

            using (var client = GetClient(false))
            {
                keys = client.GetAllKeys();
            }

            return keys;
        }

        /// <summary>
        ///     Returns the existing value of the unique key and set the new value.
        /// </summary>
        /// <param name="key">The unique cache key which hold the value</param>
        /// <param name="value">New value to be set</param>
        /// <returns>Returns string value</returns>
        public string GetAndSetValue(string key, string value)
        {
            var strReturn = string.Empty;
            try
            {
                RetryHelper.RetryOnException(m_retryCount, TimeSpan.FromSeconds(m_retrySeconds),
                    () => { strReturn = GetAndSetValueExtn(key, value); });
            }
            catch (Exception redisException)
            {
                

                throw;
            }

            return strReturn;
        }

        private string GetAndSetValueExtn(string key, string value)
        {
            string strReturn;

            using (var client = GetClient(true))
            {
                strReturn = client.GetAndSetValue(key, value);
            }

            return strReturn;
        }

        /// <summary>
        ///     Given a unique key, return the value
        /// </summary>
        /// <param name="key">The unique cache key which hold the value</param>
        /// <returns>Returns string value</returns>
        public string GetValue(string key)
        {
            var strReturn = string.Empty;
            try
            {
                RetryHelper.RetryOnException(m_retryCount, TimeSpan.FromSeconds(m_retrySeconds),
                    () => { strReturn = GetValueExtn(key); });
            }
            catch (Exception redisException)
            {
                

                throw;
            }

            return strReturn;
        }

        private string GetValueExtn(string key)
        {
            string strReturn;

            using (var client = GetClient(false))
            {
                strReturn = client.GetValue(key);
            }

            return strReturn;
        }

        /// <summary>
        ///     Given the hash ID and key, return the value
        ///     hash ID is the unique name in the system. Its like table name in the database.
        /// </summary>
        /// <param name="hashId">Unique hash ID</param>
        /// <param name="key">The unique cache key which hold the value in the specified hashId</param>
        /// <returns>Returns string value</returns>
        public string GetValueFromHash(string hashId, string key)
        {
            var strReturn = string.Empty;
            try
            {
                RetryHelper.RetryOnException(m_retryCount, TimeSpan.FromSeconds(m_retrySeconds),
                    () => { strReturn = GetValueFromHashExtn(hashId, key); });
            }
            catch (Exception redisException)
            {
                

                throw;
            }

            return strReturn;
        }

        private string GetValueFromHashExtn(string hashId, string key)
        {
            string strReturn;

            using (var client = GetClient(false))
            {
                strReturn = client.GetValueFromHash(hashId, key);
            }

            return strReturn;
        }

        /// <summary>
        ///     Generic implementation of getting value from hash
        ///     given hash ID and key, returns an object of type T
        ///     hash ID is the unique name in the system. Its like table name in the database.
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="hashid">Unique hash ID</param>
        /// <param name="key">The unique cache key which hold the value in the specified hashid</param>
        /// <returns>Returns value as T</returns>
        public T GetValueFromHash<T>(string hashid, string key)
        {
            var value = default(T);
            try
            {
                IRedisTypedClient<T> redis = null;
                RetryHelper.RetryOnException(m_retryCount, TimeSpan.FromSeconds(m_retrySeconds),
                    () => { value = GetValueFromHashExtn<T>(hashid, key); });
            }
            catch (Exception redisException)
            {
                
                throw;
            }

            return value;
        }

        private T GetValueFromHashExtn<T>(string hashid, string key)
        {
            var result = default(T);
            using (var client = GetClient(false))
            {
                var redis = client.As<T>();
                var hash = redis.GetHash<string>(hashid);
                result = hash[key];

                }
 
            return result;
        }

        /// <summary>
        ///     Generic implementation of getting value from hash
        ///     given key, returns an object of type T
        /// </summary>
        /// <typeparam name="T">Return value as T</typeparam>
        /// <param name="key">Key to set value in hash</param>
        /// <returns></returns>
        public T GetValueFromHash<T>(string key)
        {
            var value = default(T);
            var typeName = typeof(T).Name;
            try
            {
                RetryHelper.RetryOnException(m_retryCount, TimeSpan.FromSeconds(m_retrySeconds),
                    () => { value = GetValueFromHashExtn<T>(key); });
            }
            catch (Exception redisException)
            {
                

                throw;
            }

            return value;
        }

        private T GetValueFromHashExtn<T>(string key)
        {
            IRedisHash<string, T> hash = null;
            var typeName = typeof(T).Name;

            IRedisTypedClient<T> redis;
            using (var client = GetClient(false))
            {
                redis = client.As<T>();
            }
            hash = redis.GetHash<string>(typeName);
            return hash[key];
        }

        /// <summary>
        ///     Given a list of keys, returns values as list
        /// </summary>
        /// <param name="keys">List of unique keys as List<string /></param>
        /// <returns>Returns List<string></string></returns>
        public List<string> GetValues(List<string> keys)
        {
            List<string> values = null;
            try
            {
                RetryHelper.RetryOnException(m_retryCount, TimeSpan.FromSeconds(m_retrySeconds),
                    () => { values = GetValuesExtn(keys); });
            }
            catch (Exception redisException)
            {
               throw;
            }

            return values;
        }

        private List<string> GetValuesExtn(List<string> keys)
        {
            List<string> values;

            using (var client = GetClient(false))
            {
                values = client.GetValues(keys);
            }

            return values;
        }

        /// <summary>
        ///     Given hash ID and array of keys, it returns list of values
        /// </summary>
        /// <param name="hashId">Unique hash ID</param>
        /// <param name="keys">Array of keys as string array</param>
        /// <returns>Returns List<string></string></returns>
        public List<string> GetValuesFromHash(string hashId, params string[] keys)
        {
            List<string> values = null;
            try
            {
                RetryHelper.RetryOnException(m_retryCount, TimeSpan.FromSeconds(m_retrySeconds),
                    () => { values = GetValuesFromHashExtn(hashId, keys); });
            }
            catch (Exception redisException)
            {
                throw;
            }

            return values;
        }

        private List<string> GetValuesFromHashExtn(string hashId, params string[] keys)
        {
            List<string> values;

            using (var client = GetClient(false))
            {
                values = client.GetValuesFromHash(hashId, keys);
            }

            return values;
        }

        /// <summary>
        ///     Given set of keys, returns key value map as dictionery of strings
        /// </summary>
        /// <param name="keys">List of unique keys as List of strings</param>
        /// <returns>Returns string Dictionary</returns>
        public Dictionary<string, string> GetValuesMap(List<string> keys)
        {
            Dictionary<string, string> values = null;
            try
            {
                RetryHelper.RetryOnException(m_retryCount, TimeSpan.FromSeconds(m_retrySeconds),
                    () => { values = GetValuesMapExtn(keys); });
            }
            catch (Exception redisException)
            {
                throw;
            }

            return values;
        }

        private Dictionary<string, string> GetValuesMapExtn(List<string> keys)
        {
            Dictionary<string, string> values;

            using (var client = GetClient(false))
            {
                values = client.GetValuesMap(keys);
            }

            return values;
        }

        /// <summary>
        ///     check hash table contains the given key
        /// </summary>
        /// <param name="hashId">Unique hash ID</param>
        /// <param name="key">Unique key name</param>
        /// <returns>Returns true if it is success</returns>
        public bool HashContainsEntry(string hashId, string key)
        {
            var bReturn = false;
            try
            {
                RetryHelper.RetryOnException(m_retryCount, TimeSpan.FromSeconds(m_retrySeconds),
                    () => { bReturn = HashContainsEntryExtn(hashId, key); });
            }
            catch (Exception redisException)
            {
                throw;
            }

            return bReturn;
        }

        private bool HashContainsEntryExtn(string hashId, string key)
        {
            bool bReturn;

            using (var client = GetClient(false))
            {
                bReturn = client.HashContainsEntry(hashId, key);
            }

            return bReturn;
        }

        /// <summary>
        ///     Removes the entry from redis of given key
        /// </summary>
        /// <param name="key">Unique key name as string</param>
        /// <returns>Returns true if it is success</returns>
        public bool Remove(string key)
        {
            var bReturn = false;
            try
            {
                RetryHelper.RetryOnException(m_retryCount, TimeSpan.FromSeconds(m_retrySeconds),
                    () => { bReturn = RemoveExtn(key); });
            }
            catch (Exception redisException)
            {
                throw;
            }

            return bReturn;
        }

        private bool RemoveExtn(string key)
        {
            bool bReturn;

            using (var client = GetClient(true))
            {
                bReturn = client.Remove(key);
            }

            return bReturn;
        }

        /// <summary>
        ///     Removes all entries of given keys.
        /// </summary>
        /// <param name="keys">Unique keys as List of strings</param>
        /// <returns>Returns true if it is success</returns>
        public bool RemoveAll(IEnumerable<string> keys)
        {
            var bReturn = true;
            try
            {
                RetryHelper.RetryOnException(m_retryCount, TimeSpan.FromSeconds(m_retrySeconds),
                    () => { bReturn = RemoveAllExtn(keys); });
            }
            catch (Exception redisException)
            {
                bReturn = false;
                throw;
            }

            return bReturn;
        }

        private bool RemoveAllExtn(IEnumerable<string> keys)
        {
            var bReturn = true;

            using (var client = GetClient(true))
            {
                client.RemoveAll(keys);
            }

            return bReturn;
        }

        /// <summary>
        ///     Get all items from List data type. List is an another way of storing data in Redis
        /// </summary>
        /// <param name="listId">Unique list ID as string</param>
        /// <returns>Returns the values as List<string></string></returns>
        public List<string> GetAllItemFromlist(string listId)
        {
            List<string> listItems = null;
            try
            {
                RetryHelper.RetryOnException(m_retryCount, TimeSpan.FromSeconds(m_retrySeconds),
                    () => { listItems = GetAllItemFromlistExtn(listId); });
            }
            catch (Exception redisException)
            {
                
                throw;
            }

            return listItems;
        }


        private List<string> GetAllItemFromlistExtn(string listId)
        {
            List<string> listItems = null;

            using (var client = GetClient(false))
            {
                var list = client.Lists[listId];

                if (list.Count > 0)
                {
                    listItems = client.GetAllItemsFromList(listId);
                }
            }

            return listItems;
        }

        /// <summary>
        ///     Given index and list Id, return the value
        /// </summary>
        /// <param name="listId">Unique list ID as string</param>
        /// <param name="listIndex">Index of the item in the list to be retrieved</param>
        /// <returns>Return the value as string</returns>
        public string GetItemFromlist(string listId, int listIndex)
        {
            var listItem = string.Empty;
            try
            {
                RetryHelper.RetryOnException(m_retryCount, TimeSpan.FromSeconds(m_retrySeconds),
                    () => { listItem = GetItemFromlistExtn(listId, listIndex); });
            }
            catch (Exception redisException)
            {
                
                throw;
            }

            return listItem;
        }


        private string GetItemFromlistExtn(string listId, int listIndex)
        {
            var listItem = string.Empty;

            using (var client = GetClient(false))
            {
                var list = client.Lists[listId];

                if (list.Count > 0)
                {
                    listItem = client.GetItemFromList(listId, listIndex);
                }
            }

            return listItem;
        }

        /// <summary>
        ///     given a list Id, Remove all items from the list
        /// </summary>
        /// <param name="listId">Unique List ID as string</param>
        /// <returns>Returns true if it is success</returns>
        public bool RemoveAllFromList(string listId)
        {
            var breturn = true;
            try
            {
                RetryHelper.RetryOnException(m_retryCount, TimeSpan.FromSeconds(m_retrySeconds),
                    () => { breturn = RemoveAllFromListExtn(listId); });
            }
            catch (Exception redisException)
            {
                
                throw;
            }


            return breturn;
        }

        private bool RemoveAllFromListExtn(string listId)
        {
            var breturn = true;

            using (var client = GetClient(true))
            {
                var list = client.Lists[listId];

                if (list.Count > 0)
                {
                    client.RemoveAllFromList(listId);
                }
            }


            return breturn;
        }

        /// <summary>
        ///     Given list ID, removes the last item from the list
        /// </summary>
        /// <param name="listId">Unique list ID</param>
        /// <returns>Returns true if it is success</returns>
        public string RemoveEndFromList(string listId)
        {
            var result = string.Empty;

            try
            {
                RetryHelper.RetryOnException(m_retryCount, TimeSpan.FromSeconds(m_retrySeconds),
                    () => { result = RemoveEndFromListExtn(listId); });
            }
            catch (Exception redisException)
            {
                 throw;
            }


            return result;
        }


        private string RemoveEndFromListExtn(string listId)
        {
            var result = string.Empty;


            using (var client = GetClient(true))
            {
                var list = client.Lists[listId];

                if (list.Count > 0)
                {
                    result = client.RemoveEndFromList(listId);
                }
            }


            return result;
        }

        /// <summary>
        ///     Given set of keys, remove all entries from cache
        /// </summary>
        /// <param name="args">kays as array of string</param>
        /// <returns>Returns true if it is success</returns>
        public bool RemoveEntry(params string[] args)
        {
            var bReturn = false;
            try
            {
                RetryHelper.RetryOnException(m_retryCount, TimeSpan.FromSeconds(m_retrySeconds),
                    () => { bReturn = RemoveEntryExtn(args); });
            }
            catch (Exception redisException)
            {
                bReturn = false;
                

                throw;
            }

            return bReturn;
        }

        private bool RemoveEntryExtn(params string[] args)
        {
            bool bReturn;

            using (var client = GetClient(true))
            {
                bReturn = client.RemoveEntry(args);
            }

            return bReturn;
        }

        /// <summary>
        ///     Given the hashID and key, it removes value from the specified hash table
        /// </summary>
        /// <param name="hashId">Unique hash ID</param>
        /// <param name="key">Unique name of key in cache</param>
        /// <returns>Returns true if it is success</returns>
        public bool RemoveEntryFromHash(string hashId, string key)
        {
            var bReturn = false;
            try
            {
                RetryHelper.RetryOnException(m_retryCount, TimeSpan.FromSeconds(m_retrySeconds),
                    () => { bReturn = RemoveEntryFromHashExtn(hashId, key); });
            }
            catch (Exception redisException)
            {
                
                throw;
            }

            return bReturn;
        }

        private bool RemoveEntryFromHashExtn(string hashId, string key)
        {
            var bReturn = false;

            using (var client = GetClient(true))
            {
                bReturn = client.RemoveEntryFromHash(hashId, key);
            }

            return bReturn;
        }

        /// <summary>
        ///     Removes the value from the hash table
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hashId">Unique hash ID</param>
        /// <param name="key">Unique name of key in cache</param>
        /// <param name="value">Value to be removed from cache</param>
        /// <returns>Returns true if it is success</returns>
        public bool RemoveEntryFromHash<T>(string hashId, string key, T value)
        {
            var bReturn = true;

            try
            {
                RetryHelper.RetryOnException(m_retryCount, TimeSpan.FromSeconds(m_retrySeconds),
                    () => { bReturn = RemoveEntryFromHashExtn(hashId, key, value); });
            }
            catch (Exception redisException)
            {
               
                throw;
            }

            return bReturn;
        }

        private bool RemoveEntryFromHashExtn<T>(string hashId, string key, T value)
        {
            var bReturn = true;


            using (var client = GetClient(true))
            {
                var redis = client.As<T>();
                var hash = redis.GetHash<T>(hashId);

                bReturn = redis.RemoveEntryFromHash(hash, value);
            }

            return bReturn;
        }

        /// <summary>
        ///     Save the REDIS databaase. Ideally it can be configured in the redis configuration file. so no need to call it in
        ///     normal situation.
        /// </summary>
        public void Save()
        {
            try
            {
                RetryHelper.RetryOnException(m_retryCount, TimeSpan.FromSeconds(m_retrySeconds),
                    () => { SaveExtn(); });
            }
            catch (Exception redisException)
            {
                

                throw;
            }
        }

        private void SaveExtn()
        {
            using (var client = GetClient(true))
            {
                client.Save();
            }
        }

        /// <summary>
        ///     Save the REDIS databaase. Ideally it can be configured in the redis configuration file. so no need to call it in
        ///     normal situation.
        /// </summary>
        public void SaveAsynch()
        {
            try
            {
                RetryHelper.RetryOnException(m_retryCount, TimeSpan.FromSeconds(m_retrySeconds),
                    () => { SaveAsynchExtn(); });
            }
            catch (Exception redisException)
            {
                

                throw;
            }
        }

        private void SaveAsynchExtn()
        {
            using (var client = GetClient(true))
            {
                client.SaveAsync();
            }
        }

        /// <summary>
        ///     Get all keys from redis: Accepts regular expression
        ///     Eg: SearchKeys("*") - Returns all keys
        ///     SearchKeys("Key*) - Returns all keys starting with letters "Key"
        ///     SearchKeys("Key?") - Replaces ? with single character
        ///     SearchKeys("Key[0-9]") - Returns all keys starting with letters "Key" and last letter could be anything betwen 0-9
        /// </summary>
        /// <param name="pattern">string pattern</param>
        /// <returns>Returns matching result as List<string></string></returns>
        public List<string> SearchKeys(string pattern)
        {
            List<string> values = null;
            try
            {
                RetryHelper.RetryOnException(m_retryCount, TimeSpan.FromSeconds(m_retrySeconds),
                    () => { values = SearchKeysExtn(pattern); });
            }
            catch (Exception redisException)
            {
                throw;
            }

            return values;
        }

        private List<string> SearchKeysExtn(string pattern)
        {
            List<string> values = null;

            using (var client = GetClient(true))
            {
                values = client.SearchKeys(pattern);
            }

            return values;
        }

        /// <summary>
        ///     create entry in hash table with the given key and value.
        ///     hash ID also should be specified
        /// </summary>
        /// <param name="hashId">Unique hash ID</param>
        /// <param name="key">Unique name of key</param>
        /// <param name="value">Value to set in cache</param>
        /// <returns>Returns true if it is success</returns>
        public bool SetEntryInHash(string hashId, string key, string value)
        {
            var bReturn = false;
            try
            {
                RetryHelper.RetryOnException(m_retryCount, TimeSpan.FromSeconds(m_retrySeconds),
                    () => { bReturn = SetEntryInHashExtn(hashId, key, value); });
            }
            catch (Exception redisException)
            {
                throw;
            }

            return bReturn;
        }

        private bool SetEntryInHashExtn(string hashId, string key, string value)
        {
            var bReturn = false;

            using (var client = GetClient(true))
            {
                bReturn = client.SetEntryInHash(hashId, key, value);
            }

            return bReturn;
        }

        /// <summary>
        ///     create entry in hash table with the given key and type T value.
        ///     hash ID also should be specified
        /// </summary>
        /// <param name="hashid">Unique hash ID</param>
        /// <param name="key">Unique key to set value</param>
        /// <param name="value">Value of type T</param>
        /// <returns>Returns true if it is success</returns>
        [Obsolete("This function is obsolete. Use SetEntryInHash<T>(string key, T value) instead.")]
        public bool SetEntryInHash<T>(string hashid, string key, T value)
        {
            var bReturn = false;
            try
            {
                IRedisTypedClient<T> redis;


                RetryHelper.RetryOnException(m_retryCount, TimeSpan.FromSeconds(m_retrySeconds),
                    () => { bReturn = SetEntryInHashExtn(hashid, key, value); });
            }
            catch (Exception redisException)
            {
                
                throw;
            }

            return bReturn;
        }

        private bool SetEntryInHashExtn<T>(string hashid, string key, T value)
        {
            var bReturn = false;

            IRedisTypedClient<T> redis;
            using (var client = GetClient(true))
            {
                redis = client.As<T>();
            }
            var hash = redis.GetHash<string>(hashid);
            bReturn = redis.SetEntryInHash(hash, key, value);

            return bReturn;
        }

        /// <summary>
        ///     Set value in hash.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">Unique key to set value</param>
        /// <param name="value">Value to set</param>
        /// <returns></returns>
        public bool SetEntryInHash<T>(string key, T value)
        {
            var bReturn = false;
            var typeName = typeof(T).Name;
            try
            {
                RetryHelper.RetryOnException(m_retryCount, TimeSpan.FromSeconds(m_retrySeconds),
                    () => { bReturn = SetEntryInHashExtn(key, value); });
            }
            catch (Exception redisException)
            {
                

                throw;
            }

            return bReturn;
        }

        private bool SetEntryInHashExtn<T>(string key, T value)
        {
            var bReturn = false;
            var typeName = typeof(T).Name;

            IRedisTypedClient<T> redis;
            using (var client = GetClient(true))
            {
                redis = client.As<T>();
            }
            var hash = redis.GetHash<string>(typeName);
            bReturn = redis.SetEntryInHash(hash, key, value);

            return bReturn;
        }

        /// <summary>
        ///     set entry in hash if it does not exists
        /// </summary>
        /// <param name="hashId">Unique hash ID</param>
        /// <param name="key">Unique key to set value</param>
        /// <param name="value">string value</param>
        /// <returns>Returns true if it is success</returns>
        public bool SetEntryInHashIfNotExists(string hashId, string key, string value)
        {
            var bReturn = false;
            try
            {
                RetryHelper.RetryOnException(m_retryCount, TimeSpan.FromSeconds(m_retrySeconds),
                    () => { bReturn = SetEntryInHashIfNotExistsExtn(hashId, key, value); });
            }
            catch (Exception redisException)
            {
                throw;
            }

            return bReturn;
        }

        private bool SetEntryInHashIfNotExistsExtn(string hashId, string key, string value)
        {
            var bReturn = false;

            using (var client = GetClient(true))
            {
                bReturn = client.SetEntryInHashIfNotExists(hashId, key, value);
            }

            return bReturn;
        }

        /// <summary>
        ///     create entry in hash table (if the key does not exists) with the given key and type T value.
        ///     hash ID also should be specified
        /// </summary>
        /// <param name="hashId">Unique hash ID</param>
        /// <param name="key">Unique key to set value</param>
        /// <param name="value">Value to be saved as T</param>
        /// <returns>Returns true if it is success</returns>
        public bool SetEntryInHashIfNotExists<T>(string hashId, string key, T value)
        {
            var bReturn = false;
            try
            {
                RetryHelper.RetryOnException(m_retryCount, TimeSpan.FromSeconds(m_retrySeconds),
                    () => { bReturn = SetEntryInHashIfNotExistsExtn(hashId, key, value); });
            }
            catch (Exception redisException)
            {
                throw;
            }

            return bReturn;
        }

        private bool SetEntryInHashIfNotExistsExtn<T>(string hashId, string key, T value)
        {
            var bReturn = false;

            IRedisTypedClient<T> redis;
            using (var client = GetClient(true))
            {
                redis = client.As<T>();
            }
            var hash = redis.GetHash<string>(hashId);
            bReturn = redis.SetEntryInHashIfNotExists(hash, key, value);

            return bReturn;
        }

        /// <summary>
        ///     set item in list in the specified index position
        /// </summary>
        /// <param name="listId">Unique List ID</param>
        /// <param name="value">Value to save</param>
        /// <returns>Returns true if it is success</returns>
        public bool SetItemInList(string listId, string value)
        {
            var bReturn = true;
            try
            {
                RetryHelper.RetryOnException(m_retryCount, TimeSpan.FromSeconds(m_retrySeconds),
                    () => { bReturn = SetItemInListExtn(listId, value); });
            }
            catch (Exception redisException)
            {
                bReturn = false;
                throw;
            }

            return bReturn;
        }

        private bool SetItemInListExtn(string listId, string value)
        {
            var bReturn = true;

            using (var client = GetClient(true))
            {
                var list = client.Lists[listId];
                //if (List.Count == 0)
                client.AddItemToList(listId, value);

                //client.SetItemInList(listId, listIndex, value);
            }

            return bReturn;
        }

        /// <summary>
        ///     Returns value as T : generic implementation
        /// </summary>
        /// <typeparam name="T">Type T as param</typeparam>
        /// <param name="key">Unique key to identify the key</param>
        /// <returns>Returns value as T</returns>
        public T GetValue<T>(string key)
        {
            var value = default(T);
            try
            {
                RetryHelper.RetryOnException(m_retryCount, TimeSpan.FromSeconds(m_retrySeconds),
                    () => { value = GetValueExtn<T>(key); });
            }
            catch (Exception redisException)
            {
                

                throw;
            }

            return value;
        }

        private T GetValueExtn<T>(string key)
        {
            T value;

            using (var client = GetClient(false))
            {
                value = client.Get<T>(key);
            }

            return value;
        }

        /// <summary>
        ///     Set value for the given key.
        /// </summary>
        /// <param name="key">Unique key to set value</param>
        /// <param name="value">Value to be set</param>
        /// <param name="expiresAt"></param>
        /// <returns>Returns true if it is success</returns>
        public bool SetValue<T>(string key, T value, DateTime expiresAt)
        {
            var bReturn = true;
            try
            {
                RetryHelper.RetryOnException(m_retryCount, TimeSpan.FromSeconds(m_retrySeconds),
                    () => { bReturn = SetValueExtn(key, value, expiresAt); });
            }
            catch (Exception redisException)
            {
                bReturn = false;
                
                throw;
            }

            return bReturn;
        }

        private bool SetValueExtn<T>(string key, T value, DateTime expiresAt)
        {
            var bReturn = true;

            using (var client = GetClient(true))
            {
                client.Set(key, value, expiresAt);
            }

            return bReturn;
        }

        /// <summary>
        ///     Set value for the given key.
        /// </summary>
        /// <param name="key">Unique key to set value</param>
        /// <param name="value">Value to be set in cache</param>
        /// <param name="expiresAt">Expires in minutes</param>
        /// <returns>Return true if it is success</returns>
        public bool SetValue(string key, string value, DateTime expiresAt)
        {
            var bReturn = true;
            try
            {
                RetryHelper.RetryOnException(m_retryCount, TimeSpan.FromSeconds(m_retrySeconds),
                    () => { bReturn = SetValueextn(key, value, expiresAt); });
            }
            catch (Exception redisException)
            {
                bReturn = false;
                
                throw;
            }

            return bReturn;
        }

        private bool SetValueextn(string key, string value, DateTime expiresAt)
        {
            var bReturn = true;

            using (var client = GetClient(true))
            {
                client.Set(key, value, expiresAt);
            }

            return bReturn;
        }

        /// <summary>
        ///     Set value for the given key.
        /// </summary>
        /// <param name="key">Unique key to set value</param>
        /// <param name="value">Value to be set in cache</param>
        /// <returns>Returns true if it is success</returns>
        public bool SetValue(string key, string value)
        {
            var bReturn = true;
            try
            {
                RetryHelper.RetryOnException(m_retryCount, TimeSpan.FromSeconds(m_retrySeconds),
                    () => { bReturn = SetValueExtn(key, value); });
            }
            catch (Exception redisException)
            {
                bReturn = false;
                

                throw;
            }

            return bReturn;
        }

        private bool SetValueExtn(string key, string value)
        {
            var bReturn = true;

            using (var client = GetClient(true))
            {
                client.SetValue(key, value);
            }

            return bReturn;
        }

        /// <summary>
        ///     set value if it does not exists, for the given key and value
        /// </summary>
        /// <param name="key">Unique key to set value</param>
        /// <param name="value">Value to be set in cache</param>
        /// <returns>Returns true if it is success</returns>
        public bool SetValueIfNotExists(string key, string value)
        {
            var bReturn = false; //initially this will bw false
            try
            {
                RetryHelper.RetryOnException(m_retryCount, TimeSpan.FromSeconds(m_retrySeconds),
                    () => { bReturn = SetValueIfNotExistsExtn(key, value); });
            }
            catch (Exception redisException)
            {
                
                throw;
            }

            return bReturn;
        }

        private bool SetValueIfNotExistsExtn(string key, string value)
        {
            bool bReturn; //initially this will bw false

            using (var client = GetClient(true))
            {
                bReturn = client.SetValueIfNotExists(key, value);
            }

            return bReturn;
        }

        /// <summary>
        ///     Return the remaining time for the key to expire
        /// </summary>
        /// <param name="key">key in cache</param>
        /// <returns></returns>
        public TimeSpan? GetTimeTolive(string key)
        {
            TimeSpan? timeSpan = TimeSpan.Zero;
            try
            {
                RetryHelper.RetryOnException(m_retryCount, TimeSpan.FromSeconds(m_retrySeconds),
                    () => { timeSpan = GetTimeToliveExtn(key); });
            }
            catch (Exception redisException)
            {
                throw;
            }

            return timeSpan;
        }

        private TimeSpan? GetTimeToliveExtn(string key)
        {
            TimeSpan? timeSpan;

            using (var client = GetClient(false))
            {
                timeSpan = client.GetTimeToLive(key);
            }

            return timeSpan;
        }

        private IRedisClient GetClient(bool write)
        {

            var startTime = DateTime.Now;

            IRedisClient client;
            client = write ? m_redisManager.GetClient() : m_redisManager.GetReadOnlyClient();

           
            return client;
        }

        /// <summary>
        ///     Dispose the redis object
        /// </summary>
        public void Dispose()
        {

            m_redisManager?.Dispose();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(string key)
        {
            bool bReturn = false;
            try
            {
                RetryHelper.RetryOnException(m_retryCount, TimeSpan.FromSeconds(m_retrySeconds),
                    () => { bReturn = ContainsKeyExtn(key); });
            }
            catch (Exception redisException)
            {
                

                throw;
            }
            return bReturn;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private bool ContainsKeyExtn(string key)
        {
            bool bReturn = false;

            using (var client = GetClient(false))
            {
                bReturn = client.ContainsKey(key);
            }

            return bReturn;
        }

        #endregion
    }
}