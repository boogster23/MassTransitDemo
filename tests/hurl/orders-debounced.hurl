# First request
POST http://localhost:5142/api/orders/debounced
Content-Type: application/json

{
  "customerNumber": "CUST-HURL-DEBOUNCE-001",
  "amount": 250.00
}

HTTP 202
[Asserts]
jsonpath "$..customerNumber" == "CUST-HURL-DEBOUNCE-001"

# Duplicate request immediately after - should be suppressed by debounce
POST http://localhost:5142/api/orders/debounced
Content-Type: application/json

{
  "customerNumber": "CUST-HURL-DEBOUNCE-001",
  "amount": 250.00
}

HTTP 202

# Another duplicate to confirm it suppresses rapid repeats
POST http://localhost:5142/api/orders/debounced
Content-Type: application/json

{
  "customerNumber": "CUST-HURL-DEBOUNCE-001",
  "amount": 250.00
}

HTTP 202