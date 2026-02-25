# Project Steering

## Azure DevOps Defaults

When creating work items in Azure DevOps, always apply these defaults:

- **Project**: Nebula
- **Area Path**: Nebula\Ceres
- **Iteration**: Leave unassigned (Backlog)
- **Tags**: Always include `kiro-generated` tag

Apply these settings to all work item types: User Stories, Tasks, Bugs, and Features.

## Language

This project uses **C#** and **.NET**. When generating code:

- Follow Microsoft C# coding conventions
- Use nullable reference types
- Prefer async/await for I/O operations
- Use dependency injection patterns
