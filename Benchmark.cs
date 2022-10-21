using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;

namespace ListSerializer
{
	[MemoryDiagnoser]
	[Config(typeof(Config))]
	[RankColumn, MinColumn, MaxColumn, Q1Column, Q3Column, AllStatisticsColumn]
	public class Benchmark
	{
		[Params(10, 100, 1000, 10_000, 100_000)]
		public int Count;
		[Params(true, false)]
		public bool FixedDataLength;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		private FileStream readS;
		private FileStream writeS;

		private ListRand writeList;
		private ListRand readList;

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

		[Benchmark(Description = "Serialize")]
		public void Serialize()
		{
			writeS.Position = 0;
			writeList.Serialize(writeS);
		}

		[Benchmark(Description = "Deserialize")]
		public void Deserialize()
		{
			readS.Position = 0;
			readList.Deserialize(readS);
		}

		[IterationSetup]
		public void SetupIteration()
		{
			readS = new FileStream(Program.FilePath + Count, new FileStreamOptions()
			{
				Access = FileAccess.Read,
				Mode = FileMode.OpenOrCreate,
				Options = FileOptions.SequentialScan,
				Share = FileShare.ReadWrite,
			});
			writeS = new FileStream(Program.FilePath + Count, new FileStreamOptions()
			{
				Access = FileAccess.Write,
				Mode = FileMode.OpenOrCreate,
				Options = FileOptions.SequentialScan,
				Share = FileShare.ReadWrite,
			});
			writeList = Program.Initialize(Count, FixedDataLength);
			readList = new ListRand();
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
		}
	}
}