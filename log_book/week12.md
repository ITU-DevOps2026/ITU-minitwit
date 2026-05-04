## Considerations on Reverse Proxy
We were going to implement Nginx, however we could see that is had some problems with using Docker Swarm and letting the workers also be able to serve requests:
https://forums.docker.com/t/help-with-docker-swarm-and-nginx-configuration/139510
Therefore, we decided to try to use Traefik that "supports configuration discovery out-of-the-box", i.e. it is made for a Cloud enviroment, so it can automatically watch the swarm and act accordingly if a new droplet is added, and unlike Nginx, we will not have to redefine the nginx.conf, to have this in mind. In short, Traefik watches Docker events, and updates the proxy accordingly.

Traefik also has "Let's Encrypt" support, which is very easily added in the docker file to make this work.