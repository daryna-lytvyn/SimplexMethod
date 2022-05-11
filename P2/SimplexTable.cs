﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2
{
    public class SimplexTable
    {
        private double[] objectiveFunctionCoefficients;

        private double[,] restrictionCoefficients;

        private double[] B;

        private double[] CBasis;

        private double Z;

        private double[] assessments;

        private int smallestAssessmentIndex;

        private double[] conditionalVector;

        private int smallestConditionalVectorNumberIndex;

        private double[] referenceSolution;

        private int[] singleBasis;
              
        public SimplexTable(double[] objectiveFunctionCoefficients, double[,] restrictionCoefficients)
        {
            if (objectiveFunctionCoefficients is null|| restrictionCoefficients is null)
            {
                throw new ArgumentNullException();
            }

            this.objectiveFunctionCoefficients = new double[objectiveFunctionCoefficients.Length];
            Array.Copy(objectiveFunctionCoefficients, this.objectiveFunctionCoefficients, this.objectiveFunctionCoefficients.Length);

            this.restrictionCoefficients = new double[restrictionCoefficients.GetUpperBound(0) + 1, restrictionCoefficients.GetUpperBound(1) + 1];

            for (int i = 0; i < this.restrictionCoefficients.GetUpperBound(0) + 1; i++)
            {
                for (int j = 0; j < this.restrictionCoefficients.GetUpperBound(1) + 1; j++)
                {
                    this.restrictionCoefficients[i, j] = restrictionCoefficients[i, j];
                }
            }
        }
        
        public void FindBasis(double[,] restrictionNumbers)
        {
            if (restrictionNumbers is null)
            {
                throw new ArgumentNullException(nameof(restrictionNumbers));
            }

            var countOfVariablesDuplicates = new int[this.restrictionCoefficients.GetUpperBound(1) + 1];

            for (int i = 0; i < this.restrictionCoefficients.GetUpperBound(0) + 1; i++)
            {
                for (int j = 0; j < this.restrictionCoefficients.GetUpperBound(1) + 1; j++)
                {
                    if (this.restrictionCoefficients[i, j] != 0)
                    {
                        countOfVariablesDuplicates[j]++;
                    }

                }
            }

            int sizeSingleBasis = Array.FindAll(countOfVariablesDuplicates, (int x) => x == 1).Length;
            this.singleBasis = new int[sizeSingleBasis];

            int iSingleBasis = 0;
            for (int i = 0; i < countOfVariablesDuplicates.Length; i++)
            {
                if (countOfVariablesDuplicates[i] == 1)
                {
                    this.singleBasis[iSingleBasis] = i + 1;
                    iSingleBasis++;
                }
            }

            this.referenceSolution = new double[this.restrictionCoefficients.GetUpperBound(1) + 1];
            this.B = new double[this.singleBasis.Length];
            this.CBasis = new double[this.singleBasis.Length];

            int iReferenceSolution = this.referenceSolution.Length - this.singleBasis.Length;
            this.Z = 0;

            for (int j = 0; j < this.singleBasis.Length; j++)
            {
                for (int i = 0; i < this.restrictionCoefficients.GetUpperBound(0) + 1; i++)
                {
                    if (this.restrictionCoefficients[i, (this.singleBasis[j] - 1)] == 1)
                    {
                        double b = restrictionNumbers[i, restrictionNumbers.GetUpperBound(1)]; ;

                        this.referenceSolution[iReferenceSolution] = b;
                        this.B[i] = b;
                        this.CBasis[i] = this.objectiveFunctionCoefficients[iReferenceSolution];
                        this.Z += this.B[i] * this.CBasis[i];

                        iReferenceSolution++;
                        break;
                    }

                }
            }
        }

        public void FindAssessments()
        {
            if (this.CBasis is null)
            {
                throw new ArgumentNullException(nameof(this.CBasis));
            }

            this.assessments = new double[this.restrictionCoefficients.GetUpperBound(1) + 1];

            for (int j = 0; j < this.restrictionCoefficients.GetUpperBound(1) + 1; j++)
            {
                double Z = 0;
                for (int i = 0; i < this.restrictionCoefficients.GetUpperBound(0) + 1; i++)
                {
                    Z += (this.CBasis[i] * this.restrictionCoefficients[i, j]);
                }
                this.assessments[j] = (Z - this.objectiveFunctionCoefficients[j]);
            }

            var smallestAssessment = this.assessments[0];
            this.smallestAssessmentIndex = 0;

            for (int i = 1; i < this.assessments.Length; i++)
            {
                if (smallestAssessment > this.assessments[i])
                {
                    smallestAssessment = this.assessments[i];
                    this.smallestAssessmentIndex = i;
                }
            }
        }

        public void FindConditionalVector( )
        {
            if (this.B is null)
            {
                throw new ArgumentNullException(nameof(this.B));
            }
            this.conditionalVector = new double[this.restrictionCoefficients.GetUpperBound(0) + 1];

            for (int i = 0; i < this.restrictionCoefficients.GetUpperBound(0) + 1; i++)
            {
                if (this.restrictionCoefficients[i, this.smallestAssessmentIndex] > 0)
                {
                    this.conditionalVector[i] = (this.B[i] / this.restrictionCoefficients[i, this.smallestAssessmentIndex]);
                }
                else
                {
                    this.conditionalVector[i] = -1;
                }
            }

            double smallestConditionalVectorNumber = -1;
            for (int i = 0; i < this.conditionalVector.Length; i++)
            {
                if (this.conditionalVector[i] != -1)
                {
                    smallestConditionalVectorNumber = this.conditionalVector[i];
                    break;
                }
            }
            this.smallestConditionalVectorNumberIndex = 0;

            for (int i = 1; i < this.conditionalVector.Length; i++)
            {
                if (this.conditionalVector[i] != -1 && smallestConditionalVectorNumber >= this.conditionalVector[i])
                {
                    smallestConditionalVectorNumber = this.conditionalVector[i];
                    this.smallestConditionalVectorNumberIndex = i;
                }
            }

        }

        public SimplexTable JordanTpransformation()
        {
            var jordanSimplexTable = new SimplexTable(this.objectiveFunctionCoefficients, this.JordanTpransformationRestrictionCoefficients());

            jordanSimplexTable.ReplaceBAndCBasisZ(this.JordanTpransformationBAndCBasisZ());


            jordanSimplexTable.FindAssessments();
            jordanSimplexTable.FindConditionalVector();

            return jordanSimplexTable;
        }

        public SolutionVariants CanSimplexTableBeImproved()
        {
            if (this.assessments is null)
            {
                throw new ArgumentNullException(nameof(this.assessments));
            }

            if (this.assessments[this.smallestAssessmentIndex] >= 0)
            {
                return SolutionVariants.Solution;
            }

            var countPositiveNumbers = 0;
            for (int i = 0; i < this.restrictionCoefficients.GetUpperBound(0) + 1; i++)
            {
                if (this.restrictionCoefficients[i, this.smallestAssessmentIndex] > 0)
                {
                    countPositiveNumbers++;
                }
            }

            if (countPositiveNumbers == 0)
            { 
                return SolutionVariants.NoSolution;
            }
            else if (countPositiveNumbers == this.restrictionCoefficients.GetUpperBound(0) + 1)
            {
                return SolutionVariants.Solution;
            }

            return SolutionVariants.СontinueSolution;
        }

        private void ReplaceBAndCBasisZ((double[], double[], double) BAndCBasisZ)
        {
            this.B = new double[BAndCBasisZ.Item1.Length];
            this.CBasis = new double[BAndCBasisZ.Item2.Length];

            Array.Copy(BAndCBasisZ.Item1, this.B, this.B.Length);
            Array.Copy(BAndCBasisZ.Item2, this.CBasis, this.CBasis.Length);
            this.Z = BAndCBasisZ.Item3;
        }
        
        private double[,] JordanTpransformationRestrictionCoefficients()
        {            

            var jordanRestrictionCoefficients = new double[this.restrictionCoefficients.GetUpperBound(0) + 1, this.restrictionCoefficients.GetUpperBound(1) + 1];

            for (int i = 0; i < jordanRestrictionCoefficients.GetUpperBound(0) + 1; i++)
            {
                for (int j = 0; j < jordanRestrictionCoefficients.GetUpperBound(1) + 1; j++)
                {

                    if (i != this.smallestConditionalVectorNumberIndex)
                    {
                        double multiply = this.restrictionCoefficients[this.smallestConditionalVectorNumberIndex, j] * this.restrictionCoefficients[i, this.smallestAssessmentIndex];
                        double divide = multiply / this.restrictionCoefficients[this.smallestConditionalVectorNumberIndex, this.smallestAssessmentIndex];
                        jordanRestrictionCoefficients[i, j] = (this.restrictionCoefficients[i, j] - divide);
                    }
                    else
                    {
                        jordanRestrictionCoefficients[i, j] = this.restrictionCoefficients[this.smallestConditionalVectorNumberIndex, j] / this.restrictionCoefficients[i, this.smallestAssessmentIndex];
                    }
                }
            }
            return jordanRestrictionCoefficients;
        }
        
        private (double[] jordanB,  double[] jordanCBasis,  double jordanZ) JordanTpransformationBAndCBasisZ( )
        {
            var jordanB = new double[this.B.Length];
            var jordanCBasis = new double[this.CBasis.Length];
            double jordanZ = 0;

            for (int i = 0; i < jordanB.Length; i++)
            {
                if (i != this.smallestConditionalVectorNumberIndex)
                {
                    double multiply = this.B[this.smallestConditionalVectorNumberIndex] * this.restrictionCoefficients[i, this.smallestAssessmentIndex];
                    double divide = multiply / this.restrictionCoefficients[this.smallestConditionalVectorNumberIndex, this.smallestAssessmentIndex];
                    jordanB[i] = this.B[i] - divide;

                    jordanCBasis[i] = this.CBasis[i];
                }
                else
                {
                    jordanB[i] = this.B[this.smallestConditionalVectorNumberIndex] / this.restrictionCoefficients[this.smallestConditionalVectorNumberIndex, this.smallestAssessmentIndex];

                    jordanCBasis[i] = this.objectiveFunctionCoefficients[this.smallestAssessmentIndex];
                }

                jordanZ += jordanB[i] * jordanCBasis[i];
            }

            return ( jordanB, jordanCBasis,jordanZ);
        }


        public class PrintSimplex
        {
            public PrintSimplex() { }
            public void Print(SimplexTable table)
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

            public void PrintCanonical(SimplexTable table, double[] objectiveFunctionCoefficients, double[,] restrictionCoefficients, double[,] restrictionNumbers)
            {
                this.PrintObjectiveFunction(objectiveFunctionCoefficients);
                this.PrintRestriction(restrictionCoefficients, restrictionNumbers);
                this.PrintNamedNumbers("Опорне рішення", table.referenceSolution);
                this.PrintNamedNumbers("Індекси одиничного базису", table.singleBasis);
            }

            public void PrintInitial(double[] ObjectiveFunctionCoefficients, double[,] restrictionNumbers)
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

        }
    }
}
