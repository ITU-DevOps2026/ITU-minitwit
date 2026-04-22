#Script for setting up enviroment variables, and changes the default folder.

echo ". $HOME/.bashrc" >> $HOME/.bash_profile
echo -e "\nConfiguring credentials as environment variables...\n"
source $HOME/.bash_profile

echo -e "\nSelecting Minitwit Folder as default folder when you ssh into the server...\n"
echo "cd /minitwit" >> ~/.bash_profile

sed -i 's/\r$//' /minitwit/deploy.sh
chmod +x /minitwit/deploy.sh