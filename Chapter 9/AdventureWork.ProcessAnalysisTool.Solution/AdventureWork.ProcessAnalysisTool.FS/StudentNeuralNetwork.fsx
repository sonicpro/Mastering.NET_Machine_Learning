#r "nuget: numl"

open numl
open numl.Model
open numl.Supervised.NeuralNetwork

// For "Probability of passing an exam depending on hours of stydying and
// the number of beers we have two input variables.
// The number of inputs ("axons") to a neural network depends on the
// range of values for each input variable.
type Student = { [<Feature>]HoursOfStudy: float;
  [<Feature>]Beer: float;
  [<Label>] mutable PassedExam: bool }

let data = [ { HoursOfStudy=2.0; Beer=3.0; PassedExam=false },
  { HoursOfStudy=3.0;Beer=4.0;PassedExam=false },
  { HoursOfStudy=1.0;Beer=6.0;PassedExam=false },
  { HoursOfStudy=4.0;Beer=5.0;PassedExam=false },
  { HoursOfStudy=6.0;Beer=2.0;PassedExam=true },
  { HoursOfStudy=8.0;Beer=3.0;PassedExam=true },
  { HoursOfStudy=12.0;Beer=1.0;PassedExam=true },
  { HoursOfStudy=3.0;Beer=2.0;PassedExam=true } ]

// Convert F# records to Object instances.
//let data' = Seq.map box data
//let descriptor = Descriptor.Create<Student> ()
let descriptor = Descriptor.New(typeof<Student>)
                              .With("HoursOfStudy").As(typeof<float>)
                              .With("Beer").As(typeof<float>)
                              .Learn("PassedExam").As(typeof<float>);
let generator = NeuralNetworkGenerator ()
// Assigningn a mutable property of an instance of Descriptor.
generator.Descriptor <- descriptor
let model = Learner.Learn (data, 0.80, 100, generator)
