#r "nuget:Accord.Statistics, 3.8.0"

open Accord.Statistics.Analysis

// Wikipedia data that shows exam passing values (1.0 - pass, 0.0 - does not) in
// relation to hours spent on studying the day before an exam.
let xs = [| [|0.5|]; [|0.75|]; [|1.0|]; [|1.25|]; [|1.5|]; [|1.75|]; [|1.75|];
  [|2.0|]; [|2.25|]; [|2.5|]; [|2.75|]; [|3.0|]; [|3.25|]; [|3.5|]; [|4.0|];
  [|4.25|]; [|4.5|]; [|4.75|]; [|5.0|]; [|5.5|] |];

let y = [| 0.0; 0.0; 0.0; 0.0; 0.0; 0.0; 1.0; 0.0; 1.0; 0.0;
          1.0; 0.0; 1.0; 0.0; 1.0; 1.0; 1.0; 1.0; 1.0; 1.0 |];

let analysis = new LogisticRegressionAnalysis ();
analysis.Learn (xs, y);
// If pValue is less than 0.05, our model is valid.
let pValue = analysis.ChiSquare.PValue;
// We are passing an index (0) of the intercept value.
// If we do not study at all the day prior to an exam,
// we will have Intercept percent chance of passing the exam (0.017 value means 1.7%).
let coefficientOdds = analysis.Regression.GetOddsRatio (0);
// BTW the odds ratio can be computed by rising Euler's number e to
// the power of a given coefficient.
let coefficientOdds' = 2.718281828 ** analysis.Regression.Intercept;
// To pass an exam for sure we want to study coefficient[1] hours the day before an exam.
let hoursOfStudyingOdds = analysis.Regression.GetOddsRatio (1);
let hoursOfStudyingOdds' = 2.718281828 ** analysis.CoefficientValues.[1]
let coefficients = analysis.CoefficientValues;