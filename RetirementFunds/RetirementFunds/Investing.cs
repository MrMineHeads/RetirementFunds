﻿using Meta.Numerics.Statistics.Distributions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetirementFunds
{
    //This is a public static class that deals with specifically investment oriented calculations.
    //This includes a Monte-Carlo simulation, calculating real dollars, tax considerations, and 
    //customization when it comes to contribuitions.
    public static class Investing
    {
        public static int recurringInvestingFrequency = 1;        
        private static double TIME_STEP = 0.001;
        private static double RATE_STEP = 0.0001;

        public static double PortfolioWeightedAverageReturn(double bondFraction, double stockFraction, double bondReturns, double stockReturns)
        {
            return bondFraction * bondReturns + stockFraction * stockReturns;
        }


        // An interative function that finds the time to meet the goal.
        public static double GetTimeToGoal(decimal goal, decimal principal, decimal payment, double growth, double savingsGrowth)
        {
            double time = 0;
            decimal bal = 0;

            while (bal < goal)
            {
                bal = 0;
                bal += FinanceCalculations.FutureValue(principal, time, growth);

                if (savingsGrowth > 0)
                {
                    bal += FinanceCalculations.FutureVariableAnnuityValue(payment, time, growth, savingsGrowth, 365, 0, recurringInvestingFrequency);
                }
                else
                {
                    bal += FinanceCalculations.FutureFixedAnnuityValue(payment, time, growth, 365, 0, recurringInvestingFrequency);
                }

                time += TIME_STEP;
            }

            return time;
        }

        // An interative function that finds the rate to meet the goal within a given time.
        public static double GetRateOfGrowth(decimal goal, decimal startingBal, decimal initialSavings, double growth, double time)
        {
            decimal bal = startingBal;
            double rate = 0;
            while (bal < goal)
            {
                bal = FinanceCalculations.FutureValue(startingBal, time, rate, 365);
                if (growth > 0)
                {
                    bal += FinanceCalculations.FutureVariableAnnuityValue(initialSavings, time, rate, growth, 365, 0, recurringInvestingFrequency);
                }
                else
                {
                    bal += FinanceCalculations.FutureFixedAnnuityValue(initialSavings, time, rate, 365, 0, recurringInvestingFrequency);
                }
                rate += RATE_STEP;
            }

            return rate;
        }

        // Method used to calculate the financial goal.
        public static string CalculateGoal(double withdrawlRate, double taxRate, decimal currentInvestments, decimal retirementSpeding, decimal initialSavings, double returns, double savings, bool pegToInflation, double inflation)
        {
            double time = 0;
            decimal goal = currentInvestments;
            decimal fix = (decimal)((1 + taxRate) / withdrawlRate);
            decimal adjustedRetirementSpeding = retirementSpeding;

            if (!pegToInflation)
            {
                goal = retirementSpeding * fix;
                return goal.ToString("C0");
            }


            while (goal / fix < adjustedRetirementSpeding)
            {
                goal = currentInvestments;

                if (savings > 0)
                {
                    goal += FinanceCalculations.FutureVariableAnnuityValue(initialSavings, time, returns, savings, 365, 0, recurringInvestingFrequency);
                }
                else 
                {
                    goal += FinanceCalculations.FutureFixedAnnuityValue(initialSavings, time, returns, 365, 0, recurringInvestingFrequency);
                }
                
                adjustedRetirementSpeding = FinanceCalculations.FutureValue(retirementSpeding, time, inflation);
                time++;
            }             

            return goal.ToString("C0");
        }

        // Uses the inverse CDF to calculate a random return for a portfolio with inputs of a standard deviaton (represented as volatility),
        // mean return, and the allocation, all for each specific asset allocation.
        public static double CalculateRandomPortfolioReturn(double bondReturns, double bondVolatility, double bondAllocation, double stockReturns, double stockVolatility, double stockAllocation, Random r)
        {                        
            NormalDistribution bondDistrubtion = new NormalDistribution(bondReturns, bondVolatility);
            NormalDistribution stockDistrubtion = new NormalDistribution(stockReturns, stockVolatility);

            double randomBondReturn = bondDistrubtion.InverseLeftProbability(r.NextDouble());
            double randomStockReturn = stockDistrubtion.InverseLeftProbability(r.NextDouble());

            return PortfolioWeightedAverageReturn(bondAllocation, stockAllocation, randomBondReturn, randomStockReturn); ;
        }        
    }
}
