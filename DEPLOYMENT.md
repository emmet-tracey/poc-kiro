# SAR API Deployment Guide

## Quick Start

### 1. Prerequisites

- AWS CLI configured with appropriate permissions
- .NET 8 SDK installed
- AWS Lambda Tools for .NET Core

Install Lambda tools:

```bash
dotnet tool install -g Amazon.Lambda.Tools
```

### 2. Create S3 Bucket for Deployment

```bash
aws s3 mb s3://your-sar-api-deployment-bucket --region us-east-1
```

### 3. Deploy Using PowerShell Script (Recommended)

```powershell
cd infrastructure
.\deploy.ps1 -Environment dev -S3Bucket your-sar-api-deployment-bucket
```

### 4. Manual Deployment

```bash
cd src/SarApi
dotnet lambda deploy-serverless --stack-name sar-api-dev --s3-bucket your-sar-api-deployment-bucket
```

## Post-Deployment

### 1. Get API URL

```bash
aws cloudformation describe-stacks --stack-name sar-api-dev --query "Stacks[0].Outputs[?OutputKey=='ApiUrl'].OutputValue" --output text
```

### 2. Test API

```bash
curl https://your-api-url/api/sar
```

### 3. Access Swagger Documentation

Navigate to: `https://your-api-url/swagger`

## Environment-Specific Deployment

### Development

```bash
dotnet lambda deploy-serverless --stack-name sar-api-dev --template-parameters "Environment=dev"
```

### Staging

```bash
dotnet lambda deploy-serverless --stack-name sar-api-staging --template-parameters "Environment=staging"
```

### Production

```bash
dotnet lambda deploy-serverless --stack-name sar-api-prod --template-parameters "Environment=prod"
```

## Cleanup

To remove all resources:

```bash
aws cloudformation delete-stack --stack-name sar-api-dev
```

## Troubleshooting

### Common Issues

1. **Permission Denied**
   - Ensure AWS credentials have CloudFormation, Lambda, API Gateway, and DynamoDB permissions

2. **S3 Bucket Not Found**
   - Create the S3 bucket in the same region as your deployment

3. **Stack Already Exists**
   - Use `--stack-name` with a different name or delete the existing stack

### Required IAM Permissions

Your AWS user/role needs these permissions:

- `cloudformation:*`
- `lambda:*`
- `apigateway:*`
- `dynamodb:*`
- `iam:CreateRole`
- `iam:AttachRolePolicy`
- `s3:GetObject`
- `s3:PutObject`

## Monitoring Setup

After deployment, set up CloudWatch alarms:

```bash
# Lambda error rate alarm
aws cloudwatch put-metric-alarm \
  --alarm-name "SAR-API-Lambda-Errors" \
  --alarm-description "SAR API Lambda error rate" \
  --metric-name Errors \
  --namespace AWS/Lambda \
  --statistic Sum \
  --period 300 \
  --threshold 5 \
  --comparison-operator GreaterThanThreshold \
  --dimensions Name=FunctionName,Value=sar-api-dev-SarApiFunction

# API Gateway 5XX errors
aws cloudwatch put-metric-alarm \
  --alarm-name "SAR-API-Gateway-5XX" \
  --alarm-description "SAR API Gateway 5XX errors" \
  --metric-name 5XXError \
  --namespace AWS/ApiGateway \
  --statistic Sum \
  --period 300 \
  --threshold 1 \
  --comparison-operator GreaterThanThreshold
```
