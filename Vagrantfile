# -*- mode: ruby -*-
# vi: set ft=ruby :

# Load environment variables from .env file if it exists
if File.exist?(".env")
  File.readlines(".env").each do |line|
    line.strip!
    next if line.empty? || line.start_with?("#")
    key, value = line.split("=", 2)
    ENV[key] = value if key && value
  end
end

require 'droplet_kit'

Vagrant.configure("2") do |config|
  # Configuring global settings that all droplets use
  config.vm.box = 'digital_ocean'
  config.vm.box_url = "https://github.com/devopsgroup-io/vagrant-digitalocean/raw/master/box/digital_ocean.box"
  config.ssh.private_key_path = '~/.ssh/id_ed25519'
  config.vm.synced_folder ".", "/vagrant", disabled: true

  config.vm.provider :digital_ocean do |provider|
    provider.ssh_key_name = ENV["SSH_KEY_NAME"]
    provider.token = ENV["DIGITAL_OCEAN_TOKEN"]
    provider.image = 'ubuntu-22-04-x64'
    provider.region = 'fra1'
    provider.size = 's-1vcpu-1gb'
    provider.privatenetworking = true
  end

  # Configure production MiniTwit application
  config.vm.define "minitwit-3", autostart: false, primary: true do |server|
    server.vm.hostname = "minitwit-3"
    server.vm.synced_folder "remote_files/prod_env", "/minitwit", type: "rsync"
    
    server.vm.provision "shell", inline: <<-SHELL
      echo "export DOCKER_USERNAME='#{ENV['DOCKER_USERNAME']}'" > /minitwit/.env
      echo "db_connection='#{ENV['db_connection']}'" >> /minitwit/.env
      echo "MONITOR_AND_LOGGING_PRIVATE_IP='#{ENV['MONITOR_AND_LOGGING_PRIVATE_IP']}'" >> /minitwit/.env
    SHELL
    server.vm.provision "shell", inline: $app_setup_script
  end

  # Configure production database 
  config.vm.define "minitwit-prod-env-mysql", autostart: false, primary: true do |server|
    server.vm.hostname = "minitwit-prod-env-mysql"
    server.vm.provision "shell", inline: <<-SHELL
      echo "export DB_PASSWORD='#{ENV['DB_PASSWORD']}'" > /etc/profile.d/db_env.sh
      echo "export APP_SERVER_IP='#{ENV['APP_SERVER_PRIVATE_IP']}'" >> /etc/profile.d/db_env.sh
      chmod +x /etc/profile.d/db_env.sh
    SHELL
    # This runs the reusable DB script and tells it which image to use
    server.vm.provision "shell", inline: $db_setup_script, args: "mathildegk/minitwit-mysql-prod"
  end
  
  # Configure test MiniTwit application
  config.vm.define "minitwit-test-env", autostart: false, primary: true do |server|
    server.vm.hostname = "minitwit-test-env"
    server.vm.synced_folder "remote_files/test_env", "/minitwit", type: "rsync"

    server.vm.provision "shell", inline: <<-SHELL
      echo "export DOCKER_USERNAME='#{ENV['DOCKER_USERNAME']}'" > /minitwit/.env
      echo "db_connection='#{ENV['test_db_connection']}'" >> /minitwit/.env
      echo "MONITOR_AND_LOGGING_PRIVATE_IP='#{ENV['MONITOR_AND_LOGGING_PRIVATE_IP']}'" >> /minitwit/.env
    SHELL
    server.vm.provision "shell", inline: $app_setup_script
  end

  # Configure test database
  config.vm.define "minitwit-test-env-mysql", autostart: false, primary: true do |server|
    server.vm.hostname = "minitwit-test-env-mysql"
    server.vm.provision "shell", inline: <<-SHELL
      echo "export DB_PASSWORD='#{ENV['TEST_DB_PASSWORD']}'" > /etc/profile.d/db_env.sh
      echo "export APP_SERVER_IP='#{ENV['TEST_APP_SERVER_PRIVATE_IP']}'" >> /etc/profile.d/db_env.sh
      chmod +x /etc/profile.d/db_env.sh
    SHELL
    # This runs the reusable DB script and tells it which image to use
    server.vm.provision "shell", inline: $db_setup_script, args: "mathildegk/minitwit-mysql-test"
  end

  # Configure monitoring and logging droplet
  config.vm.define "minitwit-monitoring-and-logging", autostart: false, primary: true do |server|
    server.vm.hostname = "minitwit-monitoring-and-logging"
    server.vm.synced_folder "remote_files/monitoring_and_logging", "/deploy", type: "rsync"
    server.vm.synced_folder "monitoring", "/monitoring", type: "rsync" # For Grafana and Prometheus having the dashboard
    server.vm.synced_folder "logging", "/logging", type: "rsync"
    server.vm.provision "shell", inline: <<-SHELL
      if [ ! -f /swapfile ]; then
        echo "Creating 1GB Swap file for memory safety..."
        sudo fallocate -l 1G /swapfile
        sudo chmod 600 /swapfile
        sudo mkswap /swapfile
        sudo swapon /swapfile
        # Make it permanent across reboots
        echo '/swapfile none swap sw 0 0' | sudo tee -a /etc/fstab
        echo "Swap created successfully."
      else
        echo "Swap file already exists."
      fi
      
      # Set swappiness to 1 (tells the OS to only use swap as a last resort)
      sudo sysctl vm.swappiness=1
    SHELL
    server.vm.provision "shell", inline: <<-SHELL
      echo "export APP_SERVER_IP='#{ENV['TEST_APP_SERVER_PRIVATE_IP']}'" >> /etc/profile.d/env.sh #Private ip of application that will be allowed to get logs and monitor info
      chmod +x /etc/profile.d/env.sh

      PROM_CONFIG="/monitoring/prometheus/prometheus.yml"
      if [ -f "$PROM_CONFIG" ]; then
        # REMEMBER TO CHANGE THESE TO APP_SERVER_PRIVATE_IP BEFORE IT IS MERGED INTO MAIN
        sed -i "s/APP_IP_PLACEHOLDER/#{ENV['TEST_APP_SERVER_PRIVATE_IP']}/g" "$PROM_CONFIG" #These IP's should be for production application, but are currently pointing to the test application, to test the setup works
        echo "Successfully injected #{ENV['TEST_APP_SERVER_PRIVATE_IP']} into $PROM_CONFIG" #These IP's should be for production application, but are currently pointing to the test application, to test the setup works
      fi
    SHELL
    server.vm.provision "shell", inline: $monitoring_and_logging_setup_script
  end
