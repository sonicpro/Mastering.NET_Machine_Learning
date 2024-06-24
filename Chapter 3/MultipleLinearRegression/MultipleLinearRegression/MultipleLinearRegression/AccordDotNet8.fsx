
// Applying logistical regression to a Adventure works data.
// We are trying to learn the probability of having a bike a
// high margin price (margin > $800) from it's color.
#r "nuget:System.Data.SqlClient, 4.8.6"
#r "nuget:Accord.Statistics, 3.8.0"

open System.Data.SqlClient
open Accord.Statistics.Analysis

type ProductInfo = { ProductId: int; Color: string; AvgReviews: float;
  Markup: float }
// Converting "Color" field to Categorical data (yes/no field for each of the values).
type ProductInfo' = { ProductId: int; BlackInd: float; BlueInd: float;
  RedInd: float; SilverInd: float; OtherInd: float; AvgReviews: float; HighMargin: float }

let getProductInfo'(productInfo: ProductInfo) =
  { ProductInfo'.ProductId = productInfo.ProductId;
  BlackInd =
  (match productInfo.Color with
    | "Black" -> 1.0
    | _ -> 0.0);
  BlueInd = 
  (match productInfo.Color with
    | "Blue" -> 1.0
    | _ -> 0.0);
  RedInd =
  (match productInfo.Color with
    | "Red" -> 1.0
    | _ -> 0.0);
  SilverInd =
  (match productInfo.Color with
    | "Silver" -> 1.0
    | _ -> 0.0);
  OtherInd =
  (match productInfo.Color with
    | "Silver" -> 0.0
    | "Red" -> 0.0
    | "Blue" -> 0.0
    | "Black" -> 0.0
    | _ -> 1.0);
  AvgReviews = productInfo.AvgReviews;
  HighMargin =
  (match productInfo.Markup > 800.0 with
      | true -> 1.0
      | false -> 0.0);
    }

[<Literal>]
let connectionString = "data source=(localdb)\\MSSQLLocalDB;initial catalog=AdventureWorks;integrated security=True;"

[<Literal>]
let query = """
Select distinct
	P.ProductID,
	P.Color,
	avg(PR.Rating + 0.0) over (partition by PR.ProductID) as AvgReviews,
	(P.ListPrice - P.StandardCost) as MarkUp
from [Production].[Product] as P
	inner join [Sales].[SalesOrderDetail] as SOD on
		P.ProductID = SOD.ProductID
	inner join [Sales].[SalesOrderHeader] as SOH on
		SOD.SalesOrderID = SOH.SalesOrderID
	inner join [Sales].[Customer] as C on
		SOH.CustomerID = C.CustomerID
	Inner Join [Production].[ProductSubcategory] as PS on
		P.ProductSubcategoryID = PS.ProductSubcategoryID
	inner join [Production].[ProductReview] PR on
		P.ProductID = PR.ProductID
--Where PS.ProductCategoryID = 1 and
--	C.StoreID is null
"""

let productInfos = ResizeArray<ProductInfo>()
let connection = new SqlConnection(connectionString)
let command = new SqlCommand(query, connection)
connection.Open()
let reader = command.ExecuteReader()
while reader.Read() do
  productInfos.Add({ ProductId = reader.GetInt32(0);
    Color = string (reader.GetString(1));
    AvgReviews = float (reader.GetDecimal(2));
    Markup = float (reader.GetDecimal(3)); })

let productInfos' = Seq.map (fun pi -> getProductInfo' pi) productInfos
let xs = Seq.toArray (Seq.map (fun pi' -> [| pi'.BlackInd; pi'.BlueInd;
  pi'.RedInd; pi'.SilverInd; pi'.OtherInd; pi'.AvgReviews |]) productInfos')

let y = Seq.toArray (Seq.map (fun pi' -> pi'.HighMargin) productInfos')

let analysis = new LogisticRegressionAnalysis()
analysis.Learn(xs, y)
// If PValue is less than 0.05, the model is valid. For the data from three bike models our ML model is not valid.
let pValue = analysis.ChiSquare.PValue
let blackIndOdds = analysis.Regression.GetOddsRatio (1);
let blueIndOdds = analysis.Regression.GetOddsRatio (2);
let redIndOdds = analysis.Regression.GetOddsRatio (3);
let silverIndOdds = analysis.Regression.GetOddsRatio (4);
let otherIndOdds = analysis.Regression.GetOddsRatio (5);
let ratingsOdds = analysis.Regression.GetOddsRatio (6);
