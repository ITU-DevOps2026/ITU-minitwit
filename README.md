# Dev-Ops Course (Master at ITU, KSDSESM1KU 2026 Spring)
**Group**: *Pat Myaz*  \
**Members:**  *Carmen Alberte Nielsen, Casper Storm Frøding, Mads Christian Nørklit Jensen, Max Brix Koch, Mathilde Julie Gonzalez-Knudsen*

## MiniTwit C# Application 
### Requirements
- dotnet 10.0
- TBD

### How to run minitwit c# application
Navigate to the `minitwit-csharp/minitwit` folder and run the following command in the terminal:
```bash
dotnet run
```
This will start the minitwit application. You can access it by opening a web browser and going to `http://localhost:5035`. You should see the minitwit homepage where you

### How to run minitwit c# tests
In one terminal, run the minitwit application by navigating to the `minitwit-csharp/minitwit` folder and running the following command:
```bash
dotnet run
```
Then in another terminal, run the following command to execute the tests:
```bash
pytest -v refactored_minitwit_tests.py
```
You should see all tests passing successfully! 🥇


## SOMETHING SOMETHING

## Git commit message template
To tell Git to use gitmessage as commit template file, run the following command in your terminal while being in the root directory of your Git repository:

```bash
git config --local commit.template .gitmessage
```