using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace PACDMConverter
{
	public class Test
	{
		delegate void TestMethod();
		List<int> m_Source = new List<int>();
		System.Diagnostics.Stopwatch m_Watch
				= new System.Diagnostics.Stopwatch();

		public void AllTests()
		{
			for (int i = 0; i < 10000000; i++)
				m_Source.Add(i);
			var _TestFor = new TestMethod(TestFor);
			var _TestForEach = new TestMethod(TestForEach);
			var _TestForEachDelegate = new TestMethod(TestForEachDelegate);
			var _TestLinq = new TestMethod(TestLinq);
			var _TestLambda = new TestMethod(TestLambda);
			var _TestParallel = new TestMethod(TestParallel);

			// how many time to repeat each test?

			for (int i = 1; i < 17; i++)
			{
				TestThis(_TestFor);
				TestThis(_TestForEach);
				TestThis(_TestForEachDelegate);
				TestThis(_TestLinq);
				TestThis(_TestLambda);
				TestThis(_TestParallel);
				Console.WriteLine(string.Empty);
			}

			Console.Read();
		}

		void TestThis(TestMethod test)
		{
			m_Watch.Reset();
			m_Watch.Start();
			test.Invoke();
			var _Time = (int)m_Watch.Elapsed.TotalMilliseconds;
			var _Method = test.Method.Name.Replace("Test", string.Empty);
			var _Result = string.Format("{1}\t{0}", _Method, _Time);
			Console.WriteLine(_Result);
		}

		void TestFor()
		{
			var _Target = new List<int>();
			for (int i = 0; i < m_Source.Count(); i++)
				_Target.Add(m_Source[i] + 1);
			Debug.Assert(m_Source.Count() == _Target.ToArray().Count());
		}

		void TestForEach()
		{
			var _Target = new List<int>();
			foreach (int i in m_Source)
				_Target.Add(i + 1);
			Debug.Assert(m_Source.Count() == _Target.ToArray().Count());
		}

		void TestForEachDelegate()
		{
			var _Target = new List<int>();
			m_Source.ForEach((int i) =>
			{
				_Target.Add(i + 1);
			});
			Debug.Assert(m_Source.Count() == _Target.ToArray().Count());
		}

		void TestLinq()
		{
			var _Target = from i in m_Source
						  select i + 1;
			Debug.Assert(m_Source.Count() == _Target.ToArray().Count());
		}

		void TestLambda()
		{
			var _Target = m_Source.Select(i => i + 1);
			Debug.Assert(m_Source.Count() == _Target.ToArray().Count());
		}

		void TestParallel()
		{
			// TODO
		}
	}
}
