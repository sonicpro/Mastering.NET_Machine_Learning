// Learn more about F# at https://fsharp.org
// See the 'F# Tutorial' project for more help.

#load "Regression.fs"
open Regression

// Lets regress the linear equation y = A + Bx for the set of data.
let input = [| 1, 1.; 2, 2.; 3, 2.25; 4, 4.75; 5, 5 |]

// Variance for the data is the average of the squaredDifferences.
// A Squared difference is the squared difference between the piece of data
// and the mean of the data.
let variance (values: float seq) =
    let mean = Seq.average values
    let sumOfSquaredDifference = Seq.sumBy (fun x -> (x - mean) ** 2.) values
    let sourceLength = Seq.length values |> float
    sumOfSquaredDifference / sourceLength

// Standard deviation is the square root or the Variance.
let standardDeviation (values: float seq) =
  variance values |> sqrt

// Calculating the standard deviation both for axes and for y-s of the data.
let axes = Seq.map (fun (x, y) -> float x) input
let yies = Seq.map (fun (x, y) -> y) input

// Next we will find how linear the relationship between X and Y is.
// We will compute the Pearson's correlation "r". It is computed by dividing
// the sum of products of x deviation score and y deviation score by
// the square root of the product of sums of squared x deviation score and 
// squared y deviation score.
// "Deviation score" if the difference between the value and the mean.
let pearsonsCorrelation (a: float seq, b: float seq) =
  let meanX = Seq.average a
  let meanY = Seq.average b

  let deviationScoreX = Seq.map (fun x -> x - meanX) a
  let deviationScoreY = Seq.map (fun y -> y - meanY) b

  let tupledDeviationScores = Seq.zip deviationScoreX deviationScoreY
  let sumOfDeviationScoreProducts = Seq.sumBy (fun (x, y)-> x * y) tupledDeviationScores
  let sumOfSquaredx = Seq.sumBy (fun x -> x * x) deviationScoreX
  let sumOfSquaredy = Seq.sumBy (fun y -> y * y) deviationScoreY
  sumOfDeviationScoreProducts / sqrt (sumOfSquaredx * sumOfSquaredy)

let r = pearsonsCorrelation (axes, yies)

// Finally regress the linear equation y = A + bx from the 
// Pearson's correlation, standard deviations, and means.
let standardDeviationAxes = standardDeviation axes
let standardDeviationYies = standardDeviation yies
let meanX = Seq.average axes
let meanY = Seq.average yies

let b = r * (standardDeviationYies / standardDeviationAxes)
let A = meanY - b * meanX