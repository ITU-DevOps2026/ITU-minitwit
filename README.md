# Dev-Ops Course (Master at ITU, KSDSESM1KU 2026 Spring)
**Group**: *Pat Myaz*  \
**Members:**  *Carmen Alberte Nielsen, Casper Storm Frøding, Mads Christian Nørklit Jensen, Max Brix Koch, Mathilde Julie Gonzalez-Knudsen*

## Git commit message template
To tell Git to use gitmessage as commit template file, run the following command in your terminal while being in the root directory of your Git repository:
```bash
git config --local commit.template .gitmessage
```
## MiniTwit C# Application 
### Requirements to run locally
- dotnet 10.0
- A terminal capable of running sh if you want to run the control.sh script

### How to run in Container
See [Docker readme](/README.Docker.md)

### How to run minitwit C# application
Navigate to the `/minitwit` folder and run the following command in the terminal:
```bash
dotnet run
```
This will start the minitwit application. You can access it by opening a web browser and going to `http://localhost:5035`. 
You should see the minitwit homepage where you can see the public timeline, with options to sign up and sign in.

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

## Minitwit on Digital Ocean via Vagrant
### Requirements
* A terminal
* vagrant installation

### First time guide!
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
OBS! This step I have not done permanently, in this guide (i.e. this will need to be done again when the terminal is restarted), so if someone finds a way to save these variables permanently, please change the guide accordingly

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
OBS! This step I have not done permanently, in this guide (i.e. this will need to be done again when the terminal is restarted), so if someone finds a way to save these variables permanently, please change the guide accordingly


### After First time guide!
As long as you have set your `SSH_KEY_NAME` and `DIGITAL_OCEAN_TOKEN` (in the current terminal you have opened), you should be able to navigate to the minitwit folder:

```Bash
cd ./ITU-minitwit/minitwit/
```

And then run this command:
```Bash
vagrant up
```