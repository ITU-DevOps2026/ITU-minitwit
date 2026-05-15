## Infrastructure as Code
Broadly the infrastructure of this project exists as code mainly in the form of a Vagrantfile and provisioning scripts. This means we can reliably spin up a production or test environment from scratch with a singular command, ensuring each build is the same. Figure \ref{fig:Vagrant-diagram} depicts the exact flow, from executing *vagrant up* until our whole system is running. 

### Pros and cons
Our setup with a Vagrantfile and scripts for provisioning is imperative, which has pros and cons compared to a declarative alternative such as Terraform and Ansible.

Pros:
- Control of what is executed and when.
- Adaptable to our exact infrastructure.
- Less things to manage, i.e no things such as a state file that needs to be locked correctly.
- Failures produce a clear sequence of steps to trace, making debugging more approachable.

Cons:
- Ensuring idempotency is solely on the developers and is harder to achieve.
- Hard to discover if the system drifts.
- Readability diminishes with more complex systems.
- Hard to scale when the system is already running.
- Fragile when the vagrantfile is running, i.e if something crashes during provisioning, there is no built-in mechanism to resume from where we crashed, or to roll back cleanly. 

![Flow of vagrantfile from executing "vagrant up" until system is running \label{fig:Vagrant-diagram}](./images/vagrant-flow.png){width=80%}