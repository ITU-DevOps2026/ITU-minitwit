# -*- mode: ruby -*-
# vi: set ft=ruby :

# Load environment variables from .env file if it exists
if File.exist?(".env")
  File.readlines(".env").each do |line|
    line.strip!
    next if line.empty? || line.start_with?("#")
    key, value = line.split("=", 2)
    value = value.strip.gsub(/\A["']|["']\z/, '') if value
    ENV[key] = value if key && value
  end
end

require 'json'

token = ENV["DIGITAL_OCEAN_TOKEN"]
response = `curl -s -H "Authorization: Bearer #{token}" https://api.digitalocean.com/v2/account/keys?per_page=200`
keys_data = JSON.parse(response)
team_public_keys = keys_data["ssh_keys"]
  .map { |k| k["public_key"].strip }
  .join("\n")

# Create tags
client = DropletKit::Client.new(access_token: ENV['DIGITAL_OCEAN_TOKEN'])
client.tags.create(DropletKit::Tag.new(name: 'minitwit-swarm'))
client.tags.create(DropletKit::Tag.new(name: 'minitwit-swarm-test'))
client.tags.create(DropletKit::Tag.new(name: 'Prod-DB'))
client.tags.create(DropletKit::Tag.new(name: 'Test-DB'))

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
  (1..TEST_MANAGER_COUNT).each do |i|
    config.vm.define "minitwit-test-manager-#{i}", autostart: true, primary: true do |manager|
      manager.vm.provider :digital_ocean do |provider|
        provider.tags = ["minitwit-swarm-test"]
      end
      manager.vm.hostname = "minitwit-test-manager-#{i}"
      manager.vm.synced_folder "remote_files/test_env", "/minitwit", type: "rsync"
      
      manager.trigger.before :"Vagrant::Action::Builtin::Provision", type: :action do |t|
        t.info = "Write own ip to file"
        t.ruby do |env, machine|
          # This block is NATIVE Ruby to make it work on different OS.
          # We use machine.communicate.execute to run commands on the droplet
          
          # 1. Get the IP from the droplet
          command = "curl -s http://169.254.169.254/metadata/v1/interfaces/private/0/ipv4/address"
          ip = ""
          machine.communicate.execute(command) do |type, data|
            ip << data if type == :stdout
          end
          ip = ip.strip
    
          # 2. Write to the host file (Windows/Mac local folder)
          File.write("./provision_scripts/manager_IP", ip)
          puts "Successfully saved Manager IP: #{ip}"

          # 3. Assign reserved ip to droplet
          command = "curl -s http://169.254.169.254/metadata/v1/id"
          DROPLET_ID = ""
          machine.communicate.execute(command) do |type, data|
            DROPLET_ID << data if type == :stdout
          end
          DROPLET_ID = DROPLET_ID.strip
          puts "Found ID of Manager: #{DROPLET_ID}"
          
          RESERVED_IP = ENV['TEST_MANAGER_RES_IP'] || ""
          TOKEN = ENV['DIGITAL_OCEAN_TOKEN'] || ""
          command = "curl -X POST -H 'Content-Type: application/json' -H 'Authorization: Bearer #{TOKEN}' -d '{\"type\":\"assign\",\"droplet_id\":#{DROPLET_ID}}' 'https://api.digitalocean.com/v2/reserved_ips/#{RESERVED_IP}/actions'"

          machine.communicate.execute(command)
          puts "Succesfully assigned reserved ip: #{RESERVED_IP} to droplet"
        end
      end
      # Ensure that database and monitoring droplets has been created and collect their IP's
      manager.trigger.before :"Vagrant::Action::Builtin::Provision", type: :action do |t|
        t.info = "Collecting database and monitoring IP's and injecting them into manager"
        t.ruby do |env, machine|
          db_ip_file = "./provision_scripts/db_IP"
          monitoring_private_ip_file = "./provision_scripts/monitoring_private_IP"
          monitoring_public_ip_file = "./provision_scripts/monitoring_public_IP"

          until File.exist?(db_ip_file) && File.exist?(monitoring_private_ip_file) && File.exist?(monitoring_public_ip_file)
            puts "Manager: Waiting for db_IP and monitoring_IP files..."
            sleep 2
          end

          DB_IP_VAL = File.read(db_ip_file).strip
          MONITOR_PRIVATE_IP_VAL = File.read(monitoring_private_ip_file).strip
          MONITOR_PUBLIC_IP_VAL = File.read(monitoring_public_ip_file).strip

          DB_TEMP_CONN = ENV['test_db_connection'] || ""
          DOCKER_USER = ENV['DOCKER_USERNAME'] || ""

          FINAL_DB_CONN = DB_TEMP_CONN.gsub("<db_IP>", DB_IP_VAL)

          commands = [
            "echo 'export db_connection=\"#{FINAL_DB_CONN}\"' > ~/.bash_profile",
            "echo 'export TEST_MONITOR_AND_LOGGING_PRIVATE_IP=\"#{MONITOR_PRIVATE_IP_VAL}\"' >> ~/.bash_profile",
            "echo 'export TEST_MONITOR_AND_LOGGING_PUBLIC_IP=\"#{MONITOR_PUBLIC_IP_VAL}\"' >> ~/.bash_profile",
            "echo 'export DOCKER_USERNAME=\"#{DOCKER_USER}\"' >> ~/.bash_profile"
          ]

          # Execute each command on the manager droplet
          commands.each do |cmd|
            machine.communicate.execute(cmd)
          end
          puts "Successfully injected DB and Monitoring IPs into ~/.bash_profile"
        end
      end

      manager.vm.provision "shell", path: "provision_scripts/docker_setup_script.sh", binary: false
      manager.vm.provision "shell", path: "provision_scripts/manager_setup_script.sh", binary: false
      manager.vm.provision "shell", path: "provision_scripts/configure_firewall_manager.sh", binary: false
      manager.vm.provision "shell", name: "inject_ssh_keys", inline: <<-SHELL
        echo "#{team_public_keys}" >> /root/.ssh/authorized_keys
        echo "SSH keys injected"
      SHELL

      manager.trigger.after :up do |t|
        t.info = "Initializing swarm on manager and fetching worker token..."
        t.ruby do |env, machine|
          command = "curl -s http://169.254.169.254/metadata/v1/interfaces/private/0/ipv4/address"
          MANAGER_IP = ""
          machine.communicate.execute(command) do |type, data|
            MANAGER_IP << data if type == :stdout
          end
          MANAGER_IP = MANAGER_IP.strip
          
          # Initialize the swarm
          command = "docker swarm init --advertise-addr #{MANAGER_IP} || true"
          machine.communicate.execute(command)

          # Get the join token for the swarm
          command = "docker swarm join-token -q worker"
          join_token = ""
          machine.communicate.execute(command) do |type, data|
            join_token << data if type == :stdout
          end
          join_token = join_token.strip
    
          # Write the join token to local file (Windows/Mac local folder)
          File.write("./provision_scripts/join_token", join_token)
          puts "Successfully saved join token: #{join_token}"

          # Initialize the swarm
          command = "./deploy.sh"
          machine.communicate.execute(command)
        end
      end
    end
  end

  # create minitwit workers for docker swarm
  (1..WORKER_COUNT).each do |i|
    config.vm.define "minitwit-test-worker-#{i}", autostart: true, primary: true do |worker|
      worker.vm.provider :digital_ocean do |provider|
        provider.tags = ["minitwit-swarm-test"]
      end
      worker.vm.hostname = "minitwit-test-worker-#{i}"

      worker.vm.provision "shell", path: "provision_scripts/docker_setup_script.sh", binary: false
      worker.vm.provision "shell", path: "provision_scripts/configure_firewall_worker.sh", binary: false
      worker.vm.provision "shell", name: "inject_ssh_keys", inline: <<-SHELL
        echo "#{team_public_keys}" >> /root/.ssh/authorized_keys
        echo "SSH keys injected"
      SHELL

      # Join the swarm created by the manager
      worker.trigger.after [:up, :provision] do |t|
        t.info = "Joining swarm created by manager"
        t.ruby do | env, machine| 
          manager_ip_file = "./provision_scripts/manager_IP"
          join_token_file = "./provision_scripts/join_token"

          until File.exist?(manager_ip_file) && File.exist?(join_token_file)
            puts "minitwit-test-worker-#{i} is waiting for manager_IP file and join token file..."
            sleep 2
          end
          MANAGER_IP_VAL = File.read(manager_ip_file).strip
          JOIN_TOKEN = File.read(join_token_file).strip

          command = "sudo docker swarm join --token #{JOIN_TOKEN} #{MANAGER_IP_VAL}:2377"
          puts "minitwit-test-worker-#{i} trying to join at #{MANAGER_IP_VAL} with token #{JOIN_TOKEN}"

          machine.communicate.execute(command) do |type, data|
            puts data if type == :stdout
          end
        end
      end
    end
  end

  # Configure production database 
  config.vm.define "minitwit-test-env-mysql", autostart: true, primary: true do |server|
    server.vm.provider :digital_ocean do |provider|
      provider.tags = ["Test-DB"]
    end
    server.vm.hostname = "minitwit-test-env-mysql"
    server.vm.provision "shell", inline: <<-SHELL
      echo "export DB_PASSWORD='#{ENV['TEST_DB_PASSWORD']}'" >> /etc/profile.d/db_env.sh
      chmod +x /etc/profile.d/db_env.sh
    SHELL

    server.vm.provision "shell", path: "provision_scripts/docker_setup_script.sh", binary: false
    # This runs the reusable DB script and tells it which image to use
    server.vm.provision "shell", path: "provision_scripts/database_setup_script.sh", binary: false, args: "mathildegk/minitwit-mysql-prod"
    server.vm.provision "shell", name: "inject_ssh_keys", inline: <<-SHELL
      echo "#{team_public_keys}" >> /root/.ssh/authorized_keys
      echo "SSH keys injected"
    SHELL

    server.trigger.before :"Vagrant::Action::Builtin::Provision", type: :action do |t|
      t.info = "Writing own IP to file, and fetching app manager's IP"
      t.ruby do |env, machine|
          command = "curl -s http://169.254.169.254/metadata/v1/interfaces/private/0/ipv4/address"
          DB_IP = ""
          machine.communicate.execute(command) do |type, data|
            DB_IP << data if type == :stdout
          end
          DB_IP = DB_IP.strip

          File.write("./provision_scripts/db_IP", DB_IP)
          puts "Successfully saved Database IP: #{DB_IP}"

          manager_ip_file = "./provision_scripts/manager_IP"

          until File.exist?(manager_ip_file)
            puts "Host: Waiting for manager_IP file..."
            sleep 2
          end
          MANAGER_IP_VAL = File.read(manager_ip_file).strip

          machine.communicate.execute("echo 'export APP_SERVER_IP=\"#{MANAGER_IP_VAL}\"' | sudo tee /etc/profile.d/db_env.sh")
          puts "Successfully injected Manager IP into db_env.sh"

          # Assign reserved ip to droplet
          command = "curl -s http://169.254.169.254/metadata/v1/id"
          DROPLET_ID = ""
          machine.communicate.execute(command) do |type, data|
            DROPLET_ID << data if type == :stdout
          end
          DROPLET_ID = DROPLET_ID.strip
          puts "Found ID of DB: #{DROPLET_ID}"
          
          RESERVED_IP = ENV['TEST_DB_RES_IP'] || ""
          TOKEN = ENV['DIGITAL_OCEAN_TOKEN'] || ""
          command = "curl -X POST -H 'Content-Type: application/json' -H 'Authorization: Bearer #{TOKEN}' -d '{\"type\":\"assign\",\"droplet_id\":#{DROPLET_ID}}' 'https://api.digitalocean.com/v2/reserved_ips/#{RESERVED_IP}/actions'"

          machine.communicate.execute(command)
          puts "Succesfully assigned reserved ip: #{RESERVED_IP} to droplet"
      end   
    end
  end
  
  config.vm.define "minitwit-test-monitoring-and-logging", autostart: true, primary: true do |server|
    server.vm.hostname = "minitwit-test-monitoring-and-logging"
    server.vm.synced_folder "remote_files/monitoring_and_logging", "/deploy", type: "rsync"
    server.vm.synced_folder "monitoring", "/monitoring", type: "rsync" # For Grafana and Prometheus having the dashboard
    server.vm.synced_folder "logging", "/logging", type: "rsync"

    server.trigger.before :"Vagrant::Action::Builtin::Provision", type: :action do |t|
        t.info = "Writing own IP to file, and fetching app manager's IP"
        t.ruby do |env, machine|
          command = "curl -s http://169.254.169.254/metadata/v1/interfaces/private/0/ipv4/address"
          MONITORING_PRIVATE_IP = ""
          machine.communicate.execute(command) do |type, data|
            MONITORING_PRIVATE_IP << data if type == :stdout
          end
          MONITORING_PRIVATE_IP = MONITORING_PRIVATE_IP.strip
          File.write("./provision_scripts/monitoring_private_IP", MONITORING_PRIVATE_IP)
          puts "Successfully saved Monitoring Private IP: #{MONITORING_PRIVATE_IP}"
          
          public_ip_command = "curl -s http://169.254.169.254/metadata/v1/interfaces/public/0/ipv4/address"
          MONITORING_PUBLIC_IP = ""
          machine.communicate.execute(public_ip_command) do |type, data|
            MONITORING_PUBLIC_IP << data if type == :stdout
          end
          MONITORING_PUBLIC_IP = MONITORING_PUBLIC_IP.strip
          File.write("./provision_scripts/monitoring_public_IP", MONITORING_PUBLIC_IP)
          puts "Successfully saved Monitoring Public IP: #{MONITORING_PUBLIC_IP}"

          manager_ip_file = "./provision_scripts/manager_IP"

          until File.exist?(manager_ip_file)
            puts "Host: Waiting for manager_IP file..."
            sleep 2
          end
          MANAGER_IP_VAL = File.read(manager_ip_file).strip

          machine.communicate.execute("echo 'export APP_SERVER_IP=\"#{MANAGER_IP_VAL}\"' | sudo tee /etc/profile.d/env.sh")
          puts "Successfully injected Manager IP into env.sh"

          # Assign reserved ip to droplet
          command = "curl -s http://169.254.169.254/metadata/v1/id"
          DROPLET_ID = ""
          machine.communicate.execute(command) do |type, data|
            DROPLET_ID << data if type == :stdout
          end
          DROPLET_ID = DROPLET_ID.strip
          puts "Found ID of Monitoring droplet: #{DROPLET_ID}"
          
          RESERVED_IP = ENV['TEST_MONITORING_RES_IP'] || ""
          TOKEN = ENV['DIGITAL_OCEAN_TOKEN'] || ""
          command = "curl -X POST -H 'Content-Type: application/json' -H 'Authorization: Bearer #{TOKEN}' -d '{\"type\":\"assign\",\"droplet_id\":#{DROPLET_ID}}' 'https://api.digitalocean.com/v2/reserved_ips/#{RESERVED_IP}/actions'"

          machine.communicate.execute(command)
          puts "Succesfully assigned reserved ip: #{RESERVED_IP} to droplet"
      end
    end

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
      chmod +x /etc/profile.d/env.sh
      source /etc/profile.d/env.sh

      PROM_CONFIG="/monitoring/prometheus/prometheus.yml"
      if [ -f "$PROM_CONFIG" ]; then
        sed -i "s/APP_IP_PLACEHOLDER/$APP_SERVER_IP/g" "$PROM_CONFIG" 
        echo "Successfully injected $APP_SERVER_IP into $PROM_CONFIG"
      fi
    SHELL
    server.vm.provision "shell", path: "provision_scripts/docker_setup_script.sh", binary: false
    server.vm.provision "shell", path: "provision_scripts/monitoring_setup_script.sh", binary: false
    server.vm.provision "shell", name: "inject_ssh_keys", inline: <<-SHELL
      echo "#{team_public_keys}" >> /root/.ssh/authorized_keys
      echo "SSH keys injected"
    SHELL
    server.trigger.after :up do |t|
      t.ruby do |env, machine|
        command = "./deploy.sh"
        machine.communicate.execute(command)
      end
    end
  end
end