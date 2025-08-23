using BenchmarkDotNet.Running;
using MetalReleaseTracker.Benchmarks.CatalogSyncService;
using MetalReleaseTracker.Benchmarks.CoreDataService;
using MetalReleaseTracker.Benchmarks.ParserService;

BenchmarkRunner.Run<AlbumProcessedEventConsumerBenchmarks>();