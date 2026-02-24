## Choice of pipeline service

### considerations for choice of CI/CD pipeline service
* Github actions
	* Creates a new VM (Virtual Machine) for each pipeline activation. This ensures there are not older versions, and we dont have to ensure the amount of memory we save. 
		* https://docs.github.com/en/actions/concepts/runners/github-hosted-runners
	* The pipeline would be "in-house", so to pull the code is fast, and would not need any authentication for this.
	* However, because it is a "new" Vm each pipeline call, we would have to rebuild the image each time, which is sub-optimal now that Docker uses caching to see what has been changed on each iteration.
		*  However * 2, github has been aware of this, and Docker has therefore created a pagckage called Githubs Action Cache, that can use the the old image to create see the difference and use this for the new images.
		* This is however only a feature in Dockerx build
			* https://docs.docker.com/build/cache/backends/gha/
	* Reliability is a downside. It has had about 90% uptime which is not very good. 
	* All github actions users share the base, which also can be slow.
* GitLab
	* https://about.gitlab.com/solutions/github/ 
	* There is integration for gitlab CI/CD and GitHub.
	* However, it has to pull a copy of the repository, to use this for making the Docker image, and testing it, which is also slow.	
	* The compute time is way lower for this, compared to github actions, on the free tier. (gitlab: 400 m compute time, Github: 2000 m compute time.)
	* again, like before this does not integrate caching, so we have to use commands for pulling the old image, and comparing this to the new mirror of the code.
* Jenkins
	* Individually hosted on a new droplet. Can "relatively" easily change between different places we host the code, i.e. if we change to gitlab for the code, it would be realitvely easy to move over the pipeline to this.
	* It has to pull a copy of the repository, to use this for making the Docker image.
	* However, we are the SysAdmin! We have to be the ones that ensure the packages and dependencies are up to date, and collaborate correctly with each other.
		Which also in turn mean we have to ensure the "security" of this pipeline.
	* We have to maintain the storage of this droplet running the pipeline, i.e. the droplet would store old images, and at some point the storage would be filled. Therefore we would have to "prune" these old images at some point, to ensure we would have process.

With these considerations in mind, we decided to start with implementing this with github pipeline as a start, and if we had extra time we wanted to try to create a droplet, that used Jenkins instead, to see if this would work.