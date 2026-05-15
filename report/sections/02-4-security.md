## Security

Security in our system is enforced on several layers. 

We've configured a DigitalOcean firewall for our database, following a default-deny principle, i.e all inbound traffic is blocked, unless manually stated otherwise. Inbound traffic is allowed for SSH on port 22, and droplets with the minitwit-swarm tag on DO is allowed for MySQL on port 3306

Our droplets all have UFW set up, also following a default-deny principle for incoming traffic. During setup of the droplets, the configuration scripts opens specific ports for inbound traffic. The configuration scripts also updates the default ssh settings, limiting retries, reducing the graceperiod and disabling password authentication.


For transport security, we use Let's Encrypt to provision SSL/TLS certificates, ensuring that communication between clients and the application is encrypted over HTTPS. Furthermore we ensure that attempts to access the site through HTTP is redirected to a secure HTTPS connection. 

On the application level, we enforce security through frameworks implemented in the codebase. 
Database access is handled through Entity Framework, which uses parameterized queries to help prevent SQL injection attacks. 
Razor Pages automatically encodes all HTML, which protects against cross site scripting (XSS) from malicious users.
For users of the minitwit application passwords are encrypted using PBKDF2 with salting, which ensures that passwords are securely stored, and safe from brute force attacks.
Our code base has .NET analyzers, which picks up on security diagnostics, and our .editorconfig file ensures that any security issues discovered through analysers will be marked as errors, meaning the application will fail to build, if a security issue is present.

Within our CI/CD pipeline we have have two security tools, Trivy for Dockerfiles and images, and CodeQL for static analysis of the codebase. 
Trivy is run in its own workflow, which scans for misconfigurations in all Dockerfiles present in the repository. It then builds the images and scans for vulnerabilities in those. If any misconfigurations or vulnerabilities are found, it will fail the workflow.
CodeQL runs in its own workflow, which scans the codebase by running a set of queries to find common vulnerabilities in the code base. 
Both workflows runs when a pull request to main is created, on push to main, and each week on Monday. 
Running both workflows on a schedule, rather than just on PR and push is relevant, since new vulnerabilities can be discovered any time, and therefore code that was thought to be secure when the code was pushed, may be discovered to be insecure. The weekly workflow helps ensure that we become aware of newly discovered vulnerabilities.

In addition to the things discussed in this chapter, we also did a Security Assesment of our current system, to determine where we should put our effort, in terms of security, if we had more time to do this project. This can be found in the appendix, in the section [Risk Assesment](#risk-assesment) .