# Request 1
POST http://localhost:5142/api/orders/rate-limited
Content-Type: application/json

{
  "customerNumber": "CUST-HURL-RATE-001",
  "amount": 99.99
}

HTTP 202
[Asserts]
jsonpath "$..customerNumber" == "CUST-HURL-RATE-001"

# Request 2
POST http://localhost:5142/api/orders/rate-limited
Content-Type: application/json

{
  "customerNumber": "CUST-HURL-RATE-001",
  "amount": 99.99
}

HTTP 202
[Asserts]
jsonpath "$..customerNumber" == "CUST-HURL-RATE-001"

# Request 3
POST http://localhost:5142/api/orders/rate-limited
Content-Type: application/json

{
  "customerNumber": "CUST-HURL-RATE-001",
  "amount": 99.99
}

HTTP 202
[Asserts]
jsonpath "$..customerNumber" == "CUST-HURL-RATE-001"

# Request 4
POST http://localhost:5142/api/orders/rate-limited
Content-Type: application/json

{
  "customerNumber": "CUST-HURL-RATE-001",
  "amount": 99.99
}

HTTP 202
[Asserts]
jsonpath "$..customerNumber" == "CUST-HURL-RATE-001"

# Request 5
POST http://localhost:5142/api/orders/rate-limited
Content-Type: application/json

{
  "customerNumber": "CUST-HURL-RATE-001",
  "amount": 99.99
}

HTTP 202
[Asserts]
jsonpath "$..customerNumber" == "CUST-HURL-RATE-001"

# Request 6 - should hit the rate limit
POST http://localhost:5142/api/orders/rate-limited
Content-Type: application/json

{
  "customerNumber": "CUST-HURL-RATE-001",
  "amount": 99.99
}

HTTP 429