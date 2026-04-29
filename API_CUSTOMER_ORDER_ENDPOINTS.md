# Customer and Order API Endpoints

This document provides a comprehensive guide to the new Customer and Order REST API endpoints.

## Table of Contents
- [Customer Endpoints](#customer-endpoints)
- [Order Endpoints](#order-endpoints)
- [Authentication](#authentication)
- [Error Handling](#error-handling)
- [Examples](#examples)

---

## Customer Endpoints

All Customer endpoints require authentication via JWT Bearer token and are prefixed with `/customers`.

### List Customers
**GET** `/customers?page=1&pageSize=20&search=John`

List all customers with optional pagination and search filtering.

**Query Parameters:**
- `page` (optional, default: 1): Page number for pagination
- `pageSize` (optional, default: 20): Number of items per page (max: 100)
- `search` (optional): Search by customer name

**Response:**
```json
{
  "items": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "name": "John Doe",
      "createDate": "2024-04-28T10:30:00+00:00"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalItems": 1,
  "totalPages": 1,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

**Status Codes:**
- `200 OK`: Successfully retrieved customers

---

### Get Customer by ID
**GET** `/customers/{id}`

Retrieve a specific customer by their ID.

**Path Parameters:**
- `id` (required): Customer GUID

**Response:**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "John Doe",
  "createDate": "2024-04-28T10:30:00+00:00"
}
```

**Status Codes:**
- `200 OK`: Customer found
- `404 Not Found`: Customer does not exist

---

### Create Customer
**POST** `/customers`

Create a new customer.

**Request Body:**
```json
{
  "name": "Jane Smith"
}
```

**Response:**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440001"
}
```

**Status Codes:**
- `201 Created`: Customer successfully created
- `400 Bad Request`: Validation error (invalid name)

**Validation Rules:**
- `name`: Required, max 200 characters

---

### Update Customer
**PUT** `/customers/{id}`

Update an existing customer's information.

**Path Parameters:**
- `id` (required): Customer GUID

**Request Body:**
```json
{
  "name": "Jane Doe Smith"
}
```

**Status Codes:**
- `204 No Content`: Customer successfully updated
- `404 Not Found`: Customer does not exist
- `400 Bad Request`: Validation error

---

### Delete Customer
**DELETE** `/customers/{id}`

Delete a customer. Note: This will also delete all associated orders if cascade delete is configured.

**Path Parameters:**
- `id` (required): Customer GUID

**Status Codes:**
- `204 No Content`: Customer successfully deleted
- `404 Not Found`: Customer does not exist

---

## Order Endpoints

All Order endpoints require authentication via JWT Bearer token and are prefixed with `/orders`.

### List Orders
**GET** `/orders?page=1&pageSize=20&customerId=550e8400-e29b-41d4-a716-446655440000`

List all orders with optional filtering by customer and pagination.

**Query Parameters:**
- `page` (optional, default: 1): Page number for pagination
- `pageSize` (optional, default: 20): Number of items per page (max: 100)
- `customerId` (optional): Filter orders by customer ID

**Response:**
```json
{
  "items": [
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
  ],
  "page": 1,
  "pageSize": 20,
  "totalItems": 1,
  "totalPages": 1,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

**Status Codes:**
- `200 OK`: Successfully retrieved orders

---

### Get Order by ID
**GET** `/orders/{id}`

Retrieve a specific order by its ID, including all order items.

**Path Parameters:**
- `id` (required): Order GUID

**Response:**
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

**Status Codes:**
- `200 OK`: Order found
- `404 Not Found`: Order does not exist

---

### Create Order
**POST** `/orders`

Create a new order for a customer.

**Request Body:**
```json
{
  "customerId": "550e8400-e29b-41d4-a716-446655440000"
}
```

**Response:**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440100"
}
```

**Status Codes:**
- `201 Created`: Order successfully created
- `400 Bad Request`: Validation error (invalid customerId)

**Validation Rules:**
- `customerId`: Required, must be a valid GUID

---

### Add Order Item
**POST** `/orders/{id}/items`

Add an item to an existing order.

**Path Parameters:**
- `id` (required): Order GUID

**Request Body:**
```json
{
  "productId": "550e8400-e29b-41d4-a716-446655440300",
  "quantity": 2,
  "unitPrice": 29.99
}
```

**Status Codes:**
- `204 No Content`: Item successfully added to order
- `404 Not Found`: Order does not exist
- `400 Bad Request`: Validation error

**Validation Rules:**
- `productId`: Required, must be a valid GUID
- `quantity`: Required, must be greater than 0
- `unitPrice`: Required, must be greater than 0

---

### Delete Order
**DELETE** `/orders/{id}`

Delete an order and all its associated items.

**Path Parameters:**
- `id` (required): Order GUID

**Status Codes:**
- `204 No Content`: Order successfully deleted
- `404 Not Found`: Order does not exist

---

## Authentication

All endpoints require a valid JWT Bearer token in the Authorization header:

```
Authorization: Bearer {token}
```

Tokens can be obtained from the `/auth/token` endpoint. See the project's authentication documentation for details on token generation and refresh.

---

## Error Handling

### Validation Error Response
When validation fails, the API returns a 400 Bad Request with problem details:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Name": [
      "Name is required."
    ]
  }
}
```

### Not Found Response
When a resource is not found, the API returns a 404 Not Found:

```
HTTP/1.1 404 Not Found
```

---

## Examples

### Example 1: Create a Customer and Order

1. **Create a Customer:**
```bash
curl -X POST http://localhost:8080/customers \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{"name": "Alice Johnson"}'
```

Response:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000"
}
```

2. **Create an Order for the Customer:**
```bash
curl -X POST http://localhost:8080/orders \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{"customerId": "550e8400-e29b-41d4-a716-446655440000"}'
```

Response:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440100"
}
```

3. **Add Items to the Order:**
```bash
curl -X POST http://localhost:8080/orders/550e8400-e29b-41d4-a716-446655440100/items \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "productId": "550e8400-e29b-41d4-a716-446655440300",
    "quantity": 3,
    "unitPrice": 49.99
  }'
```

Response:
```
HTTP/1.1 204 No Content
```

4. **Retrieve the Complete Order:**
```bash
curl -X GET http://localhost:8080/orders/550e8400-e29b-41d4-a716-446655440100 \
  -H "Authorization: Bearer {token}"
```

Response:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440100",
  "customerId": "550e8400-e29b-41d4-a716-446655440000",
  "createDate": "2024-04-28T11:00:00+00:00",
  "items": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440200",
      "productId": "550e8400-e29b-41d4-a716-446655440300",
      "quantity": 3,
      "unitPrice": 49.99,
      "totalPrice": 149.97
    }
  ]
}
```

---

## API Documentation

The API documentation is available at:
- **OpenAPI/Swagger UI**: `http://localhost:8080/scalar`
- **OpenAPI Spec**: `http://localhost:8080/openapi/v1.json`

Access these endpoints after authentication to explore the API interactively.
