## Evolution and Refactoring

### The Shift to Statelessness (Docker Swarm):

- __Issue__: To implement Docker Swarm, the application had to be stateless. However, the "latest" value was originally stored in memory. Additionally, moving to Swarm broke our initial monitoring setup because Prometheus could not easily track dynamic service instances.
- __Solution__: We refactored the system to persist the "latest" value in the MySQL database. Regarding monitoring, we reduced the fluctations (cretaed by the inconsistency in the different instances) by regularly updating the metrics based on the database. If we had more time, we belive the better solution would be to move the prometheus service to be a global service in our swarm and then show both monitoring of each instance and an average of all instances in grafana. 
- __Lesson__: Scalability requires carefull architectural decisions regarding state. Refactoring for "Statelessness" is a prerequisite and the monitoring strategy must evolve concurrently to handle dynamic, multi-instance environments.


### The need for orchestration logging/monitoring:

- __Issue__: Our current logs in grafana only shows application-logs (retrived using serilog and elasticsearch), but as our system matured with docker swarm, we found ourself checking the manager droplets logs very often. Both the logs and monitoring of the droplets would provide great value to include in our grafana dashboards to keep all our monitoring/logging in one single space. 
- __Solution__: We did not have time to do this, but we imagine the solution could be to add a logshipper (we where considering filebeat) as a seperate service and making that send logs through elasticsearch. For now, logging of the droplets is accessed via ssh.
- __Lesson__: Application logs represent only half of the picture. Infrastructure and orchestration logging are equally important, and standard application-level logging libraries cannot substitute for proper node-level log shipping.

### Database Abstraction (EF Core):

- __Issue:__ Switching from SQLite to MySQL risked breaking our existing logic.
- __Solution:__ We introduced an abstraction layer using Entity Framework Core. This decoupled our application logic from the database implementation, allowing us to swap providers with minimal code changes.
- __Lesson__: DB Abstraction Layers are smart for decoupling.


