# SAR API Infrastructure

This directory contains infrastructure-as-code and deployment scripts for the SAR API.

## Architecture

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   API Gateway   │───▶│   Lambda Function │───▶│   DynamoDB      │
│                 │    │   (SAR API)       │    │   (SARs Table)  │
└─────────────────┘    └──────────────────┘    └─────────────────┘
```

## AWS Resources

### Lambda Function

- **Runtime**: .NET 8
- **Memory**: 512 MB
- **Timeout**: 30 seconds
- **Trigger**: API Gateway (REST API)

### API Gateway

- **Type**: REST API
- **Integration**: Lambda Proxy
- **CORS**: Enabled
- **Endpoints**: `/{proxy+}` and `/`

### DynamoDB Table

- **Name**: `SARs-{Environment}`
- **Partition Key**: `Id` (String)
- **Billing**: Pay-per-request
- **Encryption**: Server-side encryption enabled
- **Backup**: Point-in-time recovery enabled

## Deployment

### Prerequisites

1. AWS CLI installed and configured
2. .NET 8 SDK installed
3. AWS Lambda Tools for .NET Core:
   ```bash
   dotnet tool install -g Amazon.Lambda.Tools
   ```

### Deploy Script

Use the PowerShell deployment script:

```powershell
.\deploy.ps1 -Environment dev -S3Bucket your-deployment-bucket
```

### Manual Deployment

```bash
cd ../src/SarApi
dotnet lambda deploy-serverless --stack-name sar-api-dev
```

## Environment Configuration

### Development

- Stack Name: `sar-api-dev`
- DynamoDB Table: `SARs-dev`
- Log Level: Debug

### Staging

- Stack Name: `sar-api-staging`
- DynamoDB Table: `SARs-staging`
- Log Level: Information

### Production

- Stack Name: `sar-api-prod`
- DynamoDB Table: `SARs-prod`
- Log Level: Warning

## Monitoring

### CloudWatch Logs

- Log Group: `/aws/lambda/sar-api-{Environment}-SarApiFunction`
- Retention: 14 days (configurable)

### CloudWatch Metrics

- Lambda Duration
- Lambda Errors
- API Gateway 4XX/5XX Errors
- DynamoDB Read/Write Capacity

### Recommended Alarms

1. Lambda Error Rate > 5%
2. API Gateway 5XX Error Rate > 1%
3. Lambda Duration > 25 seconds
4. DynamoDB Throttling Events

## Security

### IAM Permissions

The Lambda function has minimal required permissions:

- DynamoDB: Read/Write access to SARs table only
- CloudWatch: Log creation and writing
- VPC: None (public subnet deployment)

### Data Protection

- All data encrypted at rest (DynamoDB)
- All data encrypted in transit (HTTPS/TLS)
- No sensitive data in logs
- SSN and PII handled according to compliance requirements

## Cost Optimization

### Lambda

- Right-sized memory allocation (512 MB)
- Efficient cold start handling
- Connection pooling for DynamoDB

### DynamoDB

- Pay-per-request billing (no provisioned capacity)
- Efficient query patterns
- Minimal secondary indexes

### API Gateway

- REST API (lower cost than HTTP API for this use case)
- No caching configured (add if needed for high traffic)

## Troubleshooting

### Common Issues

1. **Deployment Fails**
   - Check AWS credentials and permissions
   - Verify S3 bucket exists and is accessible
   - Ensure .NET 8 SDK is installed

2. **Lambda Timeout**
   - Check DynamoDB performance
   - Review CloudWatch logs for bottlenecks
   - Consider increasing memory allocation

3. **DynamoDB Throttling**
   - Monitor read/write capacity metrics
   - Consider switching to provisioned capacity for predictable workloads

### Logs Location

```bash
aws logs tail /aws/lambda/sar-api-{env}-SarApiFunction --follow
```
