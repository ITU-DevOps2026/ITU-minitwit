# DevOps Principles
The following evaluates how much our group adheres to the "Three Ways" of DevOps as defined in The DevOps Handbook. It maps our current practices to these principles and identifies specific adjustments to improve our workflow.
_This is based on the state as of the middle of week 8_.

## Flow
The first way focuses on ensuring that our code changes can be quickly and reliably deployed to production.

- Deployment Pipeline: We use GitHub Actions to automate our CI/CD. We have distinct workflows for Pull Requests (automated testing), Pull Requests to the main branch (testing followed by deployment to test environment), and merges to the main branch (testing followed by deployment to production). This ensures that every change is validated and deployed automatically.
- Environment Consistency: We use Vagrant and Docker to ensure that development, test, and production environments are identical.
- Batch Size Reduction: We have transitioned from large, infrequent deployments to a strategy of committing as often as possible. We still ensure each commit represents a functional unit of work to maintain stability, but we aim to keep batch sizes small to bring down the time between code completion and deployment. 
- Work in Progress: We use GitHub Projects to track issues. By limiting our active tasks to 1–3 items for our 5-person team, we maintain a high focus and reduce the overhead of context switching.

We could improve our Flow-way by including more automated checks in our pipeline, such as static code analysis or security scanning. 
This would help catch issues earlier and further reduce the time to deployment.


## Feedback
The second way emphasizes the importance of creating feedback loops to identify and resolve issues as early as possible.

- Automated Testing: Tests are integrated into every stage of our pipeline (PR creation and Deployment). This provides immediate feedback on whether a change has broken existing functionality. 
- Peer Reviews: We require at least one peer review for every PR (as far as possible). This not only helps catch issues but also supports knowledge sharing and collective code ownership.
- Telemetry and Monitoring: We have implemented Prometheus and Grafana with two distinct dashboards:
    - Technical Dashboard: Monitors system health, such as response times and status codes.
    - Business Dashboard: Tracks user growth and tweet volume to provide feedback on application value.

Currently, we have not implemented logging, which makes it difficult to determine the origin of an error when the monitoring dashboard indicates an issue. Implementing logging would enhance our ability to quickly identify and resolve issues.
We plan to use Elasticsearch (via Serilog sinks) for logging. This will allow us to correlate monitoring metrics with specific log traces, closing the feedback loop between _knowing_ there is an error and knowing _why_ it happened. We should have some logging production-ready by the end of week 8.

## Continual Learning and Experimentation
The third way encourages a culture of continuous learning and experimentation to drive improvement.

- Blameless Culture: We avoid blaming individuals for failures. Instead, we focus on understanding the system and processes that led to an issue. This encourages open communication and learning from mistakes. An example of this was when our database got hacked due to a lack of sequrity considerations. Instead of blaming the individuals who set up the database, we analyzed the situation to understand how it happened and implemented stronger security measures to prevent it from happening again.
- Knowledge Sharing: We maintain a weekly logbook to document considerations and decisions made during the project. We also keep readmes with updated guides on how to run the program, run tests, deploy to production, and more. This ensures that knowledge is shared across the team, and everyone has access to the information they need to contribute effectively.
- Refactoring: We treat technical debt as a standard part of our backlog, regularly performing "clean-up" tasks to ensure the system remains up-to-date and do not accumulate unnecessary complexity.

The current "adhoc" approach to refactoring can lead to technical debt if not managed properly. To improve this, we could implement a more structured approach to refactoring, such as dedicating specific sprints or time blocks for addressing technical debt and refactoring tasks. This would ensure that we consistently allocate time for improving our codebase and prevent the accumulation of technical debt.

# Introducing a DB abstraction layer
Introducing an abstraction layer allows us to decouple our application logic from the specifics of the database implementation. This means that when we want to switch to a different database in the future, we can do so with minimal changes to our application code.
The abstraction layer also provides a more intuitive way to interact with the database, which can improve developer productivity and reduce the likelihood of errors in our database interactions.

We chose to implement the abstraction layer using Entity Framework Core (EF-core) because it is a widely-used Object-Relational Mapping (ORM) framework for .NET applications. EF-core allows us to work with our database using C# objects and LINQ queries, which can simplify our code and improve readability. It also provides features like migrations, which can help us manage changes to our database schema over time. EF-core also supports a wide range of databases, which gives us flexibility in our choice of database technology in the future.

# Idempotent configuration management scripts
... we will look into this later.