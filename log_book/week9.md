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
```