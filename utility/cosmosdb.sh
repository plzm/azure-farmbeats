#!/bin/bash

resource_group_name="gh20cus"
account_name="documentdb-gysr4"
database_name="DataHub"
index_file="cosmosdbindex.json"

az cosmosdb sql container delete -g $resource_group_name -a $account_name -d $database_name -n "DeviceModel" -y
az cosmosdb sql container delete -g $resource_group_name -a $account_name -d $database_name -n "Device" -y
az cosmosdb sql container delete -g $resource_group_name -a $account_name -d $database_name -n "SensorModel" -y
az cosmosdb sql container delete -g $resource_group_name -a $account_name -d $database_name -n "Sensor" -y

az cosmosdb sql container create -g $resource_group_name -a $account_name -d $database_name -n "DeviceModel" -p "/partitionKey"
az cosmosdb sql container create -g $resource_group_name -a $account_name -d $database_name -n "Device" -p "/partitionKey" --idx @$index_file
az cosmosdb sql container create -g $resource_group_name -a $account_name -d $database_name -n "SensorModel" -p "/partitionKey"
az cosmosdb sql container create -g $resource_group_name -a $account_name -d $database_name -n "Sensor" -p "/partitionKey" --idx @$index_file
