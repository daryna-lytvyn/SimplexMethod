using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2
{
    public class ConsoleSimplexOutput : ISimplexOutput
    {

        

        public void PrintCanonical(SimplexTable table, double[] objectiveFunctionCoefficients, double[,] restrictionCoefficients, double[,] restrictionNumbers)
        {
            this.PrintObjectiveFunction(objectiveFunctionCoefficients);
            this.PrintRestriction(restrictionCoefficients, restrictionNumbers);
            this.PrintNamedNumbers("Опорне рішення", table.referenceSolution);
            this.PrintNamedNumbers("Індекси одиничного базису", table.singleBasis);
        }

        public void PrintInitialDatas(double[] ObjectiveFunctionCoefficients, double[,] restrictionNumbers)
        {
            Console.Write("\n\tZ(X)=");
            for (int i = 0; i < ObjectiveFunctionCoefficients.Length; i++)
            {
                if (ObjectiveFunctionCoefficients[i] >= 0)
                {
                    Console.Write($"+{ObjectiveFunctionCoefficients[i]}X{i + 1}");
                    continue;
                }
                Console.Write($"{ObjectiveFunctionCoefficients[i]}X{i + 1}");
            }

            Console.WriteLine("\n");

            for (int i = 0; i < restrictionNumbers.GetUpperBound(0) + 1; i++)
            {
                Console.WriteLine();
                for (int j = 0; j < restrictionNumbers.GetUpperBound(1); j++)
                {
                    if (restrictionNumbers[i, j] >= 0)
                    {
                        Console.Write($"+{restrictionNumbers[i, j]}X{j + 1}");
                        continue;
                    }
                    Console.Write($"{restrictionNumbers[i, j]}X{j + 1}");
                }
                Console.Write($"={restrictionNumbers[i, restrictionNumbers.GetUpperBound(1)]}");
            }
            Console.WriteLine("\n");
        }
        public void PrintSimplexTable(SimplexTable table)
        {
            this.PrintObjectiveFunction(table);
            this.PrintRestrictionCoefficients(table);
            this.PrintNamedNumbers("B", table.B);
            this.PrintNamedNumbers("Cбаз", table.CBasis);
            Console.WriteLine($"\nZ={table.Z}");
            this.PrintNamedNumbers("Оценки", table.assessments);
            this.PrintConditionalVector(table);
        }

        private void PrintObjectiveFunction(SimplexTable table)
        {
            Console.Write("\n\tZ(X)=");
            for (int i = 0; i < table.objectiveFunctionCoefficients.Length; i++)
            {
                if (table.objectiveFunctionCoefficients[i] >= 0)
                {
                    Console.Write($"+{table.objectiveFunctionCoefficients[i]}X{i + 1}");
                    continue;
                }
                Console.Write($"{table.objectiveFunctionCoefficients[i]}X{i + 1}");
            }
            Console.WriteLine("\n");
        }

        private void PrintConditionalVector(SimplexTable table)
        {
            Console.Write($"\n V=(");
            foreach (var number in table.conditionalVector)
            {
                if (number == -1)
                {
                    Console.Write($"-,");
                }
                else
                {
                    Console.Write($"{number},");
                }

            }

            Console.Write(")");
        }

        private void PrintRestrictionCoefficients(SimplexTable table)
        {
            Console.WriteLine();
            for (int i = 0; i < table.restrictionCoefficients.GetUpperBound(0) + 1; i++)
            {
                for (int j = 0; j < table.restrictionCoefficients.GetUpperBound(1) + 1; j++)
                {
                    Console.Write($"\t{table.restrictionCoefficients[i, j]}");
                }
                Console.WriteLine();
            }
        }

        private void PrintNamedNumbers<T>(string name, T[] Numbers)
        {
            Console.Write($"\n{name}=(");
            foreach (var number in Numbers)
            {
                Console.Write($"{number};");
            }

            Console.Write(")");
        }
        private void PrintObjectiveFunction(double[] objectiveFunctionCoefficients)
        {
            Console.Write("\n\tZ(X)=");
            for (int i = 0; i < objectiveFunctionCoefficients.Length; i++)
            {
                if (objectiveFunctionCoefficients[i] >= 0)
                {
                    Console.Write($"+{objectiveFunctionCoefficients[i]}X{i + 1}");
                    continue;
                }
                Console.Write($"{objectiveFunctionCoefficients[i]}X{i + 1}");
            }
            Console.WriteLine("\n");
        }
        private void PrintRestriction(double[,] restrictionCoefficients, double[,] restrictionNumbers)
        {
            for (int i = 0; i < restrictionCoefficients.GetUpperBound(0) + 1; i++)
            {
                Console.WriteLine();
                for (int j = 0; j < restrictionCoefficients.GetUpperBound(1) + 1; j++)
                {
                    if (restrictionCoefficients[i, j] >= 0)
                    {
                        Console.Write($"+{restrictionCoefficients[i, j]}X{j + 1}");
                        continue;
                    }
                    Console.Write($"{restrictionCoefficients[i, j]}X{j + 1}");
                }
                Console.Write($"={restrictionNumbers[i, restrictionNumbers.GetUpperBound(1)]}");
            }
            Console.WriteLine("\n");
        }



    }
}
