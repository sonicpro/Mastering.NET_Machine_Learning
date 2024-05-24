let input = [| 1, 1.; 2, 2.; 3, 2.25; 4, 4.75; 5, 5 |]

let x = Array.map(fun (x, y) -> float x) input
let y = Array.map(fun (x, y) -> y) input

#r "nuget:Accord, 3.8.0"
#r "nuget:Accord.Statistics, 3.8.0"
#r "nuget:Accord.Math, 3.8.0"

open Accord.Statistics.Models.Regression.Linear
open Accord.Math.Optimization.Losses

// Calculating Mean Squared Error for the inputs (x) and outputs (y).
// Use Ordinary Least Squares to learn the regression.
let ols = OrdinaryLeastSquares()
let regression = ols.Learn(x, y)

// Compute predicred values for inputs.
let predicted = regression.Transform(x)

// The Mean Squared Error between the expected that the predited is
let mse = SquareLoss(y).Loss(predicted)
// "Root Of Mean Square Error" - an error measure in the same units 
// as the original numbers.
let rmse = sqrt mse
let intersept = regression.Intercept
let slope = regression.Slope

// Computing the Coefficient of Determination.
// It is squared Pearson's correlation and it is called "r2".
let r2 = regression.CoefficientOfDetermination(x, y)