﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Heroes.Graphics.Essentials.Configuration
{
    public class Utilities
    {
        /// <param name="getValue">Function that retrieves the value.</param>
        /// <param name="timeout">The timeout in milliseconds.</param>
        /// <param name="sleepTime">Amount of sleep per iteration/attempt.</param>
        /// <param name="token">Token that allows for cancellation of the task.</param>
        /// <exception cref="Exception">Timeout expired.</exception>
        public static T TryGetValue<T>(Func<T> getValue, int timeout, int sleepTime, CancellationToken token = default)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            bool valueSet = false;
            T value = default;

            while (watch.ElapsedMilliseconds < timeout)
            {
                if (token.IsCancellationRequested)
                    return value;

                try
                {
                    value = getValue();
                    valueSet = true;
                    break;
                }
                catch (Exception) { /* Ignored */ }

                Thread.Sleep(sleepTime);
            }

            if (valueSet == false)
                throw new Exception($"Timeout limit {timeout} exceeded.");

            return value;
        }
    }
}