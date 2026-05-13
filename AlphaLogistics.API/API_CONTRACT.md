# AlphaLogistics API — Contract Document

**Version:** 1.0
**Date:** 2026-05-13
**Contact:** support@alphalogistics.com

---

## Table of Contents

1. [Overview](#overview)
2. [Base URL](#base-url)
3. [Authentication](#authentication)
4. [Authorization Roles](#authorization-roles)
5. [Standard Response Format](#standard-response-format)
6. [HTTP Status Codes](#http-status-codes)
7. [Enumerations & Reference Values](#enumerations--reference-values)
8. [Endpoints](#endpoints)
   - [User](#user-endpoints)
   - [Product](#product-endpoints)
   - [Cart](#cart-endpoints)
   - [Order](#order-endpoints)
   - [Dashboard](#dashboard-endpoints)
9. [Data Models](#data-models)
10. [File Upload Rules](#file-upload-rules)

---

## Overview

AlphaLogistics API is a RESTful backend for a multi-vendor e-commerce logistics platform. It supports vendor onboarding, product catalogue management, shopping cart operations, order processing with status tracking, and analytics reporting.

**Technology:** ASP.NET Core 8.0 · PostgreSQL · Cookie-based Authentication

---

## Base URL

```
https://<host>/api
```

All endpoints follow the pattern:

```
/api/{Controller}/{Action}
```

Interactive documentation is available at `/swagger`.

---

## Authentication

The API uses **cookie-based authentication**.

- On a successful `POST /api/User/Login` the server sets an encrypted HTTP-only cookie (`AlphaLogisticsCookie`).
- The cookie has a **30-day sliding expiration**.
- All subsequent requests to protected endpoints must carry this cookie automatically (browser) or explicitly (API clients).
- Call `POST /api/User/Logout` to invalidate the session.

### Login Request

```http
POST /api/User/Login
Content-Type: application/json
```

```json
{
  "email": "user@example.com",
  "password": "secret123",
  "rememberMe": false
}
```

### Login Response (200 OK)

```json
{
  "statusCode": 200,
  "message": "Login successful",
  "data": {
    "id": 1,
    "userName": "John Doe",
    "email": "user@example.com",
    "role": "Admin"
  }
}
```

---

## Authorization Roles

| Role       | Description                                       |
|------------|---------------------------------------------------|
| SuperAdmin | Full platform access including role management    |
| Admin      | Platform management, vendor approvals, reporting  |
| Vendor     | Manage own products, view own orders              |
| Customer   | Browse products, manage cart, place orders        |

---

## Standard Response Format

Every endpoint returns a uniform envelope:

```json
{
  "statusCode": 200,
  "message": "string",
  "data": <object | array | null>
}
```

| Field      | Type           | Description                        |
|------------|----------------|------------------------------------|
| statusCode | integer        | Mirrors the HTTP status code       |
| message    | string         | Human-readable result description  |
| data       | object / array | Payload; `null` on errors          |

---

## HTTP Status Codes

| Code | Meaning              | When Used                                    |
|------|----------------------|----------------------------------------------|
| 200  | OK                   | Successful read / update / action            |
| 201  | Created              | Resource created successfully                |
| 400  | Bad Request          | Validation failure or bad input              |
| 401  | Unauthorized         | Missing or invalid authentication cookie     |
| 409  | Conflict             | Duplicate resource (e.g. email, PAN)         |
| 500  | Internal Server Error| Unhandled server-side exception              |

---

## Enumerations & Reference Values

### Order Statuses

| ID | Label           |
|----|-----------------|
| 1  | Pending         |
| 2  | Confirmed       |
| 3  | Processing      |
| 4  | Packed          |
| 5  | Shipped         |
| 6  | InTransit       |
| 7  | OutForDelivery  |
| 8  | Delivered       |
| 9  | Cancelled       |
| 10 | Returned        |
| 11 | Refunded        |

Runtime source: `GET /api/Order/GetOrderStatuses`

### Payment Options

| ID | Label         |
|----|---------------|
| 1  | COD           |
| 2  | Bank Transfer |
| 3  | Wallet Pay    |
| 4  | QR Pay        |

Runtime source: `GET /api/Order/PaymentOptions`

### Product Sizes

`S`, `M`, `L`, `XL`, `XXL`

Runtime source: `GET /api/Product/GetProductSize`

### Product Colours

`Red`, `Blue`, `Green`, `Yellow`, `Orange`, `Purple`, `Pink`, `Brown`, `Black`, `White`

Runtime source: `GET /api/Product/GetProductColour`

---

## Endpoints

---

### User Endpoints

Base route: `/api/User`

---

#### POST /RegisterCustomer
Register a new customer account.
**Auth:** None required

**Request Body (`application/json`):**

```json
{
  "name": "string",
  "email": "string (optional)",
  "phone": "string",
  "address": "string",
  "pradeshId": 1,
  "password": "string",
  "isActive": true
}
```

**Response 201:**

```json
{
  "statusCode": 201,
  "message": "Customer registered successfully",
  "data": { "id": 5 }
}
```

---

#### PUT /UpdateCustomer
Update the authenticated customer's profile.
**Auth:** Required

**Request Body (`application/json`):** Same fields as `RegisterCustomer` (include `id`).

---

#### GET /GetCustomerList
Retrieve all customers.
**Auth:** Required

**Response 200:**

```json
{
  "statusCode": 200,
  "message": "Success",
  "data": [ { "id": 1, "name": "...", "email": "...", "phone": "...", "address": "..." } ]
}
```

---

#### GET /GetCustomerById
Get a customer by ID.
**Auth:** Required

| Query Parameter | Type    | Required | Description      |
|-----------------|---------|----------|------------------|
| customerId      | integer | Yes      | Customer user ID |

---

#### GET /ActivePradeshList
Get all active delivery regions (Pradesh).
**Auth:** None required

**Response 200:**

```json
{
  "data": [ { "id": 1, "name": "Bagmati", "charge": 100.00, "isFixed": true } ]
}
```

---

#### POST /Register
Register an internal user (Admin, Finance, etc.).
**Auth:** None required (protected by business logic)

**Request (`multipart/form-data`):**

| Field        | Type    | Required | Validation               |
|--------------|---------|----------|--------------------------|
| userName     | string  | Yes      |                          |
| email        | string  | Yes      | Valid email              |
| password     | string  | Yes      | Min 6 characters         |
| phone        | string  | Yes      |                          |
| roleId       | integer | Yes      | Must be a valid role ID  |
| profileImage | file    | No       | Image file               |

---

#### POST /Login
Authenticate and receive a session cookie.
**Auth:** None required

**Request Body (`application/json`):**

```json
{
  "email": "user@example.com",
  "password": "string",
  "rememberMe": false
}
```

---

#### GET /GetUserById/{id}
Get a user's details by their ID.
**Auth:** Required

| Path Parameter | Type    | Required |
|----------------|---------|----------|
| id             | integer | Yes      |

---

#### GET /GetAllUsers
Paginated list of users, optionally filtered by role.
**Auth:** Required

| Query Parameter | Type    | Required | Default |
|-----------------|---------|----------|---------|
| roleId          | integer | No       | –       |
| page            | integer | No       | 1       |
| pageSize        | integer | No       | 10      |

---

#### PUT /UpdateUser/{id}
Update a user's profile.
**Auth:** Required

| Path Parameter | Type    | Required |
|----------------|---------|----------|
| id             | integer | Yes      |

---

#### POST /Logout
Invalidate the current session.
**Auth:** Required

---

#### GET /GetCurrentUser
Return the currently authenticated user's profile.
**Auth:** Required

---

#### GET /GetActiveRoles
Get all active system roles.
**Auth:** None required

---

#### POST /register (Vendor)
Register a new vendor.
**Auth:** None required

**Request (`multipart/form-data`):**

| Field           | Type     | Required | Validation                        |
|-----------------|----------|----------|-----------------------------------|
| email           | string   | Yes      | Valid email                       |
| password        | string   | Yes      | Min 6 characters                  |
| phone           | string   | Yes      | Valid phone                       |
| address         | string   | Yes      |                                   |
| vendorName      | string   | Yes      |                                   |
| contactPerson   | string   | Yes      |                                   |
| pan             | string   | Yes      | Unique                            |
| vat             | string   | No       |                                   |
| bankAccountNo   | string   | Yes      |                                   |
| bankName        | string   | Yes      |                                   |
| accHolderName   | string   | Yes      |                                   |
| primaryAddress  | string   | Yes      |                                   |
| secondaryAddress| string   | No       |                                   |
| description     | string   | No       |                                   |
| customerType    | string   | No       | Default: `"Basic"`                |
| profileImage    | file     | No       |                                   |
| documents       | file[]   | No       | Array of `{documentName, documentFile}` |
| acceptTerms     | boolean  | Yes      | Must be `true`                    |

---

#### PUT /UpdateVendor/{id}
Update vendor details.
**Auth:** Required

| Path Parameter | Type    | Required |
|----------------|---------|----------|
| id             | integer | Yes      |

---

#### PATCH /VendorApprovalUpdate/{vendorId}
Approve or reject a vendor.
**Auth:** Required (Admin / SuperAdmin only)

| Path Parameter | Type    | Required |
|----------------|---------|----------|
| vendorId       | integer | Yes      |

**Request Body (`application/json`):**

```json
{
  "isApproved": true,
  "reason": "string (required if rejecting)"
}
```

---

#### POST /GetAllVendors
Query vendors with optional filters.
**Auth:** None required

**Request Body (`application/json`):**

```json
{
  "searchTerm": "string",
  "isApproved": true,
  "page": 1,
  "pageSize": 10
}
```

---

#### POST /GetActiveVendor
Get all approved (active) vendors.
**Auth:** None required

---

#### GET /GetVendorById/{vendorId}
Get a single vendor's details.
**Auth:** Required

---

#### DELETE /DeleteVendor/{vendorId}
Soft-delete a vendor.
**Auth:** Required (Admin)

---

#### POST /RestoreVendor/{vendorId}
Restore a soft-deleted vendor.
**Auth:** Required (Admin)

---

#### GET /GetVendorDocuments/{vendorId}
Retrieve all documents for a vendor.
**Auth:** Required

---

### Product Endpoints

Base route: `/api/Product`

---

#### POST /CreateProduct
Create a new product.
**Auth:** Required

| Query Parameter | Type    | Required | Description                  |
|-----------------|---------|----------|------------------------------|
| VendorId        | integer | No       | Associate with specific vendor |

**Request (`multipart/form-data`):**

| Field          | Type     | Required | Validation                          |
|----------------|----------|----------|-------------------------------------|
| productName    | string   | Yes      |                                     |
| description    | string   | Yes      |                                     |
| price          | decimal  | Yes      | > 0                                 |
| costPrice      | decimal  | No       |                                     |
| isComboType    | boolean  | No       | Default: `false`                    |
| comboProductIds| integer[]| No       | Required when `isComboType = true`  |
| stockQuantity  | integer  | Yes      | >= 0                                |
| subCategoryId  | integer  | Yes      |                                     |
| colours        | string   | No       | Comma-separated (e.g. `Red,Blue`)   |
| size           | string   | No       | One of: S, M, L, XL, XXL           |
| productImages  | file[]   | No       |                                     |

---

#### GET /GetProductById/{id}
Get a single product by ID.
**Auth:** None required

---

#### POST /GetAllProducts
Query products with filters and pagination.
**Auth:** None required

**Request Body (`application/json`):**

```json
{
  "searchTerm": "string",
  "categoryId": 1,
  "subCategoryId": 2,
  "minPrice": 0,
  "maxPrice": 5000,
  "page": 1,
  "pageSize": 10
}
```

---

#### POST /GetActiveProducts
Get all approved active products.
**Auth:** None required

---

#### GET /GetProductsByVendor/{vendorId}
Get all products for a vendor.
**Auth:** None required

---

#### GET /GetProductsBySubCategory/{subCategoryId}
Get all products in a subcategory.
**Auth:** None required

---

#### PUT /UpdateProduct/{id}
Update any product (Admin).
**Auth:** Required

---

#### PUT /UpdateVendorProduct/{vendorId}/{productId}
Vendor updates their own product.
**Auth:** Required (Vendor role)

---

#### DELETE /DeleteProduct/{id}
Soft-delete a product.
**Auth:** Required

---

#### DELETE /DeleteVendorProduct/{vendorId}/{productId}
Vendor soft-deletes their own product.
**Auth:** Required (Vendor role)

---

#### POST /RestoreProduct/{id}
Restore a soft-deleted product.
**Auth:** Required (Admin)

---

#### DELETE /DeleteProductPermanently/{id}
Permanently remove a product from the database.
**Auth:** Required (Admin)

---

#### GET /GetProductsByPriceRange
Filter products by price range.
**Auth:** None required

| Query Parameter | Type    | Required |
|-----------------|---------|----------|
| min             | decimal | Yes      |
| max             | decimal | Yes      |

---

#### POST /BulkProductApproval
Approve multiple products at once.
**Auth:** Required

**Request Body (`application/json`):**

```json
{
  "productIds": [1, 2, 3],
  "isApproved": true
}
```

---

#### GET /GetProductSize
Return configured product size options.
**Auth:** None required

---

#### GET /GetProductColour
Return configured product colour options.
**Auth:** None required

---

#### POST /CreateCategory
Create a product category.
**Auth:** Required

**Request Body (`application/json`):**

```json
{
  "name": "string",
  "description": "string"
}
```

---

#### PUT /UpdateCategory
Update an existing category.
**Auth:** Required

**Request Body:** Same as `CreateCategory` with `id` field.

---

#### GET /GetAllCategories
Retrieve all categories.
**Auth:** None required

---

#### GET /GetCategoryById
Get a category by ID.
**Auth:** Required

| Query Parameter | Type    | Required |
|-----------------|---------|----------|
| id              | integer | Yes      |

---

#### POST /CreateSubCategory
Create a subcategory under a category.
**Auth:** Required

**Request Body (`application/json`):**

```json
{
  "categoryId": 1,
  "name": "string",
  "description": "string"
}
```

---

#### PUT /UpdateSubCategory
Update an existing subcategory.
**Auth:** Required

---

#### GET /GetAllSubCategories
Retrieve all subcategories.
**Auth:** None required

---

#### GET /GetAllSubCategoriesByCategoryId
Get subcategories filtered by parent category.
**Auth:** None required

| Query Parameter | Type    | Required |
|-----------------|---------|----------|
| categoryId      | integer | Yes      |

---

#### GET /GetSubCategoryById
Get a subcategory by ID.
**Auth:** Required

| Query Parameter | Type    | Required |
|-----------------|---------|----------|
| id              | integer | Yes      |

---

### Cart Endpoints

Base route: `/api/Cart`
**Auth:** Required for all cart endpoints

---

#### GET /GetMyCart
Get the cart of the currently authenticated user.

**Response 200:**

```json
{
  "data": {
    "items": [
      {
        "cartItemId": 1,
        "productId": 10,
        "productName": "string",
        "quantity": 2,
        "unitPrice": 500.00,
        "subtotal": 1000.00,
        "imageUrl": "string"
      }
    ],
    "total": 1000.00,
    "itemCount": 2
  }
}
```

---

#### GET /GetCartByUserId/{userId}
Get a specific user's cart.

| Path Parameter | Type    | Required |
|----------------|---------|----------|
| userId         | integer | Yes      |

---

#### POST /AddToCart
Add a product to the cart.

**Request Body (`application/json`):**

```json
{
  "productId": 10,
  "quantity": 1
}
```

> `quantity` must be >= 1. Adding an already-present product increases its quantity.

---

#### DELETE /RemoveFromCart/{cartItemId}
Remove a specific cart line item.

| Path Parameter | Type    | Required |
|----------------|---------|----------|
| cartItemId     | integer | Yes      |

---

#### DELETE /RemoveProductFromCart/{productId}
Remove all cart entries for a given product.

| Path Parameter | Type    | Required |
|----------------|---------|----------|
| productId      | integer | Yes      |

---

#### DELETE /ClearCart
Remove all items from the authenticated user's cart.

---

#### GET /GetCartTotal
Return the total monetary value of the cart.

---

#### GET /GetCartItemCount
Return the number of distinct items in the cart.

---

#### GET /GetInactiveCartItems
Return cart items whose associated product has been deactivated.

---

#### POST /MergeCarts/{targetUserId}
Merge the authenticated user's cart into another user's cart.
**Auth:** Required (Admin policy)

| Path Parameter | Type    | Required |
|----------------|---------|----------|
| targetUserId   | integer | Yes      |

---

### Order Endpoints

Base route: `/api/Order`

---

#### POST /PlaceOrder
Create a new order.
**Auth:** None required

**Request Body (`application/json`):**

```json
{
  "isPlacedByAdmin": false,
  "pradeshId": 1,
  "orderNumber": "string (optional)",
  "deliveryCharge": 100.00,
  "deliveryAddress": "string",
  "branch": "string",
  "courierPartner": "string",
  "deliveryType": "string",
  "deliveryInstuctions": "string",
  "remark": "string",
  "totalAmount": 1100.00,
  "orderItems": [
    {
      "productId": 10,
      "productSize": "M",
      "productColour": "Blue",
      "quantity": 2,
      "unitPrice": 500.00
    }
  ]
}
```

---

#### PATCH /UpdateOrder
Update an existing order.
**Auth:** None required

| Query Parameter | Type    | Required |
|-----------------|---------|----------|
| orderId         | integer | Yes      |

**Request Body:** Same structure as `PlaceOrder`.

---

#### POST /AssignPradesh
Assign a delivery Pradesh to an order.
**Auth:** None required

| Query Parameter | Type    | Required |
|-----------------|---------|----------|
| pradeshId       | integer | Yes      |
| orderId         | integer | Yes      |

---

#### POST /OrderList
Paginated and filtered order list.
**Auth:** None required

**Request Body (`application/json`):**

```json
{
  "userId": 1,
  "vendorId": 2,
  "from": "2025-01-01T00:00:00Z",
  "to": "2025-12-31T23:59:59Z",
  "statusId": 1,
  "page": 1,
  "pageSize": 10
}
```

All fields are optional. Omit any field to remove that filter.

---

#### GET /GetOrderById
Get a single order with its items.
**Auth:** None required

| Query Parameter | Type    | Required |
|-----------------|---------|----------|
| orderId         | integer | Yes      |

---

#### GET /ChangeOrderStatus
Transition an order to a new status.
**Auth:** None required

| Query Parameter | Type    | Required |
|-----------------|---------|----------|
| orderId         | integer | Yes      |
| statusId        | integer | Yes      | (see [Order Statuses](#order-statuses)) |

---

#### GET /CancelOrder
Cancel an order.
**Auth:** None required

| Query Parameter | Type    | Required |
|-----------------|---------|----------|
| orderId         | integer | Yes      |

---

#### GET /OrderTracking
Get tracking history for an order.
**Auth:** None required

| Query Parameter | Type    | Required |
|-----------------|---------|----------|
| orderId         | integer | Yes      |

---

#### GET /IsExistingSKU
Check whether a given SKU already exists.
**Auth:** None required

| Query Parameter | Type   | Required |
|-----------------|--------|----------|
| sku             | string | Yes      |

**Response 200:**

```json
{ "data": true }
```

---

#### GET /PrintDeliveryLabel
Generate an HTML delivery label for an order.
**Auth:** None required

| Query Parameter | Type    | Required |
|-----------------|---------|----------|
| orderId         | integer | Yes      |

**Response:** `text/html` document.

---

#### GET /ExportOrdersToExcel
Download all orders as an Excel file.
**Auth:** None required

**Response:** `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet` binary.

---

#### GET /GetOrderStatuses
Return the full list of order status definitions.
**Auth:** None required

---

#### GET /PaymentOptions
Return available payment methods.
**Auth:** None required

---

#### GET /CreateBankTransferProcess
Return bank transfer instructions.
**Auth:** None required

| Query Parameter  | Type    | Required |
|------------------|---------|----------|
| isBanTransfer    | boolean | Yes      |

**Response 200 (when `true`):**

```json
{
  "data": {
    "bankName": "NMB Bank",
    "accountHolderName": "Your Company Name Pvt Ltd",
    "accountNumber": "12345678901234",
    "branch": "Thamel, Kathmandu",
    "verificationTimeoutHours": 24
  }
}
```

---

### Dashboard Endpoints

Base route: `/api/DashBoard`

---

#### GET /VendorMonthlySalesReport
Monthly sales summary for a vendor.
**Auth:** None required

| Query Parameter | Type    | Required |
|-----------------|---------|----------|
| vendorId        | integer | Yes      |

**Response 200:**

```json
{
  "data": [
    { "month": "January", "year": 2025, "totalSales": 45000.00, "orderCount": 120 }
  ]
}
```

---

#### GET /VendorGraphDataWithOrderList
Graph-ready data plus order list for a vendor.
**Auth:** None required

| Query Parameter | Type    | Required |
|-----------------|---------|----------|
| vendorId        | integer | Yes      |

---

## Data Models

### UserMaster

| Field         | Type     | Notes                         |
|---------------|----------|-------------------------------|
| id            | integer  | Primary key                   |
| roleId        | integer  | FK → RoleMaster               |
| userName      | string   |                               |
| email         | string   | Unique                        |
| phone         | string   |                               |
| address       | string   |                               |
| pradeshId     | integer  | FK → PradeshMaster            |
| profileImage  | string   | Relative URL                  |
| isActive      | boolean  | Default: `true`               |
| createdAt     | datetime |                               |
| lastUpdatedAt | datetime |                               |

---

### VendorMaster

| Field           | Type    | Notes                          |
|-----------------|---------|--------------------------------|
| id              | integer | Primary key                    |
| userId          | integer | FK → UserMaster (unique)       |
| vendorName      | string  |                                |
| contactPerson   | string  |                                |
| pan             | string  | Unique                         |
| vat             | string  | Optional                       |
| bankAccountNo   | string  |                                |
| bankName        | string  |                                |
| accHolderName   | string  |                                |
| primaryAddress  | string  |                                |
| secondaryAddress| string  | Optional                       |
| description     | string  | Optional                       |
| isApproved      | boolean | Default: `false`               |
| customerType    | string  | Default: `"Basic"`             |
| isActive        | boolean | Default: `true`                |
| createdAt       | datetime|                                |
| lastUpdatedAt   | datetime|                                |

---

### ProductMaster

| Field         | Type    | Notes                              |
|---------------|---------|------------------------------------|
| id            | integer | Primary key                        |
| vendorId      | integer | FK → VendorMaster                  |
| subCategoryId | integer | FK → SubCategoryMaster             |
| productName   | string  |                                    |
| description   | string  |                                    |
| sku           | string  | Auto-generated, unique             |
| price         | decimal |                                    |
| costPrice     | decimal | Optional                           |
| stockQuantity | integer |                                    |
| colours       | string  | Comma-separated values             |
| size          | string  |                                    |
| isComboType   | boolean | Default: `false`                   |
| isApproved    | boolean | Default: `false`                   |
| isActive      | boolean | Default: `true`                    |
| createdAt     | datetime|                                    |
| lastUpdatedAt | datetime|                                    |

---

### OrderMaster

| Field               | Type    | Notes                                |
|---------------------|---------|--------------------------------------|
| id                  | integer | Primary key                          |
| userId              | integer | FK → UserMaster                      |
| orderNumber         | string  | Unique identifier                    |
| orderDate           | datetime|                                      |
| totalAmount         | decimal |                                      |
| status              | integer | See [Order Statuses](#order-statuses)|
| deliveryCharge      | decimal |                                      |
| deliveryAddress     | string  |                                      |
| branch              | string  |                                      |
| courierPartner      | string  |                                      |
| deliveryType        | string  |                                      |
| deliveryInstuctions | string  |                                      |
| remark              | string  |                                      |
| pradeshId           | integer | FK → PradeshMaster                   |
| deliveryDate        | datetime| Optional                             |
| isPlacedByAdmin     | boolean |                                      |

---

### CartMaster

| Field     | Type    | Notes                          |
|-----------|---------|--------------------------------|
| id        | integer | Primary key                    |
| userId    | integer | FK → UserMaster                |
| productId | integer | FK → ProductMaster             |
| quantity  | integer |                                |
| unitPrice | decimal |                                |
| createdAt | datetime|                                |

> Unique constraint on `(userId, productId)` — one line per product per user.

---

### PradeshMaster

| Field    | Type    | Notes                     |
|----------|---------|---------------------------|
| id       | integer | Primary key               |
| name     | string  |                           |
| isFixed  | boolean | Fixed vs variable charge  |
| charge   | decimal | Delivery charge amount    |

---

### CategoryMaster

| Field       | Type    | Notes           |
|-------------|---------|-----------------|
| id          | integer | Primary key     |
| name        | string  |                 |
| description | string  |                 |

---

### SubCategoryMaster

| Field       | Type    | Notes               |
|-------------|---------|---------------------|
| id          | integer | Primary key         |
| categoryId  | integer | FK → CategoryMaster |
| name        | string  |                     |
| description | string  |                     |

---

### DocumentMaster

| Field        | Type     | Notes               |
|--------------|----------|---------------------|
| id           | integer  | Primary key         |
| vendorId     | integer  | FK → VendorMaster   |
| documentName | string   |                     |
| documentUrl  | string   | Relative file URL   |
| uploadedAt   | datetime |                     |

---

## File Upload Rules

Static files are served from the following paths:

| Upload Type      | Request Path         | Storage Path            |
|------------------|----------------------|-------------------------|
| Profile Images   | `/uploads/profiles`  | `./uploads/profiles/`   |
| Vendor Documents | `/uploads/documents` | `./uploads/documents/`  |
| Product Images   | `/uploads/products`  | `./uploads/products/`   |
| Payment Proofs   | `/uploads/payment`   | `./uploads/payment/`    |

File references returned in API responses are relative paths, e.g. `/uploads/products/image.jpg`. Clients should prepend the host base URL to construct the full URL.

---

*End of AlphaLogistics API Contract Document*
