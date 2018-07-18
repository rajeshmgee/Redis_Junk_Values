using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cache.Interface;

namespace ERND.Framework.Shared.Cache
{
    
    /// <summary>
    /// This is the manager class which helps client to communicate to in-memory data structure.
    /// usage: ICacheManager cacheManager = new CacheManager();
    /// </summary>
    public class CacheManager : ICacheManager,IDisposable
    {
        #region Private Elements
        ICacheProvider m_Provider;
        
       
        #endregion
        #region Public Constructors
        public CacheManager()
        {
            try {
                

                string provider = "REDIS";
                bool sentinel = true;
                m_Provider = Factory.GetProvider(provider, sentinel);
            }
            catch(Exception ex)
            {
                
                throw;
            }
        }

        #endregion
        #region Public Elements
        /// <summary>
        /// When a thread reads a key it can lock the key. 
        /// Acquirelock returns a IDisposable object. Lock clears when thread disposes IDisposable object.
        /// Timeout specifies, how long to wait if the lock acquired by any ther thread.
        /// timeout in seconds
        /// </summary>
        /// <param name="key">The cache key</param>
        /// <param name="timeout">Timeout for acquiring the lock</param>
        /// <returns>Returns IDisposable object</returns>
        public IDisposable Acquirelock(string key, TimeSpan timeout)
        {
            IDisposable Lock = null;
            try {
                var startTime = DateTime.Now;
                
                
                Lock = m_Provider.Acquirelock(key, timeout);

                

            }
            catch (Exception ex)
            {
                
                throw;
            }
            return Lock;
        }

        /// <summary>
        /// When a thread reads a key it can lock the key. 
        /// Acquirelock returns a IDisposable object. Lock clears when thread disposes IDisposable object.
        /// Thread should wait indefinitly until the other thread releases the lock.
        /// </summary>
        /// <param name="key">The cache key</param>
        /// <returns>Returns IDisposable object</returns>
        public IDisposable Acquirelock(string key)
        {
            IDisposable Lock = null;
            try
            {
                var startTime = DateTime.Now;
                

                Lock = m_Provider.Acquirelock(key);

                
            }
            catch (Exception ex)
            {
                
                throw;
            }
            return Lock;
        }
        /// <summary>
        /// Returns all the keys as List of strings<string>
        /// </summary>
        /// <returns>Returns List of keys</returns>
        public List<string> GetAllkeys()
        {
            List<string> lists=null;

            try {
                
                var startTime = DateTime.Now;
                lists = m_Provider.GetAllkeys();
                
            }
            catch (Exception ex)
            {
                
                throw;
            }
           
            return lists;
        }

        /// <summary>
        /// Returns the existing value of the unique key and set the new value.
        /// </summary>
        /// <param name="key">The unique cache key which hold the value</param>
        /// <param name="Value">New value to be set</param>
        /// <returns>Returns string value</returns>
        public string GetAndSetValue(string key, string Value)
        {
            string value;

            try {
                
                var startTime = DateTime.Now;
                value =  m_Provider.GetAndSetValue(key, Value);
                
            }
            catch (Exception ex)
            {
                
                throw;
            }


            return value;
        }

        /// <summary>
        /// Given a unique key, return the value
        /// </summary>
        /// <param name="key">The unique cache key which hold the value</param>
        /// <returns>Returns string value</returns>
        public string GetValue(string key)
        {
            string value;
            try {
               
                var startTime = DateTime.Now;
                value = m_Provider.GetValue(key);
                
            }
            catch (Exception ex)
            {
                
                throw;
            }

            return value;
        }

        /// <summary>
        /// Given the hash ID and key, return the value
        /// hash ID is the unique name in the system. Its like table name in the database.
        /// </summary>
        /// <param name="hashId">Unique hash ID</param>
        /// <param name="key">The unique cache key which hold the value in the specified hashId</param>
        /// <returns>Returns string value</returns>
        public string GetValueFromHash(string hashId, string key)
        {
            string value;
            try {
                
                var startTime = DateTime.Now;
                value = m_Provider.GetValueFromHash(hashId, key);
                
            }
            catch (Exception ex)
            {
                
                throw;
            }

            return value;
        }

        /// <summary>
        /// Generic implementation of getting value from hash
        /// given hash ID and key, returns an object of type T
        /// hash ID is the unique name in the system. Its like table name in the database.
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="hashId">Unique hash ID</param>
        /// <param name="key">The unique cache key which hold the value in the specified hashid</param>
        /// <returns>Returns value as T</returns>
        
