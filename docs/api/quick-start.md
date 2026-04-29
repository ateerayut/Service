# Quick Start Guide - Customer & Order APIs

## 🚀 Getting Started

The Customer and Order APIs are now available and ready to use!

### Prerequisites
- JWT Bearer token from `/auth/token` endpoint
- Customer must exist before creating orders
- Product must exist before adding to orders

---

## 📋 Quick Reference

### Customer API

#### Create a Customer
```bash
POST /customers
Content-Type: application/json
Authorization: Bearer {token}

{
  "name": "John Doe"
}
```

**Success Response:** `201 Created`
```json
{ "id": "550e8400-e29b-41d4-a716-446655440000" }
```

#### Get All Customers
```bash
GET /customers?page=1&pageSize=20&search=John
Authorization: Bearer {token}
```

**Success Response:** `200 OK` (paginated list)

#### Get Customer by ID
```bash
GET /customers/{customerId}
Authorization: Bearer {token}
```

**Success Response:** `200 OK`
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "John Doe",
  "createDate": "2024-04-28T10:30:00+00:00"
}
```

#### Update Customer
```bash
PUT /customers/{customerId}
Content-Type: application/json
Authorization: Bearer {token}

{
  "name": "John Smith"
}
```

**Success Response:** `204 No Content`

#### Delete Customer
```bash
DELETE /customers/{customerId}
Authorization: Bearer {token}
```

**Success Response:** `204 No Content`

---

### Order API

#### Create an Order
```bash
POST /orders
Content-Type: application/json
Authorization: Bearer {token}

{
  "customerId": "550e8400-e29b-41d4-a716-446655440000"
}
```

**Success Response:** `201 Created`
```json
{ "id": "550e8400-e29b-41d4-a716-446655440100" }
```

#### Get All Orders
```bash
GET /orders?page=1&pageSize=20&customerId=550e8400-e29b-41d4-a716-446655440000
Authorization: Bearer {token}
```

**Success Response:** `200 OK` (paginated list with items)

#### Get Order by ID
```bash
GET /orders/{orderId}
Authorization: Bearer {token}
```

**Success Response:** `200 OK`
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440100",
  "customerId": "550e8400-e29b-41d4-a716-446655440000",
  "createDate": "2024-04-28T11:00:00+00:00",
  "items": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440200",
      "productId": "550e8400-e29b-41d4-a716-446655440300",
      "quantity": 2,
      "unitPrice": 29.99,
      "totalPrice": 59.98
    }
  ]
}
```

#### Add Item to Order
```bash
POST /orders/{orderId}/items
Content-Type: application/json
Authorization: Bearer {token}

{
  "productId": "550e8400-e29b-41d4-a716-446655440300",
  "quantity": 2,
  "unitPrice": 29.99
}
```

**Success Response:** `204 No Content`

#### Delete Order
```bash
DELETE /orders/{orderId}
Authorization: Bearer {token}
```

**Success Response:** `204 No Content`

---

## 🔍 Testing with Scalar UI

Interactive API documentation available at:
```
http://localhost:8080/scalar
```

This provides a user-friendly interface to:
- View all endpoints
- Test requests directly
- See response schemas
- Explore documentation

---

## ✅ Common Workflows

### Workflow 1: Create Customer with Order
```bash
# 1. Create Customer
POST /customers
{ "name": "Alice Johnson" }
# Response: { "id": "cust-id" }

# 2. Create Order for Customer
POST /orders
{ "customerId": "cust-id" }
# Response: { "id": "order-id" }

# 3. Add Item to Order
POST /orders/order-id/items
{ "productId": "prod-id", "quantity": 2, "unitPrice": 49.99 }
# Response: 204 No Content

# 4. View Complete Order
GET /orders/order-id
# Response: Order with all items
```

### Workflow 2: Search and List
```bash
# 1. Search for Customers
GET /customers?search=John&page=1&pageSize=10

# 2. Get Orders for Specific Customer
GET /orders?customerId=cust-id&page=1&pageSize=20

# 3. View Single Order with Details
GET /orders/order-id
```

---

## ⚠️ Error Handling

### Validation Error (400)
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "name": ["Name is required.", "Name cannot exceed 200 characters."]
  }
}
```

### Not Found (404)
```
HTTP/1.1 404 Not Found
```

### Unauthorized (401)
```
HTTP/1.1 401 Unauthorized
Missing or invalid JWT Bearer token
```

---

## 📊 Pagination Parameters

All list endpoints support pagination:
- **`page`** (default: 1) - Page number, starts at 1
- **`pageSize`** (default: 20) - Items per page, max: 100

Paginated responses include:
```json
{
  "items": [...],
  "page": 1,
  "pageSize": 20,
  "totalItems": 45,
  "totalPages": 3,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

---

## 🔐 Authentication

All endpoints require JWT Bearer token:
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

Get token from authentication endpoint (see project auth documentation).

---

## 📚 Full Documentation

For complete API documentation including all parameters, status codes, and examples, see:
- `customer-order.md` - Comprehensive reference
- `implementation-details.md` - Implementation details

---

## 🛠️ Troubleshooting

| Issue | Solution |
|-------|----------|
| 401 Unauthorized | Ensure JWT token is valid and not expired. Get new token from `/auth/token` |
| 404 Not Found | Verify the ID exists. Use list endpoints to find valid IDs |
| 400 Bad Request | Check validation errors in response. Ensure all required fields are provided |
| Customer deletion fails | Ensure no orders reference this customer (or cascade delete is configured) |
| Order creation fails | Verify customer ID exists before creating order |

---

## 📞 Support

For more information:
1. Check endpoint documentation in Scalar UI (`/scalar`)
2. Review `API_CUSTOMER_ORDER_ENDPOINTS.md` for detailed examples
3. Check application logs for detailed error information
4. Verify database connectivity and migrations are applied

---

**Version:** 1.0  
**Last Updated:** 2024-04-28  
**Status:** ✅ Production Ready
