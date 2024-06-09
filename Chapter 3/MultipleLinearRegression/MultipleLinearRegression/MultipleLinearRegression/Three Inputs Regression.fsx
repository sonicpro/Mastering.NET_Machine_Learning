#r "nuget:System.Data.SqlClient, 4.8.6"
#r "nuget:Accord, 3.8.0"
#r "nuget:Accord.Statistics, 3.8.0"

open System.Data.SqlClient
open Accord.Statistics.Models.Regression.Linear
open Accord.Math.Optimization.Losses

type ProductInfo = { ProductId: int; AvgOrders: float; AvgReviews: float;
  ListPrice: float; Weight: float }

let productInfos = ResizeArray<ProductInfo> ()

[<Literal>]
let connectionString = "data source=(localdb)\\MSSQLLocalDB;initial catalog=AdventureWorks;integrated security=True;"


let query = """
Select 
   A.ProductID,
   AvgOrders,
   AvgReviews,
   ListPrice,
   Weight
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
        ListPrice,
        Weight
    from [Production].[Product]
    Where [Production].[Product].[Weight] is not null
    ) as C
        On A.ProductID = C.ProductID
"""

let connection = new SqlConnection(connectionString);
let command = new SqlCommand(query, connection);
connection.Open();
let reader = command.ExecuteReader();
while reader.Read() do
  productInfos.Add({ ProductId = reader.GetInt32(0);
    AvgOrders = float (reader.GetDecimal(1)); AvgReviews = float (reader.GetDecimal(2));
    ListPrice = float (reader.GetDecimal(3)); Weight = float (reader.GetDecimal(4)) });

// Learning the average sales per seller customer relation on average reviews,
// list price, and weight of a bike.
let xs = Seq.toArray (Seq.map (fun productInfo -> [| productInfo.AvgReviews;
  productInfo.ListPrice; productInfo.Weight |]) productInfos);
let y = Seq.toArray (Seq.map (fun productInfo -> productInfo.AvgOrders) productInfos);
let ols = OrdinaryLeastSquares ()
let regression = ols.Learn(xs, y);
let predicted = regression.Transform (xs);
let mse = SquareLoss(y).Loss (predicted);
let rmse = sqrt mse;
let r2 = regression.CoefficientOfDetermination (xs, y);
