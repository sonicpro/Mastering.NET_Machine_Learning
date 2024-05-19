let input = [| 1, 1.; 2, 2.; 3, 2.25; 4, 4.75; 5, 5 |]

let axes = Array.map(fun (x, y) -> float x) input
let yies = Array.map(fun (x, y) -> y) input

#r "nuget:MathNet.Numerics, 5.0.0"
open MathNet.Numerics.Statistics

let standardDeviationX = ArrayStatistics.StandardDeviation axes
let standardDeviationY = ArrayStatistics.StandardDeviation yies

// Calculate Pearson's correlation using MathDotNet.
let r = Correlation.Pearson (axes, yies)

// Calculate the linear dependency y = A + bx regression.
let b = r * (standardDeviationY / standardDeviationX)
let meanX = Array.average axes
let meanY = Array.average yies
let A = meanY - b * meanX

// MathNet.Numerics has a special type Fit that regress the linear dependency based
// on the sample data.
// We don't even need to open MathNet.Numerics.Statistics namespace
// and calculate the means that the Pearson's correlation ourselves.
open MathNet.Numerics
let struct (intercept, slope) = Fit.Line (axes, yies)
