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
* Generate a Digital Ocean Token!
* This is done in Digital ocean, under **API**, under **Tokens/keys**, and look for **Personal Access Token**. 
* Here you should generate a new Personal Access Token, by clicking the **Generate new Token** button. Give it a name that will identify you, and set the expiration date. Ensure to set it to **Full read/access rights**
* REMEMBER to copy the token generated. This will be the only time you can se this, and store it somewhere safe.
* Now you need to set this in your terminal, so the Vagrant script can extract it when needed. this is done by the commands under here:

*For Windows (PowerShell):*

```PowerShell
$env:DIGITAL_OCEAN_TOKEN="your_actual_token_here"
```

*For Mac/Linux (Terminal):*
```Bash
export DIGITAL_OCEAN_TOKEN="your_actual_token_here"
```
OBS! Setting the environment variable like this does not store it permanently on your machine (i.e. you will need to set them again when the terminal is restarted). Save them in your environment file on your machine if you don't want to set them every time. 

### Step 3 
* Input your SSH key name!
* Log in to your Digital Ocean Dashboard.
* On the left sidebar, click Settings (near the bottom).
* Click the Security tab.
* Look for the SSH Keys section.
* You will see a list of keys. Look at the Name column (e.g., "Carmens key", or "mathildes key", etc..). This is what you use for SSH_KEY_NAME.

*For Windows (PowerShell):*

```PowerShell
$env:SSH_KEY_NAME="the_name_you_found"
```

*For Mac/Linux (Terminal):*
```Bash
export SSH_KEY_NAME="the_name_you_found"
```

### Step 4
The Vagrantfiles that set up the databases for the production and test application require two more environment variables to be set. You need `DB_PASSWORD` (a strong password that will be used to secure the database - it will be part of the application's connection string and has to be used whenever you want to access the database, so you need to store it in a place where you can find it again), and you need `APP_SERVER_PRIVATE_IP` (the IP of the application that the firewall should allow to connect to the database). 

*For Windows (PowerShell):*

```PowerShell
$env:DB_PASSWORD="<password>"
$env:APP_SERVER_PRIVATE_IP="<IP of application>"
```

*For Mac/Linux (Terminal):*
```Bash
export DB_PASSWORD="<password>"
export APP_SERVER_PRIVATE_IP="<IP of application>"
```

### Step 5
The Vagrantfiles that set up the application on both the production and test environments also require two more environment variables; the `DOCKER_USERNAME` (which should be set to `mathildegk`, as this is the username that our Docker images are saved at on Docker Hub, and the docker-compose file on the droplet needs to know this in order to pull the right images), and the `db_connection` (which is the connection string for the database that you want the application to connect to, likely either the minitwit-test-env-mysql or the minitwit-prod-env-mysql).

*For Windows (PowerShell):*

```PowerShell
$env:DOCKER_USERNAME="mathildegk"
$env:db_connection="Server=<IP>;Port=<port>;Database=minitwit;Uid=root;Pwd=<password>;SslMode=None;AllowPublicKeyRetrieval=True;"
```

*For Mac/Linux (Terminal):*
```Bash
export DOCKER_USERNAME="mathildegk"
export db_connection="Server=<IP>;Port=<port>;Database=minitwit;Uid=root;Pwd=<password>;SslMode=None;AllowPublicKeyRetrieval=True;"
```

You need to set the IP and port so that it matches the droplet running the database. The password should be the one you set up earlier, when you created the droplets for the database. 

### OBS: Dependencies
As you see in step 4 and 5, there are some "circular" dependencies between the application droplets and the database droplets. The database needs to have the IP address of the application, which you don't know if your application droplet isn't running yet, and likewise the application needs to have the connection string for the database, which you don't know if your database droplet isn't running yet. 

One approach to solve this dependency could be to start the database droplet first without specifying the application's IP address. By default, the Vagrantfile for the database sets up a firewall that denies all incoming requests, and then only if the APP_SERVER_IP variable is set, the firewall will allow requests from this IP. So if you omit setting this variable, you can get the droplet up and running (with a very strict firewall), and you can then get the connection string that you need in order to start the application droplet. As soon as the application droplet is running and you can get the application's IP address, you can then manually connect to the database droplet and run `sudo ufw allow from "<IP address>" to any port 3306` in order to "open up" the firewall so that it allows the application to access the database. 

### Setting up the droplets!
The Vagrantfile to set up the application in production environment can be found in the root of the project, so you need to be in the ITU-minitwit folder, and run the following command to create the droplet:
```Bash
vagrant up
```

To set up the application in a production-like test environment, navigate to the test_env_setup folder:

```Bash
cd test_env_setup
```

And then run this command to create the droplet:
```Bash
vagrant up
```

The Vagrantfiles to set up the production and test databases can be found in the folders `prod_env_db_setup` and `test_env_db_setup`, and can be started with the same `vagrant up` command. 

Future work would be to combine all the Vagrantfiles into a single file that can be run with different arguments to set up the different droplets. 