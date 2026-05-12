echo -e "\nOpening ports for manager ...\n"
# Port 22 for ssh access, 2376 for Docker TLS, 2377 for Swarm management, 4789 for routing mesh, 5035 is the application port
# port 7946 tcp+udp: Container network discovery
sudo ufw allow 22/tcp 
sudo ufw allow 2376/tcp 
sudo ufw allow 2377/tcp
sudo ufw allow 4789/udp
sudo ufw allow 5035/tcp
sudo ufw allow 7946/tcp
sudo ufw allow 7946/udp

# Change configurations in the sshd_config file to limit ssh retries, the graceperiod of a non authorized connection
# and disabling password authentication to reduce brute force attacks
echo "Setting a higher security baseline for SSH"
sudo sed -i 's/#PasswordAuthentication yes/PasswordAuthentication no/' /etc/ssh/sshd_config
sudo sed -i 's/#LoginGraceTime 2m/LoginGraceTime 30/' /etc/ssh/sshd_config
sudo sed -i 's/#MaxAuthTries 6/MaxAuthTries 3/' /etc/ssh/sshd_config
sudo sed -i 's/#MaxSessions 10/MaxSessions 5/' /etc/ssh/sshd_config


# Only allow our monitoring's private IP to access the metrics port
echo "About to enter if statement for port 9091"
if [ ! -z "$MONITOR_AND_LOGGING_PRIVATE_IP" ]; then
echo "Entered if statement for port 9091"
sudo ufw allow from "$MONITOR_AND_LOGGING_PRIVATE_IP" to any port 9091
echo "Firewall: Allowed 9091 for $MONITOR_AND_LOGGING_PRIVATE_IP"
fi

sudo ufw reload 
sudo ufw --force enable