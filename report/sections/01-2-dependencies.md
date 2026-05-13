## Dependencies

| Dependency type | Technology     | Purpose       |
| :----------- |:-------------- | :-------------|
| Infrastructure | DigitalOcean | Cloud Hosting Droplets and Firewall |
| OS | Ubuntu (ubuntu-22-04-x64) | Operating system on DigitalOcean Droplets |
| Orchestration  | Docker Swarm | Container Clustering |
| Containerization | Docker     | Container Runtime Environment  |
| Containerization | Docker Compose | Multi-container configuration and local orchestration |
| Provisioning Tool | Vagrant | Infrastructure-as-Code for VM provisioning |
| Provisioning Tool | DropletKit | Integration between Vagrant and DigitalOcean API |
| Language | ruby | Language used for Vagrant configuration |
| Monitoring | Prometheus (Server) | Metrics collection |
| Monitoring | Grafana | Visualization and dashboarding of metrics and logging |
| Data / Search Engine | ElasticSearch | Log storage, indexing and search |
| Security | Trivy | Vulnerability scanning for containers and Dockerfiles |
| Version Control | Github | Version control platform |
| CI/CD | Github Actions | CI/CD workflows |
| Runtime Framework | .NET 10 | Application runtime and framework |
| Language | C# | Code language for Minitwit |
| Security | CodeQL | Static security analysis of code |
| Analysis Tool | SonarQube | Code quality and static analysis |
| Runtime Dependency | Java (JDK17)| Required runtime for SonarQube |
| Analysis Tool | Dockle | Analysing Dockerfiles |
| Database | MySQL 8.0 | Relation database for application data |
| Frontend Framework | Bootstrap | CSS framework for UI design |
| Analysis Tool | Codacy | Automated code quality analysis |
| Documentation Tool | TinyTex | Lightweight LaTeX distribution for report generation |
| Documentation Tool | Pandoc | Document conversion and report generation |
| Domain Management | name.com | Domain name registration and managing |
| Container hardened images | dhi.io | Docker hardened images for .NET 10 |
| Container Base Image | Python3.12/alpine | Lightweight runtime for Python containers |
| Reverse Proxy | Traefik | Load balancing and reverse proxy for routing traffic |
| Security | Let's Encrypt | Automated SSL/TLS certificate provisioning |
| Scripting | bash | Shell scripts for automated tasks |

.NET Code depdencies: 
| Dependency type | Package     | Purpose       |
| :----------- |:-------------- | :-------------|
| Code Analysis | .NET Analyzers | Analysing .NET code |
| Code Analysis | Roslynator | Linting .NET code |
| Logging Library | Serilog | Structured logging and log management |
| ORM | EntityFramework Core | Database abstraction and object-relational mapping |
| ORM Extension | Pomelo | MySQL provider for EntityFramework Core |
| Monitoring | Prometheus (Client library) | Exposing application metrics |
| Web Framework | ASP.NET Core | Razor Pages |
| API Tooling | OpenAPITools | Generating and documenting APIs |
| Testing | Xunit | Unit testing framework |