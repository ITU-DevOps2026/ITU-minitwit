## Operation

### Availability and Database Migration

- __Issue__: Migrating our live production data from SQLite to MySQL took roughly 10 minutes during testing, which would cause an unacceptable window of application downtime and data loss if executed in a single batch. Furthermore, we faced text-encoding anomalies (corrupted characters) and format incompatibilities during the dump transfer.
- __Solution__: We added utf8mb4 encoding to our schemas and cleaned SQLite-specific elements using a tailored pipeline (sed, stripping Byte Order Marks). To minimize downtime, we developed a multi-batch migration strategy: we performed a large initial seed, and right before the DNS switch, we used the Linux comm tool to calculate a dump consisting only of new records written during the sync. During the final deployment, an omission in updating the production environment connection string caused a few minutes of unintended downtime, where we missed 19 register requests according to our dashboard tracking.
- __Lesson__: Live data migrations require synchronization strategies to ensure high availability. Furthermore, even minor manual steps in configuration updates introduce human error, highlighting the necessity of automation.

### The Security Breach (Database Hack):

- __Issue__: A few hours after migrating to MySQL, our database droplet was breached by automated ransomware bots because it was exposed to the public internet without a firewall, resulting in total data erasure and a Bitcoin ransom demand.
- __Solution__: We used Vagrant to provision firewalls and configured MySQL to only accept connections from specific IP addresses (our application droplets).
- __Lesson__: "Security by Design" is vital. Infrastructure must be hardened before it goes live, not as an afterthought.

### Resource Management & OOM Errors:

- __Issue__: Our monitoring droplet kept crashing (exit code 137) because Elasticsearch consumed all available RAM.
- __Solution__: We learned to tune the JVM heap size (ES_JAVA_OPTS) limiting it to 50% of the available memory on the droplet (configured in the DockerFile).
We configured a log lifetime cycle where old logs are deleted (configured in the compose file).
For emergencies, we also implemented a swap file (configured during provisioning of the monitoring-and-logging-droplet) to take over processes if we run out of RAM.
- __Lesson__: Operating a logging/monitoring stack requires careful resource allocation.

### Limitations of Hardened Images 

- __Issue__: We experienced failing tests when running docker compose. We expected it to be caused by race conditions where the tests where running before the MiniTwit application was completely done with initialization. To prevent this, we wanted to add a simple health check that would curl the endpoint, but the curl tool can not be installed on Docker hardened images.
- __Solution__: We created a sidecar container that uses a curl image to curl the application and made the tests depend on this container.
- __Lesson__: An understanding of the limitations of hardened images.