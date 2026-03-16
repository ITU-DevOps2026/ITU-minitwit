### Docker in this project
Docker was implemented in this project by following this guide from the "Using Docker Hardened Images" tab: <https://docs.docker.com/guides/dotnet/containerize/>

We use Docker Hardened Images as they are secure and continously maintained by Docker <https://docs.docker.com/dhi/features/>

We specifically use these to DHI:
- ASP.NET.CORE: <https://hub.docker.com/hardened-images/catalog/dhi/aspnetcore?tagDefinitionId=aspnetcore%2Fdebian-13%2F10>
- dotnet 10 SDK: <https://hub.docker.com/hardened-images/catalog/dhi/dotnet?tagDefinitionId=dotnet%2Fdebian-13%2F10-sdk>

### Building and running your application
To ensure the Docker Hardened Images can be pulled if they are not already on your system, start by running.
`docker login dhi.io`.

Build the compose file with this command: `docker compose build`

When you're ready, start your application by running the following command from the root of the C# minitwit project:
`docker compose up minitwit`. You can also add the `-d` flag to run the application in detached mode, meaning that the containers are run in the background. 

This will start a container with the MySQL database, which will be seeded with some test data, and then the application will start running in another container. 

The application will be available at http://localhost:5035.

To stop the application simply press Ctrl+C in your terminal, and run `docker compose down -v` to down the application. 

### Running the tests 
Tests can be run using the following commands:

`docker compose run --rm tests`

`docker compose run --rm apitests`

`docker compose run --rm uitests`


### Deploying your application to the cloud

First, build your image, e.g.: `docker build -t myapp .`.
If your cloud uses a different CPU architecture than your development
machine (e.g., you are on a Mac M1 and your cloud provider is amd64),
you'll want to build the image for that platform, e.g.:
`docker build --platform=linux/amd64 -t myregistry.com/myapp .`. Specify a specific Dockerfile to build by setting the -f flag: 
`docker build --platform=linux/amd64 -t myregistry.com/myapp -f <Name of Dockerfile> .`

Since we are using mathildegk's Docker Hub as our registry, we can replace myregistry.com with mathildegk instead. So an example of a command could be: `docker build --platform=linux/amd64 -t mathildegk/minitwit-mysql-test -f Dockerfile-mysql .`

Then, push it to the registry, e.g. `docker push mathildegk/minitwit-mysql-test`.

Consult Docker's [getting started](https://docs.docker.com/go/get-started-sharing/)
docs for more detail on building and pushing.

### References
* [Docker's .NET guide](https://docs.docker.com/language/dotnet/)
* The [dotnet-docker](https://github.com/dotnet/dotnet-docker/tree/main/samples)
  repository has many relevant samples and docs.