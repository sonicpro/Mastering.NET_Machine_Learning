#r "nuget:System.Data.SqlClient, 4.8.6"
#r "nuget:Accord, 3.8.0"
#r "nuget:Accord.Statistics, 3.8.0"

open System.Data.SqlClient
open Accord.Statistics.Models.Regression.Linear
open Accord.Math.Optimization.Losses

type ProductInfo = { ProductId: int; AvgOrders: float; AvgReviews: float; ListPrice: float }

let productInfos = ResizeArray<ProductInfo> ()

[<Literal>]
let connectionString = "data source=(localdb)\\MSSQLLocalDB;initial catalog=AdventureWorks;integrated security=True;"

let query = """
Select 
   A.ProductID,
   AvgOrders,
   AvgReviews,
   ListPrice
From
    (Select 
        ProductID,
        (Sum(OrderQty) + 0.0)/(Count(Distinct SOH.CustomerID) + 0.0) as AvgOrders,
        Sum(OrderQty) as TotalOrders
    from [Sales].[SalesOrderDetail] as SOD
        inner join [Sales].[SalesOrderHeader] as SOH
            on SOD.SalesOrderID = SOH.SalesOrderID
        inner join [Sales].[Customer] as C
            on SOH.CustomerID = C.CustomerID
    Where C.StoreID is not null
    Group By ProductID) as A
        Inner Join 
    (Select
        ProductID,
        (Sum(Rating) + 0.0) / (Count(ProductID) + 0.0) as AvgReviews
    from [Production].[ProductReview] as PR
    Group By ProductID) as B
        on A.ProductID = B.ProductID
        Inner Join
    (Select
        ProductID,
        ListPrice
    from [Production].[Product]
    ) as C
        On A.ProductID = C.ProductID
"""

let connection = new SqlConnection(connectionString);
let command = new SqlCommand(query, connection);
connection.Open();
let reader = command.ExecuteReader();
while reader.Read() do
  productInfos.Add({ ProductId = reader.GetInt32(0);
    AvgOrders = float (reader.GetDecimal(1)); AvgReviews = float (reader.GetDecimal(2)); ListPrice = float (reader.GetDecimal(3)) });

// Learn the relationship between the bike average reviews and list price and
// its average ordered quantity by a seller customer.
let xs = Seq.toArray (Seq.map (fun productInfo -> [| productInfo.AvgReviews;
  productInfo.ListPrice |]) productInfos)
let y = Seq.toArray (Seq.map (fun productInfo -> productInfo.AvgOrders) productInfos)

let ols = OrdinaryLeastSquares ()
let regression = ols.Learn(xs, y)
// Compute the predicted values.
let predicted = regression.Transform (xs)
// Computing how good our regression in terms of rmse and
// Coefficient of Determination (r2).
let mse = SquareLoss(y).Loss (predicted)
let rmse = sqrt mse
let r2 = regression.CoefficientOfDetermination (xs, y)


