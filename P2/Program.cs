using System;

namespace P2
{
    class Program
    {
                
        static void Main(string[] args)
        {
            //var ObjectiveFunctionCoefficients = new double[5]{ 9, 5, 4, 3, 2 };
            //var RestrictionNumbers = new double[3, 6] { { 1, -2, 2, 0, 0, 6 }, { 1, 2, 1, 1, 0, 24 }, { 2, 1, -4, 0, 1, 30 } };
            //var RestrictionSigns = new Signs[3] { Signs.LessEquals, Signs.Equal, Signs.Equal };
            var ObjectiveFunctionCoefficients = new double[]{ 6, 2, 3, 5 };
            var RestrictionNumbers = new double[,] { { 3, 1, 0, 2 ,900 }, { 4, 0, 1, 4, 800 }, { 0, 1, 2, 1, 600 } };
            var RestrictionSigns = new Signs[3] { Signs.LessEquals, Signs.LessEquals, Signs.LessEquals };


            var simplexMethod = new SimplexMethod(ObjectiveFunctionCoefficients, RestrictionNumbers, RestrictionSigns);
            
            var print = new SimplexTable.PrintSimplex();

            simplexMethod.Solution(print.PrintInitial, print.PrintCanonical, print.Print);
    }
    }
}
