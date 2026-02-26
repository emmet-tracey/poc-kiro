# Project Steering

## Spec-Driven Development Process

When generating specifications for a new feature:

1. **Do not generate a full spec immediately**
2. First, ask 3-5 clarifying questions to understand:
   - Business context and goals
   - Key user personas and workflows
   - Constraints and compliance requirements
   - Integration points with existing systems
   - Edge cases and error handling expectations
3. Wait for answers before proceeding
4. After receiving answers, generate the requirements and design
5. Ask if the user wants to refine any requirements before generating tasks

This iterative approach ensures the spec reflects actual business needs, not assumptions.

## Azure DevOps Defaults

When creating work items in Azure DevOps, always apply these defaults:

- **Project**: Nebula
- **Area Path**: Nebula\Ceres
- **Iteration**: Leave unassigned (Backlog)
- **Tags**: Always include `kiro-generated` tag

Apply these settings to all work item types: User Stories, Tasks, Bugs, and Features.

## Domain Context

This project is in the **Financial Crime (FinCrime)** domain, specifically compliance and regulatory reporting. When elaborating requirements, consider:

- Regulatory compliance requirements (AML, KYC)
- Audit trail and traceability needs
- Data validation and integrity
- Security and access controls

## Technology Stack

- **Language**: C#
- **Framework**: .NET 8 with ASP.NET Core
- **Cloud**: AWS (Lambda, API Gateway, DynamoDB)
