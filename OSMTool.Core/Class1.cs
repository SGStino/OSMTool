using OsmSharp.Streams;
using System;
using System.Collections.Generic;  
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSMTool.Core
{
    public class OsmLoader
    {
        public void Load(string file)
        {
            using (var stream = File.OpenRead(file))
            {
                using (var source = new PBFOsmStreamSource(stream))
                {

                    //foreach (var t in source.SelectMany(i => i.Tags).Select(t => t.Key).Distinct().OrderBy(k => k))
                    //    Console.WriteLine(t);

                    var roads = source.Where(t => t.Tags.Any(k => k.Key == "road"));
                
                    
                }
            }
            Console.ReadLine();
        }
    }
}
