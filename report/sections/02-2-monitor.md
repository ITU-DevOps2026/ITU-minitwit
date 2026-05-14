## Monitor

Under here is a sequence diagram of the process of collecting, forwarding, and visualizing application metrics in the monitoring system.

![A sequence diagram of how monitoring works for Minitwit](./images/monitoring_n_replicas.png)

To ensure we do not leak information on our metrics of Minitwit, we have set a username and password on Grafana, that is automatically set on creation. In addition to this, only the Monitor's droplet IP adress is whitelisted to scrape [https://monitor.bigtwit.app/](https://monitor.bigtwit.app/), done via Traefik, so no other attacker would be able to access this information.

On this metrics endpoint we use `app.UseHttpMetrics();`, which writes things to the endpoint such as total amount of HTTP requests recieved, the duration of these requests, and more HTTP specific metrics. In addition to this, we added 2 "gauges" to the metrics, that showed the amount of tweets and users the app has. This data is cached, so if another metrics request is received in less than 30 seconds, we re-use the old data for tweets, and followers. This is done to not overwork the database, with requests to count the whole amount of tweets and users.

![The source code for caching, in the metrics](./images/metrics_caching.png){width=60%}

As can be seen from this code and the Sequence diagram, we have a hybrid of push/pull monitoring, as Minitwit pushes some internal data to a metrics endpoint, and Promtheus pulls from this endpoint.

As can be seen in the diagram, Traefik load balances between each container, which creates this fluctuating graph, that can be seen in the dashboard under here, due to Prometheus not knowing exactly what container it has scraped, and can therefore not make an average.

To mitigate this error, we should have moved our Prometheus container from its' own droplet to the Manager droplet, and then from here it would be able to scrape all the individual replicas, and find averages over these, via the Docker network, and communicate this to Grafana on the monitoring droplet. However, this would still be a problem due to the workers being on different droplets than the manager, therefore we would need a package that could discover the exact location of each replica, for example the prometheus-docker-sd package ([Link](https://github.com/stuckyhm/prometheus-docker-sd)).

In addition to this we also wanted to monitor all of our services, i.e. not "just" the webservice, but also the database, to make it easier to determine where an error/bottleneck is located, however we did not get to implement this.

Due to us developers not wanting to constantly monitor our system, we have set up an *Uptime Check*, that monitors our droplets, and gives everyone in the team a notification if:

* [https://bigtwit.app](https://bigtwit.app) is down

* there is latency over 1000ms when accessing [https://bigtwit.app](https://bigtwit.app)

* The SSL certificate of bigtwit.app is about to expire.(However Traefik should automatically renew this certificate, before it expires.)

In addition to this we have added a ressource alert, that monitors our test database and production database, to see when it crosses over 70% CPU in over 5 minutes, which could indicate an attack, or a rapid increase in requests that it maybe cannot handle.

### Dashboards

To see the dashboard, go to [46.101.69.11:3000](http://46.101.69.11:3000), where the dashboard and logs can be viewed. The credentials are specified in the post made by Helge.

We made 2 different dashboards, one made with a developer focus (Infrastructure Health), and one made with a Business stakeholder focus(Product Performance).

#### Minitwit Infrastructure Health Dashboard

\

Below are some images of the dashboard and descriptions of the information it provides the developers:

![Minitwit Infrastructure Health dashboard - Service health and HTTP Success Rate](./images/Dashboard_infrastructure_1.png){width=70%}

![Minitwit Infrastructure Health dashboard - Requests per second and Total HTTP requests received](./images/Dashboard_infrastructure_2.png){width=80%}

![Minitwit Infrastructure Health dashboard - Response time (P95 latency)](./images/Dashboard_infrastructure_3.png){width=60%}

- Immediate Root Cause Analysis: If the Status is UP but the Error Rate is spiking, developers know the app is running but the code or database is failing.

- Capacity Planning: Requests per second and Total Requests help developers understand if the current infrastructure (CPU/RAM) can handle the load or if they need to scale.

- Performance Regressions: By monitoring Latency(P95) and HTTP Success rate, developers can see if a recent code deployment made the app slower, even if it didn't _break_ it.

We have, in the source code, created a */healthz* endpoint, that shows when Minitwit is up and running, however this is only used for spinning up the system locally with docker compose. Our dashboard's health check is currently based on whether or not Prometheus was able to scrape the target. If we had more time, we would have used the */healthz* endpoint instead, which would give a more relevant answer, which is seperated from Prometheus.

#### Minitwit Product Performance Dashboard

\

Below are some images of the dashboard and descriptions of the information it provides the business stakeholders:

![Minitwit Product Performance dashboard - Tweet overview](./images/dashboard_product_1.png){width=70%}

![Minitwit Product Performance dashboard - User overview](./images/dashboard_product_2.png){width=70%}

![Minitwit Product Performance dashboard - Duration of HTTP Requests (Latency)](./images/dashboard_product_3.png){width=60%}

- KPI Tracking: "Total Tweets" and "amount of users" are direct Key Performance Indicators. In a real application, if registrations drop to zero, the Marketing or Product team needs to know immediately.

- Engagement Trends: The 24-hour tweet increase shows the pulse of the community. A sudden drop might indicate a trend issue or a subtle bug that doesn't trigger an error but prevents posting. Also, amount of users registerered in the last hour, indicates if a campaign or other trend has worked or not, and if we therefore can see an increase or a drop.

- User Satisfaction: Slow apps lead to lost users, so we include an overview of the latency of HTTP requests to keep the business aligned with the need for technical optimization.
