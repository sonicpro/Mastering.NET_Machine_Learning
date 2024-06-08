namespace AdventureWorks.MachineLearning
open System.Data.SqlClient
open Accord.Statistics.Models.Regression.Linear

type internal ProductReview = { ProductId: int; TotalOrders: float; AvgReviews: float }

type public OrderPrediction() =
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

  member this.PredictQuantity(productId: int) =
    use connection = new SqlConnection (connectionString)
    use command = new SqlCommand(query, connection)
    connection.Open ()
    use reader = command.ExecuteReader ()
    while reader.Read () do
      reviews.Add ({ ProductId = reader.GetInt32(0); TotalOrders = float (reader.GetInt32(1)); AvgReviews = float (reader.GetDecimal(2)) })
    // Regress the formula of TotalOrders (output) on AvgReviews (input)
    // using simple linear regression.
    let x = Seq.toArray (Seq.map (fun review -> review.AvgReviews) reviews)
    let y = Seq.toArray (Seq.map (fun review -> review.TotalOrders) reviews)
    let ols = OrdinaryLeastSquares()
    let regression = ols.Learn(x, y)
    let productStats = Seq.find (fun r -> r.ProductId = productId) reviews
    regression.Transform (productStats.AvgReviews)
