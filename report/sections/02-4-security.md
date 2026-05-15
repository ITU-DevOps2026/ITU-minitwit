## Security

Security in our system is enforced on several layers. 

We've configured a DigitalOcean firewall for our database, following a default-deny principle, i.e all inbound traffic is blocked, unless manually stated otherwise. Inbound traffic is allowed for SSH on port 22, and droplets with the minitwit-swarm tag on DO is allowed for MySQL on port 3306

All of our droplets have UFW setup, which also follows a default-deny principle, for incoming traffic, and during our setup of the droplets the configuration script allows only inbound traffic on the ports that are needed.

For transport security, we use Let's Encrypt to provision SSL/TLS certificates, ensuring that communication between clients and the application is encrypted over HTTPS. And we further ensure that attempts to access the site through HTTP is redirected to a secure HTTPS connection. 

On the application level, we enforce security through frameworks implemented in the codebase. 
Database access is handled through Entity Framework, which uses parameterized queries to help prevent SQL injection attacks. 
Razor Pages automatically encodes all HTML, which protects against cross site scripting (XSS) from malicious users.
For users of the minitwit application passwords are encrypted using PBKDF2 with salting, which ensures that passwords are securely stored, and safe from brute force attacks.
Our code base also has .NET analyzers, which will pick up on some security diagnostics, and our .editorconfig file ensures that any security issues discovered through analysers will be marked as errors, meaning the application will fail to build, if a security issue is present.

Within our CI/CD pipeline we have have two security tools, Trivy for Dockerfiles and images, and CodeQL for static analysis of the codebase. 
Trivy is run in its own workflow, which scans for misconfigurations in all Dockerfiles present in the repository, even those that aren't used on droplets. It then builds the images and scans for vulnerabilities in those. If any misconfigurations or vulnerabilities are found, it will fail the workflow.
CodeQL runs in its own workflow, which scans the codebase by running a set of queries to find common vulnerabilities in the code base. 
These workflow runs when a pull request to main is created, on push to main, and each week on Monday. 
Running these workflows on a schedule, rather than just on PR and push is relevant, since new vulnerabilities can be discovered any time, and therefore code that was thought to be secure when the code was pushed, may be discovered to be insecure. The weekly workflow helps ensure that we become aware of newly discovered vulnerabilities.