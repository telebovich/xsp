using System;
using NUnit.Framework;
using Mono.WebServer.XSP;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.IO;

namespace Mono.WebServer.Test {
	[TestFixture]
	public class Stress
	{
		[SetUp]
		public void Init ()
		{
			Utilities.LoadAssemblies ();
			Utilities.CopyLoadedAssemblies ();
			Utilities.SetLogToFail ();
		}

		[Test]
		public void ValidConnections ()
		{
			int result = Server.Main (new []{"--applications", "/:.","--port", "9000","--nonstop"});
			Assert.AreEqual (0, result);
			const int count = 1000;
			var threads = new Thread [count];
			try {
				for(int i = 0; i < count; i++) {
					threads [i] = new Thread (() => {
						var wc = new WebClient ();
						string downloaded = wc.DownloadString ("http://localhost:9000/");
						Assert.AreEqual (Environment.CurrentDirectory, downloaded);
					});
					threads [i].Start ();
				}

				foreach (Thread thread in threads)
					thread.Join();
			} catch (WebException e) {
				Assert.Fail(e.Message);
			}
		}

		[Test]
		public void InvalidConnections ()
		{
			int result = Server.Main (new []{"--applications", "/:.","--port", "9000","--nonstop"});
			Assert.AreEqual (0, result);
			const int count = 1000;
			var threads = new Thread [count];
			try {
				for(int i = 0; i < count; i++) {
					threads [i] = new Thread (() => {
						var client = new TcpClient ("localhost", 9000);
						using(var sw = new StreamWriter(client.GetStream()))
							sw.Write("\0\0\0\0");
						client.Close ();
					});
					threads [i].Start ();
				}

				foreach (Thread thread in threads)
					thread.Join();
			} catch (WebException e) {
				Assert.Fail(e.Message);
			}
		}
	}
}

