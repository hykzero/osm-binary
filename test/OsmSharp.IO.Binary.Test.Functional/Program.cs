﻿using System.IO;
using System.IO.Compression;
using Serilog;

namespace OsmSharp.IO.Binary.Test.Functional
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // enable logging.
            OsmSharp.Logging.Logger.LogAction = (origin, level, message, parameters) =>
            {
                var formattedMessage = $"{origin} - {message}";
                switch (level)
                {
                    case "critical":
                        Log.Fatal(formattedMessage);
                        break;
                    case "error":
                        Log.Error(formattedMessage);
                        break;
                    case "warning":
                        Log.Warning(formattedMessage);
                        break; 
                    case "verbose":
                        Log.Verbose(formattedMessage);
                        break; 
                    case "information":
                        Log.Information(formattedMessage);
                        break; 
                    default:
                        Log.Debug(formattedMessage);
                        break;
                }
            };

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File(Path.Combine("logs", "log-.txt"), rollingInterval: RollingInterval.Day)
                .CreateLogger();

            // download test data.
            Download.ToFile("http://files.itinero.tech/data/OSM/planet/europe/luxembourg-latest.osm.pbf", "test.osm.pbf").Wait();

            // test read/writing an existing OSM file.
            Log.Information("Testing reading/writing via OSM binary format...");
            using (var sourceStream = File.OpenRead("test.osm.pbf"))
            using (var targetStream = File.Open("test1.osm.bin", FileMode.Create))
            {
                var source = new OsmSharp.Streams.PBFOsmStreamSource(sourceStream);

                var target = new OsmSharp.Streams.BinaryOsmStreamTarget(targetStream);
                target.RegisterSource(source);
                target.Pull();
            }
            using (var sourceStream = File.OpenRead("test1.osm.bin"))
            using (var targetStream = File.Open("test2.osm.pbf", FileMode.Create))
            {
                var source = new OsmSharp.Streams.BinaryOsmStreamSource(sourceStream);

                var target = new OsmSharp.Streams.PBFOsmStreamTarget(targetStream);
                target.RegisterSource(source);
                target.Pull();
            }
            
            // test read/writing an existing OSM file via a compressed stream.
            Log.Information("Testing reading/writing via OSM binary format using a compressed stream...");
            using (var sourceStream = File.OpenRead("test.osm.pbf"))
            using (var targetStream = File.Open("test2.osm.bin.zip", FileMode.Create))
            using (var targetStreamCompressed = new System.IO.Compression.DeflateStream(targetStream, CompressionLevel.Fastest))
            {
                var source = new OsmSharp.Streams.PBFOsmStreamSource(sourceStream);

                var target = new OsmSharp.Streams.BinaryOsmStreamTarget(targetStreamCompressed);
                target.RegisterSource(source);
                target.Pull();
            }

            using (var sourceStream = File.OpenRead("test2.osm.bin.zip"))
            using (var sourceStreamCompressed = new System.IO.Compression.DeflateStream(sourceStream, CompressionMode.Decompress))
            using (var targetStream = File.Open("test2.osm.pbf", FileMode.Create))
            {
                var source = new OsmSharp.Streams.BinaryOsmStreamSource(sourceStreamCompressed);

                var target = new OsmSharp.Streams.PBFOsmStreamTarget(targetStream);
                target.RegisterSource(source);
                target.Pull();
            }

            // test reading/writing edited OSM-data.
            Log.Information("Testing reading/writing via OSM binary format of edited incomplete OSM data...");
            using (var sourceStream = File.OpenRead("./test-data/data.osm"))
            using (var targetStream = File.Open("test3.osm.bin", FileMode.Create))
            {
                var source = new OsmSharp.Streams.XmlOsmStreamSource(sourceStream);

                var target = new OsmSharp.Streams.BinaryOsmStreamTarget(targetStream);
                target.RegisterSource(source);
                target.Pull();
            }

            using (var sourceStream = File.OpenRead("test3.osm.bin"))
            using (var targetStream = File.Open("test2.osm.pbf", FileMode.Create))
            {
                var source = new OsmSharp.Streams.BinaryOsmStreamSource(sourceStream);

                var target = new OsmSharp.Streams.PBFOsmStreamTarget(targetStream);
                target.RegisterSource(source);
                target.Pull();
            }
        }
    }
}
