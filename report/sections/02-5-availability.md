## Availability

To ensure availability of our application, we use Docker Swarm for orchestration and Traefik as a reverse proxy and load balancer, across DO droplets.

Docker Swarm enables us to run replicated services across several nodes, ensuring that if one container or droplet fails, another instance continues serving requests. 
The size of our Swarm is defined in our Vagrant file, and is one manager, with two workers. In the compose_stack we define the desired amount of replicas we want for the minitwit service. Docker Swarm continuously monitors the desired state, so that if a container crashes, it automatically starts a new instance to replace it. The extra replicas ensures that the application remains available even during container failures.

Traefik distributes incoming request evenly across the services, such that no service gets overworked. If a container fails, Traefik detects that it is no longer available and stops routing requests to it, ensuring requests don't get sent to unavailable services.

### Scalability
In our setup scalability is manual, where horizontal scalability can be achieved by increasing the size of our Swarm in the Vagrant file, as well as the amount of replicas in the compose_stack. Vertical scaling can be achieved by increasing the resources of existing droplets on DO.
