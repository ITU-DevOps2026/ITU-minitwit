## Dependencies

| Dependency type | Technology     | Purpose       |
| :----------- |:-------------- | :-------------|
| Infrastructure | DigitalOcean | Cloud Hosting Droplets and Firewall |
| Orchestration  | Docker Swarm | Container Clustering |
| Containerization | Docker     | Runtime Environment  |
| | Docker Compose | |
| Provisioning Tool | Vagrant | Infrastructure as Code |
| | DropletKit (For integrating Vagrant with DigitalOcean) | |
| | ruby (language for Vangrat file) | |
| | Grafana | |
| | ElasticSearch | |
| | Trivy | |
| | Github | |
| | .NET 10 | |
| Security | CodeQL | For security analysis of .NET application |
| Analysis Tool | SonarQube | |
| | Java (JDK17) (implicit through SonarQube) | |
| Analysis Tool | Dockle | Analysing Dockerfiles |
|  | MySQL 8.0 | |
|  | Bootstrap | |
| | Prometheus | |
| | Codacy | |
| | TinyTex | |
| | Pandoc | |
| Domain name registrar | name.com | Domain name registration and managing |
| | dhi.io | |
| | Python3.12/alpine | |
| | Traefik | |
| | Lets Encrypt | |
| | bash | Shell scripts |

.NET Code depdencies: 
| Dependency type | Package     | Purpose       |
| :----------- |:-------------- | :-------------|
| Code Analysis | .NET Analyzers | Analysing .NET code |
| Code Analysis | Roslynator | Linting .NET code |
| Logging Library | Serilog | Generating logs |
| ORM | EntityFramework Core | |
| | Pomelo | |
| | Prometheus | |
| Web Framework | ASP.NET Core | Razor Pages |
| | OpenAPITools | |
| | Xunit | |