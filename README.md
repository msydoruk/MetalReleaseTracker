# Metal Release Tracker

## Overview

This project leverages a modern backend-centric microservices architecture to efficiently handle data processing, authentication, messaging, and more. The focus is on creating scalable, secure, and containerized services using cutting-edge technologies.

## Architecture Diagram

The following diagram illustrates the overall system architecture and data flow between services:

[![System Architecture](https://www.mermaidchart.com/raw/f7f9fcf9-b5bc-46e2-b406-dd55acec4305?theme=light&version=v0.1&format=svg)](https://www.mermaidchart.com/raw/f7f9fcf9-b5bc-46e2-b406-dd55acec4305?theme=light&version=v0.1&format=svg)

## Key Components and Services

1. **Authentication & Authorization**
   - **User Service / Identity Service**: Handles user authentication and issues access tokens.
   - **User Database**: Stores user-related data.

2. **Data Collection**
   - **Parser Service**: Collects and processes raw data, sending it to a queue for further processing.

3. **Data Processing**
   - **Catalog Synchronization Service**: Processes raw data and synchronizes catalog information.
   - **Metal Release Tracker Service**: Centralized service for managing processed data.

4. **Notifications**
   - **Notification Service**: Generates and sends notifications based on user subscriptions.

5. **Delivery Calculation Service**
   - Handles calculation requests and responses for specific data needs.

6. **User Interface (UI)**
   - Provides a frontend interface for interacting with backend services, managing subscriptions, and displaying data.

## Technology Stack

- **MongoDB**: NoSQL database for storing raw data.
- **PostgreSQL + Entity Framework Core**: Relational database for structured data.
- **Kafka**: Messaging and streaming service for event-driven architecture.
- **Identity Server**: Manages authentication and access control.
- **Hangfire**: Background job processing.
- **Serilog + Seq**: Structured logging and log aggregation.
- **Docker + Kubernetes**: Containerization and orchestration of services.

## Project Goals

- **Scalable Data Processing**: Efficiently handle both raw and processed data using appropriate storage solutions.
- **Event-Driven Architecture**: Enable real-time messaging and data flow with Kafka.
- **Secure User Management**: Leverage Identity Server for robust authentication.
- **Containerized Deployment**: Utilize Docker and Kubernetes for scalable and maintainable deployments.
