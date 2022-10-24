using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;
using Perfolizer.Horology;

namespace ListSerializer
{
	[MemoryDiagnoser]
	[Config(typeof(Config))]
	[RankColumn, AllStatisticsColumn]
	public class Benchmark
	{
		[Params(100, 1000, 10_000, 100_000)]
		public int Count;

		[Params(true, false)]
		public bool FixedDataLength;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		private FileStream readS;
		private FileStream writeS;		
		private FileStream writeSNodeKey;

		private ListRand writeList;
		private ListRand readList;
		
		private ListRandNodeKey writeListNodeKey;

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

		[Benchmark(Description = "ASerializeAlloc")]
		public void SerializeAlloc()
		{
			writeS.Position = 0;
			writeList.Serialize(writeS);
		}

		[Benchmark(Description = "ASerializeNodeKey")]
		public void SerializeNodeKey()
		{
			writeSNodeKey.Position = 0;
			writeListNodeKey.Serialize(writeS);
		}

		//[Benchmark(Description = "BDeserializeAlloc")]
		//public void DeserializeAlloc()
		//{
		//	readS.Position = 0;
		//	readList.Deserialize(readS);
		//}

		[IterationSetup]
		public void SetupIteration()
		{
			readS = CreateFileStream("");

			writeS = CreateFileStream("");			
			writeSNodeKey = CreateFileStream("node");

			writeList = new();			
			writeListNodeKey = new();

			Program.Initialize(writeList, Count, FixedDataLength);			
			Program.Initialize(writeListNodeKey, Count, FixedDataLength);
			readList = new ListRand();
		}

		private FileStream CreateFileStream(string name)
		{
			return new FileStream(Program.FilePath + Count + FixedDataLength + name, new FileStreamOptions()
			{
				Access = FileAccess.Write,
				Mode = FileMode.OpenOrCreate,
				Options = FileOptions.SequentialScan,
				Share = FileShare.ReadWrite,
			});
		}

		[IterationCleanup]
		public void IterationCleanup()
		{
			readS.Dispose();
			writeS.Dispose();
		}
	}

	public class Config : ManualConfig
	{
		public Config()
		{
			//AddJob(Job.MediumRun
			//	.WithLaunchCount(1)
			//	.WithToolchain(InProcessNoEmitToolchain.Instance)
			//	.WithId("InProcess"));

			AddExporter(new CsvExporter(CsvSeparator.CurrentCulture, new BenchmarkDotNet.Reports.SummaryStyle(System.Globalization.CultureInfo.CurrentCulture,
				true,
				SizeUnit.B,
				TimeUnit.Nanosecond,
				false,
				true,
				20,
				RatioStyle.Value)));
		}
	}
}