        public T GetValueFromHash<T>(string hashid, string key)
        {
            return m_Provider.GetValueFromHash<T>(hashid, key);
        }

        /// <summary>
        /// Given a list of keys, returns values as list
        /// </summary>
        /// <param name="keys">List of unique keys as List<string></param>
        /// <returns>Returns List<string></returns>
        public List<string> GetValues(List<string> keys)
        {
            List<string> lists = null;

            try {
               
                var startTime= DateTime.Now;
                lists = m_Provider.GetValues(keys);
                
            }
            catch (Exception ex)
            {
                
                throw;
            }

            return lists;
        }

        /// <summary>
        /// Given hash ID and array of keys, it returns list of values
        /// </summary>
        /// <param name="hashId">Unique hash ID</param>
        /// <param name="keys">Array of keys as string array</param>
        /// <returns>Returns List<string></string></returns>

        public List<string> GetValuesFromHash(string hashId, params string[] keys)
        {
            List<string> lists = null;

            try {
                
                var startTime = DateTime.Now;
                lists = m_Provider.GetValuesFromHash(hashId, keys);
                
            }
            catch (Exception ex)
            {
                
                throw;
            }

            return lists;
        }
        /// <summary>
        /// Given set of keys, returns key value map as dictionery of strings
        /// </summary>
        /// <param name="keys">List of unique keys as List of strings</param>
        /// <returns>Returns Dictionery<string,string></returns>
        public Dictionary<string, string> GetValuesMap(List<string> keys)
        {
            
            var startTime = DateTime.Now;
            Dictionary<string,string> values=  m_Provider.GetValuesMap(keys);
            
            return values;
        }
        /// <summary>
        /// check hash table contains the given key
        /// </summary>
        /// <param name="hashId">Unique hash ID</param>
        /// <param name="key">Unique key name</param>
        /// <returns>Returns true if it is success</returns>
        public bool HashContainsEntry(string hashId, string key)
        {
            
            var startTime = DateTime.Now;
            bool result = m_Provider.HashContainsEntry(hashId, key);
            
            return result;
        }
        /// <summary>
        /// Removes the entry from redis of given key
        /// </summary>
        /// <param name="key">Unique key name as string</param>
        /// <returns>Returns true if it is success</returns>
        public bool Remove(string key)
        {
            
            var startTime = DateTime.Now;
            bool result = m_Provider.Remove(key);
            
            return result;
        }
        /// <summary>
        /// Removes all entries of given keys.
        /// </summary>
        /// <param name="keys">Unique keys as List of strings</param>
        /// <returns>Returns true if it is success</returns>
        public bool RemoveAll(IEnumerable<string> keys)
        {
            
            var startTime = DateTime.Now;
            bool result = m_Provider.RemoveAll(keys);
            
            return result;
        }
        /// <summary>
        /// given a list Id, Remove all items from the list
        /// </summary>
        /// <param name="listId">Unique List ID as string</param>
        /// <returns>Returns true if it is success</returns>
        public bool RemoveAllFromList(string listId)
        {
            
            var startTime = DateTime.Now;
            bool result = m_Provider.RemoveAllFromList(listId);
            
            return result;
        }
        /// <summary>
        /// Given list ID, removes the last item from the list
        /// </summary>
        /// <param name="listId">Unique list ID</param>
        /// <returns>Returns true if it is success</returns>
        public string RemoveEndFromList(string listId)
        {
            
            var startTime = DateTime.Now;
            string result =  m_Provider.RemoveEndFromList(listId);
            
            return result;
        }
        /// <summary>
        /// Given set of keys, remove all entries from cache
        /// </summary>
        /// <param name="args">kays as array of string</param>
        /// <returns>Returns true if it is success</returns>
        public bool RemoveEntry(params string[] args)
        {
            
            var startTime = DateTime.Now;
            
            bool result= m_Provider.RemoveEntry(args);
            
            return result;
        }
        /// <summary>
        /// Given the hashID and key, it removes value from the specified hash table
        /// </summary>
        /// <param name="hashId">Unique hash ID</param>
        /// <param name="key">Unique name of key in cache</param>
        /// <returns>Returns true if it is success</returns>
        public bool RemoveEntryFromHash(string hashId, string key)
        {
            
            var startTime = DateTime.Now;
            bool result = m_Provider.RemoveEntryFromHash(hashId, key);
             return result;
        }
        /// <summary>
        /// Removes the value from the hash table
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hashId">Unique hash ID</param>
        /// <param name="key">Unique name of key in cache</param>
        /// <param name="value">Value to be removed from cache</param>
        /// <returns>Returns true if it is success</returns>
        public bool RemoveEntryFromHash<T>(string hashId, string key, T value)
        {
            
            var startTime =DateTime.Now;
            bool result =m_Provider.RemoveEntryFromHash<T>(hashId, key, value);
            
            return result;
        }
        /// <summary>
        /// Save the REDIS databaase. Ideally it can be configured in the redis configuration file. so no need to call it in normal situation.
        /// </summary>
        public void Save()
        {
            m_Provider.Save();
        }
            /// <summary>
            /// Save the REDIS databaase. Ideally it can be configured in the redis configuration file. so no need to call it in normal situation.
            /// </summary>
        public void SaveAsynch()
        {
            m_Provider.SaveAsynch();
        }
        /// <summary>
        /// Get all keys from redis: Accepts regular expression
        /// Eg: SearchKeys("*") - Returns all keys
        ///     SearchKeys("Key*) - Returns all keys starting with letters "Key"
        ///     SearchKeys("Key?") - Replaces ? with single character
        ///     SearchKeys("Key[0-9]") - Returns all keys starting with letters "Key" and last letter could be anything betwen 0-9
        /// </summary>
        /// <param name="pattern">string pattern</param>
        /// <returns>Returns matching result as List<string></string></returns>
        public List<string> SearchKeys(string pattern)
        {
            
            var startTime = DateTime.Now;
            List<string> values = m_Provider.SearchKeys(pattern);
            return values;
        }
        /// <summary>
        /// create entry in hash table with the given key and value.
        /// hash ID also should be specified
        /// </summary>
        /// <param name="hashId">Unique hash ID</param>
        /// <param name="key">Unique name of key</param>
        /// <param name="value">Value to set in cache</param>
        /// <returns>Returns true if it is success</returns>
        public bool SetEntryInHash(string hashId, string key, string value)
        {
            
            var startTime = DateTime.Now;
            
            bool result = m_Provider.SetEntryInHash(hashId, key, value);

             return result;
        }
        /// <summary>
        /// create entry in hash table with the given key and type T value.
        /// hash ID also should be specified
        /// </summary>
        /// <param name="hashId">Unique hash ID</param>
        /// <param name="key">Unique key to set value</param>
        /// <param name="value">Value of type T</param>
        /// <returns>Returns true if it is success</returns>
       
