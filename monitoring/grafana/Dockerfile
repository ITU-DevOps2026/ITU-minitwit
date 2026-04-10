FROM grafana/grafana:12.1

ENV PROMETHEUS_HOST='host.docker.internal:9090'
ENV ELASTICSEARCH_HOST='host.docker.internal:9200'

COPY dashboards /etc/grafana/dashboards
COPY datasources /etc/grafana/datasources

EXPOSE 3000
