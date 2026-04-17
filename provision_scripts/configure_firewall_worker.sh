echo -e "\nOpening ports for worker ...\n"
# Port 22 for ssh access, 4789 for routing mesh, 5035 is the application port
# port 7946 tcp+udp: Container network discovery
sudo ufw allow 22/tcp 
sudo ufw allow 4789/udp
sudo ufw allow 5035/tcp
sudo ufw allow 7946/tcp
sudo ufw allow 7946/udp


# Only allow our monitoring's private IP to access the metrics port
echo "About to enter if statement for port 9091"
if [ ! -z "$MONITOR_AND_LOGGING_PRIVATE_IP" ]; then
echo "Entered if statement for port 9091"
sudo ufw allow from "$MONITOR_AND_LOGGING_PRIVATE_IP" to any port 9091
echo "Firewall: Allowed 9091 for $MONITOR_AND_LOGGING_PRIVATE_IP"
fi

sudo reload 
sudo ufw --force enable