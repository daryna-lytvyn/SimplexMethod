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

    public delegate void PrintІnitial(double[] ObjectiveFunctionCoefficients, double[,] RestrictionNumbers);
    public delegate void PrintCanonicalForm(SimplexTable table, double[] objectiveFunctionCoefficients, double[,] restrictionCoefficients, double[,] RestrictionNumbers);
    public delegate void PrintTable(SimplexTable table);
    

    public class SimplexMethod
    {
        public double[] ObjectiveFunctionCoefficients { get; } //Коэффиценты функции-цели

        public double[,] RestrictionNumbers { get; } // Ограничения коєффициенты + правая часть

        public Signs[] RestrictionSigns { get; } // знаки в ограничениях

        public SimplexMethod(double[] objectiveFunctionCoefficients, double[,] restrictionNumbers, Signs[] restrictionSigns)
        {
            if (objectiveFunctionCoefficients is null || restrictionNumbers is null || restrictionSigns is null)
            {
                throw new ArgumentNullException();
            }

            this.ObjectiveFunctionCoefficients = new double[objectiveFunctionCoefficients.Length];
            this.RestrictionNumbers = new double[restrictionNumbers.GetUpperBound(0) + 1, restrictionNumbers.GetUpperBound(1) + 1];
            this.RestrictionSigns = new Signs[restrictionSigns.Length];

            for (int i = 0; i < this.ObjectiveFunctionCoefficients.Length; i++)
            {
                this.ObjectiveFunctionCoefficients[i] = objectiveFunctionCoefficients[i];
            }

            for (int i = 0; i < this.RestrictionNumbers.GetUpperBound(0) + 1; i++)
            {
                for (int j = 0; j < this.RestrictionNumbers.GetUpperBound(1) + 1; j++)
                {
                    this.RestrictionNumbers[i, j] = restrictionNumbers[i, j];
                }
            }
            for (int i = 0; i < this.RestrictionSigns.Length; i++)
            {
                this.RestrictionSigns[i] = restrictionSigns[i];
            }

        }

        public SolutionVariants Solution(PrintІnitial printІnitial , PrintCanonicalForm printCanonicalForm,PrintTable printTable)
        {
            printІnitial(ObjectiveFunctionCoefficients, RestrictionNumbers);

            double[] canonicalObjectiveFunctionCoefficients;
            double[,] canonicalRestrictionCoefficients;

            this.ToCanonicalForm(out canonicalObjectiveFunctionCoefficients, out canonicalRestrictionCoefficients);

            var firstTable = new SimplexTable(canonicalObjectiveFunctionCoefficients, canonicalRestrictionCoefficients);

            firstTable.FindBasis(RestrictionNumbers);
            firstTable.FindAssessments();
            firstTable.FindConditionalVector();

            printCanonicalForm(firstTable, canonicalObjectiveFunctionCoefficients, canonicalRestrictionCoefficients, this.RestrictionNumbers);
            printTable(firstTable);

            var canSolutionBeImproved = firstTable.CanSimplexTableBeImproved();

            List<SimplexTable> tableList = new List<SimplexTable>();
            tableList.Add(firstTable);
            int iteration = 0;
            while (canSolutionBeImproved == SolutionVariants.СontinueSolution)
            {
                tableList.Add(tableList[iteration].JordanTpransformation());

                iteration++;

                printTable(tableList[iteration]);

                canSolutionBeImproved = tableList[iteration].CanSimplexTableBeImproved();
            }
            return canSolutionBeImproved;
        }

        private void ToCanonicalForm(out double[] canonicalObjectiveFunctionCoefficients, out double[,] canonicalRestrictionCoefficients)
        {
            int countVariables = this.RestrictionNumbers.GetUpperBound(1);

            for (int i = 0; i < this.RestrictionSigns.Length; i++)
            {
                switch (this.RestrictionSigns[i])
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
            canonicalRestrictionCoefficients = new double[this.RestrictionNumbers.GetUpperBound(0) + 1, countVariables];

            if (countVariables > this.RestrictionNumbers.GetUpperBound(1))
            {
                for (int i = 0; i < this.ObjectiveFunctionCoefficients.Length; i++)
                {
                    canonicalObjectiveFunctionCoefficients[i] = this.ObjectiveFunctionCoefficients[i];
                }

                for (int i = this.ObjectiveFunctionCoefficients.Length; i < countVariables; i++)
                {
                    canonicalObjectiveFunctionCoefficients[i] = 0;
                }


                for (int i = 0; i < this.RestrictionNumbers.GetUpperBound(0) + 1; i++)
                {
                    for (int j = 0; j < this.RestrictionNumbers.GetUpperBound(1); j++)
                    {
                        canonicalRestrictionCoefficients[i, j] = this.RestrictionNumbers[i, j];
                    }
                }

                int col = this.RestrictionNumbers.GetUpperBound(1);

                for (int i = 0; i < this.RestrictionNumbers.GetUpperBound(0) + 1; i++)
                {

                    if (this.RestrictionSigns[i] == Signs.LessEquals)
                    {

                        for (int k = 0; k < this.RestrictionNumbers.GetUpperBound(0) + 1; k++)
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
            else if (countVariables < this.RestrictionNumbers.GetUpperBound(1))
            {
                for (int i = 0; i < countVariables; i++)
                {
                    canonicalObjectiveFunctionCoefficients[i] = this.ObjectiveFunctionCoefficients[i];
                }

                for (int i = 0; i < this.RestrictionNumbers.GetUpperBound(0) + 1; i++)
                {
                    for (int j = 0; j < countVariables; j++)
                    {
                        canonicalRestrictionCoefficients[i, j] = this.RestrictionNumbers[i, j];
                    }
                }
            }
            else
            {
                for (int i = 0; i < this.ObjectiveFunctionCoefficients.Length; i++)
                {
                    canonicalObjectiveFunctionCoefficients[i] = this.ObjectiveFunctionCoefficients[i];
                }

                for (int i = 0; i < this.RestrictionNumbers.GetUpperBound(0) + 1; i++)
                {
                    for (int j = 0; j < this.RestrictionNumbers.GetUpperBound(1); j++)
                    {
                        canonicalRestrictionCoefficients[i, j] = this.RestrictionNumbers[i, j];
                    }
                }
            }
        }
       
    }

    }
