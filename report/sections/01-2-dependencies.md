## Dependencies
Technologies:

| Dependency type           | Technology                | Purpose                                               |
| :-----------              |:--------------            | :-------------                                        |
| Infrastructure            | DigitalOcean              | Cloud Hosting Droplets and Firewall                   |
| OS                        | Ubuntu (ubuntu-22-04-x64) | Operating system on DigitalOcean Droplets             |
| Orchestration             | Docker Swarm              | Container Clustering                                  |
| Containerization          | Docker                    | Container Runtime Environment                         |
| Containerization          | Docker Compose            | Multi-container configuration and local orchestration |
| Container Registry        | DockerHub                 | Storing repositories of our minitwit docker images    |
| Container Hardened Images | dhi.io                    | Docker hardened images for .NET 10                    |
| Container Base Image      | Python3.12/alpine         | Lightweight runtime for Python containers             |
| Container Base Image      | mysql:8.0                 | MySQL base image for docker container                 |
| Container Base Image      | grafana/grafana:12.1      | Grafana base image for docker containers              |
| Container Base Image      | prom/prometheus:v3.5.1    | Prometheus base image for docker containers           |
| Container Base Image      | docker.elastic.co/elasticsearch/ elasticsearch:7.17.10 | ElasticSearch base image for docker containers |
| Container Base Image      | curlimages/curl           | Curl base image for docker containers used for healthchecks |
| Provisioning Tool         | Vagrant                   | Infrastructure-as-Code for VM provisioning            |
| Provisioning Tool         | DropletKit                | Integration between Vagrant and DigitalOcean API      |
| Language                  | ruby                      | Language used for Vagrant configuration               |
| Domain Management         | name.com                  | Domain name registration and managing                 |
| Reverse Proxy             | Traefik                   | Load balancing and reverse proxy for routing traffic  |
| Security                  | Let's Encrypt             | Automated SSL/TLS certificate provisioning            |
| Security                  | Fail2Ban                  | Protects droplets by banning malicious IPs            |
| Scripting                 | bash                      | Shell scripts for automated tasks                     |
| Monitoring                | Prometheus (Server)       | Metrics collection                                    |
| Monitoring                | Grafana                   | Visualization and dashboarding of metrics and logging |
| Logging                   | ElasticSearch             | Log storage, indexing and search                      |
| Runtime Framework         | .NET 10                   | Application runtime and framework                     |
| Language                  | C#                        | Code language for Minitwit                            |
| Database                  | MySQL 8.0                 | Relation database for application data                |
| Styling Language          | CSS                       | Frontend styling language                             |
| Frontend Framework        | Bootstrap                 | CSS framework for UI design                           |
| Scripting Language        | JavaScript                | Client-side logic and interaction                     |
| Version Control           | Github                    | Version control platform                              |
| CI/CD                     | Github Actions            | CI/CD workflows                                       |
| Security                  | CodeQL                    | Static security analysis of code                      |
| Security                  | Trivy                     | Vulnerability scanning for containers and Dockerfiles |
| Analysis Tool             | Dockle                    | Analysis of Dockerfiles                               |
| Analysis Tool             | Codacy                    | Automated code quality analysis                       |
| Analysis Tool             | SonarQube                 | Code quality and static analysis                      |
| Runtime Dependency        | Java (JDK17)              | Required runtime for SonarQube                        |
| Documentation Tool        | TinyTex                   | Lightweight LaTeX distribution for report generation  |
| Documentation Tool        | Pandoc                    | Document conversion and report generation             |
| Language                  | Python 3                  | Code language for UI and API tests                    |
| Web browser               | Firefox                   | Web browser for automated UI tests                    |
| Testing Tool              | Geckodriver               | WebDriver implementation for controlling Firefox in tests |

.NET depencies: 

| Dependency type   | Package                          | Purpose                                            |
| :-----------      |:--------------                   | :-------------                                     |
| Web Framework     | ASP.NET Core                     | Razor Pages                                        |
| API Tooling       | OpenAPITools                     | Generating and documenting APIs                    |
| Code Analysis     | .NET Analyzers                   | Analysing .NET code                                |
| Code Analysis     | Roslynator                       | Linting .NET code                                  | 
| ORM               | EntityFramework Core             | Database abstraction and object-relational mapping |
| ORM Extension     | Pomelo.EntityFrameworkCore.MySql | MySQL provider for EntityFramework Core            |
| Monitoring        | Prometheus (Client library)      | Exposing application metrics                       |
| Logging Sink      | Elastic.Serilog.Sinks            | Sending Serilog's to ElasticSearch                 |
| Logging Library   | Serilog                          | Structured logging and log management              |
| Testing Framework | Xunit                            | Unit testing framework                             |
| Testing           | Microsoft.NET.Test.Sdk           | Enables running and executing tests                |
| Testing           | Microsoft.AspNetCore.Mvc.Testing | Supports integration testing of ASP.NET Core apps  |
| Test Runner       | xunit.runner.visualstudio        | Runs xUnit tests in development environments       |

Python dependencies:

| Dependency type    | Package       | Purpose                                              |
| :-----------       |:--------------| :-------------                                       |
| Testing Framework  | pytest        | Running and managing automated tests                 |
| Browser Automation | selenium      | Automating browser interactions for testing          |
| HTTP Client        | requests      | Lightweight web framework for building APIs/services |
| Database Driver    | PyMySQL       | Connecting to and interacting with MySQL databases   |