# Dev-Ops Course (Master at ITU, KSDSESM1KU 2026 Spring)
**Group**: *Pat Myaz*  \
**Members:**  *Carmen Alberte Nielsen, Casper Storm Frøding, Mads Christian Nørklit Jensen, Max Brix Koch, Mathilde Julie Gonzalez-Knudsen*

## Description
This repository contains our group's MiniTwit project, developed in accordance to the specifications of the *Dev-Ops Course (Master at ITU, KSDSESM1KU 2026 Spring)*.

Our MiniTwit project is a simplified Twitter/X 'esque' application, emulating the most basic functionality of creating users, posting tweets and following/unfollowing other users. The application is hosted on Digital Ocean, with a pipeline, containing workflows, that is run through GitHub Actions. The application is publically available (for now), but is not truly meant for actual users, but rather to simulate an application, which is being used by simulated users, in production, where we build a pipeline around it to handle Continous Integration and Deployment. In summary the project is for the group members, teachers and examinators of the course.

## Prerequisites
Clone the repository and navigate to the root of it
```
git clone https://github.com/ITU-DevOps2026/ITU-minitwit.git
cd ITU-minitwit
```

### Locally
- The program can be run locally with Docker, with installation steps for it being found here for [Docker Engine](https://docs.docker.com/engine/install/) and here for [Docker Desktop](https://docs.docker.com/desktop/).
### In the cloud
You need to ensure that the following Docker images have been built and pushed to Docker Hub: 
- mathildegk/minitwitimage (This is the production application, it is automatically updated when we push new changes to main)
- mathildegk/minitwit-dev (This is the development/test application, it is automatically updated when a new PR is created)
- mathildegk/minitwit-mysql-prod (This is the production database, simply a MySQL image with an initialized database containing the necessary tables, but no data)
- mathildegk/minitwit-mysql-test (This is the development/test database, simply a MySQL image with an initialized database containing the necessary tables, but no data)

See guide in [Docker readme](/README.Docker.md) on how to build an push images to Docker Hub. 
- To spin up the project in the cloud (this project uses Digital Ocean), Vagrant is required. Installation of Vagrant is available [here](https://developer.hashicorp.com/vagrant/install). The Vagrantfile also use the Digital Ocean plugin which can be installed with the following command:
```
vagrant plugin install vagrant-digitalocean
```

## Configuration requirements to run the project
Running the project requires configuration of certain environment variables. We recommend a .env file in the root of the project containing the following:
```
 # Global secrets
# Digital Ocean credentials
DIGITAL_OCEAN_TOKEN=<Insert your Digital Ocean Token>
SSH_KEY_NAME=<Insert name of your SSH Key>
# Docker Hub (Also global) 
DOCKER_USERNAME=mathildegk 

# Production environment secrets
prod_db_connection=Server=<db_IP>;Port=3306;Database=minitwit;Uid=root;Pwd=<PROD_DB_PASSWORD>;SslMode=None;AllowPublicKeyRetrieval=True;
PROD_DB_PASSWORD=<Your_secure_Database_password>
PROD_DB_RES_IP=<Reserved IP for your droplet which contains your production database>
PROD_MONITORING_RES_IP=<Reserved IP for your droplet which contains your monitoring and logging view>
PROD_MANAGER_RES_IP=<Reserved IP for your droplet which contains your Manager container in the Docker Swarm>

# Test environment secrets
test_db_connection=Server=<db_IP>;Port=3306;Database=minitwit;Uid=root;Pwd=<TEST_DB_PASSWORD>;SslMode=None;AllowPublicKeyRetrieval=True;
TEST_DB_PASSWORD=Your_secure_Database_password
TEST_DB_RES_IP=<Reserved IP for your droplet which contains your test database>
TEST_MONITORING_RES_IP=<Reserved IP for your droplet which contains your test monitoring and logging view>
TEST_MANAGER_RES_IP=<Reserved IP for your droplet which contains your test Manager container in the Docker Swarm>

# Monitoring and logging secrets
GF_SECURITY_ADMIN_USER=<Desired username for your Grafana user>
GF_SECURITY_ADMIN_PASSWORD=<Desired password for your Grafana user>
```

You will need to [create a Digital Ocean token](https://docs.digitalocean.com/reference/api/create-personal-access-token/) and [add your ssh key to your Digital Ocean team](https://docs.digitalocean.com/platform/teams/how-to/upload-ssh-keys/).

## Running the project
### Locally
To ensure the Docker Hardened Images can be pulled if they are not already on your system, start by running.
```
docker login dhi.io
```
In the root of the project run the following docker command:
```
docker compose up
```
You can also add the `-d` flag to run the application in detached mode, meaning that the containers are run in the background

| Service | URL |
|---|---|
| Frontend | http://localhost:5035 |
| Backend API | http://localhost:8080 |
| Prometheus | http://localhost:9090 |
| Elasticsearch | http://localhost:9200 | 
| Grafana | http://localhost:3000 |

### In the cloud
In the root of of the project run the following vagrant command:
```
vagrant up
```
In your terminal you will be prompted to specify which environment you want to spin up, enter either prod or test. 

| Service | URL |
|---|---|
| Frontend | https://bigtwit.app |
| Backend API | https://bigtwit.app/api |
| Prometheus | Secured behind firewall, is available through Grafana|
| Elasticsearch | Secured behind firewall, is available through Grafana | 
| Grafana | http://46.101.69.11:3000 |

### Running tests
Tests can be run using the following commands:
```
docker compose run --rm tests
docker compose run --rm apitests
docker compose run --rm uitests
```

### Running static analysis
Navigate to the `/minitwit` directory and run the following command in the terminal:
```bash
dotnet build
```
This will build the application and run .NET code analysis and Roslynator on the project, displaying errors and warnings in the terminal.
.NET code analysis will only display warnings on the newly built code. 
If you want to build the whole application run the following commands in the terminal:
```bash
dotnet clean
dotnet build
```
Roslynator can automatically fix some warnings and errors by running the following command in `/minitwit` directory:
```bash
dotnet roslynator fix MiniTwit.csproj
```
There are some warnings that cannot be fixed automatically, these will need to be handled manually. 

## CI/CD Pipeline - Add when we have described it in the report?
### Required Github Action secrets
- DIGITAL_OCEAN_TOKEN - A personal access token that has acces to the Digital Ocean team
- DOCKER_USERNAME - Docker username, of user that has r/w access to the images used
- DOCKER_PASSWORD - Docker password corresponding the the user
- SONAR_TOKEN - Token generated by SonarCloud to allow it access to read the repo
- SSH_HOST - Reserved ip of the production manager droplet
- SSH_HOST_TEST_ENV - Reserved ip of the test manager droplet
- SSH_KEY - An SSH key that is allowed ssh access into the droplets
- SSH_USER - The user the workflows use to ssh into the droplets in prod
- SSH_USER_TEST_ENV - The user the workflows use to ssh into the droplets in test

### Releases
#### Making a release draft using tags
On the main branch first create a tag with the intended version number:
```bash
git tag vx.x.x
```
Where you replace each x with a number from 0-9.
After creating the tag, you push the tag which triggers the workflow and creates a draft release:
```bash
git push origin <tag>
```
#### Semantic versioning
Versioning numbers are determined by following the SemVer versioning scheme, see: <https://semver.org/> for full documentation

## Monitoring
### Metrics

### Dashboards

### Logs

## Tech Stack

| Layer     | Technology |
| :---------| :----------|
| Backend | C#, .NET 10|
| Frontend | Razor Pages, HTML, CSS-Bootstrap, Javascript |
| Database | MySQL 8.0.45 |
| Reverse Proxy | Traefik |
| TLS | Lets encrypt |
| Orchestration & Containerization | Docker Swarm |
| CI/CD | GitHub Actions |
| Observability | Prometheus, Grafana, Elasticsearch, Serilog |
| Code Quality & Security | SonarCloud, Codacy, .NET analyzers, Roslynator, Dockle, CodeQL, Trivy |
| Infrastructure | Digital Ocean, Vagrant |

## Project Structure
| Path      | Description |
| :---------| :----------|
| [.github/workflows](.github/workflows) | Workflows for CI/CD, Static analysis tools, security scanners, tests and report building|
| [.github/ISSUE_TEMPLATE](.github/ISSUE_TEMPLATE) | Template used when creating issues |
| [.log_book](./log_book/) | Contains log books in markdown |
| [.logging/elasticsearch](./logging/elasticsearch/) | Dockerfile for Elasticsearch |
| [.minitwit/Api](./minitwit/Api/) | Attributes, Controllers and Models for API |
| [.minitwit/Migrations](./minitwit/Migrations/) | Migrations used by EFcore |
| [.minitwit/Model](./minitwit/Model/) | Models for Minitwit used by EFcore |
| [.minitwit/Pages](./minitwit/Pages/) | cshtml and cshtml.cs files for Razor Pages |
| [.minitwit/Properties](./minitwit/Properties/) | Launchsettings |
| [.minitwit/wwwroot](./minitwit/wwwroot/) | css, javascript and bootstrap files for frontend |
| [.minitwit/appsettings.json](./minitwit/appsettings.json) | Configuration file for minitwit |
| [.minitwit/dotnet-tools.json](./minitwit/dotnet-tools.json) | Specification of dotnet tools |
| [.minitwit/minitwit.cs](./minitwit/minitwit.cs) | Main entrypoint for Minitwit |
| [.monitoring/grafana](./monitoring/grafana/) | Dockerfile, datasource and dashboards configuration for Grafana |
| [.monitoring/prometheus](./monitoring/prometheus/) | Dockerfile and configuration for Prometheus |
| [.provision_scripts](./provision_scripts/) | Scripts handling provisioning of droplets. Vagrantfile uses these |
| [.remote_files](./remote_files/) | Compose files and deploy scripts that is put onto droplets |
| [.report](./report/) | Files pertaining the report for the project |
| [.tests](./tests/) | Files pertaining tests |
| [.dockerignore](.dockerignore) | Things Docker should ignore |
| [.dockleignore](.dockleignore) | Issues Dockle should ignore |
| [.editorconfig](.editorconfig) | Configuration of dotnet analyzer |
| [.gitmessage](.gitmessage) | Template used in commit messages |
| [.mailmap](.mailmap) | Maps users that committed to the repo together |
| [.compose.yaml](/compose.yaml) | Compose file for local deployment, also used in workflows |
| [.Dockerfile-minitwit](/Dockerfile-minitwit) | Dockerfile for minitwit application |
| [.Dockerfile-minitwit-api-tests](/Dockerfile-minitwit-api-tests) | Dockerfile for api tests |
| [.Dockerfile-minitwit-tests](/Dockerfile-minitwit-tests) | Dockerfile for dotnet tests (Unit and Integration)  |
| [.Dockerfile-minitwit-ui-tests](/Dockerfile-minitwit-ui-tests) | Dockerfile for ui-tests (UI and End-to-End)  |
| [.Dockerfile-mysql](/Dockerfile-mysql) | Image for MySQL db |
| [.init_db.sql](/init_db.sql) | Schema used to initialize db |
| [.minitwit_db_seed.sql](/minitwit_db_seed.sql) | File to seed database when run locally  |
| [.README.Docker.md](/README.Docker.md) | Extra information pertaining to Docker |
| [.README.md](/README.md) | Description of repository  |
| [.Vagrantfile](/Vagrantfile) | IaC - Creates and configures 5 droplets |

## Team Configurations and conventions
### Git commit message template
To tell Git to use gitmessage as commit template file, run the following command in your terminal while being in the root directory of the repository:
```bash 
git config --local commit.template .gitmessage
```
### Branches and Issues
Create issues to describe desired features, known bugs to fix, refactorings, etc. These should use the Template issue, which specifies the title of the issue to be: [Type of issue, i.e Refactor, Bug, etc] - Title of issue as well as adding a /Timespent Xh Ym to each issue.

Branches should be linked to the corresponding issue, and should follow the same convention, i.e [Type of work, i.e Refactor, Bug, etc] - Specific name of what you are working on. Ex: Refactor-vagrantfile (we used / but this can give issues in bash)

### Reviews and checks
When a branch is ready to be merged into main, create a pull request to main. This triggers several workflows, which should all pass for the branch to be eligible for a merge. Furthermore a branch should be reviewed and approved by atleast one team member, who has not worked on the branch, before it is merged.

## License
MIT License

Copyright (c) 2026 - Carmen Alberte Nielsen, Casper Storm Frøding, Mads Christian Nørklit Jensen, Mathilde Julie Gonzalez-Knudsen, Max Brix Koch.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

## Acknowledgements
- Helge Pfeiffer
- Mircea Lungu
- Ahmet Talha Akgül
- Babette Bækgaard
- Patrick Wittendorff Abarzua Neira