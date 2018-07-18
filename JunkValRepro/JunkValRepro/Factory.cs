using System;
using System.Collections.Specialized;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using Cache.Interface;


namespace ERND.Framework.Shared.Cache
{
    /// <summary>
    /// It loads the assembly from current executing folder and create instance of the configured type
    /// configuration is set in the app controller app.config file.
    /// </summary>
    public static class Factory
    {
        #region Static Elements
       
        private static ICacheProvider sm_adapters;
        private static object sm_syncRoot = new object();

        public static ICacheProvider GetProvider(string provider, bool sentinel)
        {
            if (!string.IsNullOrEmpty(provider))
            {
                try
                {
                    if (sm_adapters == null)
                    {
                        lock (sm_syncRoot)
                        {
                            if (sm_adapters == null)
                            {
                                var catalog =
                                    new AggregateCatalog(new DirectoryCatalog(".",
                                        "Adapters.Cache.dll"));
                                catalog.Catalogs.Add(new DirectoryCatalog(".", "Adapters.Cache.*.dll"));

                                var container = new CompositionContainer(catalog);

                                //todo: This checking has been added temporarily. Once the sentinel start working, ICacheRedisProvider can be removed
                                if (!sentinel)
                                {
                                    sm_adapters = container.GetExportedValue<ICacheRedisProvider>(provider);
                                }
                                else
                                {
                                    sm_adapters = container.GetExportedValue<ICacheSentinelProvider>(provider);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    
                    throw;
                }
            }
            return sm_adapters;
        }
        #endregion
    }

}
