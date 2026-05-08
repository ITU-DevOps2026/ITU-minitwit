FROM grafana/grafana:12.1

ENV PROMETHEUS_HOST='host.docker.internal:9090'
ENV ELASTICSEARCH_HOST='host.docker.internal:9200'

COPY --chown=472:472 dashboards /etc/grafana/dashboards
COPY --chown=472:472 datasources /etc/grafana/datasources

EXPOSE 3000

USER 472