end

# Define scripts that are used for setting up application and database
# to be able to reuse it for production environment and test environment
$app_setup_script = <<-SHELL
  set -e # Exit on error

  while fuser /var/lib/apt/lists/lock >/dev/null 2>&1 ; do
    echo "Waiting for other software managers to finish..."
    sleep 5
  done
  # The following addresses an issue in DO's Ubuntu images, which still contain a lock file
  sudo apt-get update

  # Install docker and docker compose
  sudo apt install -qq -y docker.io
  sudo apt install -qq -y docker-compose-v2

  sudo systemctl status docker

  echo -e "\nVerifying that docker works ...\n"
  docker run --rm hello-world
  docker rmi hello-world

  source /minitwit/.env

  echo -e "\nOpening port for minitwit ...\n"
  sudo ufw allow 5035/tcp && \
  sudo ufw allow 22/tcp && \
  sudo ufw allow 3000/tcp

  # Only allow our monitoring's private IP to access the metrics port
  echo "About to enter if statement for port 9091"
  if [ ! -z "$MONITOR_AND_LOGGING_PRIVATE_IP" ]; then
    echo "Entered if statement for port 9091"
    sudo ufw allow from "$MONITOR_AND_LOGGING_PRIVATE_IP" to any port 9091
    echo "Firewall: Allowed 9091 for $MONITOR_AND_LOGGING_PRIVATE_IP"
  fi

  sudo ufw --force enable

  echo ". $HOME/.bashrc" >> $HOME/.bash_profile
  echo -e "\nConfiguring credentials as environment variables...\n"
  source $HOME/.bash_profile

  echo -e "\nSelecting Minitwit Folder as default folder when you ssh into the server...\n"
  echo "cd /minitwit" >> ~/.bash_profile

  sed -i 's/\r$//' /minitwit/deploy.sh
  chmod +x /minitwit/deploy.sh
  echo -e "\nVagrant setup done ..."
SHELL

$db_setup_script = <<-SHELL
  set -e # Exit on error

  while fuser /var/lib/apt/lists/lock >/dev/null 2>&1 ; do
    echo "Waiting for other software managers to finish..."
    sleep 5
  done

  sudo apt-get update

  sudo apt install -qq -y docker.io docker-compose-v2 ufw

  # 1. GET THE PRIVATE IP
  # In DigitalOcean, eth1 is typically the private network interface
  PRIVATE_IP=$(ip -4 addr show eth1 | grep -oP '(?<=inet\\s)\\d+(\\.\\d+){3}')
  echo "Detected Private IP: $PRIVATE_IP"
  source /etc/profile.d/db_env.sh

  # 2. CONFIGURE FIREWALL
  sudo ufw default deny incoming
  sudo ufw default allow outgoing
  sudo ufw allow ssh
  
  # Only allow your application's private IP to access the DB port
  if [ ! -z "$APP_SERVER_IP" ]; then
    sudo ufw allow from "$APP_SERVER_IP" to any port 3306
    echo "Firewall: Allowed 3306 for $APP_SERVER_IP"
  fi
  
  sudo ufw --force enable

  # 3. RUN DOCKER BOUND TO PRIVATE IP
  docker run -d \
    --name minitwit_mysql \
    --restart unless-stopped \
    -p $PRIVATE_IP:3306:3306 \
    --mount type=volume,source=minitwit_db_data,target=/var/lib/mysql \
    -e MYSQL_ROOT_PASSWORD="$DB_PASSWORD" \
    $1
SHELL

$monitoring_and_logging_setup_script = <<-SHELL
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
SHELL
