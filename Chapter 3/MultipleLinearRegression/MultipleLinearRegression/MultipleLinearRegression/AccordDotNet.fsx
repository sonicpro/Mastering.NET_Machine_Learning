#r "nuget:Accord.Statistics, 3.8.0"
#r "nuget:Accord.Math, 3.8.0"

open Accord.Statistics.Models.Regression.Linear
open Accord.Math.Optimization.Losses

// Is there a relationship between student's age, student's IQ and their GPA.
let xs = [| [| 15.0; 130.0 |]; [| 18.0; 127.0 |]; [| 15.0; 128.0 |];
  [| 17.0; 120.0 |]; [| 16.0; 115.0 |] |]
// Their GPA:
let y = [| 3.6; 3.5; 3.8; 3.4; 2.6 |]
// Learn from the data to find the fit as an equation in the form a*x0 + b*x1 + c = y
let ols = OrdinaryLeastSquares ()
let regression = ols.Learn(xs, y)
let a = regression.Weights.[0]
let b = regression.Weights.[1]
let c = regression.Intercept

// Compute the predicted values.
let predicted = regression.Transform(xs)

// Computing how good our regression in terms of rmse and
// Coefficient of Determination (r2).
let mse = SquareLoss(y).Loss (predicted)
let rmse = sqrt mse
let r2 = regression.CoefficientOfDetermination (xs, y)

// Regression of the students' GPA on age, IQ, and previous year GPA.
let xs' = [| [| 15.0; 130.0; 3.6 |]; [| 18.0; 127.0; 3.5 |];
  [| 15.0; 128.0; 3.7 |]; [| 17.0; 120.0; 3.5 |];
  [| 17.0; 120.0; 2.5 |] |]

let ols' = OrdinaryLeastSquares ()
let regression' = ols.Learn(xs', y)
let a' = regression'.Weights.[0]
let b' = regression'.Weights.[1]
let c' = regression'.Weights.[2]
let d' = regression'.Intercept
let predicted' = regression'.Transform(xs')
let mse' = SquareLoss(y).Loss (predicted')
let rmse' = sqrt mse'
let r2' = regression'.CoefficientOfDetermination (xs', y)
// For three-parameter model the rmse is 0.46, which is about 1.15% error (maximum GPA is 4).
// r2 is about 99%, that means that we can explain 99% GPA based on
// on a student's age, IQ, and the last year GPA.