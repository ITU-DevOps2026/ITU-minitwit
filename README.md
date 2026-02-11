# Dev-Ops Course (Master at ITU, KSDSESM1KU 2026 Spring)
**Group**: *Pat Myaz*  \
**Members:**  *Carmen Alberte Nielsen, Casper Storm Frøding, Mads Christian Nørklit Jensen, Max Brix Koch, Mathilde Julie Gonzalez-Knudsen*


## How to run
* TODO


## SOMETHING SOMETHING

## Running the control.sh script in the C# application
The script can be run with different arguments from the root folder of the repository.

### Init
```sh
./control.sh init
```
With init the script will check if there is database in the same root as the script, if there is it exits, if not it runs runs dotnet run init, which will trigger the Init_db() function, using the schema.sql file to drop any user, follower, and message table and then create them.

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