        public bool SetEntryInHash<T>(string hashid, string key, T value)
        {
            var startTime = DateTime.Now;
            bool result= m_Provider.SetEntryInHash<T>(hashid, key, value);
            return result;
        }

        /// <summary>
        /// set entry in hash if it does not exists
        /// </summary>
        /// <param name="hashId">Unique hash ID</param>
        /// <param name="key">Unique key to set value</param>
        /// <param name="value">string value</param>
        /// <returns>Returns true if it is success</returns>
        public bool SetEntryInHashIfNotExists(string hashId, string key, string value)
        {
            bool returnval = false;
            
            var startTime = DateTime.Now;
            returnval = m_Provider.SetEntryInHashIfNotExists(hashId, key, value);
            

            return returnval;
        }
        /// <summary>
        /// create entry in hash table (if the key does not exists) with the given key and type T value.
        /// hash ID also should be specified
        /// </summary>
        /// <param name="hashId">Unique hash ID</param>
        /// <param name="key">Unique key to set value</param>
        /// <param name="value">Value to be saved as T</param>
        /// <returns>Returns true if it is success</returns>
        public bool SetEntryInHashIfNotExists<T>(string hashId, string key, T value)
        {
            bool breturn = false;
            
            var startTime = DateTime.Now;
            breturn = m_Provider.SetEntryInHashIfNotExists<T>(hashId, key, value);
            

            return breturn;
        }
        /// <summary>
        /// set item in list in the specified index position
        /// </summary>
        /// <param name="listId">Unique List ID</param>
        /// <param name="value">Value to save</param>
        /// <returns>Returns true if it is success</returns>
        public bool SetItemInList(string listId, string value)
        {
            bool bReturn = false;
            
            var startTime = DateTime.Now;
            bReturn = m_Provider.SetItemInList(listId, value);
            

            return bReturn;
        }
        /// <summary>
        /// Set value for the given key.
        /// </summary>
        /// <param name="key">Unique key to set value</param>
        /// <param name="value">Value to be set in cache</param>
        /// <returns>Returns true if it is success</returns>
        public bool SetValue(string key, string value)
        {
           
            var startTime = DateTime.Now;
            bool result = m_Provider.SetValue(key, value);
             return result;
        }

