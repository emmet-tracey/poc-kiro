# Project Steering

## Azure DevOps Defaults

When creating work items in Azure DevOps, always apply these defaults:

- **Project**: Nebula
- **Area Path**: Nebula\Ceres
- **Iteration**: Leave unassigned (Backlog)
- **Tags**: Always include `kiro-generated` tag

Apply these settings to all work item types: User Stories, Tasks, Bugs, and Features.

## Technology Stack

### Language & Framework

- **Language**: C#
- **Framework**: .NET 8
- **API Style**: ASP.NET Core Web API

When generating code:

- Follow Microsoft C# coding conventions
- Use nullable reference types
- Prefer async/await for I/O operations
- Use dependency injection patterns

### Infrastructure

- **Cloud Provider**: AWS
- **Compute**: AWS Lambda (prefer serverless where appropriate)
- **API Layer**: Amazon API Gateway
- **Database**: Amazon DynamoDB (for NoSQL) or Amazon RDS (for relational)
- **Storage**: Amazon S3

When designing solutions:

- Default to serverless architecture using Lambda
- Use API Gateway for REST endpoints
- Consider DynamoDB for high-throughput, flexible schema data
- Include IAM permissions in designs
- Follow AWS Well-Architected Framework principles
