using System;

namespace Cache.Interface
{
    /// <summary>
    /// ICacheProvider interface contains all methods from ICacheManager interface
    /// component which implements ICacheProvider, should implement IDisposable also.
    /// </summary>
    public interface ICacheProvider : ICacheManager,IDisposable
    {
        //
    }

    /// <summary>
    /// 
    /// </summary>
    public interface ICacheRedisProvider : ICacheProvider
    { }
    /// <summary>
    /// 
    /// </summary>
    public interface ICacheSentinelProvider : ICacheProvider
    {
        //
    }
}
