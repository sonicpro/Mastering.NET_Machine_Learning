namespace AdventureWorks.MachineLearning
open System.Data.SqlClient
open Accord.Statistics.Models.Regression.Linear

type internal ProductInfo = { ProductId: int; AvgOrders: float; AvgReviews: float; ListPrice: float }

type public OrderPrediction() =
  let reviews = ResizeArray<ProductInfo>()

  [<Literal>]
  let connectionString = "data source=(localdb)\\MSSQLLocalDB;initial catalog=AdventureWorks;integrated security=True;"

  [<Literal>]
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

  member this.PredictQuantity(productId: int) =
    use connection = new SqlConnection (connectionString)
    use command = new SqlCommand(query, connection)
    connection.Open ()
    use reader = command.ExecuteReader ()
    while reader.Read () do
      reviews.Add ({ ProductId = reader.GetInt32(0); AvgOrders = float (reader.GetDecimal(1));
        AvgReviews = float (reader.GetDecimal(2)); ListPrice = float (reader.GetDecimal(3)) })
    // Regress the formula of average orders per saler customer (output) on average reviews and a list price (input)
    // using multiple linear regression.
    let xs = Seq.toArray (Seq.map (fun review -> [| review.AvgReviews; review.ListPrice |]) reviews)
    let y = Seq.toArray (Seq.map (fun review -> review.AvgOrders) reviews)
    let ols = OrdinaryLeastSquares()
    let regression = ols.Learn(xs, y)
    let productInfo = Seq.find (fun r -> r.ProductId = productId) reviews
    // We are using a linear regression with two input variables, so we need to pass an array to Transform().
    let productInputs = [| productInfo.AvgReviews; productInfo.ListPrice; |];
    regression.Transform (productInputs);
