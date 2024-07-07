#r "nuget:Accord.Math, 3.8.0"
#r "nuget:Accord.MachineLearning, 3.8.0"

open Accord;
open Accord.Math;
open Accord.MachineLearning;

// The first number in an input array is the number of hours studying the day before an exam.
// The second number in an input array is the number of beers drank the day before an exam.
let inputs = [| [|5.0; 1.0|]; [|4.5; 1.5|]; [|5.1; 0.75|];
  [|1.0; 3.5|]; [|0.5; 4.0|]; [|1.25; 4.0|] |]

// How did they pass the exam.
let outputs = [| 1; 1; 1; 0; 0; 0 |]
// How many type of values to consider? Two: the hours spent in studying and the quantity of beer consumed.
let classes = 2
// How many data points to use for the calculation? We take three to not mix failed students with the ones
// who passed an exam.
let k = 3
let knn = new KNearestNeighbors(k)
knn.NumberOfClasses = classes
knn.Learn (inputs, outputs, null)

// The seventh student studied 5 hours and drank half a beer the day before an exam.
let input = [| 5.0; 0.5 |]
let output = knn.Decide input