using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSMTool.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var loader = new Core.OsmLoader();

            loader.Load(@"C:\Users\stijn\Downloads\luxembourg-latest.osm.pbf");
        }
    }
}
