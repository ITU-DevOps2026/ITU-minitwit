## Current State
### SonarQube Cloud

Figure \ref{fig:current_state_sonarqube} shows a summary of the issues in our codebase from SonarQube Cloud. We currently have the following types of issues:

- **Security**: These two issues state that we should use 100.000 iterations for password hashing instead of 50.000 

- **Reliability**: Relates to adding "alt" attributes to images and using double instead of single square brackets for conditionals in shell scripts

- **Maintainability**: Relates to various code smells; unused variables, naming conventions, unnecessary modifiers, comments vs. documentation, exception handling etc. 

![Summary of SonarQube Cloud analysis of entire codebase \label{fig:current_state_sonarqube}](./images/current_state_sonarqube.png){width=80%}

### Codacy

Figue \ref{fig:current_state_codacy} shows the quality evolution of our codebase according to Codacy. We currently have the following types of issues: 

- **Security**, XX issues: Relates to our python tests; lack of timeouts on requests and hardcoded passwords (not really an issue, since they are test passwords)

- **Error prone**, XX issues: Relates to missing implementations of partial methods, unused variables and string operations

- **Compatibility**, XX issues: Syntax errors in our init_db.sql script

![Codacy quality evolution of codebase \label{fig:current_state_codacy}](./images/current_state_codacy.png){width=80%}

### CodeQL

CodeQL scans all our C# files and analyzes them for security vulnerabilities. Currently no security vulnerabilities have been found. 

### Trivy

TrivyScanner scans our Dockerfiles for vulnerabilities. Figure \ref{fig:current_state_trivy} shows the report summary for the latest scan. Currently we have one issue that we haven't found a fix for yet, which is caused by our MySQL dockerfile being run with root instead of a non-root user. From our initial research, it seems like this might not be easily possible with the MySQL 8.0 base image that we have chosen, so we have not managed to fix this issue yet. 

![Report from TrivyScanner on Dockerfile vulnerabilities \label{fig:current_state_trivy}](./images/current_state_trivy.png){width=60%}

If we had more time, we would have assigned someone the task of going through all the issues from SonarQube Cloud, Codacy and Trivy when setting up the tools in order to get the initial number of issues lowered. However, we have tried to fix issues continously if they got reported during our CI pipeline for a pull request. 
