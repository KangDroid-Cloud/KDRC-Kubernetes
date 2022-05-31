package kdrcKubernetes

import (
	"dagger.io/dagger"
	"universe.dagger.io/bash"
	"universe.dagger.io/docker"
)

dagger.#Plan & {
	client: {
		filesystem: {
		    "./":read: {
		    	contents: dagger.#FS
		    	exclude: [".idea"]
		    },
		    "~/.docker/buildx":read: {
		        contents: dagger.#FS
		    }
		},
		network:"unix:///var/run/docker.sock": {
			connect: dagger.#Socket
		},
		env: {
		    CRI_URL: dagger.#Secret,
		    CRI_USERNAME: dagger.#Secret,
		    CRI_SECRET: dagger.#Secret
		}
	},
	
	actions: {
		dependencies: docker.#Build & {
			steps: [
				docker.#Pull & {
					source: "mcr.microsoft.com/dotnet/sdk:6.0"
				},
				
				bash.#Run & {
					workdir: "/"
					script:  contents: #"""
						apt-get update
						DEBIAN_FRONTEND='noninteractive' apt-get install -y --no-install-recommends ca-certificates curl gnupg lsb-release
						curl -fsSL https://download.docker.com/linux/debian/gpg | gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg
						echo "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] https://download.docker.com/linux/debian $(lsb_release -cs) stable" | tee /etc/apt/sources.list.d/docker.list > /dev/null
						apt-get update
						DEBIAN_FRONTEND='noninteractive' apt-get install -y --no-install-recommends docker-ce-cli
					"""#
				},
				
				docker.#Copy & {
					contents: client.filesystem."./".read.contents
					dest: "/src"
				}
			]
		}
		
		deployTestImage: bash.#Run & {
		    input: dependencies.output
			workdir: "/src"
			mounts: docker: {
		    		contents: client.network."unix:///var/run/docker.sock".connect
		    		dest: "/var/run/docker.sock"
		    },
			mounts: buildx: {
					contents: client.filesystem."~/.docker/buildx".read.contents
					dest: "/root/.docker/buildx"
		    }
			env: {
			    CRI_URL: client.env.CRI_URL
			    CRI_USERNAME: client.env.CRI_USERNAME
			    CRI_SECRET: client.env.CRI_SECRET
			}
			script: contents: #"""
			        ls -al ~/.docker/buildx
			        docker login --username $CRI_USERNAME --password $CRI_SECRET $CRI_URL
			        docker buildx build -f KDRC-Kubernetes/Dockerfile --platform linux/amd64,linux/arm64 -t $CRI_URL/kubernetes/test:latest --push --builder kdr-integration .
			"""#
		}
	}
}