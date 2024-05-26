#r "nuget:System.Data.SqlClient, 4.8.6"
#r "nuget:Accord, 3.8.0"
#r "nuget:Accord.Statistics, 3.8.0"
#r "nuget:Accord.Math, 3.8.0"
#r "nuget:FSharp.Data, 6.4.0"

open System.Data.SqlClient
open Accord.Statistics.Models.Regression.Linear
open Accord.Math.Optimization.Losses

type ProductReview = { ProductId: int; TotalOrders: float; AvgReviews: float }

let reviews = ResizeArray<ProductReview>()

[<Literal>]
let connectionString = "data source=(localdb)\\MSSQLLocalDB;initial catalog=AdventureWorks;integrated security=True;"

[<Literal>]
let query = """
  select distinct ProductID,
    sum(OrderQty) over (partition by ProductReviewID) as TotalOrders,
    AvgReviews
  from
  (
    select SOD.ProductID,
      SOD.OrderQty,
      ((sum(PR.Rating) over(partition by SOD.ProductID)) + 0.0) / ((count(PR.ProductID) over(partition by SOD.ProductID)) + 0.0) as AvgReviews,
    ProductReviewID
    from [Sales].[SalesOrderDetail] as SOD
      inner join [Sales].[SalesOrderHeader] as SOH on
        SOD.SalesOrderID = SOH.SalesOrderID
      inner join [Sales].[Customer] as C on
        SOH.CustomerID = C.CustomerID
      inner join [Production].[ProductReview] as PR on
        SOD.ProductID = PR.ProductID
    where
      C.StoreID is not null
  ) as ReviewRating
"""

let connection = new SqlConnection (connectionString)
let command = new SqlCommand(query, connection)
connection.Open()
let reader = command.ExecuteReader ()
while reader.Read () do
  reviews.Add ({ ProductId = reader.GetInt32(0); TotalOrders = float (reader.GetInt32(1)); AvgReviews = float (reader.GetDecimal(2)) })

// Regress the formula of TotalOrders (output) on AvgReviews (input)
// using simple linear regression.
let x = Seq.toArray (Seq.map (fun review -> review.AvgReviews) reviews)
let y = Seq.toArray (Seq.map (fun review -> review.TotalOrders) reviews)
let ols = OrdinaryLeastSquares()
let regression = ols.Learn(x, y)

// Compute predicred values for inputs.
let predicted = regression.Transform(x)

// The Mean Squared Error between the expected that the predited is
let mse = SquareLoss(y).Loss(predicted)
let rmse = sqrt mse
// Computing the Coefficient of Determination.
// It is squared Pearson's correlation and it is called "r2".
let r2 = regression.CoefficientOfDetermination(x, y)