FROM docker.elastic.co/elasticsearch/elasticsearch:7.17.10

ENV xpack.monitoring.enabled=true
ENV xpack.watcher.enabled=false
ENV ES_JAVA_OPTS="-Xms256m -Xmx256m"
ENV discovery.type=single-node

# Run as non-root user for security (Elasticsearch image has a built-in user with UID 1000)
USER 1000 