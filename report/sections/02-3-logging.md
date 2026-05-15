## Logging
Logs from MiniTwit uses a Push-based model, where Serilog directly pushes logs to Elasticsearch. Since Serilog is directly implemented in MiniTwit, this means all MiniTwit replicas are pushing logs to Elasticsearch, where they are collected and displayed in a singular view. Our default logging level is configured to be *Information*, i.e any logs from the MiniTwit application will be logged if it is at the information level or above. We override this for Microsoft, AspNetCore and EFCore where we only log *Warning* and above. MiniTwit logs use the *minitwit-logs* index prefix, making them easy to distinguish from other logs. 

### What our current logging implementation is missing
Since our infrastructure is running in docker containers, we should also have set up a log-shipper, such as Filebeat, to collect docker container logs and ship them to Elasticsearch. 

Furthermore, our acutal logging statements in MiniTwit.cs are also quite sparse, with some functions not actually logging anything. Whilst not everything needs to be logged, this should be a more deliberate choice than it is currently