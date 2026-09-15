# Submit order exceeding $1000 threshold to test Saga payment decline & cancellation
POST http://localhost:5142/api/orders/
Content-Type: application/json

{
  "customerNumber": "CUST-HURL-SAGA-DECLINED-001",
  "amount": 1500.00
}

HTTP 202
[Asserts]
jsonpath "$..customerNumber" == "CUST-HURL-SAGA-DECLINED-001"
jsonpath "$..amount" == 1500.00
jsonpath "$..orderId" exists

