## Risk Assesment

### Risk identification

#### Assets
* Web application
* Web API
* Grafana dashboard
* Prometheus instance
* Database

#### Threat Sources
* Cookie manipulation.
* Root-user on Docker images
* DDOS attacks on our webpage, due to no limit on amount of requests.
* SQL injection on our web page.
* SSH attack on our server.

#### Risk Scenarios
1. Attacker performs SQL injection on web application to download sensitive user data.
2. Attacker downs our website by using DDOS attacks on our webpage.
3. Attacker connects to one of our droplets via SSH attacks.
4. Attacker connects to our database and requires ransom to get the data back.
5. Attacker manages to guess the credentials for the API, and uses them to perform a DDOS attack.
6. Attacker escalates privileges or impersonates another user, via cookies.
7. Attacker gets access to Grafana and gets access to important information through metrics.


### Risk Analysis
Here we use the scenarios from above, via the index of them, and determine the likelihood and impact of each of the scenarios.

#### Determine likelihood
1. Low - because we use EF-Core, which automatically parameterizes queries and escapes all input strings. As long as all database access goes through EF-Core and not raw SQL queries to the database, the probability of a successful SQL injection is very low.
2. High - We have not set any rate limiting on our page, so all people would be able to pretty easily create a bot that would DDOS attack us.
3. Low - an attacker would need a compromised private key or exploit an unknown SSH vulnerability, both of which are much harder.
4. Low - same reasons as for number 3.
5. High - these credentials are hardcoded in our source code, for the API, so an attacker would easily be able to see what the credentials are, and how they should be encoded.
6. Medium - The session cookies are encrypted via HTTPS, and expires after 15 minutes, so an attacker should be quick to use it.
7. High - As the credentials are the ones Helge has posted on the Teams group, several other students of this course have access to it, and we do not know if Helge, Mircea or another student is spreading these credentials around.


#### Determine impact

1. High - could expose user data, and credentials.
2. High - could prevent users from acessing our site, lowering the customer satisfaction.
3. High - An attacker would be able to change our program, stop it, or create a backdoor to our system.
4. High - An attacker could steal our data and demand ransom, which would damage our users and our reputation.
5. High - could prevent users from acessing our site, lowering the customer satisfaction.
6. Medium - could impersonate other users and write things they do not want. However because all users have the same priveliges, it would not be more severe than this.
7. Medium - An attacker could gain access to our log/monitoring data, however these are not as sensitive as e.g. our secrets or user data. They could also change or delete our dashboard views, but these can be restored. In addition to this, an attacker would be able to determine the design of the system, error codes, and latency of different requests, which is not something you want an adversary to know, as this can be the basis for a roadmap to attack our system.

#### Risk Matrix to prioritize risk of scenarios

| | Low likelihood | Medium likelihood | High likelihood |
|------------------------------------|-----------|----------------|-----------|
| High impact                        | __1.__ SQL injection \newline __3.__ SSH attack \newline __4.__ ransom attack | -           | __2.__ DDOS attack \newline __5.__ guess credentials for API to DDOS      |
| Medium impact                      | -       | __6.__ cookie exploitation | __7.__ Access to Grafana |
| Low impact                         | -       | -            | -    |


#### What we could do to mitigate some of these scenarios

1. Already mitigated, also CI-CD pipeline check for security vulnerabilities.
2. Use Traefik support for setting up rate limiting.
3. Already mitigated.
4. We would need to ensure that our keys are not stolen/shared to an attacker.
5. Same solution as to 2. And then saving the credentials in a secret.
6. Use Token-based authentication instead.
7. Create new credentials only our team knows of, and set-up 2 factor authentication.

