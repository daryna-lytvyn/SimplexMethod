using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2
{
    public interface ISimplexOutput
    {
        public void PrintInitialDatas(double[] ObjectiveFunctionCoefficients, double[,] restrictionNumbers);

        public void PrintCanonical(SimplexTable table, double[] objectiveFunctionCoefficients, double[,] restrictionCoefficients, double[,] restrictionNumbers);

        public void PrintSimplexTable(SimplexTable table);
        

    }
}
