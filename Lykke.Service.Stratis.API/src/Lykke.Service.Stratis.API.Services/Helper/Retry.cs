﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Common.Log;

namespace Lykke.Service.Stratis.API.Services.Helper
{
    public class Retry
    {
        public static async Task<T> Try<T>(Func<Task<T>> action, Func<Exception, bool> exceptionFilter,
            int tryCount, ILog logger, int delayAfterException = 0)
        {
            var @try = 0;

            while (true)
            {
                try
                {
                    return await action();
                }
                catch (Exception ex)
                {
                    @try++;

                    if (!exceptionFilter(ex) || @try >= tryCount)
                    {
                        throw;
                    }

                    if (delayAfterException > 0)
                    {
                        await Task.Delay(delayAfterException);
                    }

                    await logger.WriteErrorAsync("Retry", "Try", null, ex);
                }
            }
        }
    }
}
