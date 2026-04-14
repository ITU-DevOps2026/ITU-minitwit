## Choice of Logging method

### Different methods we have considered
Here we discuss some different possibilities of logging, and discuss which one we see as the best for our case.

*The one we deem most fit for our use case, (Due to us not having a full scale program with millions of users) is Serilog Sink, primarily due to it not having metrics and is very easy to integrate, without us having to change our business logic at all.* 

#### Serilog Sink
* Serilog Sink, is very easy to integrate, by just a NuGet package (and some short additions to the program.cs file), and it only supports logging, no metrics (which we already have integrated) . 
* This is a diagnostic logging library that allows us to send logs to different "destinations" aka, these sinks.
* Another big plus is that it handles the JSON formatting required by Elasticsearch ( Which is ECS (Elastic Common Schema) ) automatically, so we do not have to worry about this.
* NOTICE: If we do this, it is important that the multible sinks we have on the logging, that one of them is also just the "Console sink" (i.e. our normal terminal), and the Elasticsearch sink. This also makes sure we have the logs both places, and would see it when running locally, and also in the log files for Docker.

#### OpenTelemetry (OTel)
* If we wanted to continue our way of doing things like Prometheus and Grafana, we could use OpenTelemetry (OTel), to do this in most of the same manner. 
* The reason i say it is the same manner, is because it is run in a droplet, where minitwit then would send all outputs (via OTLP protocol) to this droplet.
* This also makes it pretty versatile if we later wanted to change where the logs are stored, or if we don't want Elasticsearch, but instead Grafan Loki, this would be able to be changed in this droplet, without having to change any C# code. 
* The biggest minus is having another Container to keep track of and ensure that it runs well and does not go down.


#### Filebeat
* This is more the "infrastructure" way. This is done by logging to the terminal or another file, and then another process (aka. Filebeat) is watching it, and shipping it to Elasticsearch.
* This would also be another container running specifically for this task.
* Again some overhead in ensuring that this docker containre is alive and well functioning.
* A plus of this is that Minitwit would be completely unaware of this, and we would not have to program anything new in Minitwit.
* Another plus is that if for example the logging container goes down you do not lose the logs, you only lose the "functionality" of giving it to ElasticSearch

# Seperating monitoring and logging to a seperate droplet
After implementing logging and getting it working locally, when moving onto running it on one of our droplets, we quickly encountered memory issues. ElasticSearch is memory intensive, and we decided to restructure our droplet setup, so we went from having a droplet running our MiniTwit container, our Grafana container, and our Prometheus container, to having a droplet with our MiniTwit application, and another droplet, running our Grafana, Prometheus and ElasticSearch containers. Even before we implemented logging, we had talked about seperating our monitoring away from the droplet running our MiniTwit application, to ensure as little drain on the ressources on the droplet as possible, which should make the application more robust (and if the droplet started lacking ressources, we would have less culprits to worry about).

The droplet containing Grafana, Prometheus and ElasticSearch, did quite quickly run into memory issues, with Elasticsearch being the culprit. The container would exit after a bit of time (around the 15-30 min mark), and if we on the droplet ran docker ps -a, we could see the container had exited with code 137, indicating the system killed the process because it was running out of memory. 
Whilst choosing ElasticSearch as part of our locking stack, is heavier on the memory usage, it has direct integration with Serilog, which is a logging library for .NET applications, which we deemed the most suitable logging library for our tech stack. We did also want to go in the direction of an ELK stack, as it did seem like it was one of the more widespread solutions. So we settled on using ElasticSearch, Serilog and Grafana. Grafana was chosen for vizualisation to try and keep our dashboards in one place, and because it seemed to potentially have lower memory costs than something like Kibana. 

Choosing to stick with ElasticSearch, we had to try and solve our memory issues. One solution to this could be linear scaling, in this case allocating more CPU and RAM for our droplet, but for now we have decided to refrain from this, as we have limited free credits on Digital Ocean (and we don't think the scale of our program requires ElasticSearch to be using several gigabytes of memory). Instead we limit the heap size of ElasticSearch based on the available ram of the droplet (this line in the Dockerfile: ENV ES_JAVA_OPTS="-Xms256m -Xmx256m"). We set this to no more than 50% of the total memory available, in accordance with the documentation: https://www.elastic.co/docs/reference/elasticsearch/jvm-settings

This alone was not enough, as the other 50% was used up by a combination of things, such as our monitoring, the off heap memory usage of elasticsearch (for things such as caching), the operating system of the droplet, etc. We therefore decided to also implement a swap file (https://medium.com/@adebanjoemmanuel01/understanding-swap-files-1967fe8c5c89), to take over processes, so we would not run out of memory on the droplet. ElasticSearch docs does warn against this, https://www.elastic.co/docs/deploy-manage/deploy/self-managed/setup-configuration-memory, so in accordance to their documentation we do set swappiness to 1, which reduces the kernel's tendencies to swap, meaning it should only happen in emergency situations. Configuration of the swap file, happens in the Vagrantfile, during provisioning of the monitoring-and-logging droplet. 

Another consideration we made before deploying any logging to production, was management of older logs to ensure we do not run out of disk space. As we have chosen Elasticsearch, which requires more storage space, in comparison to other tools like Loki https://signoz.io/blog/loki-vs-elasticsearch/, we need to configure a log lifetime cycle, wherein we specify how long we store logs for, and probably also a maximum amount of space logs can use on the droplet. 

If you want to log to a txt file aswell as elasticsearch (locally), you do the following:
In the compose.yaml file you add this volume: - ./logs:/app/logs, and in the appsettings.json you add the package Serilog.Sinks.File, to the Using section and you add the following to the WriteTo section:

```json 
"WriteTo": [
      { 
        "Name": "Console"},
      {
        "Name": "File",
        "Args": {
          "path": "logs/log.txt",
          "rollingInterval": "Day" 
        }
      },
...
]
```

Creating policies for managing logs in Elasticsearch are either done through the Kibana ui or through API calls https://www.elastic.co/docs/manage-data/lifecycle/index-lifecycle-management/configure-lifecycle-policy. Creating and adding our policies is handled through an intermediate docker container, that simply runs curl request against the Elasticsearch container. The current policy limits the primary shard size to 2GB. When any primary shard reaches this size, or if the index reaches a 7-day lifespan, a rollover occurs. This means the current index is marked as read-only (rolled over), and a new index is created for active writing. Finally, when an index reaches a total lifespan of 14 days, it is deleted to free up space. The size of the files were chosen, keeping in mind that Elasticsearch is running on a droplet with only 25GB disk space. These settings might need further configuration, as the logging system has not been stress tested, so we will need to monitor it when it is deployed to production. 

## Issues with tests and struggles of using Docker hardened images
Somewhere during this implementation the tests, when run through docker compose, began failing. Whilst this was hard to truly verify, the working theory is that this was caused by a race condition, being that the tests ran before the MiniTwit application, that the tests run against, was up and running. Previously in our docker compose we did have our tests depend on the MiniTwit container, but only to the extent that it was created. With more services added, the startup time for the application probably got longer, thus making it so the tests now ran before the app was ready. To prevent this, we wanted to add a simple health check condition to the container, that would curl localhost:5035 and let the tests run when this returned status code 200. This turned out to not be possible, as we are using Docker hardened images for our MiniTwit application, meaning the curl tool was not available, and could not be installed for this container. The solution we ended up using was making a sidecar container, that uses a curl image, which handles the curling MiniTwit and verifying it is healthy, and then making the tests dependent on this sidecar container returning service_healthy. 