        /// <summary>
        /// set value if it does not exists, for the given key and value
        /// </summary>
        /// <param name="key">Unique key to set value</param>
        /// <param name="Value">Value to be set in cache</param>
        /// <returns>Returns true if it is success</returns>
        public bool SetValueIfNotExists(string key, string value)
        {
            bool returnval = false;
            
            var startTime = DateTime.Now;
            returnval = m_Provider.SetValueIfNotExists(key, value);
            
            return returnval;
        }

        public void Dispose()
        {
            m_Provider?.Dispose();
        }
        /// <summary>
        /// Given index and list Id, return the value
        /// </summary>
        /// <param name="listId">Unique list ID as string</param>
        /// <param name="listIndex">Index of the item in the list to be retrieved</param>
        /// <returns>Return the value as string</returns>
        public string GetItemFromlist(string listId, int listIndex)
        {
            string item = string.Empty;
           
            var startTime = DateTime.Now;
            
            item = m_Provider.GetItemFromlist(listId, listIndex);
            
            return item;
        }
        /// <summary>
        /// Get all items from List data type. List is an another way of storing data in Redis
        /// </summary>
        /// <param name="listId"></param>
        /// <returns></returns>
        public List<string> GetAllItemFromlist(string listId)
        {
            List<string> lists = null;
            
            var startTime = DateTime.Now;
            
            lists = m_Provider.GetAllItemFromlist(listId);
            
            return lists;
        }

        /// <summary>
        /// Set value for the given key.
        /// </summary>
        /// <param name="key">Unique key to set value</param>
        /// <param name="value">Value to be set</param>
        /// /// <param name="expiresAt">expiry time</param>
        /// <returns>Returns true if it is success</returns>
        public bool SetValue<T>(string key, T value, DateTime expiresAt)
        {
            
            var startTime = DateTime.Now;
            bool result= m_Provider.SetValue(key, value, expiresAt);
             return result;
        }

        /// <summary>
        /// Set value for the given key.
        /// </summary>
        /// <param name="key">Unique key to set value</param>
        /// <param name="value">Value to be set in cache</param>
        /// <param name="expiresAt">Expires in minutes</param>
        /// <returns>Return true if it is success</returns>
        public bool SetValue(string key, string value, DateTime expiresAt)
        {
           
            var startTime= DateTime.Now;
            bool result= m_Provider.SetValue(key, value, expiresAt);
            return result;
        }
        /// <summary>
        /// Returns value as T : generic implementation
        /// </summary>
        /// <typeparam name="T">Type T as param</typeparam>
        /// <param name="key">Unique key to identify the key</param>
        /// <returns>Returns value as T</returns>
        public T GetValue<T>(string key)
        {
           
            var startTime = DateTime.Now;
            var result = m_Provider.GetValue<T> (key);
             return (T) Convert.ChangeType(result, typeof(T));
        }

        /// <summary>
        /// Return the remaining time for the key to expire
        /// </summary>
        /// <param name="key">key in cache</param>
        /// <returns></returns>
        public TimeSpan? GetTimeTolive(string key)
        {
            
            return m_Provider.GetTimeTolive(key);
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
           
            var startTime = DateTime.Now;
            var result = m_Provider.GetValueFromHash<T>(key);
           return (T) Convert.ChangeType(result, typeof(T));
        }
        /// <summary>
        /// Set value in hash.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">Unique key to set value</param>
        /// <param name="value">Value to set</param>
        /// <returns></returns>
        public bool SetEntryInHash<T>(string key, T value)
        {
           
            var startTime =DateTime.Now;
            bool result = m_Provider.SetEntryInHash<T>(key,value);
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(string key)
        {
            
            var startTime = DateTime.Now;
            bool result = m_Provider.ContainsKey(key);
            return result;
        }
        #endregion
    }
}
