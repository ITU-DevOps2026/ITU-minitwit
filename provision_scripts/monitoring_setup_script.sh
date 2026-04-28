set -e # Exit on error

while fuser /var/lib/apt/lists/lock >/dev/null 2>&1 ; do
    echo "Waiting for other software managers to finish..."
    sleep 5
done

sudo apt-get update
sudo apt install -qq -y docker.io docker-compose-v2 ufw

# In DigitalOcean, eth1 is typically the private network interface
PRIVATE_IP=$(ip -4 addr show eth1 | grep -oP '(?<=inet\\s)\\d+(\\.\\d+){3}')
echo "Detected Private IP: $PRIVATE_IP"

# Make .env file that Docker Compose will be using
echo "MONITOR_AND_LOGGING_PRIVATE_IP=$PRIVATE_IP" > /deploy/.env

# CONFIGURE FIREWALL
sudo ufw default deny incoming
sudo ufw default allow outgoing
sudo ufw allow ssh

source /etc/profile.d/env.sh

# Allow Grafana UI (Public) - fine to allow since grafana is locked behind a user
sudo ufw allow 3000/tcp

# Allow applications private ip to access ports containing logs and monitoring
if [ ! -z "$APP_SERVER_IP" ]; then
    sudo ufw allow from "$APP_SERVER_IP" to any port 9200
    sudo ufw allow from "$APP_SERVER_IP" to any port 9090
    echo "Firewall: Monitoring ports opened for $APP_SERVER_IP"
fi

echo -e "\nSelecting deploy Folder as default folder when you ssh into the server...\n"
echo "cd /deploy" >> ~/.bash_profile

sed -i 's/\r$//' /deploy/deploy.sh
chmod +x /deploy/deploy.sh
echo -e "\nVagrant setup done ..."