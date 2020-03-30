# Introduction
A very demostration on how to use Azure Service Bus to call an internal REST API from an Azure Function

# Setup
* Run ./infra/create_infrastructure.sh -g {Resource Group} -l {location} -s {Subscription Name}.
    * This will create:
        * Azure Service Bus with two Queues - ping and pong
        * Azure Cosmos DB with a Database named Requests and a Collection named Items
        * Azure Function
* Manually create the following in Azure:
    * A virtual network with subnets:
        * Kubernetes
        * Servers
    * An AKS cluster deployed to the Kubernetes subnet
    * A dedicated Azure Storage for 
    * A ubuntu VM deployed to the Servers subnet
        * If an NSG is created for the VM, please open port 8081 

# Application Deployment 
## API
* cd api
* go build main.go -o api
* scp api manager@{ubuntu-vm-public-ip}:/opt/api
* ssh manager@{ubuntu-vm-public-ip}
* ./api

## Producer
* cd servicebus-producer
* func azure functionapp publish {function-app-name}

## Consumer
* Copy the access keys/connection strings for Service Bus
* cd servicebus-consumer
* Update local.settings.json
    * SERVICEBUS_CONSTR
    * AzureWebJobsStorage 
        * Should create an Azure Storage Account dedicated just for this Function even though it executes inside AKS
    * API_ENDPOINT
        * Should be in the form of 'http://<ubntu-vm-internal-ip>:8081
* func kubernetes install --namespace keda
* func kubernetes deploy --name internal-consumer --min-replicas 1 --namespace servicebusdemo

# Test
# To Do List 
# Issues