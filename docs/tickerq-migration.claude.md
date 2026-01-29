\# Claude Code – Project Refactor Task



You are Claude Code.

You are operating in repository context.



\## ROLE

Act as a senior .NET backend engineer with strong experience in:

\- .NET 8

\- Background jobs and schedulers

\- Clean Architecture

\- Hangfire and TickerQ

\- Production-ready systems



\## CONTEXT

Project: MetalReleaseTracker.ParserService

Language: C#

Framework: .NET 8



The project currently uses Hangfire for recurring background jobs.

Your task is to migrate the scheduling subsystem from Hangfire to TickerQ.



Official documentation:

https://tickerq.net/getting-started/quick-start.html



Do NOT use any other scheduler.



---



\## GLOBAL RULES (IMPORTANT)



These rules apply to all generated code:



\- Target framework: .NET 8 ONLY

\- Use modern C# features supported by .NET 8

\- No legacy patterns

\- No comments in code unless absolutely necessary

\- No static state

\- No service locator pattern

\- No reflection-based magic

\- Prefer explicit dependencies

\- Use async/await correctly

\- Respect CancellationToken everywhere

\- Code must be production-ready

\- Minimal but readable code

\- Fail fast on misconfiguration



---



\## ARCHITECTURE RULES



\- Follow existing project structure

\- Jobs are application-level concerns

\- Scheduling configuration belongs to application startup

\- Business logic must NOT be inside schedulers

\- Jobs must be idempotent

\- Jobs must be safe for retries



---



\## TASKS



\### 1. Remove Hangfire

\- Remove Hangfire packages

\- Remove Hangfire configuration

\- Remove IRecurringJobManager usage

\- Remove BackgroundService scheduling logic related to Hangfire



---



\### 2. Introduce TickerQ



\- Register TickerQ in Program.cs

\- Configure job execution and storage

\- Enable TickerQ Dashboard

\- Dashboard must be reachable via HTTP endpoint



---



\### 3. Refactor scheduling logic



Current behavior:

\- Parsing jobs are created dynamically per ParserDataSource

\- Jobs are scheduled daily

\- Publisher job is scheduled separately and runs daily



New behavior:

\- Use TickerQ jobs instead of Hangfire recurring jobs

\- Preserve configuration-driven job creation

\- No manual loops or polling

\- Use Cron-based scheduling where appropriate



---



\### 4. Job implementation rules



\- One job = one responsibility

\- Jobs must:

&nbsp; - Be async

&nbsp; - Accept CancellationToken

&nbsp; - Use DI

&nbsp; - Not block threads

&nbsp; - Not store state internally



---



\### 5. Configuration



\- Use appsettings.json + IOptions

\- No hardcoded schedules

\- Validate configuration on startup



---



\## OUTPUT EXPECTATIONS



You must:

\- Modify existing files

\- Create new job classes if needed

\- Update Program.cs

\- Update appsettings.json only if required

\- Ensure solution builds



Do NOT:

\- Add unrelated features

\- Change business logic

\- Rename domain concepts unnecessarily



---



\## EXECUTION STRATEGY



1\. Analyze existing Hangfire-based scheduling

2\. Identify all recurring jobs

3\. Replace them with TickerQ equivalents

4\. Verify correct scheduling semantics

5\. Ensure clean shutdown and cancellation support



Proceed step by step.



