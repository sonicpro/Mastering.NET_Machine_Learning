#r "nuget:Accord.Math, 3.8.0"
#r "nuget:Accord.MachineLearning, 3.8.0"

open Accord.MachineLearning.Bayes;

// Applying Naïve Bayes classification model to a "Do we play tennis today?"
// decision tree.
// The inputs are like the following: [outlook; temperature; humidity; wind].
// The values a turned into integers like this:
// Outlook ID
//------------
// Sunny    0
// Overcast 1
// Rain     2
//
// Temperature ID
//---------------
// Hot      0
// Mild     1
// Cool     2
//
// Humidity ID
//-------------
// High     0
// Normal   1
//
// Wind     ID
//-------------
// Weak     0
// String   1
let inputs: int[][] = [| [|0;0;0;0|]; [|0;0;0;1|]; [|1;0;0;0|]; [|2;1;0;0|];
  [|2;2;1;0|]; [|2;2;1;1;|]; [|1;2;1;1|]; [|0;1;0;0|]; [|0;2;1;0|];
  [|2;1;1;0|]; [|0;2;1;1;|]; [|1;1;0;1|]; [|1;0;1;0|]; [|2;1;0;1|] |];
// The outputs mean 0 - no tennis today, 1 - we are playing today.
let outputs = [| 0;0;1;1;1;0;1;0;1;1;1;1;1;0 |]

// For Accors.MachineLearning.Bayes class we need to provide the "symbols"
// value. That is the -arity for every input feature, i.e. that is 3
// for "Outlook" and "Temperature" features and 2 for "Humidity" and "Wind".
let symbols = [| 3;3;2;2 |]
let bayes = new NaiveBayes (4, symbols)
let error = bayes.Estimate (inputs, outputs)
// The above line returns 0.142857. We have 14% failure rate for our predictions.

// Will we go tennis if the weather is sunny, mild temperature, normal humidity,
// and weak wind?
let input = [|0;1;1;0|]
let output = bayes.Decide input
// return 1 - we are most likely go tennis.