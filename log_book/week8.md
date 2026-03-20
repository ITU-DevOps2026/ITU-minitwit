## Choice of Logging method

### Different methods we have considered
Here we discuss some different possibilities of logging, and discuss which one we see as the best for our case.

*The one we deem most fit for our use case, (Due to us not having a full scale program with milling of users) is Serilog Sink, primarily due to it not having metrics and is very easy to integrate, without us having to change our business logic at all.* **This was written by an unsure Casper, and could probably be changed if we do not seem this as the correct solution. The decision was primarily due to the comment of the old students who said that this was the "easy way" of doinng this.**

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


