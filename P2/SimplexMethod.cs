using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2
{
    public enum Signs
    {
        Equal = 0,
        LessEquals = 1,
        MoreEqual = 2
    }

    public enum SolutionVariants
    {
        NoSolution = 0,
        Solution= 1,
        СontinueSolution = 2
    }

    

    public class SimplexMethod 
    {
        private double[] objectiveFunctionCoefficients; //Коэффиценты функции-цели

        private double[,] restrictionNumbers; // Ограничения коєффициенты + правая часть

        private Signs[] restrictionSigns; // знаки в ограничениях

        private readonly ConsoleSimplexOutput write;

        public SimplexMethod(double[] objectiveFunctionCoefficients, double[,] restrictionNumbers, Signs[] restrictionSigns, ConsoleSimplexOutput write)
        {
            if (objectiveFunctionCoefficients is null || restrictionNumbers is null || restrictionSigns is null|| write is null)
            {
                throw new ArgumentNullException();
            }

            this.objectiveFunctionCoefficients = new double[objectiveFunctionCoefficients.Length];
            this.restrictionNumbers = new double[restrictionNumbers.GetUpperBound(0) + 1, restrictionNumbers.GetUpperBound(1) + 1];
            this.restrictionSigns = new Signs[restrictionSigns.Length];

            for (int i = 0; i < this.objectiveFunctionCoefficients.Length; i++)
            {
                this.objectiveFunctionCoefficients[i] = objectiveFunctionCoefficients[i];
            }

            for (int i = 0; i < this.restrictionNumbers.GetUpperBound(0) + 1; i++)
            {
                for (int j = 0; j < this.restrictionNumbers.GetUpperBound(1) + 1; j++)
                {
                    this.restrictionNumbers[i, j] = restrictionNumbers[i, j];
                }
            }
            for (int i = 0; i < this.restrictionSigns.Length; i++)
            {
                this.restrictionSigns[i] = restrictionSigns[i];
            }
            this.write = write;

        }

        public SolutionVariants Solution()
        {
            if (this.write is null )
            {
                throw new ArgumentNullException();
            }
            write.PrintInitialDatas(objectiveFunctionCoefficients, restrictionNumbers);
           

            double[] canonicalObjectiveFunctionCoefficients;
            double[,] canonicalRestrictionCoefficients;

            this.ToCanonicalForm(out canonicalObjectiveFunctionCoefficients, out canonicalRestrictionCoefficients);

            var firstTable = new SimplexTable(canonicalObjectiveFunctionCoefficients, canonicalRestrictionCoefficients);

            firstTable.FindBasis(restrictionNumbers);
            firstTable.FindAssessments();
            firstTable.FindConditionalVector();

            write.PrintCanonical(firstTable, canonicalObjectiveFunctionCoefficients, canonicalRestrictionCoefficients, this.restrictionNumbers);
            write.PrintSimplexTable(firstTable);
           

            var canSolutionBeImproved = firstTable.CanSimplexTableBeImproved();

            List<SimplexTable> tableList = new List<SimplexTable>();

            tableList.Add(firstTable);
            int iteration = 0;

            while (canSolutionBeImproved == SolutionVariants.СontinueSolution)
            {
                tableList.Add(tableList[iteration].JordanTpransformation());

                iteration++;

                write.PrintSimplexTable(tableList[iteration]);

                canSolutionBeImproved = tableList[iteration].CanSimplexTableBeImproved();
            }

            return canSolutionBeImproved;
        }

        private void ToCanonicalForm(out double[] canonicalObjectiveFunctionCoefficients, out double[,] canonicalRestrictionCoefficients)
        {
            int countVariables = this.restrictionNumbers.GetUpperBound(1);

            for (int i = 0; i < this.restrictionSigns.Length; i++)
            {
                switch (this.restrictionSigns[i])
                {
                    case Signs.LessEquals:
                        countVariables++;
                        break;
                    case Signs.MoreEqual:
                        countVariables--;
                        break;
                }
            }

            canonicalObjectiveFunctionCoefficients = new double[countVariables];
            canonicalRestrictionCoefficients = new double[this.restrictionNumbers.GetUpperBound(0) + 1, countVariables];

            if (countVariables > this.restrictionNumbers.GetUpperBound(1))
            {
                Array.Copy(this.objectiveFunctionCoefficients, canonicalObjectiveFunctionCoefficients, this.objectiveFunctionCoefficients.Length);

                for (int i = this.objectiveFunctionCoefficients.Length; i < countVariables; i++)
                {
                    canonicalObjectiveFunctionCoefficients[i] = 0;
                }

                this.CopyArray(this.restrictionNumbers, canonicalRestrictionCoefficients, this.restrictionNumbers.GetUpperBound(0) + 1, this.restrictionNumbers.GetUpperBound(1));
                
                int col = this.restrictionNumbers.GetUpperBound(1);

                for (int i = 0; i < this.restrictionNumbers.GetUpperBound(0) + 1; i++) 
                {

                    if (this.restrictionSigns[i] == Signs.LessEquals)
                    {

                        for (int k = 0; k < this.restrictionNumbers.GetUpperBound(0) + 1; k++)
                        {
                            if (k == i)
                            {
                                canonicalRestrictionCoefficients[k, col] = 1;
                            }
                            else
                            {
                                canonicalRestrictionCoefficients[k, col] = 0;
                            }
                        }

                        col++;
                    }

                }

            }
            else if (countVariables < this.restrictionNumbers.GetUpperBound(1))
            {
                Array.Copy(this.objectiveFunctionCoefficients, canonicalObjectiveFunctionCoefficients, countVariables);
                this.CopyArray(this.restrictionNumbers, canonicalRestrictionCoefficients, this.restrictionNumbers.GetUpperBound(0) + 1, countVariables);
            }
            else
            {
                Array.Copy(this.objectiveFunctionCoefficients, canonicalObjectiveFunctionCoefficients, this.objectiveFunctionCoefficients.Length);
                this.CopyArray(this.restrictionNumbers, canonicalRestrictionCoefficients, this.restrictionNumbers.GetUpperBound(0) + 1, this.restrictionNumbers.GetUpperBound(1));
            }
        }

        private void CopyArray<T>(T[,] array, T[,] arraycopy, int rowLenght, int colLenght)
        {
            if (array is null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (arraycopy == null)
            {
                arraycopy = new T[rowLenght, colLenght];
            }

            for (int i = 0; i < rowLenght; i++)
            {
                for (int j = 0; j < colLenght; j++)
                {
                    arraycopy[i, j] = array[i, j];
                }
            }
        }



    }

    }
