# Dev-Ops Course (Master at ITU, KSDSESM1KU 2026 Spring)
**Group**: *Pat Myaz*  \
**Members:**  *Carmen Alberte Nielsen, Casper Storm Frøding, Mads Christian Nørklit Jensen, Max Brix Koch, Mathilde Julie Gonzalez-Knudsen*

## Git commit message template
To tell Git to use gitmessage as commit template file, run the following command in your terminal while being in the root directory of your Git repository:
```bash
git config --local commit.template .gitmessage
```

## Releases
### Making a release draft using tags
On the main branch first create a tag with the intended version number:
```bash
git tag vx.x.x
```
Where you replace each x with a number from 0-9.
After creating the tag, you push the tag which triggers the workflow and creates a draft release:
```bash
git push origin <tag>
```
### Semantic versioning
Versioning numbers are determined by following the SemVer versioning scheme, see: <https://semver.org/> for full documentation

## MiniTwit C# Application 
### How to run in Container
Running the application with Docker sets up a production-like environment on your machine, where you will have a container running with a MySQL database server and another container with the application, which can then connect to the MySQL database. This setup is similar to what is happening on our droplets on Digital Ocean.

See [Docker readme](/README.Docker.md) for instructions on how to run using Docker.

### Requirements to run locally
- dotnet 10.0
- A terminal capable of running sh if you want to run the control.sh script

### How to run minitwit C# application
Navigate to the `/minitwit` folder and run the following command in the terminal:
```bash
dotnet run
```
This will start the minitwit application. You can access it by opening a web browser and going to `http://localhost:5035`. 
You should see the minitwit homepage where you can see the public timeline, with options to sign up and sign in.

OBS: You need to make sure that you **don't** have an environment variable called DbPath set up on your machine, as this variable points to the path for a MySQL database. The application specifically looks for this variable to determine which database to connect to, and when using `dotnet run`, the application should connect to the Sqlite database file, since there will be no running instance of a MySQL database server. 

### How to run C# tests against C# minitwit
In one terminal, run the minitwit application by navigating to the `minitwit` folder and running the following command:
```bash
dotnet run
```
Then in another terminal, run the test suite by navigating to the `/tests` folder and running the following command:
```bash
dotnet test
```
You should see all tests passing successfully! 🥇

(Do note that the tests modifies the database, so you have to remove the changes between each run, otherwise they will fail)

## Running the control.sh script in the C# application
The script can be run with different arguments from the root folder of the repository.

### Init
```sh
./control.sh init
```
With init the script will check if there is a database in the same root as the script, if there is it exits, if not it runs dotnet run init, which will trigger the Init_db() function, using the schema.sql file to drop any user, follower, and message table and then create them.

### Startprod
```sh
./control.sh startprod
```
With startprod as the argument the script executes the following line 

```sh
nohup dotnet run -c Release --urls "http://0.0.0.0:5035" > /tmp/out.log 2>&1 &
```
- nohup means no hang up, and it ensures the program keeps running even if we exit the terminal or the user has logged out (not application wise).
- \> /tmp/out.log redirects output such as Console.Writeline into the specified file.
- 2>&1 redirects standard errors, such as crashes and exceptions to the same file. 
- the final & symbol puts all of it in the background so our terminal is ready for use again immediately. 

### Start
```sh
./control.sh start
```
With start as the argument the following line is executed
```sh
nohup dotnet run > /tmp/out.log 2>&1 &
```
All arguments are explained in the Startprod section

### Stop
```sh
./control.sh stop
```
With stop as the argument the following line is executed
```sh
pkill -f minitwit
```
- pkill with the -f flag terminates any process that has the string minitwit in it.

### Inspectdb
```sh
./control.sh inspectdb
```
With inspectdb as the argument the following line is executed
```sh
./flag_tool -i | less
```
- This executes the flag_tool.c.
- The -i argument stands for inspect or interactive and tells the tool to dump a list of all users or messages from the database.
- | pipes the left argument into the right argument.
- less captures the output and lets us scroll up and down with arrow keys, search for text by typing starting with /, and lets us quit by pressing q. 

### Flag
```sh
./control.sh flag
```
With flag as the argument the following line is executed
```sh
    shift   
    ./flag_tool "$@"
```
- Shift ensures that the script ignores the first word (flag) and captures everything from the second position onwards.
- So if we call ./control.sh flag 500 501 the script runs ./flag_tool 500 501.
- If the flag_tool is called with several arguments it executes and update query, where it looks for the message id, and sets flagged=1 for that message id (there is a column in the database for messages which is called flag).


