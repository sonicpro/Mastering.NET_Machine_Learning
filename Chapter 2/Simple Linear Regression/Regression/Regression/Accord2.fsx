type ProduceReview = { ProductId: int; TotalOrders: float; AvgReviews: float }

let reviews = ResizeArray<ProduceReview>()

[<Literal>]
let connectionString = "data source=(localdb)\\MSSQLLocalDB;initial catalog=Northwind;integrated security=True;"

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
