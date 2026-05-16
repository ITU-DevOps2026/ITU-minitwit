## Maintenance

### Automated Quality Assurance:

- __Issue__: Maintaining code across different environments often leads to "it works on my machine" syndromes. 
- __Solution__: We used shellcheck on our script (which has later been deleted) and implemented a strict GitHub Actions CI/CD pipeline that runs containerized tests on every PR and every push to the main branch.
- __Lesson__: Automation and containerization reduces the maintenance burden of environment inconsistency.

### Log Retention Policies:

- __Issue__: We realized that our 25GB disk space on the logging droplet would quickly fill up with logs.
- __Solution__: We configured Index Lifecycle Management (ILM) rules within Elasticsearch to enforce an explicit rollover policy, ensuring older indices are automatically pruned and deleted after reaching age thresholds.
- __Lesson__: Managing the data lifecycle to prevent system exhaustion is also part of maintainence.

### DevOps Style of Work
In this project, our DevOps style was defined by the "Three Ways" of DevOps:

- __Flow__: Unlike previous projects where we might have worked in long-lived branches, we focused on small batch sizes and a deployment pipeline ensuring value gets to prod quickly. We used GitHub Actions to ensure that code was tested and deployed to "Test" and "Prod" environments automatically. The use of vagrant and docker ensures environment consistensy. 

- __Feedback__: We implemented monitoring and logging and added downtime/latency email alerts. This gave us immediate feedback on whether a deployment was successful or if the system was under stress. 
We also included tests and static code analysis tools in our pipeline telling us exactly which part of our code had issues so they could be resolved quickly. 

- __Continuous Learning and Experimentation__: We followed a blameless culture with room for mistakes and focus on understanding the issues. We maintained a weekly logbook to document considerations and decisions and regularly updated readmes with guides on running, deploying and etc. This ensured that knowledge was shared across the team, and everyone had access to the information they needed to contribute effectively.

__How it differed from previous projects:__
In past projects, we were "just" developers. In this project, we were responsible for the entire lifecycle, from the C# code to the Linux firewall and the Docker orchestration. This cross-functional responsibility made us write more robust code because we were the ones who would have to fix it at 2 AM if the server crashed. 