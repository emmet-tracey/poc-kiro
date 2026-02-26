# SAR API - Suspicious Activity Reports

A REST API for generating and managing Suspicious Activity Reports (SARs) built with C# and .NET 8, designed for deployment on AWS Lambda with API Gateway and DynamoDB storage.

## Features

- **Create SARs**: Accept alert data including transaction details, customer information, and suspicion reasons
- **Validate Input**: Comprehensive validation against regulatory requirements using FluentValidation
- **Data Enrichment**: Automatically enrich data with sensible defaults for missing fields
- **Status Management**: Track SAR lifecycle through Draft → Submitted → Filed states
- **Filtering & Search**: List SARs with filtering by status, date range, customer, and account
- **Regulatory Compliance**: Structured JSON output ready for regulatory filing

## API Endpoints

### Core Operations

- `POST /api/sar` - Create a new SAR
- `GET /api/sar/{id}` - Retrieve SAR by ID
- `GET /api/sar` - List SARs with filtering
- `PUT /api/sar/{id}` - Update existing SAR
- `DELETE /api/sar/{id}` - Delete SAR (draft only)

### Workflow Operations

- `POST /api/sar/{id}/submit` - Submit SAR for review
- `POST /api/sar/{id}/file` - File SAR with regulatory authorities
- `POST /api/sar/{id}/assign` - Assign SAR to a user or team

## Technology Stack

- **Framework**: ASP.NET Core Web API (.NET 8)
- **Cloud**: AWS Lambda + API Gateway
- **Database**: Amazon DynamoDB
- **Validation**: FluentValidation
- **Documentation**: Swagger/OpenAPI

## Data Model

### SAR Status Flow

```
Draft → Submitted → Filed
```

### Key Entities

- **SuspiciousActivityReport**: Main entity containing all SAR data
- **CustomerInformation**: Customer details and identification
- **TransactionDetail**: Individual transaction information
- **SuspicionDetails**: Reason for suspicion and investigation notes

### Assignment & Workflow

- **AssignedTo**: User or team responsible for the SAR
- **AssignedBy**: User who made the assignment
- **AssignedAt**: Timestamp of assignment
- **SuspicionDetails**: Reason for suspicion and investigation notes

## Validation Rules

### Customer Information

- First/Last name required (max 50 chars)
- Valid SSN format (XXX-XX-XXXX)
- Complete address with valid ZIP code
- Valid phone number and email (if provided)

### Transactions

- Unique transaction ID required
- Amount must be greater than zero
- Transaction date cannot be in future
- Transaction type required

### Suspicion Details

- Primary reason from predefined enum
- Description 10-2000 characters
- Suspicion date cannot be in future

## Deployment

### Prerequisites

- AWS CLI configured
- .NET 8 SDK
- AWS Lambda Tools for .NET

### Deploy to AWS

```bash
cd src/SarApi
dotnet lambda deploy-serverless
```

### Local Development

```bash
cd src/SarApi
dotnet run
```

Access Swagger UI at: `https://localhost:5001`

## Configuration

### Environment Variables

- `ASPNETCORE_ENVIRONMENT`: Environment (Development/Production)
- `SAR_TABLE_NAME`: DynamoDB table name (auto-configured in Lambda)

### AWS Resources Created

- Lambda Function: `sar-api-SarApiFunction`
- API Gateway: REST API with proxy integration
- DynamoDB Table: `SARs-{Environment}` with encryption and point-in-time recovery

## Example Usage

### Create a SAR

```bash
curl -X POST https://api-gateway-url/api/sar \
  -H "Content-Type: application/json" \
  -d @examples/create-sar-example.json
```

### List SARs by Status

```bash
curl "https://api-gateway-url/api/sar?status=Draft&limit=10"
```

### Submit SAR for Filing

```bash
curl -X POST https://api-gateway-url/api/sar/{id}/submit
```

### Assign SAR to User

```bash
curl -X POST https://api-gateway-url/api/sar/{id}/assign \
  -H "Content-Type: application/json" \
  -d @examples/assign-sar-example.json
```

## Security Considerations

- All data encrypted at rest in DynamoDB
- HTTPS enforced for all API calls
- Input validation prevents injection attacks
- Filed SARs are immutable (cannot be modified or deleted)

## Compliance Notes

This API generates structured SAR data but does not automatically file with FinCEN. The generated JSON output should be reviewed by compliance officers before regulatory submission. The API includes fields and validation rules based on common SAR requirements but should be customized for specific regulatory jurisdictions.
