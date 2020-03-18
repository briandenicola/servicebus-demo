# Introduction
A very demostration on how to use Azure Service Bus to call an internal REST API from an Azure Function

# Setup
* Create the following in Azure:
    * Azure Service Bus with two Queues - ping and pong
    * Azure Cosmos DB with a Database named Requests and a Collection named Items
        - /RequestId is the partition Key
    * Azure Function with Runtime set to version 2
    * A virtual network with subnets:
        * Kubernetes
        * Servers
    * An AKS cluster deployed to the Kubernetes subnet
    * A ubuntu VM deployed to the Servers subnet
        * If an NSG is created for the VM, please open port 8081
* Copy the access keys/connection strings for Cosmos DB and Service Bus

# Application Deployment 
## API
* cd api
* go build main.go
* scp main manager@<ubuntu-vm-ip>:/opt/api
* ssh manager@<ubuntu-vm-ip>
* ./api

## Producer
* Update Application Settings for the Azure Function
    * COSMOSDB_CONSTR
    * SERVICEBUS_CONSTR
* cd servicebus-producer
* func azure functionapp publish <function-app-name>

## Consumer
* cd servicebus-consumer
* Update local.settings.json
    * SERVICEBUS_CONSTR
    * AzureWebJobsStorage 
        * Should create an Azure Storage Account dedicated just for this Function even though it executes inside AKS
    * API_ENDPOINT
        * Should be in the form of 'http://<ubntu-vm-ip>:8081
* func kubernetes install --namespace keda
* func kubernetes deploy --name internal-consumer --min-replicas 1 --namespace servicebusdemo

# Test

# To Do List 

# Issues

