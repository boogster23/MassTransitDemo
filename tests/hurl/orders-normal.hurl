POST http://localhost:5142/api/orders/
Content-Type: application/json

{
  "customerNumber": "CUST-HURL-001",
  "amount": 299.99
}

HTTP 202
[Asserts]
jsonpath "$..customerNumber" == "CUST-HURL-001"
jsonpath "$..amount" == 299.99
jsonpath "$..orderId" exists