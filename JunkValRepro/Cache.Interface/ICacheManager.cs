using System;
using System.Collections.Generic;

namespace Cache.Interface
{
    /// <summary>
    /// 
    /// </summary>
    public interface ICacheManager
    {
        /// <summary>
        /// When client reads a key it can acquire lock. 
        /// Acquirelock returns a idisosable object. Lock clears when dispose this object.
        /// timeout speiciies , how long to wait if the lock acquired by any ther component.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        IDisposable Acquirelock(string Key, TimeSpan timeout);
        /// <summary>
        /// When client reads a key it can acquire lock. 
        /// Acquirelock returns a idisosable object. Lock clears when dispose this object.
        /// since time out cannot speiciies , it wait indefinitly until the other component release the lock.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        IDisposable Acquirelock(string Key);
        /// <summary>
        /// Get all keys used in the cache
        /// </summary>
        /// <returns>List of keys</returns>
        List<String> GetAllkeys();
        /// <summary>
        /// Returns the existing value of the key and set the new value.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        string GetAndSetValue(string Key, string Value);
        /// <summary>
        /// Given a key, return the value
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        string GetValue(string Key);
        /// <summary>
        /// Given the hash ID and key, return the value
        /// hash ID is the unique name in the system. Its like table name in the database.
        /// </summary>
        /// <param name="hashId"></param>
        /// <param name="Key"></param>
        /// <returns></returns>
        string GetValueFromHash(string hashId, string Key);
        /// <summary>
        /// Given a list of keys, returns values as list
        /// </summary>
        /// <param name="Keys"></param>
        /// <returns></returns>
        List<string> GetValues(List<string> Keys);
        /// <summary>
        /// Given hash ID and array of keys, it returns list of values
        /// </summary>
        /// <param name="hashId"></param>
        /// <param name="Keys"></param>
        /// <returns></returns>
        List<string> GetValuesFromHash(string hashId, params string[] Keys);
        /// <summary>
        /// Given set of keys, returns key value map as dictionery of strings
        /// </summary>
        /// <param name="Keys"></param>
        /// <returns></returns>
        Dictionary<string, string> GetValuesMap(List<string> Keys);
        /// <summary>
        /// check hahs table contains value of given key
        /// </summary>
        /// <param name="hashId"></param>
        /// <param name="Key"></param>
        /// <returns></returns>
        bool HashContainsEntry(string hashId, string Key);
        /// <summary>
        /// given a list Id, Remove all items from the list
        /// </summary>
        /// <param name="listId"></param>
        /// <returns></returns>
        bool RemoveAllFromList(string listId);
        /// <summary>
        /// Given list ID, returns the last item from the list
        /// </summary>
        /// <param name="listId"></param>
        /// <returns></returns>
        string RemoveEndFromList(string listId);
        /// <summary>
        /// Given set of keys, remove all entries from redis
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        bool RemoveEntry(params string[] args);
        /// <summary>
        /// Given the hashID and key, it removes value from the specified hash table
        /// </summary>
        /// <param name="hashId"></param>
        /// <param name="Key"></param>
        /// <returns></returns>
        bool RemoveEntryFromHash(string hashId, string Key);
        /// <summary>
        /// Save the REDIS databaase. Ideally it can be configured in the redis configuration file. so no need to call it in normal situation.
        /// </summary>
        void Save();
        /// <summary>
        /// Save the REDIS databaase. Ideally it can be configured in the redis configuration file. so no need to call it in normal situation.
        /// </summary>
        void SaveAsynch();
        /// <summary>
        /// Get all keys from redis: Accepts regular expression
        /// Eg: SearchKeys("*") - Returns all keys
        ///     SearchKeys("Key*) - Returns all keys starting with letters "Key"
        ///     SearchKeys("Key?") - Replaces ? with single character
        ///     SearchKeys("Key[0-9]") - Returns all keys starting with letters "Key" and last letter could be anything betwen 0-9
        /// </summary>
        /// <param name="Pattern"></param>
        /// <returns></returns>
        List<string> SearchKeys(string Pattern);
        /// <summary>
        /// Set value for the given key.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        bool SetValue(string Key, string Value);
        /// <summary>
        /// set value if it does not exists, for the given key and value
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool SetValueIfNotExists(string Key, string value);
        /// <summary>
        /// create entry in hash table with the given key and value.
        /// hash ID also should be specified
        /// </summary>
        /// <param name="hashId"></param>
        /// <param name="Key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool SetEntryInHash(string hashId, string Key, string value);
        /// <summary>
        /// set entry in hash if it does not exists
        /// </summary>
        /// <param name="hashId"></param>
        /// <param name="Key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool SetEntryInHashIfNotExists(string hashId, string Key, string value);
        /// <summary>
        /// set item in list in the specified index position
        /// </summary>
        /// <param name="listId"></param>
        /// <param name="listIndex"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool SetItemInList(string listId, string value);
        /// <summary>
        /// Given index and list Id, return the value
        /// </summary>
        /// <param name="listId"></param>
        /// <returns></returns>
        string GetItemFromlist(string listId, int listIndex);
        /// <summary>
        /// Get all items from List data type. List is an another way of storing data in Redis
        /// </summary>
        /// <param name="listId"></param>
        /// <returns></returns>
        List<string> GetAllItemFromlist(string listId);
        /// <summary>
        /// Removes the entry from redis of given key
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        bool Remove(string Key);
        /// <summary>
        /// Removes all entries of given keys.
        /// </summary>
        /// <param name="Keys"></param>
        /// <returns></returns>
        bool RemoveAll(IEnumerable<string> Keys);
        /// <summary>
        /// given hash ID and key, returns an object of type T
        /// hash ID is the unique name in the system. Its like table name in the database.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hashid"></param>
        /// <param name="Key"></param>
        /// <returns></returns>
        T GetValueFromHash<T>(string hashid, string Key);
        /// <summary>
        /// create entry in hash table with the given key and type T value.
        /// hash ID also should be specified
        /// </summary>
        /// <param name="hashId"></param>
        /// <param name="Key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool SetEntryInHash<T>(string hashid, string Key, T value);
        /// <summary>
        /// create entry in hash table (if the key does not exists) with the given key and type T value.
        /// hash ID also should be specified
        /// </summary>
        /// <param name="hashId"></param>
        /// <param name="Key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool SetEntryInHashIfNotExists<T>(string hashId, string Key, T value);
        /// <summary>
        /// Removes the value from the hash table
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hashId"></param>
        /// <param name="Key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool RemoveEntryFromHash<T>(string hashId, string Key, T value);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        /// <param name="ExpiresAt"></param>
        /// <returns></returns>
        bool SetValue<T>(string Key, T Value, DateTime ExpiresAt);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        /// <param name="ExpiresAt"></param>
        /// <returns></returns>
        bool SetValue(string Key, string Value, DateTime ExpiresAt);
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Key"></param>
        /// <returns></returns>
        T GetValue<T>(string Key);
        /// <summary>
        /// Return the remaining time for the key to expire
        /// </summary>
        /// <param name="key">key in cache</param>
        /// <returns></returns>
        TimeSpan? GetTimeTolive(string key);

        /// <summary>
        ///     Generic implementation of getting value from hash
        ///     given key, returns an object of type T 
        /// </summary>
        /// <typeparam name="T">Return value as T</typeparam>
        /// <param name="key">Key to set value in hash</param>
        /// <returns></returns>
        T GetValueFromHash<T>(string key);
        /// <summary>
        /// Set value in hash.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">Unique key to set value</param>
        /// <param name="value">Value to set</param>
        /// <returns></returns>
        bool SetEntryInHash<T>(string key, T value);
        /// <summary>
        /// key exists
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        bool ContainsKey(string key);
    }
}

