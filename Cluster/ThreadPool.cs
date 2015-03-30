using System;
using System.Threading;

delegate void ThrWork();


/*
 * As seen in class solutions:
 * http://groups.tecnico.ulisboa.pt/~meic-padi.daemon/labs/aula2/lab2-solutions.zip
 * 
 */
namespace ThreadPool
{

	class ThrPool
	{
		private CircularBuffer<ThrWork> buf;
		private Thread [] pool;
		public ThrPool(int thrNum, int bufSize)
		{		
			buf = new CircularBuffer<ThrWork>(bufSize);
			pool= new Thread[thrNum];
			for (int i=0; i< thrNum; i++)
			{
				pool[i] = new Thread(new ThreadStart(consomeExec));
				pool[i].Start();
			}
		}

		public void AssyncInvoke(ThrWork action)
		{
			buf.Produce(action);

			//Console.WriteLine("Submitted action");
		}

		public void consomeExec()
		{
			while(true)
			{
				ThrWork tw = buf.Consume();
				tw();
			}
		}
	}
}