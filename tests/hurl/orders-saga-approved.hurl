# Submit order eligible for Saga payment approval (Amount <= $1000)
POST http://localhost:5142/api/orders/
Content-Type: application/json

{
  "customerNumber": "CUST-HURL-SAGA-APPROVED-001",
  "amount": 499.00
}

HTTP 202
[Asserts]
jsonpath "$..customerNumber" == "CUST-HURL-SAGA-APPROVED-001"
jsonpath "$..amount" == 499.00
jsonpath "$..orderId" exists

