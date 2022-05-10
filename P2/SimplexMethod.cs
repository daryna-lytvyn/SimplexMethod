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
        public double[] ObjectiveFunctionCoefficients { get; } //Коэффиценты функции-цели

        public double[,] RestrictionNumbers { get; } // Ограничения коєффициенты + правая часть

        public Signs[] RestrictionSigns { get; } // знаки в ограничениях

        public SimplexMethod(double[] objectiveFunctionCoefficients, double[,] restrictionNumbers, Signs[] restrictionSigns)
        {
            if (objectiveFunctionCoefficients is null|| restrictionNumbers is null|| restrictionSigns is null)
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

        public SolutionVariants Solution()
        {
            double[] canonicalObjectiveFunctionCoefficients;
            double[,] canonicalRestrictionCoefficients;

            this.ToCanonicalForm(out canonicalObjectiveFunctionCoefficients, out canonicalRestrictionCoefficients);
            this.PrintObjectiveFunction(canonicalObjectiveFunctionCoefficients);
            this.PrintRestriction(canonicalRestrictionCoefficients);

            double[] referenceSolution;
            int[] singleBasis;
            double[] B;
            double[] CBasis;
            double Z = 0;

            this.FindBasis(canonicalObjectiveFunctionCoefficients, canonicalRestrictionCoefficients, out referenceSolution, out singleBasis, out B, out CBasis, out Z);

            double[] assessments;
            double smallestAssessment;
            int smallestAssessmentIndex;

            this.FindAssessments(CBasis, canonicalObjectiveFunctionCoefficients, canonicalRestrictionCoefficients, out assessments, out smallestAssessment, out smallestAssessmentIndex);

            this.PrintNamedNumbers("Оцінки", assessments);
            Console.WriteLine($"\nНаймeнша оцінка = {smallestAssessment}");
            

            var canSolutionBeImproved = this.CanSolutionBeImproved(assessments, smallestAssessment, canonicalRestrictionCoefficients, smallestAssessmentIndex);

            while (canSolutionBeImproved == SolutionVariants.СontinueSolution)
            {

                double[] conditionalVector;
                double smallestConditionalVectorNumber;
                int smallestConditionalVectorNumberIndex;

                this.FindConditionalVector(canonicalRestrictionCoefficients, B, smallestAssessmentIndex, out conditionalVector, out smallestConditionalVectorNumber, out smallestConditionalVectorNumberIndex);
              
                double[,] jordanRestrictionCoefficients;

                this.JordanTpransformationRestrictionCoefficients(out jordanRestrictionCoefficients, canonicalRestrictionCoefficients, smallestAssessmentIndex, smallestConditionalVectorNumberIndex);

                this.PrintRestrictionCoefficients(canonicalRestrictionCoefficients);
                this.PrintNamedNumbers("B", B);
                Console.WriteLine($"\nZ={Z}");
                this.PrintNamedNumbers("Cбаз", CBasis);                
                this.PrintConditionalVector(conditionalVector);
                this.PrintNamedNumbers("Оцінки", assessments);
                Console.WriteLine($"\nНаймeнша оцінка = {smallestAssessment}");
                this.PrintRestrictionCoefficients(jordanRestrictionCoefficients);

                double[] jordanB;
                double jordanZ;
                double[] jordanCBasis;

                this.JordanTpransformationBAndCBasis(out jordanB, out jordanCBasis, out jordanZ, canonicalRestrictionCoefficients, canonicalObjectiveFunctionCoefficients, B, CBasis, smallestAssessmentIndex, smallestConditionalVectorNumberIndex);
                              
                this.FindAssessments(jordanCBasis, canonicalObjectiveFunctionCoefficients, jordanRestrictionCoefficients, out assessments, out smallestAssessment, out smallestAssessmentIndex);
                this.PrintNamedNumbers("Оцінки", assessments);
                Console.WriteLine($"\nНаймeнша оцінка = {smallestAssessment}");

                canSolutionBeImproved = this.CanSolutionBeImproved(assessments, smallestAssessment, canonicalRestrictionCoefficients, smallestAssessmentIndex);
                
                if (canSolutionBeImproved == SolutionVariants.СontinueSolution)
                {
                    for (int i = 0; i < canonicalRestrictionCoefficients.GetUpperBound(0)+1; i++)
                    {
                        for (int j = 0; j < canonicalRestrictionCoefficients.GetUpperBound(1) + 1; j++)
                        {
                            canonicalRestrictionCoefficients[i, j] = jordanRestrictionCoefficients[i, j];
                        }
                    }
                    for (int i = 0; i < B.Length; i++)
                    {
                        B[i] = jordanB[i];
                        CBasis[i] = jordanCBasis[i];
                    }
                    Z = jordanZ;
                }
            }
            return canSolutionBeImproved;
        }
        
        public void Print()
        {
            Console.Write("\n\tZ(X)=");
            for (int i = 0; i < this.ObjectiveFunctionCoefficients.Length; i++)
            {
                if (this.ObjectiveFunctionCoefficients[i] >= 0)
                {
                    Console.Write($"+{this.ObjectiveFunctionCoefficients[i]}X{i+1}");
                    continue;
                }
                Console.Write($"{this.ObjectiveFunctionCoefficients[i]}X{i+1}");
            }

            Console.WriteLine("\n");

            for (int i = 0; i < this.RestrictionNumbers.GetUpperBound(0)+1; i++)
            {
                Console.WriteLine();
                for (int j = 0; j < this.RestrictionNumbers.GetUpperBound(1); j++)
                {
                    if (this.RestrictionNumbers[i, j] >= 0)
                    {
                        Console.Write($"+{this.RestrictionNumbers[i,j]}X{j+1}");
                        continue;
                    }
                    Console.Write($"{this.RestrictionNumbers[i, j]}X{j+1}");
                }
                Console.Write($"={this.RestrictionNumbers[i, this.RestrictionNumbers.GetUpperBound(1)]}");
            }
            Console.WriteLine("\n");
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

        private void PrintRestriction(double[,] restrictionCoefficients)
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
                Console.Write($"={this.RestrictionNumbers[i, this.RestrictionNumbers.GetUpperBound(1)]}");
            }
            Console.WriteLine("\n");
        }

        private void PrintConditionalVector(double[] conditionalVector)
        {
            Console.Write($"\n V=(");
            foreach (var number in conditionalVector)
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

        private void PrintRestrictionCoefficients(double[,] jordanRestrictionCoefficients)
        {
            Console.WriteLine();
            for (int i = 0; i < jordanRestrictionCoefficients.GetUpperBound(0) + 1; i++)
            {
                for (int j = 0; j < jordanRestrictionCoefficients.GetUpperBound(1) + 1; j++)
                {
                    Console.Write($"\t{jordanRestrictionCoefficients[i, j]}"); 
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

        private void ToCanonicalForm(out double[] canonicalObjectiveFunctionCoefficients, out double[,] canonicalRestrictionCoefficients )
        {
            int countVariables= this.RestrictionNumbers.GetUpperBound(1);

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
            canonicalRestrictionCoefficients = new double[this.RestrictionNumbers.GetUpperBound(0)+1,countVariables];

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


                for (int i = 0; i < this.RestrictionNumbers.GetUpperBound(0)+1; i++)
                {
                    for (int j = 0; j < this.RestrictionNumbers.GetUpperBound(1); j++)
                    {
                        canonicalRestrictionCoefficients[i,j] = this.RestrictionNumbers[i,j];
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
            else if(countVariables < this.RestrictionNumbers.GetUpperBound(1))
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

        private void FindBasis(double[] canonicalObjectiveFunctionCoefficients, double[,] canonicalRestrictionCoefficients, out double[] referenceSolution, out int[] singleBasis, out double[] B, out double[] CBasis, out double Z)
        {
            var countOfVariablesDuplicates = new int[canonicalRestrictionCoefficients.GetUpperBound(1) + 1];
            
            for (int i = 0; i < canonicalRestrictionCoefficients.GetUpperBound(0) + 1; i++)
            {                
                for (int j = 0; j < canonicalRestrictionCoefficients.GetUpperBound(1) + 1; j++)
                { 
                    if (canonicalRestrictionCoefficients[i,j] != 0)
                    {
                        countOfVariablesDuplicates[j]++;
                    }
                
                }
            }          

            int sizeSingleBasis = Array.FindAll(countOfVariablesDuplicates, (int x) => x == 1).Length;
            singleBasis = new int[sizeSingleBasis];

            int iSingleBasis = 0;
            for (int i = 0; i < countOfVariablesDuplicates.Length; i++)
            {
                if (countOfVariablesDuplicates[i] == 1)
                {
                    singleBasis[iSingleBasis] = i + 1;
                    iSingleBasis++;
                }
            }

            referenceSolution = new double[canonicalRestrictionCoefficients.GetUpperBound(1) + 1];
            B = new double[singleBasis.Length];
            CBasis = new double[singleBasis.Length];

            int iReferenceSolution = referenceSolution.Length - singleBasis.Length;
            Z = 0;

            for (int j = 0; j < singleBasis.Length; j++)
            {
                for (int i = 0; i < canonicalRestrictionCoefficients.GetUpperBound(0) + 1; i++)
                {
                    if (canonicalRestrictionCoefficients[i, (singleBasis[j] - 1)]==1)
                    {
                        double b = this.RestrictionNumbers[i, RestrictionNumbers.GetUpperBound(1)];
                       
                        referenceSolution[iReferenceSolution] = b;
                        B[i] = b;
                        CBasis[i] = canonicalObjectiveFunctionCoefficients[iReferenceSolution];
                        Z += B[i] * CBasis[i];

                        iReferenceSolution++;
                        break;
                    }

                }
            }

            this.PrintNamedNumbers("Б1", singleBasis);
            this.PrintNamedNumbers("Х1", referenceSolution);
            this.PrintNamedNumbers("B", B);
            this.PrintNamedNumbers("Cбаз", CBasis);
            Console.WriteLine($"\nZ={Z}");
        }
        
        private void FindAssessments(  double[] CBasis, double[] ObjectiveFunctionCoefficients, double[,] RestrictionCoefficients, out double[] Assessments,out double smallestAssessment,out int smallestAssessmentIndex)
        {
            Assessments = new double[RestrictionCoefficients.GetUpperBound(1) + 1];

            for (int j = 0; j < RestrictionCoefficients.GetUpperBound(1) + 1; j++)
            {
                double Z = 0;
                for (int i = 0; i < RestrictionCoefficients.GetUpperBound(0) + 1; i++)
                {
                    Z += (CBasis[i] * RestrictionCoefficients[i, j]);
                }
                Assessments[j] = (Z - ObjectiveFunctionCoefficients[j]);              
            }

            smallestAssessment = Assessments[0];
            smallestAssessmentIndex = 0;

            for (int i = 1; i < Assessments.Length; i++)
            {
                if (smallestAssessment > Assessments[i])
                {
                    smallestAssessment = Assessments[i];
                    smallestAssessmentIndex = i;
                }
            }
        }

        private SolutionVariants CanSolutionBeImproved(double[] assessments, double smallestAssessment, double[,] canonicalRestrictionCoefficients, int smallestAssessmentIndex) {

            if (smallestAssessment >= 0)
            {
                Console.WriteLine("\nРішення покращити не можливо");
                return SolutionVariants.Solution;
            }

            // var countPositiveNumbers = assessments.Where(a => a > 0).Count();
            var countPositiveNumbers = 0;
            for (int i = 0; i < canonicalRestrictionCoefficients.GetUpperBound(0)+1; i++)
            {
                if (canonicalRestrictionCoefficients[i, smallestAssessmentIndex] > 0)
                {
                    countPositiveNumbers++;
                }
            }

            if (countPositiveNumbers == 0)
            {
                Console.WriteLine("\nУ стовпчику немає додатнього числа, тому опорний план необмежений на даному напрямку, немає вершини перетину(немае розв'язку)");
                return SolutionVariants.NoSolution;
            }
            else if (countPositiveNumbers == canonicalRestrictionCoefficients.GetUpperBound(0)+1)
            {
                Console.WriteLine("\nОпорний план, який розглядається оптимальний");
                return SolutionVariants.Solution;
            }

            Console.WriteLine($"\nУ стовпчику є додатне число, продовжуемо розв'язок, знаходимо ще кращий план");
            return SolutionVariants.СontinueSolution;
        }

        private void FindConditionalVector( double [,] canonicalRestrictionCoefficients, double[] B, int smallestAssessmentIndex, out double[] V, out double smallestConditionalVectorNumber, out int smallestConditionalVectorNumberIndex)
        {
            V = new double[canonicalRestrictionCoefficients.GetUpperBound(0) + 1];

            for (int i = 0; i < canonicalRestrictionCoefficients.GetUpperBound(0) + 1; i++)
            {
                if (canonicalRestrictionCoefficients[i, smallestAssessmentIndex] > 0)
                {
                    V[i] = (B[i] / canonicalRestrictionCoefficients[i, smallestAssessmentIndex]);
                }
                else
                {
                    V[i] = -1;
                }
            }

            smallestConditionalVectorNumber = -1;
            for (int i = 0; i < V.Length; i++)
            {
                if (V[i] != -1)
                {
                    smallestConditionalVectorNumber = V[i];
                    break;
                }
            }
            smallestConditionalVectorNumberIndex = 0;

            for (int i = 1; i < V.Length; i++)
            {
                if (V[i] != -1 && smallestConditionalVectorNumber >= V[i]  )
                {
                    smallestConditionalVectorNumber = V[i];
                    smallestConditionalVectorNumberIndex = i;
                }
            }

        }



        private void JordanTpransformationRestrictionCoefficients(out double[,] jordanRestrictionCoefficients, double[,] canonicalRestrictionCoefficients, int smallestAssessmentIndex, int smallestConditionalVectorNumberIndex)
        {
            jordanRestrictionCoefficients = new double[canonicalRestrictionCoefficients.GetUpperBound(0) + 1, canonicalRestrictionCoefficients.GetUpperBound(1) + 1];

            for (int i = 0; i < jordanRestrictionCoefficients.GetUpperBound(0)+1; i++)
            {
                for (int j = 0; j < jordanRestrictionCoefficients.GetUpperBound(1) + 1; j++)
                {
                    
                    if (i!= smallestConditionalVectorNumberIndex)
                    {
                        double multiply = canonicalRestrictionCoefficients[smallestConditionalVectorNumberIndex, j] * canonicalRestrictionCoefficients[i, smallestAssessmentIndex];
                        double divide = multiply / canonicalRestrictionCoefficients[smallestConditionalVectorNumberIndex, smallestAssessmentIndex];
                        jordanRestrictionCoefficients[i, j] = (canonicalRestrictionCoefficients[i, j] - divide);
                    }
                    else
                    {
                        jordanRestrictionCoefficients[i, j] = canonicalRestrictionCoefficients[smallestConditionalVectorNumberIndex, j] / canonicalRestrictionCoefficients[i, smallestAssessmentIndex];
                    }
                }
            }
        }
        
        private void JordanTpransformationBAndCBasis(out double[] jordanB, out double[] jordanCBasis, out double jordanZ, double[,] canonicalRestrictionCoefficients, double[] canonicalObjectiveFunctionCoefficients, double[] B, double[] CBasis, int smallestAssessmentIndex, int smallestConditionalVectorNumberIndex)
        {
            jordanB = new double[B.Length];
            jordanCBasis = new double[CBasis.Length];
            jordanZ = 0;

            for (int i = 0; i < jordanB.Length; i++)
            {
                if (i != smallestConditionalVectorNumberIndex)
                {
                    double multiply = B[smallestConditionalVectorNumberIndex] * canonicalRestrictionCoefficients[i, smallestAssessmentIndex];
                    double divide = multiply / canonicalRestrictionCoefficients[smallestConditionalVectorNumberIndex, smallestAssessmentIndex];
                    jordanB[i] = B[i] - divide;

                    jordanCBasis[i] = CBasis[i];
                }
                else
                {
                    jordanB[i] = B[smallestConditionalVectorNumberIndex] / canonicalRestrictionCoefficients[smallestConditionalVectorNumberIndex, smallestAssessmentIndex];

                    jordanCBasis[i] = canonicalObjectiveFunctionCoefficients[ smallestAssessmentIndex];
                }

                jordanZ += jordanB[i] * jordanCBasis[i];
            }

            this.PrintNamedNumbers("B", jordanB);
            this.PrintNamedNumbers("Cбаз", jordanCBasis);
            Console.WriteLine($"\nZ={jordanZ}");
        }

        
    }

    
}
