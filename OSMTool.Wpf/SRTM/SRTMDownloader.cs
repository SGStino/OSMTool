using OsmSharp.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OSMTool.Wpf.SRTM
{
    public static class SRTMDownloader
    {
        public static Task DownloadAsync(Bounds bounds, string folder)
        {
            var tasks = new List<Task>();
            var url = "https://dds.cr.usgs.gov/srtm/version2_1/SRTM3/Eurasia/";
            for (int lat = (int)bounds.MinLatitude; lat < bounds.MinLatitude; lat++)
            {
                for (int lon = (int)bounds.MinLongitude; lon < bounds.MinLongitude; lon++)
                {
                    var latitude = (lat > 0 ? "N" : "S") + string.Format("{0:00}", Math.Abs(lat));
                    var longitude = (lon > 0 ? "E" : "W") + string.Format("{0:000}", Math.Abs(lon));
                    var file = latitude + longitude + ".hgt";
                    var zipFile = file + ".zip";

                    var dest = Path.Combine(folder, file);
                    var destZip = Path.Combine(folder, zipFile);
                    var source = url + zipFile;

                    if (!File.Exists(dest))
                    {
                        tasks.Add(downloadAsync(destZip, source));
                    }
                }
            }
            return Task.WhenAll(tasks);
        }

        private async static Task downloadAsync(string destZip, string source)
        {
            using (var client = new WebClient())
            {
                await client.DownloadFileTaskAsync(source, destZip);
                await Task.Run(() =>
                {
                    ZipFile.ExtractToDirectory(destZip, Path.GetDirectoryName(destZip));
                    File.Delete(destZip);
                });
            }
        }
    }
}
