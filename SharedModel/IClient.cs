using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Padi.SharedModel
{
    public interface IClient
    {
        void Submit(string inputPath, string outputPath, int splits, string className, string dllPath);
        
      
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mapContent"></param>
        /// <param name="splitNumber"></param>
        void onSplitDone(string mapContent, int splitNumber);

        /// <summary>
        /// All splits processed and returned
        /// </summary>
        void onJobDone();


       /// <summary>
       /// Returns the content of the file in the given split number
       /// </summary>
       /// <param name="splitNumber"></param>
       /// <returns></returns>
        byte[] returnSplit(int splitNumber);

        /// <summary>
        /// Checks if the client is currently working on a job 
        /// </summary>
        bool hasJob();

    }
}

