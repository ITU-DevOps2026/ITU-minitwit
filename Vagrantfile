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

  MANAGER_COUNT = 1
  WORKER_COUNT = 2

  TEST_MANAGER_COUNT = 1
  TEST_WORKER_COUNT = 2


  # Create minitwit manager for docker swarm
  (1..MANAGER_COUNT).each do |i|
    config.vm.define "minitwit-manager-#{i}", autostart: false, primary: true do |manager|
      manager.vm.hostname = "minitwit-manager-#{i}"
      manager.vm.synced_folder "remote_files/prod_env", "/minitwit", type: "rsync"

      manager.vm.provision "shell", inline: <<-SHELL
        echo "export DOCKER_USERNAME='#{ENV['DOCKER_USERNAME']}'" > ~/.bash_profile
        echo "export db_connection='#{ENV['db_connection']}'" >> ~/.bash_profile
        echo "export MONITOR_AND_LOGGING_PRIVATE_IP='#{ENV['MONITOR_AND_LOGGING_PRIVATE_IP']}'" >> ~/.bash_profile
      SHELL
      manager.vm.provision "shell", path: "provision_scripts/docker_setup_script.sh"
      manager.vm.provision "shell", path: "provision_scripts/manager_setup_script.sh"
      manager.vm.provision "shell", path: "provision_scripts/configure_firewall_manager.sh"

      # starting the swarm and getting the token for the workers to join the swarm
      manager.trigger.after [:up, :provision] do |t|
        t.info = "Initializing swarm on manager and fetching worker token..."

        t.run = {
          inline: <<-SHELL
            bash -c "
              mkdir -p ./provision_scripts/.tokens
              MANAGER_IP=$(vagrant ssh minitwit-manager-#{i} -c 'curl -s http://169.254.169.254/metadata/v1/interfaces/private/0/ipv4/address' | tr -d '\\r')
              echo \\"$MANAGER_IP\\" > ./provision_scripts/.tokens/MANAGER_IP
              
              echo \\"Manager IP is:\\"
              echo $MANAGER_IP

              vagrant ssh minitwit-manager-#{i} -c \\"docker swarm init --advertise-addr $MANAGER_IP || true\\"

              vagrant ssh minitwit-manager-#{i} -c 'docker swarm join-token -q worker' | tr -d '\\r' > ./provision_scripts/.tokens/join_token

              vagrant ssh minitwit-manager-#{i} -c './deploy.sh'
            "
          SHELL
        }
      end
    end
  end

  # create minitwit workers for docker swarm
  (1..WORKER_COUNT).each do |i|
    config.vm.define "minitwit-worker-#{i}", autostart: false, primary: true do |worker|
      worker.vm.hostname = "minitwit-worker-#{i}"

      worker.vm.provision "shell", path: "provision_scripts/docker_setup_script.sh"
      worker.vm.provision "shell", path: "provision_scripts/configure_firewall_worker.sh"

      # Join the swarm created by the manager
      worker.trigger.after [:up, :provision] do |t|
        t.info = "Joining swarm created by manager"

        t.run = {
          inline: <<-SHELL
            bash -c "
              #set manager_IP
              MANAGER_IP=\\"$(tr -d '\r\n' < ./provision_scripts/.tokens/MANAGER_IP)\\"
              echo \\"Worker found Managers IP to be:\\"
              echo $MANAGER_IP

              #set the join-token
              JOIN_TOKEN=\\"$(tr -d '\r\n' < ./provision_scripts/.tokens/join_token)\\"
              echo \\"Worker found the join-token to be:\\"
              echo $JOIN_TOKEN

              vagrant ssh minitwit-worker-#{i} -c \\"docker swarm join --token $JOIN_TOKEN $MANAGER_IP:2377\\"
            "
          SHELL
        }
      end
    end
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
  
  # Configure Test MiniTwit Manager application
  (1..TEST_MANAGER_COUNT).each do |i|
    config.vm.define "minitwit-test-env-manager-#{i}", autostart: false, primary: true do |test_manager|
      test_manager.vm.hostname = "minitwit-test-env-manager-#{i}"
      test_manager.vm.synced_folder "remote_files/test_env", "/minitwit", type: "rsync"

      test_manager.vm.provision "shell", inline: <<-SHELL
        echo "export DOCKER_USERNAME='#{ENV['DOCKER_USERNAME']}'" > ~/.bash_profile
        echo "export db_connection='#{ENV['test_db_connection']}'" >> ~/.bash_profile
        echo "export MONITOR_AND_LOGGING_PRIVATE_IP='#{ENV['TEST_MONITOR_AND_LOGGING_PRIVATE_IP']}'" >> ~/.bash_profile
      SHELL
      test_manager.vm.provision "shell", path: "provision_scripts/docker_setup_script.sh"
      test_manager.vm.provision "shell", path: "provision_scripts/manager_setup_script.sh"
      test_manager.vm.provision "shell", path: "provision_scripts/configure_firewall_manager.sh"

      # starting the swarm and getting the token for the workers to join the swarm
      test_manager.trigger.after [:up, :provision] do |t|
        t.info = "Initializing swarm on manager and fetching worker token..."

        t.run = {
          inline: <<-SHELL
            bash -c "
              mkdir -p ./provision_scripts/.tokens
              MANAGER_IP=$(vagrant ssh minitwit-test-env-manager-#{i} -c 'curl -s http://169.254.169.254/metadata/v1/interfaces/private/0/ipv4/address' | tr -d '\\r')
              echo \\"$MANAGER_IP\\" > ./provision_scripts/.tokens/TEST_MANAGER_IP
              
              echo \\"Manager IP is:\\"
              echo $MANAGER_IP

              vagrant ssh minitwit-test-env-manager-#{i} -c \\"docker swarm init --advertise-addr $MANAGER_IP || true\\"

              vagrant ssh minitwit-test-env-manager-#{i} -c 'docker swarm join-token -q worker' | tr -d '\\r' > ./provision_scripts/.tokens/test_join_token

              vagrant ssh minitwit-test-env-manager-#{i} -c './deploy.sh'
            "
          SHELL
        }
      end
    end
  end


  # Configure Test MiniTwit Workers application
  (1..TEST_WORKER_COUNT).each do |i|
    config.vm.define "minitwit-test-env-worker-#{i}", autostart: false, primary: true do |test_worker|
      test_worker.vm.hostname = "minitwit-test-env-worker-#{i}"

      test_worker.vm.provision "shell", path: "provision_scripts/docker_setup_script.sh"
      test_worker.vm.provision "shell", path: "provision_scripts/configure_firewall_worker.sh"

      # Join the swarm created by the manager
      test_worker.trigger.after [:up, :provision] do |t|
        t.info = "Joining swarm created by manager"

        t.run = {
          inline: <<-SHELL
            bash -c "
              #set manager_IP
              MANAGER_IP=\\"$(tr -d '\r\n' < ./provision_scripts/.tokens/TEST_MANAGER_IP)\\"
              echo \\"Worker found Managers IP to be:\\"
              echo $MANAGER_IP

              #set the join-token
              JOIN_TOKEN=\\"$(tr -d '\r\n' < ./provision_scripts/.tokens/test_join_token)\\"
              echo \\"Worker found the join-token to be:\\"
              echo $JOIN_TOKEN

              vagrant ssh minitwit-test-env-worker-#{i} -c \\"docker swarm join --token $JOIN_TOKEN $MANAGER_IP:2377\\"
            "
          SHELL
        }
      end
    end
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
      echo "export APP_SERVER_IP='#{ENV['APP_SERVER_PRIVATE_IP']}'" >> /etc/profile.d/env.sh #Private ip of application that will be allowed to get logs and monitor info
      chmod +x /etc/profile.d/env.sh

      PROM_CONFIG="/monitoring/prometheus/prometheus.yml"
      if [ -f "$PROM_CONFIG" ]; then
        # REMEMBER TO CHANGE THESE TO APP_SERVER_PRIVATE_IP BEFORE IT IS MERGED INTO MAIN
        sed -i "s/APP_IP_PLACEHOLDER/#{ENV['APP_SERVER_PRIVATE_IP']}/g" "$PROM_CONFIG" #These IP's should be for production application, but are currently pointing to the test application, to test the setup works
        echo "Successfully injected #{ENV['APP_SERVER_PRIVATE_IP']} into $PROM_CONFIG" #These IP's should be for production application, but are currently pointing to the test application, to test the setup works
      fi
    SHELL
    server.vm.provision "shell", inline: $monitoring_and_logging_setup_script
  end
end

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
