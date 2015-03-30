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
    }
}

