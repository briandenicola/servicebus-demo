#!/bin/bash

while (( "$#" )); do
  case "$1" in
    -g|--resource-group)
      RG=$2
      shift 2
      ;;
    -l|--location)
      location=$2
      shift 2
      ;;
    -s|--subscription)
      subscription=$2
      shift 2
      ;;
    -n|--name)
      appName=$2
      shift 2
      ;;
    -h|--help)
      echo "Usage: ./create_infrastructure.sh -n {App Name} -g {Resource Group} -l {location} -s {Subscription Name}"
      exit 0
      ;;
    --) 
      shift
      break
      ;;
    -*|--*=) 
      echo "Error: Unsupported flag $1" >&2
      exit 1
      ;;
  esac
done

az_cli_ver=`az --version | grep -i azure-cli | awk '{print $2}'`
if dpkg --compare-versions ${az_cli_ver} le 2.0.78; then
  echo "This script requires az cli to be at least 2.0.78"
  exit 1
fi

if [[ -z "${appName}" ]]; then
  appName=`cat /dev/urandom | tr -dc 'a-z' | fold -w 8 | head -n 1`
fi 

cosmosDBAccountName=db${appName}001
servicebusNameSpace=hub${appName}001
functionAppName=func${appName}001
funcStorageName=${appName}sa001

az account show  >> /dev/null 2>&1
if [[ $? -ne 0 ]]; then
  az login
fi

#Get Subscription Id
az account set -s ${subscription}

#Create Resource Group
az group create -n $RG -l $location

#Create Cosmos
database=Requests
collection=Items

az cosmosdb create -g ${RG} -n ${cosmosDBAccountName} --kind GlobalDocumentDB 
az cosmosdb sql database create  -g ${RG} -a ${cosmosDBAccountName} -n ${database}
az cosmosdb sql container create -g ${RG} -a ${cosmosDBAccountName} -d ${database} -n ${collection} --partition-key-path '/requestId'
cosmosConnectionString=`az cosmosdb list-connection-strings -n ${cosmosDBAccountName} -g ${RG} --query 'connectionStrings[0].connectionString' -o tsv`

#Create Service Bus
queues=(ping pong)
az servicebus namespace create -g ${RG} -n ${servicebusNameSpace} -l ${location} --sku Premium --capacity 1
for queue in ${queues[*]}; 
do 
  az servicebus queue create -g ${RG} --name ${queue} --namespace-name ${servicebusNameSpace}
done
sbConnectionString=`az servicebus namespace authorization-rule keys list -g ${RG} --namespace-name ${servicebusNameSpace} --name RootManageSharedAccessKey -o tsv --query primaryConnectionString`

#Create Azure Function
if ! `az functionapp show --name $functionAppName --resource-group $RG -o none`
then
    az storage account create --name $funcStorageName --location $location --resource-group $RG --sku Standard_LRS
    az functionapp create --name $functionAppName --storage-account $funcStorageName --consumption-plan-location $location --resource-group $RG
    az functionapp identity assign --name $functionAppName --resource-group $RG
fi
az functionapp config appsettings set -g $RG -n $functionAppName --settings SERVICEBUS_CONSTR=${sbConnectionString}
az functionapp config appsettings set -g $RG -n $functionAppName --settings COSMOSDB_CONSTR=${cosmosConnectionString}

# echo Application name
echo ------------------------------------
echo "Infrastructure built successfully. Application Name: ${appName}"
echo ------------------------------------