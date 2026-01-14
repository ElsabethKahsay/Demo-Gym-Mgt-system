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
            StreamWriter writer = new StreamWriter("C:\\EthiopianServices\\SoulfFitness\\" + "_SoulfFitness" + " " + DateTime.Now.ToString("yyyy-MM-ddThh tt") + ".txt", true);
            writer.WriteLine(log);
            writer.Close();
            return true;
        }
    }

}
