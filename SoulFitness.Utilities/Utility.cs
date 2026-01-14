using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulFitness.Utilities
{
    public class Utility
    {
        public static bool WriteLog(string log)
        {
            try
            {
                string baseDir = AppContext.BaseDirectory;
                string path = Path.Combine(baseDir, "Logs");
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                string filePath = Path.Combine(path, $"SoulFitness_{DateTime.Now:yyyy-MM-dd}.txt");
                File.AppendAllText(filePath, log + Environment.NewLine);
                return true;
            }
            catch { return false; }
        }
    }

}
