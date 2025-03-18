# README

## Setup
Run `dotnet ef database update --project JobSagaRunner` to create the job sagas database. This assumes that postgres is running on `localhost:5432` with the login `postgres/postgres`. If you need to change the connection string, do so in `JobSagaRunner/appsettings.json`.

## Running
Run the following in 3 different terminals:

`dotnet run --project JobSagaRunner`
`dotnet run --project JobConsumer --launch-profile "JobConsumer BlueTenant"`
`dotnet run --project JobConsumer --launch-profile "JobConsumer RedTenant"`

Each JobConsumer submits a recurring job to run every 10 seconds, however only one of the JobConsumer instances actually processes jobs, and the `ServiceAddress` column in `job_saga` shows the same address for both tenants' jobs.

Note that JobSagaConsumer logs "Found 0 instances for Tenant RedTenant" (or  BlueTenant)" and "Found 1 instances for Tenant BlueTenant" (or RedTenant - the opposite of the one that logged 0 instances) and it always logs "JobType total instances: 1". The total instances should be 2 as there are two unique job sagas not one.