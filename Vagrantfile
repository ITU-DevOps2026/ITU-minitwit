# -*- mode: ruby -*-
# vi: set ft=ruby :

$ip_file = "db_ip.txt"

Vagrant.configure("2") do |config|
  config.vm.box = 'digital_ocean'
  config.vm.box_url = "https://github.com/devopsgroup-io/vagrant-digitalocean/raw/master/box/digital_ocean.box"
  config.ssh.private_key_path = '~/.ssh/id_ed25519'
  config.vm.synced_folder "remote_files", "/minitwit", type: "rsync"
  config.vm.synced_folder "data", "/data", type: "rsync"
  config.vm.synced_folder ".", "/vagrant", disabled: true

  config.vm.define "minitwit-3", primary: true do |server|
    server.vm.provider :digital_ocean do |provider|
      provider.ssh_key_name = ENV["SSH_KEY_NAME"]
      provider.token = ENV["DIGITAL_OCEAN_TOKEN"]
      provider.image = 'ubuntu-22-04-x64'
      provider.region = 'fra1'
      provider.size = 's-1vcpu-1gb'
      provider.privatenetworking = true
    end

    server.vm.hostname = "minitwit-3"
    server.vm.provision "shell", inline: 'echo "export DOCKER_USERNAME=' + "'" + ENV["DOCKER_USERNAME"] + "'" + '" >> ~/.bash_profile'
    server.vm.provision "shell", inline: 'echo "export DOCKER_PASSWORD=' + "'" + ENV["DOCKER_PASSWORD"] + "'" + '" >> ~/.bash_profile'


    # # TRIGGER: Assign the Reserved IP using DropletKit
    # server.trigger.after :up do |trigger|
    #   trigger.info = "Assigning Reserved IP to Droplet..."
    #   trigger.ruby do |env, machine|
    #     # Initialize client with your environment token
    #     client = DropletKit::Client.new(access_token: ENV['DIGITAL_OCEAN_TOKEN'])
        
    #     # The Reserved IP from your provided image
    #     reserved_ip = '209.38.114.224' 
        
    #     # Uses the dynamic ID from Vagrant instead of the hardcoded one
    #     target_id = machine.id 

    #     begin
    #       # The assignment logic from your snippet
    #       client.reserved_ip_actions.assign(ip: reserved_ip, droplet_id: target_id)
    #     rescue => e
    #       puts "Could not assign Reserved IP: #{e.message}"
    #     end
    #   end
    # end

    # # Not sure if all of this trigger stuff is necessary, copied it from Helge's file
    # server.trigger.after :up do |trigger|
    #   trigger.info =  "Writing minitwit's IP to file..."
    #   trigger.ruby do |env,machine|

    #     reserved_ip = '209.38.114.224' 

    #     File.write($ip_file, reserved_ip)
    #   end
    # end

    server.vm.provision "shell", inline: <<-SHELL
    # The following addresses an issue in DO's Ubuntu images, which still contain a lock file
    sudo apt-get update

    # The following address an issue in DO's Ubuntu images, which still contain a lock file
    #sudo killall apt apt-get
    #sudo rm /var/lib/dpkg/lock-frontend

    # Install docker and docker compose
    #sudo apt-get install -y docker.io docker-compose-v2
    sudo apt install -qq -y docker.io
    sudo apt install -qq -y docker-compose-v2

    sudo systemctl status docker
    # sudo usermod -aG docker ${USER}

    echo -e "\nVerifying that docker works ...\n"
    docker run --rm hello-world
    docker rmi hello-world

    echo -e "\nOpening port for minitwit ...\n"
    ufw allow 5035 && \
    ufw allow 22/tcp

    echo ". $HOME/.bashrc" >> $HOME/.bash_profile

    echo -e "\nConfiguring credentials as environment variables...\n"

    source $HOME/.bash_profile

    echo -e "\nSelecting Minitwit Folder as default folder when you ssh into the server...\n"
    echo "cd /minitwit" >> ~/.bash_profile

    chmod +x /minitwit/deploy.sh

    echo -e "\nVagrant setup done ..."

    SHELL
  end
end