FROM prom/prometheus:v3.5.1

COPY --chown=65534:65534 prometheus.yml /etc/prometheus/prometheus.yml

EXPOSE 9090

USER 65534
