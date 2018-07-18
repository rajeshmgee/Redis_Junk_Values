#region FileInfo

// *****************************************************************************************************
// Author : ERND
// Copyright (c) Etimad Research & Development LLC
// Description and Purpose : 
// *****************************************************************************************************

#endregion

using System;
using System.Threading.Tasks;


namespace Adapters.Cache.Redis
{
    /// <summary>
    ///     Retry every command
    /// </summary>
    public static class RetryHelper
    {
        #region Non Public Fields

       

        #endregion

        #region Public Methods

        /// <summary>
        /// </summary>
        /// <param name="times">How many times it should repeat</param>
        /// <param name="delay">Delay in the iteration</param>
        /// <param name="operation">Function delegates</param>
        public static void RetryOnException(int times, TimeSpan delay, Action operation)
        {
            var attempts = 0;
            do
            {
                try
                {
                    attempts++;
                    operation();
                    break; //Success exit the loop
                }
                catch (Exception e)
                {
                    if (attempts == times)
                    {
                        throw;
                    }
                    Task.Delay(delay).Wait();
                }
            } while (true);
        }

        #endregion
    }
}