## Monitoring and Dashboards
Minitwit has monitoring implemented, so we can log things like responsetime, and amount of tweets. This data can be found on the `/metrics` endpoint from the minitwit url: [http://209.38.114.224:5035/metrics](http://209.38.114.224:5035/metrics) .

If you want to see specific data, you can use prometheus to query this endpoint. The prometheus page can be found on the minitwit url, on port `9090`, i.e. [http://209.38.114.224:9090](http://209.38.114.224:9090) .

### The 2 dashboards
We have integrated Grafana, to use Prometheus to pull data from Minitwit and show 2 dashboards. One dashboard for a more developerr view, with information on reponse time status and more, and another one with more of a business perspective showing how many users and tweets we are gaining, and what the average response time is for a user.

* The developer focused dashboard can be [found on this link!](http://209.38.114.224:3000/public-dashboards/80d599614bec4f2d978930fddc345520?refresh=10s&from=now-3h&to=now&timezone=browser)
* The business focused dashboard can be [found on this link!](http://209.38.114.224:3000/public-dashboards/c42f9a112ff24e4eb48e1ddd2a9b34a3?from=now-5m&to=now&timezone=browser&refresh=10s)

## Minitwit on Digital Ocean via Vagrant
### Requirements
* A terminal
* vagrant installation

### Creating the droplets
#### Step 0
You need to ensure that the following Docker images have been built and pushed to Docker Hub: 
- mathildegk/minitwitimage (This is the production application, it is automatically updated when we push new changes to main)
- mathildegk/minitwit-dev (This is the development/test application, it is automatically updated when a new PR is created)
- mathildegk/minitwit-mysql-prod (This is the production database, simply a MySQL image with an initialized database containing the necessary tables, but no data)
- mathildegk/minitwit-mysql-test (This is the development/test database, simply a MySQL image with an initialized database containing the necessary tables, but no data)

See guide in [Docker readme](/README.Docker.md) on how to build an push images to Docker Hub. 

The reason why we need these images, is because our Vagrantfiles set up our droplets with Docker installed, so that the droplets can simply pull the necessary images from Docker Hub and run them using Docker compose. This way we can avoid installing fx Dotnet and MySQL directly on the droplets. 

#### Step 1
* Install the API package
```cmd
vagrant plugin install droplet_kit
```

#### Step 2
Create a .env file in the root of the project, so in the root of ITU-minitwit. Make sure that this file is ignored by the .gitignore file, as it should not be in the versioning, because it will contain secrets.

Your .env file should contain the following (We will describe how to get the different variables in later steps):
```dotenv
# Global secrets
# Digital Ocean credentials
DIGITAL_OCEAN_TOKEN=<your digital ocean token>

SSH_KEY_NAME=<Name of your ssh key>

# Docker Hub (Also global)
DOCKER_USERNAME=mathildegk

# Production environment secrets
db_connection=Server=<Private IP of droplet containing prodcution mysql database>;Port=3306;Database=minitwit;Uid=root;Pwd=<Secure Database Password (same as what you set as DB_PASSWORD)>;SslMode=None;AllowPublicKeyRetrieval=True;

DB_PASSWORD=<Secure Database Password>

APP_SERVER_PRIVATE_IP=<Private IP of droplet containing production MiniTwit application>

MONITOR_AND_LOGGING_PRIVATE_IP=<Private IP of droplet containing monitoring and logging>

# Test environment secrets
test_db_connection=Server=<Private IP of droplet containing test mysql database>;Port=3306;Database=minitwit;Uid=root;Pwd=<Secure Database Password (same as what you set as TEST_DB_PASSWORD)>;SslMode=None;AllowPublicKeyRetrieval=True;

TEST_DB_PASSWORD=<Secure Test Database Password>

TEST_APP_SERVER_PRIVATE_IP=<Private IP of droplet containing test MiniTwit application>
```

#### Step 3
* Generate a Digital Ocean Token!
* This is done in Digital ocean, under **API**, under **Tokens/keys**, and look for **Personal Access Token**. 
* Here you should generate a new Personal Access Token, by clicking the **Generate new Token** button. Give it a name that will identify you, and set the expiration date. Ensure to set it to **Full read/access rights**
* REMEMBER to copy the token generated and store it somewhere safe. This will be the only time you can see it.
* Now you need to set this in your .env file as the DIGITAL_OCEAN_TOKEN

### Step 4
* Input your SSH key name!
* Log in to your Digital Ocean Dashboard.
* On the left sidebar, click Settings (near the bottom).
* Click the Security tab.
* Look for the SSH Keys section.
* You will see a list of keys. Look at the Name column (e.g., "Carmens key", or "mathildes key", etc..). This is what you use for SSH_KEY_NAME in your .env file.

### Step 5
The Vagrantfiles that set up the databases for the production and test application require two more variables to be set in the .env file. You need `DB_PASSWORD` (a strong password that will be used to secure the database - it will be part of the application's connection string and has to be used whenever you want to access the database), and you need `APP_SERVER_PRIVATE_IP` (the IP of the application that the firewall should allow to connect to the database). 

The same applies to `TEST_DB_PASSWORD` and `TEST_APP_SERVER_PRIVATE_IP` these are simply to keep the testing environment and production environment seperated.

### Step 6
The Vagrantfiles that set up the application on both the production and test environments also require two more environment variables; the `DOCKER_USERNAME` (which should be set to `mathildegk`, as this is the username that our Docker images are saved at on Docker Hub, and the docker-compose file on the droplet needs to know this in order to pull the right images), and the `db_connection` (which is the connection string for the database that you want the application to connect to, likely either the minitwit-test-env-mysql or the minitwit-prod-env-mysql).

You need to set the IP and port so that it matches the droplet running the database.

### Step 7 
The application and our monitoring and logging, also need to have the environment variable `MONITOR_AND_LOGGING_PRIVATE_IP` set. The application needs it to open up its port where we put our metrics, so prometheus can scrape it. The application also needs it, so it knows where to push the logs. The monitoring and logging droplet needs it, so it can bind prometheus and elasticsearch to a specific address instead of publicizing  it. 

### OBS: Dependencies
As you see in step 5 and 6, there are some "circular" dependencies between the application droplets and the database droplets. The database needs to have the IP address of the application, which you don't know if your application droplet isn't running yet, and likewise the application needs to have the connection string for the database, which you don't know if your database droplet isn't running yet. 

An approach to solving this is mentioned below in **Setting up the droplets!**, but is something that mainly should apply to the testing application and testing database, as there currently is no intention of redeploying the production application or production database.

One approach to solve this dependency could be to start the database droplet first without specifying the application's IP address. By default, the Vagrantfile for the database sets up a firewall that denies all incoming requests, and then only if the APP_SERVER_IP variable is set, the firewall will allow requests from this IP. So if you omit setting this variable, you can get the droplet up and running (with a very strict firewall), and you can then get the connection string that you need in order to start the application droplet. As soon as the application droplet is running and you can get the application's IP address, you can then manually connect to the database droplet and run `sudo ufw allow from "<IP address>" to any port 3306` in order to "open up" the firewall so that it allows the application to access the database. 

### Setting up the droplets!
Both the production and testing environment, are set up using the same Vagrantfile, found in the root of the project, so you need to be in the ITU-minitwit folder to run your commands. Below is a list of commands and an explanation of which part of the environment it spins up. Be aware that the production application and database should not be needlessy messed with, as they are what is being used in the simulator. As mentioned earlier, the intention is for application and database to be up at all times (or as close to it as possible).
#### Setup of production MiniTwit application
First, make sure the database you want the application to connect to is up and running and
the variable `db_connection` in your .env file is updated with the correct private ip for this database.

Then, the current MiniTwit application which is in production, can be spun up by running this command:
```Bash
vagrant up minitwit-3
```
This creates a droplet with the necessities to run the containers for MiniTwit. The containers are not run as part of this process, and you will have to ssh into the droplet and run the deploy.sh script, by writing the following command in the folder it is located in (which should be the starting point for the terminal).
```Bash
./deploy.sh
```
#### Setup of production database 
If you already have the droplet for the production application up and running, you should first make sure to update your .env file with the private ip for this accordingly. If not, read the section above named "OBS: Dependencies".

Then, to create the droplet containing the production database, run the following command:
```Bash
vagrant up minitwit-prod-env-mysql
```

#### Setup of test MiniTwit application
First, make sure the test database you want the application to connect to is up and running and
the variable `test_db_connection` in your .env file is updated with the correct private ip for this database.

Then, the current MiniTwit test application, can be spun up by running this command:
```Bash
vagrant up minitwit-test-env
```
This creates a droplet with the necessities to run the containers for MiniTwit. The containers are not run as part of this process, and you will have to ssh into the droplet and run the deploy.sh script, by writing the following command in the folder it is located in (which should be the starting point for the terminal).
```Bash
./deploy.sh
```

#### Setup of test database
If you already have the droplet for the test application up and running, you should first make sure to update your .env file with the private ip for this accordingly. If not, read the section above named "OBS: Dependencies".

Then, to create the droplet containing the test database, run the following command:
```Bash
vagrant up minitwit-test-env-mysql
```

#### Connecting the database and the application through vagrant commands:
As mentioned before, here is an approach to solve the circular dependencies and connect the database and application. Note that this approach assumes you want to spin up a fresh application and database:

First run the following command to spin up both droplets:
```Bash
vagrant up minitwit-test-env-mysql minitwit-test-env
```
When both droplets have been created, update your .env file with the corresponding private IP's they have been assigned, then run the following command:
```Bash
vagrant provision minitwit-test-env-mysql minitwit-test-env
```
This runs the sections of the vagrant file for the machines, marked with server.vm.provision, which in this case updates things, such as the variables containing the Private IP's. When this command has finished, you should be able to ssh into the droplet containing the application, and running
```Bash
./deploy.sh
```
and you should now have a working MiniTwit application, which you access on the public ip of the droplet containing the application and adding :5035 at the end of it.

#### Setup of monitoring and logging
Assumption here is that there already exists a running application.

First make sure the variables in your .env file are up to date. Then run:
```Bash
vagrant up minitwit-monitoring-and-logging
```
When the droplet have been created, update your .env file, the `MONITOR_AND_LOGGING_PRIVATE_IP`, with the corresponding private IP it was assigned, then run the following command:
```Bash
vagrant provision minitwit-3 minitwit-monitoring-and-logging
```
This runs the sections of the vagrant file for the machines, marked with server.vm.provision, which in this case updates things, such as the variables containing the Private IP's. When this command has finished, you should be able to ssh into the droplet containing monitoring and logging, and running
```Bash
./deploy.sh
```
