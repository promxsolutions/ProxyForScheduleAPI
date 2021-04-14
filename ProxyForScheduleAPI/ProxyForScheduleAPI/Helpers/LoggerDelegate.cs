using System;
using System.Collections.Generic;

namespace ProxyForScheduleAPI.Helpers
{
   static public class LoggerDelegate
   {
      public delegate void DxLogger(string message);

      static public DxLogger GetNestedLogger(this DxLogger logger, string prefix)
      {
         return (message) => logger($"{prefix} - {message}");
      }

      static public DxLogger GetTimestampedLogger(this DxLogger logger)
      {
         return (message) => logger($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffffff K")}] {message}");
      }

      static public void LogCollection(this DxLogger logger, string headerMessage, IEnumerable<object> collection)
      {
         logger(headerMessage);

         var index = 0;
         foreach (var item in collection)
            logger($"\t[{index++}] '{item}'");
      }
   }
}
