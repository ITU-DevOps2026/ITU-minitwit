## Dependencies
Technologies:

| Dependency type           | Technology                | Purpose                                               |
| :-----------              |:--------------            | :-------------                                        |
| Infrastructure            | DigitalOcean              | Cloud Hosting Droplets and Firewall                   |
| OS                        | Ubuntu (ubuntu-22-04-x64) | Operating system on DigitalOcean Droplets             |
| Orchestration             | Docker Swarm              | Container Clustering                                  |
| Containerization          | Docker                    | Container Runtime Environment                         |
| Containerization          | Docker Compose            | Multi-container configuration and local orchestration |
| Container hardened images | dhi.io                    | Docker hardened images for .NET 10                    |
| Container Base Image      | Python3.12/alpine         | Lightweight runtime for Python containers             |
| Provisioning Tool         | Vagrant                   | Infrastructure-as-Code for VM provisioning            |
| Provisioning Tool         | DropletKit                | Integration between Vagrant and DigitalOcean API      |
| Language                  | ruby                      | Language used for Vagrant configuration               |
| Domain Management         | name.com                  | Domain name registration and managing                 |
| Reverse Proxy             | Traefik                   | Load balancing and reverse proxy for routing traffic  |
| Security                  | Let's Encrypt             | Automated SSL/TLS certificate provisioning            |
| Scripting                 | bash                      | Shell scripts for automated tasks                     |
| Monitoring                | Prometheus (Server)       | Metrics collection                                    |
| Monitoring                | Grafana                   | Visualization and dashboarding of metrics and logging |
| Logging                   | ElasticSearch             | Log storage, indexing and search                      |
| Runtime Framework         | .NET 10                   | Application runtime and framework                     |
| Language                  | C#                        | Code language for Minitwit                            |
| Database                  | MySQL 8.0                 | Relation database for application data                |
| Frontend Framework        | Bootstrap                 | CSS framework for UI design                           |
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

.NET depencies: 

| Dependency type | Package                     | Purpose                                            |
| :-----------    |:--------------              | :-------------                                     |
| Web Framework   | ASP.NET Core                | Razor Pages                                        |
| API Tooling     | OpenAPITools                | Generating and documenting APIs                    |
| Code Analysis   | .NET Analyzers              | Analysing .NET code                                |
| Code Analysis   | Roslynator                  | Linting .NET code                                  | 
| ORM             | EntityFramework Core        | Database abstraction and object-relational mapping |
| ORM Extension   | Pomelo                      | MySQL provider for EntityFramework Core            |
| Monitoring      | Prometheus (Client library) | Exposing application metrics                       |
| Logging Library | Serilog                     | Structured logging and log management              |
| Testing         | Xunit                       | Unit testing framework                             |