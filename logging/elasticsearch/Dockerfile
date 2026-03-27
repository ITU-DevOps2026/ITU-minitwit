FROM docker.elastic.co/elasticsearch/elasticsearch:7.17.10

ENV xpack.monitoring.enabled=true
ENV xpack.watcher.enabled=false
ENV ES_JAVA_OPTS="-Xms512m -Xmx512m"
ENV discovery.type=single-node