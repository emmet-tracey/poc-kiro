# SAR API Deployment Script
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("dev", "staging", "prod")]
    [string]$Environment,

    [Parameter(Mandatory = $true)]
    [string]$S3Bucket,

    [string]$Region = "us-east-1",
    [string]$Profile = "default"
)

Write-Host "Deploying SAR API to $Environment environment..." -ForegroundColor Green

# Set working directory
Set-Location "$PSScriptRoot\..\src\SarApi"

# Restore packages
Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore

# Build the project
Write-Host "Building project..." -ForegroundColor Yellow
dotnet build --configuration Release

# Deploy to AWS
Write-Host "Deploying to AWS Lambda..." -ForegroundColor Yellow
$stackName = "sar-api-$Environment"

dotnet lambda deploy-serverless `
    --stack-name $stackName `
    --s3-bucket $S3Bucket `
    --s3-prefix "sar-api/$Environment/" `
    --region $Region `
    --profile $Profile `
    --template-parameters "Environment=$Environment"

if ($LASTEXITCODE -eq 0) {
    Write-Host "Deployment completed successfully!" -ForegroundColor Green

    # Get the API Gateway URL
    Write-Host "Getting API Gateway URL..." -ForegroundColor Yellow
    $outputs = aws cloudformation describe-stacks `
        --stack-name $stackName `
        --region $Region `
        --profile $Profile `
        --query "Stacks[0].Outputs" `
        --output json | ConvertFrom-Json

    $apiUrl = ($outputs | Where-Object { $_.OutputKey -eq "ApiUrl" }).OutputValue

    Write-Host "API URL: $apiUrl" -ForegroundColor Cyan
    Write-Host "Swagger UI: ${apiUrl}swagger" -ForegroundColor Cyan
}
else {
    Write-Host "Deployment failed!" -ForegroundColor Red
    exit 1
}